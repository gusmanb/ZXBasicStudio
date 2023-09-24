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
                var data = SpritePatternsList.Select(d => d.SpriteData).Serializar();
                if (!ServiceLayer.Files_SaveFileString(FileName, data))
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
        public SpriteEditor(string fileName)
        {
            InitializeComponent();
            new Thread(()=>Initialize(fileName)).Start();
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
        }


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
                case "CLONE":
                    SpriteList_Clone(sender.SpriteData);
                    SpriteList_Modified(sender.SpriteData);
                    break;
                case "REFRESH":
                    ctrlEditor.SpriteData = sender.SpriteData;
                    SpriteList_Modified(sender.SpriteData);
                    if (ctrlEditor.SpriteData.CurrentFrame !=actualFrame)
                    {
                        actualFrame = ctrlEditor.SpriteData.CurrentFrame;
                        if (sldFrame.Maximum < actualFrame)
                        {
                            sldFrame.Maximum = actualFrame;
                        }
                        sldFrame.Value = actualFrame;
                        Refresh();
                    }
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


        private void SldFrame_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            byte v = (byte)sldFrame.Value;
            if (ctrlProperties.SpriteData == null || v < 0 || v >= (ctrlProperties.SpriteData.Frames))
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
            var dlg = new FontGDUExportDialog();
            // TODO: Export...
            //dlg.Initialize(fileType, patterns.Select(d=>d.Pattern).ToArray());
            //dlg.ShowDialog(this.VisualRoot as Window);
        }

       #endregion


    }
}
