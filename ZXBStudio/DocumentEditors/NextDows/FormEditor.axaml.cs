using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using ZXBasicStudio.DocumentEditors.NextDows.neg;

namespace ZXBasicStudio.DocumentEditors.NextDows
{
    public partial class FormEditor : UserControl
    {
        public List<ControlItem> Controls { get; internal set; }

        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }


        /// <summary>
        /// Zoom level of the editor
        /// </summary>
        public int Zoom { get; set; }


        /// <summary>
        /// Defines is grid is visible
        /// </summary>
        public bool IsGridOn { get; set; }


        private ControlItem Viewport = null;
        private PaletteColor[] Palette=null;


        public FormEditor()
        {
            InitializeComponent();
        }


        public bool Initialize()
        {
            Controls = new List<ControlItem>();
            Palette = log.ServiceLayer.CrateNextDefaultPalette();
            Zoom = 4;
            WindowWidth = 300;
            WindowHeight = 226;
            Refresh();

            return true;
        }




        /// <summary>
        /// Refresh the editor UI
        /// </summary>
        public void Refresh()
        {            
            cnvEditor.Children.Clear();
            Viewport = Controls.FirstOrDefault(d => d.Id == 0);
            if (Viewport == null)
            {
                Viewport = new ControlItem()
                {
                    ContainerId = 0,
                    ControlType = ControlsTypes.Panel,
                    Height = WindowHeight,
                    Id = 0,
                    Ink = 0,
                    Left = 0,
                    Name = "Main panel",
                    Paper = 255,
                    Top = 0,
                    Visible = true,
                    Width = WindowWidth,
                };
                Viewport.Properties = new List<ControlProperty>();
                Controls.Add(Viewport);
            }            

            foreach (var control in Controls.OrderBy(d=>d.Id))
            {
                switch(control.ControlType)
                {
                    case ControlsTypes.Panel:
                        Draw_Panel(control);
                        break;
                }
            }

            cnvEditor.Width = WindowWidth * Zoom;
            cnvEditor.Height = WindowHeight * Zoom;
       }


        public void Draw_Panel(ControlItem control)
        {
            var r = new Rectangle();
            r.Width = Zoom * control.Width;
            r.Height = Zoom *control.Height;
            r.Stroke = Palette[control.Paper].Brush;
            r.Fill = Palette[control.Paper].Brush;
            cnvEditor.Children.Add(r);
            Canvas.SetTop(r, control.Top*Zoom);
            Canvas.SetLeft(r, control.Left*Zoom);
        }
    }
}
