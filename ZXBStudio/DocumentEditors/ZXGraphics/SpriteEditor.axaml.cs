using Avalonia.Controls;
using System.Diagnostics;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.Linq;
using ZXBasicStudio.Common;
using Avalonia;
using ZXBasicStudio.DocumentModel.Classes;
using System.IO;
using ZXBasicStudio.Classes;
using AvaloniaEdit.Folding;
using Avalonia.Threading;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.Folding;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ShimSkiaSharp;
using Avalonia.Interactivity;
using Avalonia.Input;
using AvaloniaEdit;
using System.Reflection;
using FFmpeg.AutoGen;
using Avalonia.Media;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    /// <summary>
    /// Editor for GDUs and Fonts
    /// </summary>
    public partial class SpriteEditor : ZXDocumentEditorBase, IObserver<AvaloniaPropertyChangedEventArgs>
    {
        #region Events

        public override event EventHandler? DocumentRestored;
        public override event EventHandler? DocumentModified;
        public override event EventHandler? DocumentSaved;
        public override event EventHandler? RequestSaveDocument;

        #endregion


        #region ZXDocumentBase properties

        public override string DocumentName
        {
            get
            {
                return Path.GetFileName(FileName);
            }
        }


        public override string DocumentPath
        {
            get
            {
                return Path.GetFullPath(FileName);
            }
        }

        public override bool Modified
        {
            get
            {
                return _Modified;
            }
        }

        private bool _Modified = false;

        protected virtual AbstractFoldingStrategy? foldingStrategy { get { return null; } }

        private Guid documentTypeId = Guid.Empty;

        #endregion


        #region IObserver implementation for modified document notifications

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(AvaloniaPropertyChangedEventArgs value)
        {

        }

        #endregion


        #region Private variables

        private string FileName = "";

        /// <summary>
        /// List of sprites patterns controls in the set
        /// </summary>
        private List<SpritePatternControl> SpritePatternsList = null;

        /// <summary>
        /// last zoom value
        /// </summary>
        private int lastZoom = 0;

        /// <summary>
        /// Zooms values
        /// </summary>
        private int[] zooms = new int[]
        {
            1,2,4,8,16,24,32,48,64
        };

        private byte actualFrame = 0;

        private FoldingManager? fManager;
        private DispatcherTimer? updateFoldingsTimer;

        #endregion


        #region ZXDocumentBase functions

        public override bool SaveDocument(TextWriter OutputLog)
        {
            try
            {
                var masterList = SpritePatternsList.Select(d => d.SpriteData).ToArray();
                var sprList = new List<Sprite>();
                foreach (var spr in masterList)
                {
                    sprList.Add(spr.Clonar<Sprite>());
                }
                foreach (Sprite spr in sprList)
                {
                    if (spr == null)
                    {
                        continue;
                    }

                    if (spr.Frames < spr.Patterns.Count())
                    {
                        spr.Patterns = spr.Patterns.Take(spr.Frames).ToList();
                    }
                }

                var dataJSon = sprList.Serializar();
                if (!ServiceLayer.Files_SaveFileString(FileName, dataJSon))
                {
                    return false;
                };

                _Modified = false;
                DocumentSaved?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                OutputLog.WriteLine($"Error saving file {FileName}: {ex.Message}");
                return false;
            }
        }


        public override bool RenameDocument(string NewName, TextWriter OutputLog)
        {
            FileName = NewName;
            return true;
        }


        public override bool CloseDocument(TextWriter OutputLog, bool ForceClose)
        {
            return true;
        }


        public override void Dispose()
        {
        }


        public override void Activated()
        {
            this.Focus();
            ctrlPreview.Start();
            base.Activated();
        }


        public override void Deactivated()
        {
            ctrlPreview.Stop();
            base.Deactivated();
        }

        #endregion





        /// <summary>
        /// Constructor, without parameters is mandatory to cal Initialize
        /// </summary>
        public SpriteEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        public SpriteEditor(string fileName, Guid documentTypeId)
        {
            this.documentTypeId = documentTypeId;
            InitializeComponent();
            new Thread(() => Initialize(fileName)).Start();
        }


        /// <summary>
        /// Initialize the system
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if OK, or False if error</returns>
        public bool Initialize(string fileName)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _Initialize(fileName);
            });
            return true;
        }


        private void _Initialize(string fileName)
        {
            _Modified = false;

            ServiceLayer.Initialize();

            FileName = fileName;

            SpritePatternsList = new List<SpritePatternControl>();

            var data = ServiceLayer.GetFileData(fileName);
            if (data != null)
            {
                var dataS = Encoding.UTF8.GetString(data);
                if (!string.IsNullOrEmpty(dataS))
                {
                    Sprite[] sprites = dataS.Deserializar<Sprite[]>();

                    foreach (var sprite in sprites)
                    {
                        // Check attributes for ZX Spectrum mode
                        if (sprite != null && sprite.GraphicMode == GraphicsModes.ZXSpectrum)
                        {
                            foreach (var pattern in sprite.Patterns)
                            {
                                int cW = sprite.Width / 8;
                                int cH = sprite.Height / 8;
                                int l = cW * cH;
                                if (pattern.Attributes == null)
                                {
                                    pattern.Attributes = new AttributeColor[cW * cH];
                                    for (int n = 0; n < pattern.Attributes.Length; n++)
                                    {
                                        pattern.Attributes[n] = new AttributeColor()
                                        {
                                            Ink = 1,
                                            Paper = 0
                                        };
                                    }
                                }
                                if (pattern.Attributes.Length != l)
                                {
                                    pattern.Attributes = pattern.Attributes.Take(l).ToArray();
                                }
                            }
                        }
                        // Create pattern list
                        var spc = new SpritePatternControl();
                        spc.Initialize(sprite, SpriteList_Command);
                        SpritePatternsList.Add(spc);
                        wpSpriteList.Children.Add(spc);
                    }
                }
            }

            if (SpritePatternsList.Count == 0)
            {
                SpriteList_AddSprite();
                ctrlEditor.Initialize(Editor_Command);
                ctrlPreview.Initialize(null);
                ctrlProperties.Initialize(null, SpriteProperties_Command);
            }
            else
            {
                ctrlEditor.Initialize(Editor_Command);
                ctrlPreview.Initialize(SpritePatternsList[0].SpriteData);
                ctrlProperties.Initialize(SpritePatternsList[0].SpriteData, SpriteProperties_Command);
            }

            sldZoom.PropertyChanged += SldZoom_PropertyChanged;
            sldFrame.PropertyChanged += SldFrame_PropertyChanged;

            btnClear.Tapped += BtnClear_Tapped;
            btnCut.Tapped += BtnCut_Tapped;
            btnCopy.Tapped += BtnCopy_Tapped;
            btnPaste.Tapped += BtnPaste_Tapped;
            btnHMirror.Tapped += BtnHMirror_Tapped;
            btnVMirror.Tapped += BtnVMirror_Tapped;
            btnRotateLeft.Tapped += BtnRotateLeft_Tapped;
            btnRotateRight.Tapped += BtnRotateRight_Tapped;
            btnShiftUp.Tapped += BtnShiftUp_Tapped;
            btnShiftRight.Tapped += BtnShiftRight_Tapped;
            btnShiftDown.Tapped += BtnShiftDown_Tapped;
            btnShiftLeft.Tapped += BtnShiftLeft_Tapped;
            btnMoveUp.Tapped += BtnMoveUp_Tapped;
            btnMoveRight.Tapped += BtnMoveRight_Tapped;
            btnMoveDown.Tapped += BtnMoveDown_Tapped;
            btnMoveLeft.Tapped += BtnMoveLeft_Tapped;
            btnInvert.Tapped += BtnInvert_Tapped;
            btnMask.Tapped += BtnMask_Tapped;
            btnExport.Tapped += BtnExport_Tapped;

            Refresh();
            _Modified = false;

            if (SpritePatternsList.Count > 1)
            {
                SpriteList_Command(SpritePatternsList.ElementAt(0), "SELECTED");
            }

            btnPaper.Tapped += BtnPaper_Click;
            btnInk.Tapped += BtnInk_Tapped;
            UpdateColorPanel();

            this.AddHandler(KeyDownEvent, Keyboard_Down, handledEventsToo: true);
            this.Focus();
        }


        #region Color

        private void BtnPaper_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ctrlColorPicker.IsVisible)
            {
                ctrlColorPicker.IsVisible = false;
                return;
            }
            var sprite = ctrlEditor.SpriteData;
            if (sprite.GraphicMode == GraphicsModes.Monochrome)
            {
                return;
            }
            ctrlColorPicker.IsVisible = true;
            ctrlColorPicker.Inicialize(sprite.GraphicMode, sprite.Palette, ctrlEditor.SecondaryColorIndex, ColorPickerPaper_Action);
        }


        private void BtnInk_Tapped(object? sender, TappedEventArgs e)
        {
            if (ctrlColorPicker.IsVisible)
            {
                ctrlColorPicker.IsVisible = false;
                return;
            }
            var sprite = ctrlEditor.SpriteData;
            if (sprite.GraphicMode == GraphicsModes.Monochrome)
            {
                return;
            }
            ctrlColorPicker.IsVisible = true;
            ctrlColorPicker.Inicialize(sprite.GraphicMode, sprite.Palette, ctrlEditor.PrimaryColorIndex, ColorPickerInk_Action);
        }


        private void UpdateColorPanel()
        {
            var sprite = ctrlEditor.SpriteData;
            if (sprite == null)
            {
                return;
            }
            switch (sprite.GraphicMode)
            {
                case GraphicsModes.Monochrome:
                    {
                        var ink = sprite.Palette[1];
                        var paper = sprite.Palette[0];
                        grdPaper.Background = new SolidColorBrush(Color.FromRgb(paper.Red, paper.Green, paper.Blue));
                        txtPaper.Foreground = new SolidColorBrush(Color.FromRgb(ink.Red, ink.Green, ink.Blue));
                        txtPaper.Text = "0";
                        grdInk.Background = new SolidColorBrush(Color.FromRgb(ink.Red, ink.Green, ink.Blue));
                        txtInk.Foreground = new SolidColorBrush(Color.FromRgb(paper.Red, paper.Green, paper.Blue));
                        txtInk.Text = "1";
                    }
                    break;

                case GraphicsModes.ZXSpectrum:
                case GraphicsModes.Next:
                    {
                        var ink = sprite.Palette[ctrlEditor.PrimaryColorIndex];
                        var paper = sprite.Palette[ctrlEditor.SecondaryColorIndex];
                        grdPaper.Background = new SolidColorBrush(Color.FromRgb(paper.Red, paper.Green, paper.Blue));
                        txtPaper.Foreground = new SolidColorBrush(Color.FromRgb(ink.Red, ink.Green, ink.Blue));
                        txtPaper.Text = ctrlEditor.SecondaryColorIndex.ToString();
                        grdInk.Background = new SolidColorBrush(Color.FromRgb(ink.Red, ink.Green, ink.Blue));
                        txtInk.Foreground = new SolidColorBrush(Color.FromRgb(paper.Red, paper.Green, paper.Blue));
                        txtInk.Text = ctrlEditor.PrimaryColorIndex.ToString();
                    }
                    break;
            }
        }



        private void ColorPickerPaper_Action(string command, int indexColor)
        {
            ctrlEditor.SecondaryColorIndex = indexColor;
            UpdateColorPanel();
        }


        private void ColorPickerInk_Action(string command, int indexColor)
        {
            ctrlEditor.PrimaryColorIndex = indexColor;
            UpdateColorPanel();
        }

        #endregion


        #region SpriteList

        private void SpriteList_Command(SpritePatternControl sender, string command)
        {
            switch (command)
            {
                case "ADD":
                    SpriteList_AddSprite();
                    ctrlEditor.SpriteData = sender.SpriteData;
                    ctrlPreview.SpriteData = sender.SpriteData;
                    ctrlPreview.Refresh();
                    ctrlProperties.SpriteData = sender.SpriteData;
                    SpriteList_Modified(sender.SpriteData);
                    break;
                case "SELECTED":
                    SpriteList_Unselect(sender);
                    ctrlEditor.SpriteData = sender.SpriteData;
                    ctrlPreview.SpriteData = sender.SpriteData;
                    ctrlPreview.Refresh();
                    ctrlProperties.SpriteData = sender.SpriteData;
                    ctrlProperties.Refresh();
                    break;
            }
        }


        private void SpritePreview_Command(SpritePreviewControl sender, string command)
        {

        }


        private void SpriteProperties_Command(SpritePropertiesControl sender, string command)
        {
            switch (command)
            {
                case "DELETE":
                    {
                        var sprite = SpritePatternsList.FirstOrDefault(d => d.SpriteData != null && d.SpriteData.Id == sender.SpriteData.Id);
                        if (sprite != null)
                        {
                            SpriteList_Delete(sprite);
                            SpriteList_Modified(sender.SpriteData);
                        }
                    }
                    break;
                case "INSERT":
                    SpriteList_Insert(sender.SpriteData);
                    SpriteList_Modified(sender.SpriteData);
                    break;
                case "CLONE":
                    SpriteList_Clone(sender.SpriteData);
                    SpriteList_Modified(sender.SpriteData);
                    break;
                case "REFRESH":
                    ctrlEditor.SpriteData = sender.SpriteData;
                    SpriteList_Modified(sender.SpriteData);
                    if (ctrlEditor.SpriteData.CurrentFrame != actualFrame)
                    {
                        actualFrame = ctrlEditor.SpriteData.CurrentFrame;
                        if (sldFrame.Maximum < actualFrame)
                        {
                            sldFrame.Maximum = actualFrame;
                        }
                        sldFrame.Value = actualFrame;
                        Refresh();
                    }
                    UpdateColorPanel();
                    break;
                case "CHANGEMODE":
                    switch (sender.SpriteData.GraphicMode)
                    {
                        case GraphicsModes.Monochrome:
                            ctrlEditor.PrimaryColorIndex = 1;
                            ctrlEditor.SecondaryColorIndex = 0;
                            break;
                        case GraphicsModes.ZXSpectrum:
                            ctrlEditor.PrimaryColorIndex = 0;
                            ctrlEditor.SecondaryColorIndex = 7;
                            break;
                    }
                    ctrlEditor.SpriteData = sender.SpriteData;
                    SpriteList_Modified(sender.SpriteData);
                    UpdateColorPanel();
                    break;
            }
        }


        private void SpriteList_AddSprite()
        {
            SpritePatternControl selectedSprite = null;
            int id = 0;
            if (SpritePatternsList.Count > 0)
            {
                id = SpritePatternsList.Where(d => d.SpriteData != null).Max(d => d.SpriteData.Id) + 1;
            }
            foreach (var spc in SpritePatternsList)
            {
                if (spc.SpriteData != null && spc.SpriteData.Id < 0)
                {
                    spc.SpriteData.Id = id;
                    if (string.IsNullOrEmpty(spc.SpriteData.Name))
                    {
                        spc.SpriteData.Name = "Sprite " + spc.SpriteData.Id.ToString();
                    }
                    spc.SpriteData.Export = true;
                    selectedSprite = spc;
                    break;
                }
            }

            var spritePattern = new SpritePatternControl();
            SpritePatternsList.Add(spritePattern);
            wpSpriteList.Children.Add(spritePattern);
            spritePattern.Initialize(null, SpriteList_Command);

            SpriteList_Unselect(selectedSprite);
        }


        private void SpriteList_Unselect(SpritePatternControl selected)
        {
            foreach (var control in SpritePatternsList)
            {
                if (control == selected)
                {
                    control.IsSelected = true;
                }
                else
                {
                    control.IsSelected = false;
                }
            }
        }


        private void SpriteList_Delete(SpritePatternControl control)
        {
            SpritePatternsList.Remove(control);
            wpSpriteList.Children.Remove(control);
            ctrlEditor.SpriteData = null;
            ctrlPreview.SpriteData = null;
            ctrlProperties.SpriteData = null;
            control = null;
        }


        private void SpriteList_Insert(Sprite spriteData)
        {
            var current = spriteData.CurrentFrame;
            var curPat = spriteData.Patterns[current];

            var pat = curPat.Clonar<Pattern>();
            pat.RawData = new int[pat.RawData.Length];

            var pats = spriteData.Patterns.Take(current).ToList();
            pats.Add(pat);
            pats.AddRange(spriteData.Patterns.Skip(current));
            spriteData.Patterns = pats;
            spriteData.Frames++;
            ctrlProperties.Refresh();
            ctrlEditor.Refresh();
            sldFrame.Maximum = spriteData.Frames;
        }


        private void SpriteList_Clone(Sprite spriteData)
        {
            SpritePatternControl selectedSprite = null;
            int id = SpritePatternsList.Where(d => d.SpriteData != null).Max(d => d.SpriteData.Id) + 1;
            var spc = SpritePatternsList.FirstOrDefault(d => d.SpriteData == null);
            if (spc == null)
            {
                return;
            }
            var sd = spriteData.Clonar<Sprite>();
            sd.Id = id;
            sd.Name = spriteData.Name + " - copy";
            spc.SpriteData = sd;

            var spritePattern = new SpritePatternControl();
            SpritePatternsList.Add(spritePattern);
            wpSpriteList.Children.Add(spritePattern);
            spritePattern.Initialize(null, SpriteList_Command);

            spc.Select();
        }


        private void SpriteList_Modified(Sprite spriteData)
        {
            if (!_Modified)
            {
                _Modified = true;
                DocumentModified?.Invoke(this, EventArgs.Empty);
            }

            if (spriteData == null)
            {
                return;
            }
            var ctrl = SpritePatternsList.FirstOrDefault(d => d.SpriteData.Id == spriteData.Id);
            if (ctrl != null)
            {
                ctrl.SpriteData = spriteData;
                ctrl.Refresh();
            }
        }

        #endregion


        #region Sprite editor

        private void Editor_Command(SpritePatternEditor sender, string command)
        {
            switch (command)
            {
                case "REFRESH":
                    {
                        var cur = SpritePatternsList.FirstOrDefault(d => d.IsSelected);
                        if (cur != null)
                        {
                            cur.Refresh();
                            SpriteList_Modified(sender.SpriteData);
                            ctrlProperties.SpriteData = sender.SpriteData;
                            ctrlProperties.PrimaryColor = sender.PrimaryColorIndex;
                            ctrlProperties.SecondaryColor = sender.SecondaryColorIndex;
                            ctrlPreview.SpriteData = sender.SpriteData;
                        }
                    }
                    break;
            }
        }

        #endregion


        #region Main editor


        private void Refresh()
        {
            txtFrame.Text = "Frame " + actualFrame.ToString();
            if (ctrlProperties.SpriteData != null)
            {
                sldFrame.Maximum = ctrlProperties.SpriteData.Frames;
            }
            else
            {
                sldFrame.Maximum = 0;
            }
            sldFrame.UpdateLayout();
        }


        /// <summary>
        /// Zoom changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SldZoom_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            int z = (int)sldZoom.Value;
            if (z == 0 || z == lastZoom)
            {
                return;
            }
            lastZoom = z;

            z = zooms[z - 1];
            txtZoom.Text = "Zoom " + z.ToString() + "x";
            ctrlEditor.Zoom = z;
        }


        private void ZoomIn()
        {
            int v = sldZoom.Value.ToInteger();
            if (v < zooms.Length - 1)
            {
                sldZoom.Value = v - 1;
            }
        }


        private void ZoomOut()
        {
            int v = sldZoom.Value.ToInteger();
            if (v > 0)
            {
                sldZoom.Value = v - 1;
            }

        }


        private void SldFrame_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            byte v = (byte)sldFrame.Value;
            if (actualFrame == v || ctrlProperties.SpriteData == null || v < 0 || v >= (ctrlProperties.SpriteData.Frames))
            {
                return;
            }
            actualFrame = v;
            if (ctrlEditor.SpriteData != null)
            {
                ctrlEditor.SpriteData.CurrentFrame = actualFrame;
                ctrlEditor.Refresh();
            }
            Refresh();
        }

        #endregion


        #region ToolBar

        /// <summary>
        /// Clear click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.Clear();
        }

        /// <summary>
        /// Cut click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCut_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.Cut();
        }


        //Copy click
        private void BtnCopy_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.Copy();
        }


        /// <summary>
        /// Paste click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPaste_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.Paste();
        }


        /// <summary>
        /// Horizontal mirror click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHMirror_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.HorizontalMirror();
        }


        /// <summary>
        /// Vertical mirror click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVMirror_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.VerticalMirror();
        }


        /// <summary>
        /// Rotate left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRotateLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.RotateLeft();
        }


        /// <summary>
        /// Rotate right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRotateRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.RotateRight();
        }


        /// <summary>
        /// Shift up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShiftUp_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.ShiftUp();
        }


        /// <summary>
        /// Shift right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShiftRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.ShiftRight();
        }


        /// <summary>
        /// Shift down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShiftDown_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.ShiftDown();
        }


        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShiftLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.ShiftLeft();
        }


        /// <summary>
        /// Move up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMoveUp_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.MoveUp();
        }


        /// <summary>
        /// Move right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMoveRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.MoveRight();
        }


        /// <summary>
        /// Move down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMoveDown_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.MoveDown();
        }


        /// <summary>
        /// Move left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMoveLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.MoveLeft();
        }


        /// <summary>
        /// Invert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInvert_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.Invert();
        }


        /// <summary>
        /// Mask
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMask_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrlEditor.Mask();
        }


        private void BtnExport_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Export();
        }


        private void Export()
        {
            var dlg = new SpriteExportDialog();
            dlg.Initialize(FileName, SpritePatternsList.Select(d => d.SpriteData));
            dlg.ShowDialog(this.VisualRoot as Window);
        }

        #endregion



        #region Keyboard shortcus

        private Dictionary<Guid, Action> _keybCommands = new Dictionary<Guid, Action>();

        internal static Dictionary<string, ZXKeybCommand> keyboardCommands = new Dictionary<string, ZXKeybCommand> {
            { "Save", new ZXKeybCommand{ CommandId = Guid.Parse("87f7d73b-d28a-44f4-ba0c-41baa4de238c"), CommandName = "Save", Key = Key.S, Modifiers = KeyModifiers.Control } },
            { "Copy",new ZXKeybCommand{ CommandId = Guid.Parse("fee014bb-222b-42e3-80f3-048325b70e34"), CommandName = "Copy", Key = Key.C, Modifiers = KeyModifiers.Control } },
            { "Cut",new ZXKeybCommand{ CommandId = Guid.Parse("1edf352f-238b-421e-b69f-613dc63c0e47"), CommandName = "Cut", Key = Key.X, Modifiers = KeyModifiers.Control } },
            { "Paste", new ZXKeybCommand{ CommandId = Guid.Parse("f5d450b0-d126-4f62-885b-b3e28e638542"), CommandName = "Paste", Key = Key.V, Modifiers = KeyModifiers.Control } },
            { "Undo", new ZXKeybCommand{ CommandId = Guid.Parse("912c1887-ab37-4c0a-9aee-65d84b4521c7"), CommandName = "Undo", Key = Key.Z, Modifiers = KeyModifiers.Control } },
            { "Redo", new ZXKeybCommand{ CommandId = Guid.Parse("c5c506f0-d5e4-429f-ad21-af8cee7d1d9a"), CommandName = "Redo", Key = Key.Z, Modifiers = KeyModifiers.Control | KeyModifiers.Shift } },

            { "Clear", new ZXKeybCommand{ CommandId = Guid.Parse("246e1449-0a64-4327-a8d1-f1dbecbc69d2"), CommandName = "Clear", Key = Key.Q, Modifiers = KeyModifiers.Control | KeyModifiers.Shift } },
            { "Rotate Right", new ZXKeybCommand{ CommandId = new Guid("490e8830-f268-48f3-8989-92d6e22b8790"), CommandName = "Rotate Right", Key = Key.R, Modifiers = KeyModifiers.None } },
            { "Rotate Left", new ZXKeybCommand{ CommandId = new Guid("1da1bae1-2242-4061-9fa2-9265fc7f74b1"), CommandName = "Rotate Left", Key = Key.R, Modifiers = KeyModifiers.Shift } },
            { "Horizontal Mirror", new ZXKeybCommand{ CommandId = new Guid("aad6fb75-15c7-4f09-aa1c-af1e1b62f849"), CommandName = "Horizontal Mirror", Key = Key.H, Modifiers = KeyModifiers.None } },
            { "Vertical Mirror", new ZXKeybCommand{ CommandId = new Guid("3e20f906-c2e5-46b1-aa6d-8ee503769917"), CommandName = "Vertical Mirror", Key = Key.V, Modifiers = KeyModifiers.None } },
            { "Shift Up", new ZXKeybCommand{ CommandId = new Guid("7a2f0381-b603-4c5f-aefd-08ebafc74e5c"), CommandName = "Shift Up", Key = Key.Up, Modifiers = KeyModifiers.None } },
            { "Shift Right", new ZXKeybCommand{ CommandId = new Guid("75a310dd-66ec-41a5-bc8d-1b3408fd4f41"), CommandName = "Shift Right", Key = Key.Right, Modifiers = KeyModifiers.None } },
            { "Shift Down", new ZXKeybCommand{ CommandId = new Guid("2a73e9cd-f318-4a15-bdc7-daf35429c6e5"), CommandName = "Shift Down", Key = Key.Down, Modifiers = KeyModifiers.None } },
            { "Shift Left", new ZXKeybCommand{ CommandId = new Guid("1c8f1ec5-596c-4e7f-9fb6-9d5ff527b37a"), CommandName = "Shift Left", Key = Key.Left, Modifiers = KeyModifiers.None } },
            { "Move Up", new ZXKeybCommand{ CommandId = new Guid("6c202c3d-921f-43f7-b810-33febc8ad947"), CommandName = "Move Up", Key = Key.Up, Modifiers = KeyModifiers.Shift } },
            { "Move Right", new ZXKeybCommand{ CommandId = new Guid("8e2e96d0-0aa4-426e-a340-32cb53368000"), CommandName = "Move Right", Key = Key.Right, Modifiers = KeyModifiers.Shift } },
            { "Move Down", new ZXKeybCommand{ CommandId = new Guid("bdd1c206-e3ec-4a7d-8e61-42b620097b31"), CommandName = "Move Down", Key = Key.Down, Modifiers = KeyModifiers.Shift } },
            { "Move Left", new ZXKeybCommand{ CommandId = new Guid("6a6b98cf-5427-4d85-a230-6ab18544e312"), CommandName = "Move Left", Key = Key.Left, Modifiers = KeyModifiers.Shift } },
            { "Invert", new ZXKeybCommand{ CommandId = new Guid("e88125e8-f671-4085-ae8c-8d1f4866738a"), CommandName = "Invert", Key = Key.I, Modifiers = KeyModifiers.None } },
            { "Mask", new ZXKeybCommand{ CommandId = new Guid("efd5a1f4-cab8-4003-8370-8adf81176081"), CommandName = "Mask", Key = Key.M, Modifiers = KeyModifiers.None } },
            { "Export", new ZXKeybCommand{ CommandId = new Guid("321119c0-5c9b-40b5-9385-a62ca013f83c"), CommandName = "Export", Key = Key.E, Modifiers = KeyModifiers.None } },
            { "Zoom In", new ZXKeybCommand{ CommandId = new Guid("7e958c3d-c135-46b2-b041-63b8c7828787"), CommandName = "Zoom In", Key = Key.Add, Modifiers = KeyModifiers.None } },
            { "Zoom Out", new ZXKeybCommand{ CommandId = new Guid("d69843bb-8ad0-4eea-be58-29646dddaeed"), CommandName = "Zoom Out", Key = Key.Subtract, Modifiers = KeyModifiers.None } },
        };


        private void InitializeShortcuts()
        {
            DisableCommand(ApplicationCommands.Cut);
            DisableCommand(ApplicationCommands.Copy);
            DisableCommand(ApplicationCommands.Paste);

            _keybCommands = new Dictionary<Guid, Action>()
            {
                { keyboardCommands["Save"].CommandId, () => { RequestSaveDocument?.Invoke(this, EventArgs.Empty); } },
                { keyboardCommands["Copy"].CommandId, () => { ctrlEditor.Copy(); } },
                { keyboardCommands["Cut"].CommandId, () => { ctrlEditor.Cut(); } },
                { keyboardCommands["Paste"].CommandId, () => { ctrlEditor.Paste(); } },
                { keyboardCommands["Undo"].CommandId, () => { ctrlEditor.Undo(); } },
                { keyboardCommands["Redo"].CommandId, () => { ctrlEditor.Redo(); } },

                { keyboardCommands["Clear"].CommandId, () => { ctrlEditor.Clear(); } },
                { keyboardCommands["Rotate Right"].CommandId, () => { ctrlEditor.RotateRight(); } },
                { keyboardCommands["Rotate Left"].CommandId, () => { ctrlEditor.RotateLeft(); } },
                { keyboardCommands["Horizontal Mirror"].CommandId, () => { ctrlEditor.HorizontalMirror(); } },
                { keyboardCommands["Vertical Mirror"].CommandId, () => { ctrlEditor.VerticalMirror(); } },
                { keyboardCommands["Shift Up"].CommandId, () => { ctrlEditor.ShiftUp(); } },
                { keyboardCommands["Shift Right"].CommandId, () => { ctrlEditor.ShiftRight(); } },
                { keyboardCommands["Shift Down"].CommandId, () => { ctrlEditor.ShiftDown(); } },
                { keyboardCommands["Shift Left"].CommandId, () => { ctrlEditor.ShiftLeft(); } },
                { keyboardCommands["Move Up"].CommandId, () => { ctrlEditor.MoveUp(); } },
                { keyboardCommands["Move Right"].CommandId, () => { ctrlEditor.MoveRight(); } },
                { keyboardCommands["Move Down"].CommandId, () => { ctrlEditor.MoveDown(); } },
                { keyboardCommands["Move Left"].CommandId, () => { ctrlEditor.MoveLeft(); } },
                { keyboardCommands["Invert"].CommandId, () => { ctrlEditor.Invert(); } },
                { keyboardCommands["Mask"].CommandId, () => { ctrlEditor.Mask(); } },
                { keyboardCommands["Export"].CommandId, () => { Export(); } },
                { keyboardCommands["Zoom In"].CommandId, () => { ZoomIn(); } },
                { keyboardCommands["Zoom Out"].CommandId, () => { ZoomOut(); } },
            };
        }


        private void DisableCommand(RoutedCommand cut)
        {
            var field = typeof(RoutedCommand).GetField("<Gesture>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            KeyGesture g = new KeyGesture(Key.None, KeyModifiers.None);
            field.SetValue(cut, g);
        }

        private void Keyboard_Down(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            var commandId = ZXKeybMapper.GetCommandId(documentTypeId, e.Key, e.KeyModifiers);

            if (commandId != null && _keybCommands.ContainsKey(commandId.Value))
            {
                _keybCommands[commandId.Value]();
            }
        }

        #endregion

    }
}
