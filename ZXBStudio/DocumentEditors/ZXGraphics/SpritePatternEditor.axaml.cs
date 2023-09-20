using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Linq;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class SpritePatternEditor : UserControl
    {
        #region Public properties

        /// <summary>
        /// Actual Sprite data
        /// </summary>
        public Sprite SpriteData
        {
            get
            {
                return _SpriteData;
            }
            set
            {
                _SpriteData = value;
                Refresh();
            }
        }


        /// <summary>
        /// Zoom
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


        /// <summary>
        /// Color index of the primary color
        /// </summary>
        public int PrimaryColorIndex { get; set; }

        /// <summary>
        /// Color index of the secondary color
        /// </summary>
        public int SecondaryColorIndex { get; set; }

        #endregion


        #region Private fields

        private Sprite _SpriteData = null;
        private int _Zoom = 24;
        private Action<SpritePatternEditor,string> CallBackCommand = null;

        /// <summary>
        /// True when mouse left button is pressed
        /// </summary>
        private bool MouseLeftPressed = false;
        /// <summary>
        /// True when moude right button is pressed
        /// </summary>
        private bool MouseRightPressed = false;

        /// <summary>
        /// Dispatcher for refresh operations
        /// </summary>
        private DispatcherTimer tmr = null;

        #endregion


        #region Public Methods

        public SpritePatternEditor()
        {
            InitializeComponent();

            PrimaryColorIndex = 1;
            SecondaryColorIndex = 0;

            cnvEditor.PointerMoved += CnvEditor_PointerMoved;
            cnvEditor.PointerPressed += CnvEditor_PointerPressed;
            cnvEditor.PointerReleased += CnvEditor_PointerReleased;
            cnvEditor.PointerExited += CnvEditor_PointerExited;
        }


        /// <summary>
        /// Initializes the control
        /// </summary>
        /// <param name="callBackCommand">CallBack for commands: "REFRESH"</param>
        /// <returns>True if OK or False if error</returns>
        public bool Initialize(Action<SpritePatternEditor,string> callBackCommand)
        {
            this.CallBackCommand = callBackCommand;
            return true;
        }


        /// <summary>
        /// Refresh the draw area
        /// </summary>
        public void Refresh()
        {
            cnvEditor.Children.Clear();
            if (SpriteData == null)
            {
                return;
            }

            for (int oy = 0; oy < SpriteData.Height; oy++)
            {
                for (int ox = 0; ox < SpriteData.Width; ox++)
                {
                    int colorIndex = 0;

                    var frame = SpriteData.Patterns[SpriteData.CurrentFrame];
                    var p = frame.Data.FirstOrDefault(d => d.X == ox && d.Y == oy);
                    if (p != null)
                    {
                        colorIndex = p.ColorIndex;
                    }

                    var r = new Rectangle();
                    r.Width = _Zoom + 1;
                    r.Height = _Zoom + 1;
                    r.Stroke = Brushes.White;
                    r.StrokeThickness = 1;

                    var palette = SpriteData.Palette[colorIndex];
                    r.Fill = new SolidColorBrush(new Color(255, palette.Red, palette.Green, palette.Blue));

                    cnvEditor.Children.Add(r);
                    Canvas.SetTop(r, oy * _Zoom);
                    Canvas.SetLeft(r, ox * _Zoom);
                }
            }

            for (int oy = 0; oy < SpriteData.Height; oy+=8)
            {
                for (int ox = 0; ox < SpriteData.Width; ox+=8)
                {
                    var r = new Rectangle();
                    int mx = (ox+8) > SpriteData.Width ? (SpriteData.Width %8) : 8;
                    int my = (oy+8) > SpriteData.Height ? (SpriteData.Height %8) : 8;
                    r.Width = (_Zoom * mx) + 1;
                    r.Height = (_Zoom * my) + 1;
                    r.Stroke = Brushes.Red;
                    r.StrokeThickness = 1;
                    r.Fill = Brushes.Transparent;
                    cnvEditor.Children.Add(r);
                    Canvas.SetTop(r, oy * _Zoom);
                    Canvas.SetLeft(r, ox * _Zoom);
                }
            }
            cnvEditor.Width = (_Zoom * SpriteData.Width);
            cnvEditor.Height = (_Zoom * SpriteData.Height);
        }

        #endregion


        #region Drawing events

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
                SetPoint(p.Position.X, p.Position.Y, PrimaryColorIndex);
                MouseLeftPressed = true;
                MouseRightPressed = false;
            }
            else if (p.Properties.IsRightButtonPressed)
            {
                SetPoint(p.Position.X, p.Position.Y, SecondaryColorIndex);
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
                SetPoint(p.Position.X, p.Position.Y, PrimaryColorIndex);
            }
            else if (MouseRightPressed)
            {
                SetPoint(p.Position.X, p.Position.Y, SecondaryColorIndex);
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

            x = x / _Zoom;
            y = y / _Zoom;

            if (x < 0 || y < 0 || x>=SpriteData.Width || y>=SpriteData.Height)
            {
                return;
            }

            var p = SpriteData.Patterns[SpriteData.CurrentFrame].Data.FirstOrDefault(d=>d.X==x && d.Y==y);
            if (p == null)
            {
                return;
            }

            p.ColorIndex = value;
            Refresh();

            if (tmr == null)
            {
                tmr = new DispatcherTimer();
                tmr.Tick += Tmr_Tick;
                tmr.Interval = TimeSpan.FromMilliseconds(250);
            }
            tmr.Stop();
            tmr.Start();            
        }

        private void Tmr_Tick(object? sender, EventArgs e)
        {
            CallBackCommand?.Invoke(this,"REFRESH");
            tmr.Stop();
        }

        #endregion
    }
}
