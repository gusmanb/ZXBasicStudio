using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ZXBasicStudio.DocumentEditors.NextDows.neg;

namespace ZXBasicStudio.DocumentEditors.NextDows
{
    public partial class FormEditor : UserControl
    {
        public List<ControlItem> Controls
        {
            get
            {
                return _controls;
            }
            set
            {
                _controls = value;
                Refresh();
            }
        }

        private List<ControlItem> _controls = null;


        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }


        public ControlsTypes ControlType { get; set; }

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
        /// Defines is grid is visible
        /// </summary>
        public bool IsGridOn { get; set; }


        private int _Zoom = 4;
        private PaletteColor[] Palette = null;
        private Action<string, ControlItem> callBackCommand = null;

        public FormEditor()
        {
            InitializeComponent();
        }


        public bool Initialize(Action<string, ControlItem> callBackCommand)
        {
            Palette = log.ServiceLayer.CrateNextDefaultPalette();
            this.callBackCommand = callBackCommand;

            cnvEditor.PointerEntered += CnvEditor_PointerEntered;
            cnvEditor.PointerExited += CnvEditor_PointerExited;
            cnvEditor.Tapped += CnvEditor_Tapped;

            Controls = new List<ControlItem>();
            Zoom = 4;
            WindowWidth = 300;
            WindowHeight = 226;
            Refresh();

            return true;
        }


        private void CnvEditor_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            cnvEditor.Cursor = new Cursor(StandardCursorType.Arrow);
        }


        private void CnvEditor_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (ControlType != ControlsTypes.None)
            {
                cnvEditor.Cursor = new Cursor(StandardCursorType.Cross);
            }
        }


        private void CnvEditor_Tapped(object? sender, TappedEventArgs e)
        {
            var rel = e.GetPosition(cnvEditor);
            AddControl(ControlType, (int)(rel.X / _Zoom), (int)(rel.Y / _Zoom));
        }


        private void AddControl(ControlsTypes controlType, int x, int y)
        {
            var container = SearchContainer(x, y);
            if (container == null)
            {
                return;
            }

            var c = new ControlItem();
            c.ContainerId = container.Id;
            c.ControlType = controlType;
            c.Height = 8;
            c.Id = Controls.Max(d => d.Id) + 1;
            c.Ink = 0;
            c.Left = x;
            c.Name = controlType.ToString() + (Controls.Where(d => d.ControlType == controlType).Count() + 1).ToString();
            c.Paper = 255;
            c.Properties = new List<ControlProperty>();
            c.Top = y;
            c.Visible = true;
            c.Width = 32;

            Controls.Add(c);
            Refresh();

            callBackCommand("UPDATE", c);
        }


        private ControlItem SearchContainer(int x, int y)
        {
            var container = (from d in Controls
                             where d.ControlType == ControlsTypes.Panel &&
                                y >= d.Top && x >= d.Left &&
                                y <= (d.Height + d.Top) && x <= (d.Width + d.Left)
                             orderby d.Id descending
                             select d).FirstOrDefault();
            return container;
        }


        /// <summary>
        /// Refresh the editor UI
        /// </summary>
        public void Refresh()
        {
            var viewport = Controls.FirstOrDefault(d => d.Id == 0);
            if (viewport == null)
            {
                viewport = new ControlItem()
                {
                    ContainerId = 0,
                    ControlType = ControlsTypes.Panel,
                    Height = 0,
                    Id = 0,
                    Ink = 0,
                    Left = 0,
                    Name = "Main panel",
                    Paper = 255,
                    Top = 0,
                    Visible = true,
                    Width = 0,
                };
                viewport.Properties = new List<ControlProperty>();
                Controls.Add(viewport);
            }
            if (viewport.Width == 0 && WindowWidth != 0)
            {
                viewport.Width = WindowWidth;
                viewport.Height = WindowHeight;
                callBackCommand?.Invoke("UPDATE", viewport);
            }

            cnvEditor.Children.Clear();
            cnvEditor.Width = WindowWidth * Zoom;
            cnvEditor.Height = WindowHeight * Zoom;

            Draw_Panel(viewport);
            Draw_Controls(viewport);
            Draw_Grid();
        }


        private void Draw_Grid()
        {
            if (_Zoom > 1)
            {
                var brush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
                var z = _Zoom * 8;
                for (int y = z; y < cnvEditor.Height; y += z)
                {
                    {
                        var l = new Line();
                        l.StartPoint = new Avalonia.Point(0, y);
                        l.EndPoint = new Avalonia.Point(cnvEditor.Width, y);
                        l.Stroke = brush;
                        l.StrokeThickness = 1;
                        l.StrokeDashArray = new AvaloniaList<double>(new double[] { 2, 2 });
                        cnvEditor.Children.Add(l);
                    }
                }
                for (int x = z; x < cnvEditor.Width; x += z)
                {
                    {
                        var l = new Line();
                        l.StartPoint = new Avalonia.Point(x, 0);
                        l.EndPoint = new Avalonia.Point(x, cnvEditor.Height);
                        l.Stroke = brush;
                        l.StrokeThickness = 1;
                        l.StrokeDashArray = new AvaloniaList<double>(new double[] { 2, 2 });
                        cnvEditor.Children.Add(l);
                    }
                }
            }
        }


        private void Draw_Controls(ControlItem viewport)
        {
            var controls = from d in Controls
                           where d.Id != 0 &&
                             d.ContainerId == viewport.Id
                           orderby d.Id
                           select d;

            foreach (var control in controls)
            {
                switch (control.ControlType)
                {
                    case ControlsTypes.Panel:
                        Draw_Panel(control);
                        if (control.Id != 0)
                        {
                            Draw_Controls(control);
                        }
                        break;
                    default:
                        Draw_NotAvailable(control);
                        break;
                }
            }

        }


        private void Draw_Panel(ControlItem control)
        {
            if (Palette == null)
            {
                return;
            }
            var r = new Rectangle();
            r.Width = Zoom * control.Width;
            r.Height = Zoom * control.Height;
            r.Stroke = Palette[control.Paper].Brush;
            r.Fill = Palette[control.Paper].Brush;
            cnvEditor.Children.Add(r);
            Canvas.SetTop(r, control.Top * Zoom);
            Canvas.SetLeft(r, control.Left * Zoom);
        }


        private void Draw_NotAvailable(ControlItem control)
        {
            var r = new Rectangle();
            r.Width = Zoom * control.Width;
            r.Height = Zoom * control.Height;
            r.Stroke = new SolidColorBrush(Colors.Orange);
            r.Fill = new SolidColorBrush(Colors.Yellow);
            cnvEditor.Children.Add(r);
            Canvas.SetTop(r, control.Top * Zoom);
            Canvas.SetLeft(r, control.Left * Zoom);
        }
    }
}
