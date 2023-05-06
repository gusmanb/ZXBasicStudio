using Avalonia.Controls;
using System.Diagnostics;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.Linq;
using ZXBasicStudio.Common;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    /// <summary>
    /// Editor for GDUs and Fonts
    /// </summary>
    public partial class FontGDU : UserControl
    {
        /// <summary>
        /// Dpcument modified event
        /// </summary>
        public event EventHandler? DocumentModified;

        /// <summary>
        /// Document saved event
        /// </summary>
        public event EventHandler? DocumentSaved;

        /// <summary>
        /// True if the document was modified
        /// </summary>
        public bool Modified {
            get
            {
                return _Modified;
            }
            set
            {
                if(_Modified==false && value == true)
                {
                    DocumentModified?.Invoke(this, EventArgs.Empty);
                }
                _Modified = value;               
            }
        }
        private bool _Modified;

        /// <summary>
        /// Filename of the actual document
        /// </summary>
        public string FileName 
        {
            get
            {
                return fileType.FileName;
            }
            set
            {
                fileType.FileName = value;
            } 
        }

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


        /// <summary>
        /// Constructor, without parameters is mandatory to cal Initialize
        /// </summary>
        public FontGDU()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        public FontGDU(string fileName)
        {
            InitializeComponent();
            Initialize(fileName);
        }


        /// <summary>
        /// Initialize the system
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if OK, or False if error</returns>
        public bool Initialize(string fileName)
        {
            Modified = false;
            
            ServiceLayer.Initialize();

            fileType = ServiceLayer.GetFileType(fileName);
            fileData = ServiceLayer.GetFileData(fileName);

            if(fileData==null || fileData.Length == 0)
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

            return true;
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
                    case FileTypes.GDU:
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
                patterns[n].Initialize(p,Pattern_Click);
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
            foreach(var pattern in patterns)
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
            if (z==0 ||z == lastZoom)
            {
                return;
            }
            lastZoom = z;

            z = zooms[z-1];
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
        private void SetPattern(int id,Pattern pattern)
        {
            Modified = true;
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
            if(!ServiceLayer.Files_Save_GDUorFont(fileType, patterns?.Select(d=>d.Pattern)))
            {
                return false;
            };
            
            Modified = false;
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

        #endregion

    }
}
