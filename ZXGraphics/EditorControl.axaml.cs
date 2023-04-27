using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.ComponentModel;
using ZXGraphics.neg;

namespace ZXGraphics
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


        public bool Initialize(int idPattern, Func<int, Pattern> callbackGetPattern, Action<int,Pattern> callBackSetPattern)
        {
            this.callbackGetPattern = callbackGetPattern;
            this.callBackSetPattern= callBackSetPattern;
            this.IdPattern = idPattern;
            return true;
        }


        public void Refresh()
        {
            cnvEditor.Children.Clear();
            int actual = _IdPattern;
            int posX = 0;
            int posY = 0;

            for(int oy=0; oy<ItemsHeight; oy++)
            {
                for(int ox=0; ox<ItemsWidth; ox++)
                {
                    posX = (ox * 8);
                    posY = (oy * 8);
                    var pattern = callbackGetPattern(actual);
                    if (pattern != null)
                    {
                        foreach(var p in pattern.Data)
                        {
                            var r = new Rectangle();
                            r.Width = _Zoom+1;
                            r.Height = _Zoom+1;                            
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
                            Canvas.SetTop(r, (posY+p.Y) * _Zoom);
                            Canvas.SetLeft(r, (posX+p.X) * _Zoom);
                        }
                    }
                    {
                        var r = new Rectangle();
                        r.Width = (_Zoom*8)+1;
                        r.Height = (_Zoom*8)+1;
                        r.Stroke = Brushes.Red;
                        r.StrokeThickness = 1;
                        r.Fill = Brushes.Transparent;
                        cnvEditor.Children.Add(r);
                        Canvas.SetTop(r, posY*_Zoom);
                        Canvas.SetLeft(r, posX*_Zoom);
                    }
                    actual++;
                }
            }
            cnvEditor.Width = (_Zoom * ItemsWidth) * 8;
            cnvEditor.Height = (_Zoom * ItemsHeight) * 8;
        }


        private void CnvEditor_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var p=e.GetCurrentPoint(cnvEditor);
            if (p.Properties.IsLeftButtonPressed)
            {
                SetPoint(p.Position.X, p.Position.Y, 1);
                MouseLeftPressed = true;
                MouseRightPressed = false;
            }
            else if(p.Properties.IsRightButtonPressed)
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
            else if(MouseRightPressed)
            {
                SetPoint(p.Position.X, p.Position.Y, 0);
            }
        }

        private void SetPoint(double mx, double my, int value)
        {
            int x = (int)mx;
            int y = (int)my;

            x=x/_Zoom; 
            y=y/_Zoom;

            int px = x / 8;
            int py = y / 8;
            x = x % 8;
            y = y % 8;

            var id = IdPattern+(py * ItemsWidth) + px;
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
    }
}
