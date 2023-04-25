using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ZXGraphics.neg;

namespace ZXGraphics.ui
{
    public partial class PatternControl : UserControl
    {
        /// <summary>
        /// pattern data
        /// </summary>
        public Pattern Data { get; set; }


        /// <summary>
        /// Constructor, madatory to set Data property
        /// </summary>
        public PatternControl()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Constructor with pattern data included
        /// </summary>
        /// <param name="patternData">Data for the Pattern</param>
        public PatternControl(Pattern patternData)
        {
            InitializeComponent();
            Data = patternData;
        }


        public void Refresh()
        {
            if (Data == null)
            {
                return;
            }

            lblNumber.Text = Data.Number;
            lblName.Text = Data.Name;

            if (Data.Data == null)
            {
                return;
            }

            cnvPoints.Children.Clear();
            foreach (var d in Data.Data)
            {
                if (d.ColorIndex == 1)
                {
                    var r = new Rectangle();
                    r.Width = 4;
                    r.Height = 4;
                    r.Fill = Brushes.Black;
                    cnvPoints.Children.Add(r);
                    Canvas.SetTop(r, d.Y * 4);
                    Canvas.SetLeft(r, d.X * 4);
                }
            }
        }
    }
}
