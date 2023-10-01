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
using Avalonia.Input;
using System.Collections.Generic;
using AvaloniaEdit;
using System.Reflection;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    /// <summary>
    /// Editor for GDUs and Fonts
    /// </summary>
    public partial class FontGDUEditor : ZXDocumentEditorBase, IObserver<AvaloniaPropertyChangedEventArgs>
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
                return Path.GetFileName(fileType.FileName);
            }
        }


        public override string DocumentPath
        {
            get
            {
                return Path.GetFullPath(fileType.FileName);
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

        /// <summary>
        /// File type of the actual document
        /// </summary>
        private FileTypeConfig fileType = null;

        /// <summary>
        /// Binary data of the actual doocument
        /// </summary>
        private byte[] fileData = null;

        /// <summary>
        /// Patterns of the actual document
        /// </summary>
        private PatternControl[] patterns = null;

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

        private FoldingManager? fManager;
        private DispatcherTimer? updateFoldingsTimer;

        private Guid documentTypeId = Guid.Empty;

        #endregion


        #region ZXDocumentBase functions

        public override bool SaveDocument(TextWriter OutputLog)
        {
            try
            {
                if (!Modified)
                {
                    return true;
                }


                if (!ServiceLayer.Files_Save_GDUorFont(fileType, patterns.Select(d => d.Pattern)))
                {
                    OutputLog.WriteLine($"Error saving file {fileType.FileName}: " + ServiceLayer.LastError);
                    return false;
                }
                _Modified = false;

                OutputLog.WriteLine($"File {fileType.FileName} saved successfully.");
                DocumentSaved?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                OutputLog.WriteLine($"Error saving file {fileType.FileName}: {ex.Message}");
                return false;
            }
        }


        public override bool RenameDocument(string NewName, TextWriter OutputLog)
        {
            fileType.FileName = NewName;
            return true;
        }


        public override bool CloseDocument(TextWriter OutputLog, bool ForceClose)
        {
            return true;
        }


        #endregion





        /// <summary>
        /// Constructor, without parameters is mandatory to cal Initialize
        /// </summary>
        public FontGDUEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        public FontGDUEditor(string fileName, Guid documentTypeId)
        {
            InitializeComponent();
            this.documentTypeId = documentTypeId;
            Initialize(fileName);
        }


        /// <summary>
        /// Initialize the system
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if OK, or False if error</returns>
        public bool Initialize(string fileName)
        {
            _Modified = false;

            ServiceLayer.Initialize();

            fileType = ServiceLayer.GetFileType(fileName);
            fileData = ServiceLayer.GetFileData(fileName);

            if (fileData == null || fileData.Length == 0)
            {
                fileData = ServiceLayer.Files_CreateData(fileType);
            }

            ctrlPreview.Initialize(GetPattern, fileType.NumerOfPatterns);

            CreatePatterns();

            sldZoom.PropertyChanged += SldZoom_PropertyChanged;
            txtEditorWidth.ValueChanged += TxtEditorWidth_ValueChanged;
            txtEditorHeight.ValueChanged += TxtEditorHeight_ValueChanged;

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

            InitializeShortcuts();

            this.AddHandler(KeyDownEvent, Keyboard_Down, handledEventsToo: true);
            this.Focus();

            return true;
        }


        public override void Dispose()
        {
            sldZoom.PropertyChanged -= SldZoom_PropertyChanged;
            txtEditorWidth.ValueChanged -= TxtEditorWidth_ValueChanged;
            txtEditorHeight.ValueChanged -= TxtEditorHeight_ValueChanged;

            btnClear.Tapped -= BtnClear_Tapped;
            btnCut.Tapped -= BtnCut_Tapped;
            btnCopy.Tapped -= BtnCopy_Tapped;
            btnPaste.Tapped -= BtnPaste_Tapped;
            btnHMirror.Tapped -= BtnHMirror_Tapped;
            btnVMirror.Tapped -= BtnVMirror_Tapped;
            btnRotateLeft.Tapped -= BtnRotateLeft_Tapped;
            btnRotateRight.Tapped -= BtnRotateRight_Tapped;
            btnShiftUp.Tapped -= BtnShiftUp_Tapped;
            btnShiftRight.Tapped -= BtnShiftRight_Tapped;
            btnShiftDown.Tapped -= BtnShiftDown_Tapped;
            btnShiftLeft.Tapped -= BtnShiftLeft_Tapped;
            btnMoveUp.Tapped -= BtnMoveUp_Tapped;
            btnMoveRight.Tapped -= BtnMoveRight_Tapped;
            btnMoveDown.Tapped -= BtnMoveDown_Tapped;
            btnMoveLeft.Tapped -= BtnMoveLeft_Tapped;
            btnInvert.Tapped -= BtnInvert_Tapped;
            btnMask.Tapped -= BtnMask_Tapped;
            btnExport.Tapped -= BtnExport_Tapped;

            this.KeyDown -= Keyboard_Down;
            ctrEditor.KeyDown -= Keyboard_Down;
        }


        /// <summary>
        /// Create patterns
        /// </summary>
        public void CreatePatterns()
        {
            if (patterns == null)
            {
                // Create patterns if not exist
                patterns = new PatternControl[fileType.NumerOfPatterns];
                //int x = 0;
                //int y = 0;
                for (int n = 0; n < fileType.NumerOfPatterns; n++)
                {
                    var ctrl = new PatternControl();
                    wpPatterns.Children.Add(ctrl);
                    patterns[n] = ctrl;
                }
                patterns[0].IsSelected = true;
            }

            // Update patterns
            for (int n = 0; n < patterns.Length; n++)
            {
                var p = new Pattern();
                p.Id = n;
                p.Number = "";
                switch (fileType.FileType)
                {
                    case FileTypes.UDG:
                        {
                            var id = n;
                            p.Number = id.ToString();
                            var c = Convert.ToChar(n + 65);
                            p.Name = c.ToString();
                        }
                        break;
                    case FileTypes.Font:
                        {
                            var id = n + 32;
                            p.Number = id.ToString();
                            var c = Convert.ToChar(n + 32);
                            p.Name = c.ToString();
                        }
                        break;
                    default:
                        p.Number = n.ToString();
                        p.Name = "";
                        break;
                }
                p.Data = ServiceLayer.Binary2PointData(n, fileData, 0, 0);
                patterns[n].Initialize(p, Pattern_Click);
                patterns[n].Refresh();
            }
            ctrEditor.Initialize(0, GetPattern, SetPattern);
        }


        /// <summary>
        /// Click on the pattern
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Pattern_Click(PatternControl selectedPattern)
        {
            foreach (var pattern in patterns)
            {
                pattern.IsSelected = false;
            }
            selectedPattern.IsSelected = true;
            ctrEditor.IdPattern = selectedPattern.Pattern.Id;
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
            ctrEditor.Zoom = z;
        }


        /// <summary>
        /// Height changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtEditorHeight_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            int h = txtEditorHeight.Text.ToInteger();
            ctrEditor.ItemsHeight = h;
        }


        /// <summary>
        /// Width changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtEditorWidth_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            int w = txtEditorWidth.Text.ToInteger();
            ctrEditor.ItemsWidth = w;
        }



        /// <summary>
        /// Get pattern from his id
        /// </summary>
        /// <param name="id">Id of the pattern to recover</param>
        /// <returns>Pattern or null if no pattern</returns>
        private Pattern GetPattern(int id)
        {
            var pat = patterns.FirstOrDefault(d => d.Pattern.Id == id);
            if (pat != null)
            {
                return pat.Pattern;
            }
            return null;
        }


        /// <summary>
        /// Set pattern
        /// </summary>
        /// <param name="id">Id of the pattern to set</param>
        /// <param name="pattern">Pattern to set</param>
        private void SetPattern(int id, Pattern pattern)
        {
            if (!_Modified)
            {
                _Modified = true;
                DocumentModified?.Invoke(this, EventArgs.Empty);
            }

            var pat = patterns.FirstOrDefault(d => d.Pattern.Id == pattern.Id);
            if (pat != null)
            {
                pat.Pattern = pattern;
                pat.Refresh();
            }
        }



        /// <summary>
        /// Save the actual document to disk
        /// </summary>
        /// <returns>True if ook or false if error</returns>
        public bool SaveDocument()
        {
            if (!ServiceLayer.Files_Save_GDUorFont(fileType, patterns?.Select(d => d.Pattern)))
            {
                return false;
            };

            _Modified = false;
            DocumentSaved?.Invoke(this, EventArgs.Empty);
            return true;
        }


        #region ToolBar

        /// <summary>
        /// Clear click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Clear();
        }

        /// <summary>
        /// Cut click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCut_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Cut();
        }


        //Copy click
        private void BtnCopy_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Copy();
        }


        /// <summary>
        /// Paste click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPaste_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Paste();
        }


        /// <summary>
        /// Horizontal mirror click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHMirror_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.HorizontalMirror();
        }


        /// <summary>
        /// Vertical mirror click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVMirror_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.VerticalMirror();
        }


        /// <summary>
        /// Rotate left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRotateLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.RotateLeft();
        }


        /// <summary>
        /// Rotate right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRotateRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.RotateRight();
        }


        /// <summary>
        /// Shift up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShiftUp_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.ShiftUp();
        }


        /// <summary>
        /// Shift right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShiftRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.ShiftRight();
        }


        /// <summary>
        /// Shift down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShiftDown_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.ShiftDown();
        }


        /// <summary>
        /// Shift left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnShiftLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.ShiftLeft();
        }


        /// <summary>
        /// Move up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMoveUp_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.MoveUp();
        }


        /// <summary>
        /// Move right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMoveRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.MoveRight();
        }


        /// <summary>
        /// Move down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMoveDown_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.MoveDown();
        }


        /// <summary>
        /// Move left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMoveLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.MoveLeft();
        }


        /// <summary>
        /// Invert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInvert_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Invert();
        }


        /// <summary>
        /// Mask
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMask_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Mask();
        }


        private void BtnExport_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var dlg = new FontGDUExportDialog();
            dlg.Initialize(fileType, patterns.Select(d=>d.Pattern).ToArray());
            dlg.ShowDialog(this.VisualRoot as Window);
        }

        #endregion


        #region Keyboard shortcus

        private Dictionary<Guid, Action> _keybCommands = new Dictionary<Guid, Action>();

        internal static Dictionary<string, ZXKeybCommand> keyboardCommands = new Dictionary<string, ZXKeybCommand> {
            { "Save", new ZXKeybCommand{ CommandId = Guid.Parse("87f7d73b-d28a-44f4-ba0c-41baa4de238c"), CommandName = "Save", Key = Key.S, Modifiers = KeyModifiers.Control } },
            { "Copy",new ZXKeybCommand{ CommandId = Guid.Parse("fee014bb-222b-42e3-80f3-048325b70e34"), CommandName = "Copy", Key = Key.C, Modifiers = KeyModifiers.Control } },
            { "Cut",new ZXKeybCommand{ CommandId = Guid.Parse("1edf352f-238b-421e-b69f-613dc63c0e47"), CommandName = "Cut", Key = Key.X, Modifiers = KeyModifiers.Control } },
            { "Paste", new ZXKeybCommand{ CommandId = Guid.Parse("f5d450b0-d126-4f62-885b-b3e28e638542"), CommandName = "Paste", Key = Key.V, Modifiers = KeyModifiers.Control } }
        };


        private void InitializeShortcuts()
        {
            DisableCommand(ApplicationCommands.Cut);
            DisableCommand(ApplicationCommands.Copy);
            DisableCommand(ApplicationCommands.Paste);

            _keybCommands = new Dictionary<Guid, Action>()
            {
                { keyboardCommands["Save"].CommandId, () => { RequestSaveDocument?.Invoke(this, EventArgs.Empty); } },
                { keyboardCommands["Copy"].CommandId, () => { ctrEditor.Copy(); } },
                { keyboardCommands["Cut"].CommandId, () => { ctrEditor.Cut();} },
                { keyboardCommands["Paste"].CommandId, () => { ctrEditor.Paste(); } }
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
