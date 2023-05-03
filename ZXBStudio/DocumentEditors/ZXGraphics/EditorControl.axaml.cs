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
    public partial class EditorControl : UserControl
    {
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


        private Action<int, Pattern> callBackSetPattern = null;
        private Func<int, Pattern> callbackGetPattern = null;

        private bool MouseLeftPressed = false;
        private bool MouseRightPressed = false;


        public EditorControl()
        {
            InitializeComponent();

            cnvEditor.PointerMoved += CnvEditor_PointerMoved;
            cnvEditor.PointerPressed += CnvEditor_PointerPressed;
            cnvEditor.PointerReleased += CnvEditor_PointerReleased;
            cnvEditor.PointerExited += CnvEditor_PointerExited;
        }


        public bool Initialize(int idPattern, Func<int, Pattern> callbackGetPattern, Action<int, Pattern> callBackSetPattern)
        {
            this.callbackGetPattern = callbackGetPattern;
            this.callBackSetPattern = callBackSetPattern;
            this.IdPattern = idPattern;
            return true;
        }


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


        private void CnvEditor_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            MouseLeftPressed = false;
            MouseRightPressed = false;
        }


        private void CnvEditor_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            MouseLeftPressed = false;
            MouseRightPressed = false;
        }


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
                pattern.Data = pts.ToArray();
                callBackSetPattern(pattern.Id, pattern);
            }
            Refresh();
        }


        public void Cut()
        {
            Copy();
            Clear();
        }


        public void Copy()
        {
            var patterns = GetSelectedPatterns();
            Application.Current.Clipboard.SetTextAsync(patterns.Serializar()).Wait();
        }


        public async void Paste()
        {
            var patterns = GetSelectedPatterns();
            var cbData = await Application.Current.Clipboard.GetTextAsync();
            if (string.IsNullOrEmpty(cbData))
            {
                return;
            }
            var cbPatterns = cbData.Deserializar<Pattern[]>();
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


        public void Mask()
        {
            // TODO: Not implemented yet
        }




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
