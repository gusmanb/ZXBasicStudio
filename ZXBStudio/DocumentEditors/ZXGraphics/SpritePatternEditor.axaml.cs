using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Schema;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using ZXBasicStudio.Extensions;

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
                if (_SpriteData == null)
                {
                    lastId = null;
                }
                else
                {
                    if (lastId != _SpriteData.Id)
                    {
                        _SpriteData.CurrentFrame = 0;
                        lastId = _SpriteData.Id;
                    }
                }
                Refresh(true);
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
                Refresh(true);
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

        public bool Bright { get; set; }
        public bool Flash { get; set; }

        #endregion


        #region Private fields

        private Sprite _SpriteData = null;
        private int _Zoom = 24;
        private Action<SpritePatternEditor, string> CallBackCommand = null;
        private int? lastId = null;

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
        public bool Initialize(Action<SpritePatternEditor, string> callBackCommand)
        {
            this.CallBackCommand = callBackCommand;
            return true;
        }


        /// <summary>
        /// Refresh the draw area
        /// </summary>
        public void Refresh(bool callBack = false)
        {
            cnvEditor.Children.Clear();
            if (SpriteData == null)
            {
                return;
            }

            cnvEditor.Children.Clear();
            for (int oy = 0; oy < SpriteData.Height; oy++)
            {
                for (int ox = 0; ox < SpriteData.Width; ox++)
                {

                    var frame = SpriteData.Patterns[SpriteData.CurrentFrame];
                    int colorIndex = frame.RawData[(SpriteData.Width * oy) + ox];

                    var r = new Rectangle();
                    r.Width = _Zoom + 1;
                    r.Height = _Zoom + 1;
                    r.Stroke = Brushes.White;
                    r.StrokeThickness = 1;

                    switch (SpriteData.GraphicMode)
                    {
                        case GraphicsModes.ZXSpectrum:
                            {
                                var attr = GetAttribute(frame, ox, oy);
                                PaletteColor palette = null;
                                if (colorIndex == 0)
                                {
                                    palette = SpriteData.Palette[attr.Paper];
                                }
                                else
                                {
                                    palette = SpriteData.Palette[attr.Ink];
                                }
                                r.Fill = new SolidColorBrush(new Color(255, palette.Red, palette.Green, palette.Blue));
                            }
                            break;
                        case GraphicsModes.Monochrome:
                        case GraphicsModes.Next:
                            {
                                var palette = SpriteData.Palette[colorIndex];
                                r.Fill = new SolidColorBrush(new Color(255, palette.Red, palette.Green, palette.Blue));
                            }
                            break;

                    }

                    cnvEditor.Children.Add(r);
                    Canvas.SetTop(r, oy * _Zoom);
                    Canvas.SetLeft(r, ox * _Zoom);
                }
            }

            for (int oy = 0; oy < SpriteData.Height; oy += 8)
            {
                for (int ox = 0; ox < SpriteData.Width; ox += 8)
                {
                    var r = new Rectangle();
                    int mx = (ox + 8) > SpriteData.Width ? (SpriteData.Width % 8) : 8;
                    int my = (oy + 8) > SpriteData.Height ? (SpriteData.Height % 8) : 8;
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

            if (callBack)
            {
                CallBackCommand?.Invoke(this, "REFRESH");
            }
        }

        #endregion


        #region Undo and Redo

        private List<Operation> operations = new List<Operation>();
        private List<Operation> operationsDeleted = new List<Operation>();

        public void Undo()
        {
            // Get last operation
            var op = operations.LastOrDefault();
            if (op != null)
            {
                // Move from operations to operationsDeleted
                operations.Remove(op);
                // Restore point value (Undo)
                SetPoint(op.X, op.Y, op.ColorIndex);
                // Remove new operation (Undo don't add operation)
                var op2 = operations.LastOrDefault();
                if (op2 != null)
                {
                    operations.Remove(op2);
                    operationsDeleted.Add(op2);
                }
            }
        }


        public void Redo()
        {
            // Get last undo operation
            var op = operationsDeleted.LastOrDefault();
            if (op != null)
            {
                // Delete from operationsDeleted
                operationsDeleted.Remove(op);
                // Set point value (Undo)
                SetPoint(op.X, op.Y, op.ColorIndex);
            }
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

            if (x < 0 || y < 0 || x >= SpriteData.Width || y >= SpriteData.Height)
            {
                return;
            }

            int dir = (SpriteData.Width * y) + x;
            operations.Add(new Operation()
            {
                ColorIndex = SpriteData.Patterns[SpriteData.CurrentFrame].RawData[dir],
                X = (int)mx,
                Y = (int)my
            });

            var sprite = SpriteData.Patterns[SpriteData.CurrentFrame];

            switch (SpriteData.GraphicMode)
            {
                case GraphicsModes.Monochrome:
                    sprite.RawData[dir] = value;
                    break;
                case GraphicsModes.ZXSpectrum:
                    {
                        if (value == PrimaryColorIndex)
                        {
                            sprite.RawData[dir] = 1;
                        }
                        else
                        {
                            sprite.RawData[dir] = 0;
                        }
                        SetAttribute(sprite, x, y);
                    }
                    break;
                case GraphicsModes.Next:
                    sprite.RawData[dir] = value;
                    break;
            }

            Refresh(false);

            if (tmr == null)
            {
                tmr = new DispatcherTimer();
                tmr.Tick += Tmr_Tick;
                tmr.Interval = TimeSpan.FromMilliseconds(250);
            }
            tmr.Stop();
            tmr.Start();
        }


        private void SetAttribute(Pattern pattern, int x, int y)
        {
            int cW = SpriteData.Width / 8;
            int cX = x / 8;
            int cY = y / 8;
            var attr = pattern.Attributes[(cY * cW) + cX];
            attr.Ink = PrimaryColorIndex;
            attr.Paper = SecondaryColorIndex;
        }


        private AttributeColor GetAttribute(Pattern pattern, int x, int y)
        {
            int cW = SpriteData.Width / 8;
            int cX = x / 8;
            int cY = y / 8;
            return pattern.Attributes[(cY * cW) + cX];
        }


        private void Tmr_Tick(object? sender, EventArgs e)
        {
            Refresh(true);
            tmr.Stop();
        }

        #endregion


        #region Icon actions


        /// <summary>
        /// Clear sprite
        /// </summary>
        public void Clear()
        {
            if (SpriteData == null || SpriteData.Patterns == null ||
                SpriteData.CurrentFrame >= (SpriteData.Patterns.Count) ||
                SpriteData.Patterns[SpriteData.CurrentFrame].RawData == null)
            {
                return;
            }
            for (int n = 0; n < SpriteData.Patterns[SpriteData.CurrentFrame].RawData.Length; n++)
            {
                SpriteData.Patterns[SpriteData.CurrentFrame].RawData[n] = SecondaryColorIndex;
            }
            Refresh(true);
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
            var patterns = new Pattern[1] { SpriteData.Patterns[SpriteData.CurrentFrame] };
            TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(patterns.Serializar()).Wait();
        }


        /// <summary>
        /// Paste patterns from clipboard
        /// </summary>
        public async void Paste()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var cbData = await TopLevel.GetTopLevel(this).Clipboard.GetTextAsync();
            if (string.IsNullOrEmpty(cbData))
            {
                return;
            }
            var cbPatterns = cbData.Deserializar<Pattern[]>();
            if (cbPatterns == null)
            {
                return;
            }

            if (cbPatterns.Length == 1)
            {
                SpriteData.Patterns[SpriteData.CurrentFrame] = cbPatterns[0];
            }
            else
            {
                // Create an empty pattern
                var pat2 = pattern.Clonar<Pattern>();
                pat2.RawData = new int[SpriteData.Width * SpriteData.Height];

                if (cbPatterns[0].RawData == null)
                {
                    // Paste from PointData
                    int ox = 0;
                    int oy = 0;
                    for (int n = 0; n < cbPatterns.Length; n++)
                    {
                        for (int py = 0; py < 8; py++)
                        {
                            for (int px = 0; px < 8; px++)
                            {
                                var po = cbPatterns[n].Data.FirstOrDefault(d => d.X == px && d.Y == py);
                                if (po != null)
                                {
                                    var pd = pat2.Data.FirstOrDefault(d => d.X == px + ox && d.Y == py + oy);
                                    if (pd != null)
                                    {
                                        pd.ColorIndex = po.ColorIndex;
                                    }
                                }
                            }
                        }
                        ox += 8;
                        if (ox >= SpriteData.Width)
                        {
                            ox = 0;
                            oy += 8;
                        }
                    }
                    SpriteData.Patterns[SpriteData.CurrentFrame].Data = pat2.Data;
                }
                else
                {
                    // Paste from RawData
                    var dat = SpriteData.Patterns[SpriteData.CurrentFrame].RawData;
                    var cbDat = cbPatterns[0].RawData;
                    for (int n = 0; n < cbDat.Length && n < dat.Length; n++)
                    {
                        dat[n] = cbDat[n];
                    }
                }
            }
            Refresh(true);
        }


        /// <summary>
        /// Horizontal mirror of selected patterns
        /// </summary>
        public void HorizontalMirror()
        {
            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            var pat1 = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pat2 = pat1.Clonar<Pattern>();
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pat1);
                    SetPointValue(maxWidth - x - 1, y, pd1, ref pat2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pat2;
            Refresh(true);
        }


        /// <summary>
        /// Vertical mirror of selected patterns
        /// </summary>
        public void VerticalMirror()
        {
            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            var pat1 = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pat2 = pat1.Clonar<Pattern>();
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pat1);
                    SetPointValue(x, maxHeight - y - 1, pd1, ref pat2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pat2;
            Refresh(true);
        }


        /// <summary>
        /// Rotate left of selected patterns
        /// </summary>
        public void RotateLeft()
        {
            if (SpriteData.Width != SpriteData.Height)
            {
                Window.GetTopLevel(this)?.ShowError("Can't do this!", "Only square graphics can be rotated (with the same width as height).");
                return;
            }

            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    SetPointValue(y, maxWidth - x - 1, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Rotate right of selected patterns
        /// </summary>
        public void RotateRight()
        {
            if (SpriteData.Width != SpriteData.Height)
            {
                Window.GetTopLevel(this)?.ShowError("Can't do this!", "Only square graphics can be rotated (with the same width as height).");
                return;
            }

            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    SetPointValue(maxHeight - y - 1, x, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Move up and put the disappearing pixels at the bottom.
        /// </summary>
        public void ShiftUp()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    int y2 = y - 1;
                    if (y2 < 0)
                    {
                        y2 = maxHeight - 1;
                    }
                    SetPointValue(x, y2, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Move right and put the disappearing pixels at the left.
        /// </summary>
        public void ShiftRight()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    int x2 = x + 1;
                    if (x2 >= maxWidth)
                    {
                        x2 = 0;
                    }
                    SetPointValue(x2, y, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Move down and put the disappearing pixels at the top.
        /// </summary>
        public void ShiftDown()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    int y2 = y + 1;
                    if (y2 >= maxHeight)
                    {
                        y2 = 0;
                    }
                    SetPointValue(x, y2, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Move left and put the disappearing pixels at the right.
        /// </summary>
        public void ShiftLeft()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    int x2 = x - 1;
                    if (x2 < 0)
                    {
                        x2 = maxWidth - 1;
                    }
                    SetPointValue(x2, y, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Move up
        /// </summary>
        public void MoveUp()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    int y2 = y - 1;
                    if (y2 < 0)
                    {
                        y2 = maxHeight - 1;
                        pd1 = 0;
                    }
                    SetPointValue(x, y2, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Move rigth
        /// </summary>
        public void MoveRight()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    int x2 = x + 1;
                    if (x2 >= maxWidth)
                    {
                        x2 = 0;
                        pd1 = 0;
                    }
                    SetPointValue(x2, y, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Move down
        /// </summary>
        public void MoveDown()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    int y2 = y + 1;
                    if (y2 >= maxHeight)
                    {
                        y2 = 0;
                        pd1 = 0;
                    }
                    SetPointValue(x, y2, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Move left
        /// </summary>
        public void MoveLeft()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    int x2 = x - 1;
                    if (x2 < 0)
                    {
                        x2 = maxWidth - 1;
                        pd1 = 0;
                    }
                    SetPointValue(x2, y, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Invert pixels
        /// </summary>
        public void Invert()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            var pattern2 = pattern.Clonar<Pattern>();

            int maxWidth = SpriteData.Width;
            int maxHeight = SpriteData.Height;
            for (int y = 0; y < maxHeight; y++)
            {
                for (int x = 0; x < maxWidth; x++)
                {
                    var pd1 = GetPointValue(x, y, pattern);
                    if (pd1 == PrimaryColorIndex)
                    {
                        pd1 = SecondaryColorIndex;
                    }
                    else if (pd1 == SecondaryColorIndex)
                    {
                        pd1 = PrimaryColorIndex;
                    }
                    SetPointValue(x, y, pd1, ref pattern2);
                }
            }
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Create a mask
        /// </summary>
        public void Mask()
        {
            var pattern = SpriteData.Patterns[SpriteData.CurrentFrame];
            int maxX = (SpriteData.Width) - 1;
            int maxY = (SpriteData.Height) - 1;

            var pattern2 = new Pattern()
            {
                Id = pattern.Id,
                Name = pattern.Name,
                Number = pattern.Number,
                RawData = new int[SpriteData.Width * SpriteData.Height]
            };

            _Mask(0, 0, ref pattern, ref pattern2);
            _Mask(maxX, 0, ref pattern, ref pattern2);
            _Mask(0, maxY, ref pattern, ref pattern2);
            _Mask(maxX, maxY, ref pattern, ref pattern2);
            SpriteData.Patterns[SpriteData.CurrentFrame] = pattern2;
            Refresh(true);
        }


        /// <summary>
        /// Create a mask (fills pattern2 from pattern1 data) called recursively
        /// </summary>
        /// <param name="x">Coord x</param>
        /// <param name="y">Coord Y</param>
        /// <param name="pattern1">Original patterns</param>
        /// <param name="pattern2">Void pattens</param>
        private void _Mask(int x, int y, ref Pattern pattern1, ref Pattern pattern2)
        {
            var p1 = GetPointValue(x, y, pattern1);
            if (p1 != 0)
            {
                return;
            }
            var p2 = GetPointValue(x, y, pattern2);
            if (p2 != 0)
            {
                return;
            }
            p2 = 1;
            SetPointValue(x, y, p2, ref pattern2);

            if (x > 1)
            {
                _Mask(x - 1, y, ref pattern1, ref pattern2);
            }
            if (x < (SpriteData.Width) - 1)
            {
                _Mask(x + 1, y, ref pattern1, ref pattern2);
            }
            if (y > 1)
            {
                _Mask(x, y - 1, ref pattern1, ref pattern2);
            }
            if (y < (SpriteData.Height) - 1)
            {
                _Mask(x, y + 1, ref pattern1, ref pattern2);
            }
        }


        /// <summary>
        /// Get a point (pixel) from the selected pattern
        /// </summary>
        /// <param name="x">X coord</param>
        /// <param name="y">Y coord</param>
        /// <param name="pattern">Pattern to use</param>
        /// <returns>Point data or null if no data</returns>
        private int GetPointValue(int x, int y, Pattern pattern)
        {
            if (x < 0 || y < 0 || x > (SpriteData.Width - 1) || y > (SpriteData.Height - 1))
            {
                return SecondaryColorIndex;
            }
            return pattern.RawData[(SpriteData.Width * y) + x];
        }


        /// <summary>
        /// Set a point (pixel) from the selected pattern
        /// </summary>
        /// <param name="x">X coord</param>
        /// <param name="y">Y coord</param>
        /// <param name="colorIndex">Color index to set</param>
        /// <param name="pattern">Pattern to use</param>
        private void SetPointValue(int x, int y, int colorIndex, ref Pattern pattern)
        {
            if (x < 0 || y < 0 || x > (SpriteData.Width - 1) || y > (SpriteData.Height - 1))
            {
                return;
            }
            pattern.RawData[(SpriteData.Width * y) + x] = colorIndex;
        }

        #endregion
    }
}
