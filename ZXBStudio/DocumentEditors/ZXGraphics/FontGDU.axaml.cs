using Avalonia.Controls;
using System.Diagnostics;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.Linq;
using ZXBasicStudio.Common;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class FontGDU : UserControl
    {
        public event EventHandler? DocumentModified;
        public event EventHandler? DocumentSaved;

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

        private FileTypeConfig fileType = null;
        private byte[] fileData = null;
        private PatternControl[] patterns = null;
        private int lastZoom = 0;

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
            //btnMask.Tapped += BtnMask_Tapped;

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
                int x = 0;
                int y = 0;
                for (int n = 0; n < fileType.NumerOfPatterns; n++)
                {
                    var ctrl = new PatternControl();                    
                    cnvPatterns.Children.Add(ctrl);
                    Canvas.SetTop(ctrl, y);
                    Canvas.SetLeft(ctrl, x);
                    x = x + 36;
                    if (x > 139)
                    {
                        x = 0;
                        y = y + 56;
                    }
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

            cnvPatterns.Height = (fileType.NumerOfPatterns / 4) * 60;

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


        private void TxtEditorHeight_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            int h = txtEditorHeight.Text.ToInteger();
            ctrEditor.ItemsHeight = h;
        }


        private void TxtEditorWidth_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            int w = txtEditorWidth.Text.ToInteger();
            ctrEditor.ItemsWidth = w;
        }



        private Pattern GetPattern(int id)
        {
            var pat = patterns.FirstOrDefault(d => d.Pattern.Id == id);
            if (pat != null)
            {
                return pat.Pattern;
            }
            return null;
        }


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

        private void BtnClear_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Clear();
        }


        private void BtnCut_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Cut();
        }


        private void BtnCopy_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Copy();
        }


        private void BtnPaste_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Paste();
        }


        private void BtnHMirror_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.HorizontalMirror();
        }


        private void BtnVMirror_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.VerticalMirror();
        }


        private void BtnRotateLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {            
            this.ctrEditor.RotateLeft();
        }


        private void BtnRotateRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.RotateRight();
        }


        private void BtnShiftUp_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.ShiftUp();
        }


        private void BtnShiftRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.ShiftRight();
        }


        private void BtnShiftDown_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.ShiftDown();
        }


        private void BtnShiftLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.ShiftLeft();
        }


        private void BtnMoveUp_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.MoveUp();
        }


        private void BtnMoveRight_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.MoveRight();
        }


        private void BtnMoveDown_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.MoveDown();
        }


        private void BtnMoveLeft_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.MoveLeft();
        }


        private void BtnInvert_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Invert();
        }


        private void BtnMask_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.ctrEditor.Mask();
        }

        #endregion

    }
}
