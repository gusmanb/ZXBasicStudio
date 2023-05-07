using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ZXBasicStudio.Common;
using Avalonia.Input;
using System.Text;
using System.Security.Cryptography;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    /// <summary>
    /// Main editor control for ZXGraphics
    /// </summary>
    public partial class EditorControl : UserControl
    {
        /// <summary>
        /// Selected pattern
        /// </summary>
        public int IdPattern
        {
            get
            {
                return _IdPattern;
            }
            set
            {
                _IdPattern = value;
                Refresh();
            }
        }
        private int _IdPattern = 0;

        /// <summary>
        /// Number of horizontal patterns
        /// </summary>
        public int ItemsWidth
        {
            get
            {
                return _ItemsWidth;
            }
            set
            {
                _ItemsWidth = value;
                Refresh();
            }
        }
        private int _ItemsWidth = 1;

        /// <summary>
        /// Number of vertical patterns
        /// </summary>
        public int ItemsHeight
        {
            get
            {
                return _ItemsHeight;
            }
            set
            {
                _ItemsHeight = value;
                Refresh();
            }
        }
        private int _ItemsHeight = 1;

        /// <summary>
        /// Zoom level of the editor
        /// </summary>
        public int Zoom
        {
            get
            {
                return _Zoom;
            }
            set
            {
                _Zoom = value;
                Refresh();
            }
        }
        private int _Zoom = 24;

        /// <summary>
        /// Delegate to set a pattern
        /// </summary>
        private Action<int, Pattern> callBackSetPattern = null;
        /// <summary>
        /// Delegate to get a pattern
        /// </summary>
        private Func<int, Pattern> callbackGetPattern = null;

        /// <summary>
        /// True when mouse left button is pressed
        /// </summary>
        private bool MouseLeftPressed = false;
        /// <summary>
        /// True when moude right button is pressed
        /// </summary>
        private bool MouseRightPressed = false;


        /// <summary>
        /// Constructor
        /// </summary>
        public EditorControl()
        {
            InitializeComponent();

            cnvEditor.PointerMoved += CnvEditor_PointerMoved;
            cnvEditor.PointerPressed += CnvEditor_PointerPressed;
            cnvEditor.PointerReleased += CnvEditor_PointerReleased;
            cnvEditor.PointerExited += CnvEditor_PointerExited;
        }


        /// <summary>
        /// Initialize the control
        /// </summary>
        /// <param name="idPattern">Id of the initial selected patteern</param>
        /// <param name="callbackGetPattern">Delegate for get pattern data</param>
        /// <param name="callBackSetPattern">Delegate to set pattern data</param>
        /// <returns>True if ok or false if error</returns>
        public bool Initialize(int idPattern, Func<int, Pattern> callbackGetPattern, Action<int, Pattern> callBackSetPattern)
        {
            this.callbackGetPattern = callbackGetPattern;
            this.callBackSetPattern = callBackSetPattern;
            this.IdPattern = idPattern;
            return true;
        }


        /// <summary>
        /// Refresh the editor UI
        /// </summary>
        public void Refresh()
        {
            cnvEditor.Children.Clear();
            int actual = _IdPattern;
            int posX = 0;
            int posY = 0;

            for (int oy = 0; oy < ItemsHeight; oy++)
            {
                for (int ox = 0; ox < ItemsWidth; ox++)
                {
                    posX = (ox * 8);
                    posY = (oy * 8);
                    var pattern = callbackGetPattern(actual);
                    if (pattern != null)
                    {
                        foreach (var p in pattern.Data)
                        {
                            var r = new Rectangle();
                            r.Width = _Zoom + 1;
                            r.Height = _Zoom + 1;
                            r.Stroke = Brushes.White;
                            r.StrokeThickness = 1;
                            if (p.ColorIndex == 0)
                            {
                                r.Fill = Brushes.LightGray;
                            }
                            else
                            {
                                r.Fill = Brushes.Black;
                            }
                            cnvEditor.Children.Add(r);
                            Canvas.SetTop(r, (posY + p.Y) * _Zoom);
                            Canvas.SetLeft(r, (posX + p.X) * _Zoom);
                        }
                    }
                    {
                        var r = new Rectangle();
                        r.Width = (_Zoom * 8) + 1;
                        r.Height = (_Zoom * 8) + 1;
                        r.Stroke = Brushes.Red;
                        r.StrokeThickness = 1;
                        r.Fill = Brushes.Transparent;
                        cnvEditor.Children.Add(r);
                        Canvas.SetTop(r, posY * _Zoom);
                        Canvas.SetLeft(r, posX * _Zoom);
                    }
                    actual++;
                }
            }
            cnvEditor.Width = (_Zoom * ItemsWidth) * 8;
            cnvEditor.Height = (_Zoom * ItemsHeight) * 8;
        }


        /// <summary>
        /// Event for mouse pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CnvEditor_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var p = e.GetCurrentPoint(cnvEditor);
            if (p.Properties.IsLeftButtonPressed)
            {
                SetPoint(p.Position.X, p.Position.Y, 1);
                MouseLeftPressed = true;
                MouseRightPressed = false;
            }
            else if (p.Properties.IsRightButtonPressed)
            {
                SetPoint(p.Position.X, p.Position.Y, 0);
                MouseLeftPressed = false;
                MouseRightPressed = true;
            }
        }


        /// <summary>
        /// Event for mouse released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CnvEditor_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            MouseLeftPressed = false;
            MouseRightPressed = false;
        }


        /// <summary>
        /// Event for pointer moved outside control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CnvEditor_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            MouseLeftPressed = false;
            MouseRightPressed = false;
        }


        /// <summary>
        /// Event for pointer moved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CnvEditor_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            var p = e.GetCurrentPoint(cnvEditor);
            if (MouseLeftPressed)
            {
                SetPoint(p.Position.X, p.Position.Y, 1);
            }
            else if (MouseRightPressed)
            {
                SetPoint(p.Position.X, p.Position.Y, 0);
            }
        }


        /// <summary>
        /// Set point to a color value
        /// </summary>
        /// <param name="mx">Absolute x</param>
        /// <param name="my">Absolute y</param>
        /// <param name="value">Value of the point</param>
        private void SetPoint(double mx, double my, int value)
        {
            int x = (int)mx;
            int y = (int)my;

            if (x < 0 || y < 0)
            {
                return;
            }

            x = x / _Zoom;
            y = y / _Zoom;

            int px = x / 8;
            int py = y / 8;
            x = x % 8;
            y = y % 8;

            if (py >= ItemsHeight || px >= ItemsWidth)
            {
                return;
            }

            var id = IdPattern + (py * ItemsWidth) + px;
            var pattern = callbackGetPattern(id);
            if (pattern != null)
            {
                var pointData = pattern.Data.FirstOrDefault(d => d.X == x && d.Y == y);
                if (pointData == null)
                {
                    pointData = new PointData()
                    {
                        ColorIndex = value,
                        Y = y,
                        X = x
                    };
                    var lst = pattern.Data.ToList();
                    lst.Add(pointData);
                    pattern.Data = lst.ToArray();
                    callBackSetPattern(id, pattern);
                    Refresh();
                }
                else
                {
                    if (pointData.ColorIndex != value)
                    {
                        pointData.ColorIndex = value;
                        callBackSetPattern(id, pattern);
                        Refresh();
                    }
                }
            }
        }


        #region Toolbar

        /// <summary>
        /// Clear active patterns
        /// </summary>
        public void Clear()
        {
            var patterns = GetSelectedPatterns();
            var pts = new List<PointData>();
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var p = new PointData()
                    {
                        ColorIndex = 0,
                        X = x,
                        Y = y
                    };
                    pts.Add(p);
                }
            }
            foreach (var pattern in patterns)
            {
                pattern.Data = pts.Clonar<PointData[]>();
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Cut patterns to clipboard
        /// </summary>
        public void Cut()
        {
            Copy();
            Clear();
        }


        /// <summary>
        /// Copy patterns to clipboard
        /// </summary>
        public void Copy()
        {
            var patterns = GetSelectedPatterns();
            Application.Current.Clipboard.SetTextAsync(patterns.Serializar()).Wait();
        }


        /// <summary>
        /// Paste patterns from clipboard
        /// </summary>
        public async void Paste()
        {
            try
            {
                var patterns = GetSelectedPatterns();
                var cbData = await Application.Current.Clipboard.GetTextAsync();
                if (string.IsNullOrEmpty(cbData))
                {
                    return;
                }
                var cbPatterns = cbData.Deserializar<Pattern[]>();
                if (cbPatterns == null)
                {
                    return;
                }
                int idx = 0;
                foreach (var pattern in patterns)
                {
                    if (idx < cbPatterns.Length)
                    {
                        pattern.Data = cbPatterns[idx].Data;
                        idx++;
                    }
                    else
                    {
                        break;
                    }
                    callBackSetPattern(pattern.Id, pattern);
                }
                Refresh();
            }
            catch { }
        }


        /// <summary>
        /// Horizontal mirror of selected patterns
        /// </summary>
        public void HorizontalMirror()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    SetPointValue(maxWidth - x - 1, y, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Vertical mirror of selected patterns
        /// </summary>
        public void VerticalMirror()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    SetPointValue(x, maxHeight - y - 1, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Rotate left of selected patterns
        /// </summary>
        public void RotateLeft()
        {
            if (ItemsWidth != ItemsHeight)
            {
                Window.GetTopLevel(this)?.ShowError("Can't do this!", "Only square graphics can be rotated, i.e. with the same width as height.");
                return;
            }

            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    SetPointValue(y, maxWidth - x - 1, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Rotate right of selected patterns
        /// </summary>
        public void RotateRight()
        {
            if (ItemsWidth != ItemsHeight)
            {
                Window.GetTopLevel(this)?.ShowError("Can't do this!", "Only square graphics can be rotated, i.e. with the same width as height.");
                return;
            }

            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    SetPointValue(maxHeight - y - 1, x, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Move up and put the disappearing pixels at the bottom.
        /// </summary>
        public void ShiftUp()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    int y2 = y - 1;
                    if (y2 < 0)
                    {
                        y2 = maxHeight - 1;
                    }
                    SetPointValue(x, y2, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Move right and put the disappearing pixels at the left.
        /// </summary>
        public void ShiftRight()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    int x2 = x + 1;
                    if (x2 >= maxWidth)
                    {
                        x2 = 0;
                    }
                    SetPointValue(x2, y, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Move down and put the disappearing pixels at the top.
        /// </summary>
        public void ShiftDown()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    int y2 = y + 1;
                    if (y2 >=maxHeight)
                    {
                        y2 = 0;
                    }
                    SetPointValue(x, y2, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Move left and put the disappearing pixels at the right.
        /// </summary>
        public void ShiftLeft()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    int x2 = x - 1;
                    if (x2 < 0)
                    {
                        x2 = maxWidth - 1;
                    }
                    SetPointValue(x2, y, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Move up
        /// </summary>
        public void MoveUp()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    int y2 = y - 1;
                    if (y2 < 0)
                    {
                        y2 = maxHeight - 1;
                        pd1.ColorIndex = 0;
                    }
                    SetPointValue(x, y2, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Move rigth
        /// </summary>
        public void MoveRight()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    int x2 = x + 1;
                    if (x2 >= maxWidth)
                    {
                        x2 = 0;
                        pd1.ColorIndex = 0;
                    }
                    SetPointValue(x2, y, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Move down
        /// </summary>
        public void MoveDown()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    int y2 = y + 1;
                    if (y2 >= maxHeight)
                    {
                        y2 = 0;
                        pd1.ColorIndex = 0;
                    }
                    SetPointValue(x, y2, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Move left
        /// </summary>
        public void MoveLeft()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    int x2 = x - 1;
                    if (x2 < 0)
                    {
                        x2 = maxWidth - 1;
                        pd1.ColorIndex = 0;
                    }
                    SetPointValue(x2, y, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Invert pixels
        /// </summary>
        public void Invert()
        {
            var patterns = GetSelectedPatterns();
            var patterns2 = patterns.Clonar<Pattern[]>();

            int maxWidth = ItemsWidth * 8;
            int maxHeight = ItemsHeight * 8;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, patterns);
                    if (pd1.ColorIndex == 0)
                    {
                        pd1.ColorIndex = 1;
                    }
                    else
                    {
                        pd1.ColorIndex = 0;
                    }
                    SetPointValue(x, y, pd1, ref patterns2);
                }
            }
            patterns = patterns2;
            foreach (var pattern in patterns)
            {
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        /// <summary>
        /// Create a mask
        /// </summary>
        public void Mask()
        {
            var patterns = GetSelectedPatterns();
            int maxX = (ItemsWidth * 8) - 1;
            int maxY = (ItemsHeight * 8) - 1;
            
            var patterns2 = new Pattern[ItemsWidth*ItemsHeight];
            for(int n=0; n<patterns2.Length; n++)
            {
                var p = new Pattern()
                {
                    Id = patterns[n].Id,
                    Name = patterns[n].Name,
                    Number = patterns[n].Number
                };
                var pdLst = new List<PointData>();
                for(int y = 0; y<8 ; y++)
                {
                    for(int x=0; x<8; x++)
                    {
                        var pd = new PointData()
                        {
                            ColorIndex = 0,
                            X = x,
                            Y = y
                        };
                        pdLst.Add(pd);
                    }
                }
                p.Data= pdLst.ToArray();
                patterns2[n] = p;
            }

            _Mask(0, 0, ref patterns, ref patterns2);
            _Mask(maxX, 0, ref patterns, ref patterns2);
            _Mask(0, maxY, ref patterns, ref patterns2);
            _Mask(maxX, maxY, ref patterns, ref patterns2);
            
            foreach(var p in patterns2)
            {
                callBackSetPattern(p.Id, p);
            }
            Refresh();
        }


        /// <summary>
        /// Create a mask (fills pattern2 from pattern1 data) called recursively
        /// </summary>
        /// <param name="x">Coord x</param>
        /// <param name="y">Coord Y</param>
        /// <param name="pattern1">Original patterns</param>
        /// <param name="pattern2">Void pattens</param>
        private void _Mask(int x, int y, ref Pattern[] pattern1, ref Pattern[] pattern2)
        {
            var p1=GetPointValue(x,y, pattern1);
            if(p1==null || p1.ColorIndex != 0)
            {
                return;
            }
            var p2 = GetPointValue(x, y, pattern2);
            if(p2==null || p2.ColorIndex != 0)
            {
                return;
            }
            p2.ColorIndex = 1;
            SetPointValue(x, y, p2, ref pattern2);

            if (x > 1)
            {
                _Mask(x-1,y,ref pattern1, ref pattern2);
            }
            if (x < (ItemsWidth*8)-1)
            {
                _Mask(x + 1, y, ref pattern1, ref pattern2);
            }
            if (y > 1)
            {
                _Mask(x, y-1, ref pattern1, ref pattern2);
            }
            if (y < (ItemsHeight * 8)-1)
            {
                _Mask(x, y+1, ref pattern1, ref pattern2);
            }
        }



        /// <summary>
        /// Returns the selected pattern
        /// </summary>
        /// <returns></returns>
        private Pattern[] GetSelectedPatterns()
        {
            var pts = new List<Pattern>();
            var ini = IdPattern;
            var fin = IdPattern + ItemsWidth * ItemsHeight;
            for (var n = IdPattern; n < fin; n++)
            {
                var p = callbackGetPattern(n);
                if (p == null)
                {
                    p = new Pattern()
                    {
                        Id = n,
                        Data = new PointData[0],
                        Name = "",
                        Number = n.ToString()
                    };
                }
                pts.Add(p);
            }
            return pts.ToArray();
        }


        /// <summary>
        /// Get a point (pixel) from the selected patterns
        /// </summary>
        /// <param name="x">X coord</param>
        /// <param name="y">Y coord</param>
        /// <param name="patterns">Selected patterns</param>
        /// <returns>Point data or null if no data</returns>
        private PointData GetPointValue(int x, int y, Pattern[] patterns)
        {
            var maxX = ItemsWidth * 8;
            var maxY = ItemsHeight * 8;

            int pat = ((y / 8) * ItemsWidth) + (x / 8);
            int py = y % 8;
            int px = x % 8;

            var pd = patterns[pat].Data;
            var p = pd.FirstOrDefault(d => d.X == px && d.Y == py);
            return p;
        }


        /// <summary>
        /// Set a point (pixel) from the selected patterns
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pointData"></param>
        /// <param name="patterns"></param>
        private void SetPointValue(int x, int y, PointData pointData, ref Pattern[] patterns)
        {
            var maxX = ItemsWidth * 8;
            var maxY = ItemsHeight * 8;

            int pat = ((y / 8) * ItemsWidth) + (x / 8);
            int py = y % 8;
            int px = x % 8;

            var pd = patterns[pat].Data;
            var p = pd.FirstOrDefault(d => d.X == px && d.Y == py);
            if (p != null)
            {
                p.ColorIndex = pointData.ColorIndex;
            }
        }

        #endregion
    }
}
