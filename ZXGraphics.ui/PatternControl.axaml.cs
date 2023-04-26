using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ZXGraphics.neg;

namespace ZXGraphics.ui
{
    public partial class PatternControl : UserControl
    {
        public Action<PatternControl> callBackClik = null;

        /// <summary>
        /// pattern data
        /// </summary>
        public Pattern Pattern { get; set; }

        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
                Refresh();
            }
        }

        private bool _IsSelected = false;


        /// <summary>
        /// Constructor, madatory to call Initialize
        /// </summary>
        public PatternControl()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Initializes the control
        /// </summary>
        /// <param name="patternData">Data for the Pattern</param>
        /// <param name="callBackClik">Delegate for click event</param>
        /// <returns>True if ok or False if error</returns>
        public bool Initialize(Pattern patternData, Action<PatternControl> callBackClik)
        {
            Pattern = patternData;
            this.callBackClik = callBackClik;
            this.Tapped += PatternControl_Tapped;
            return true;
        }

        /// <summary>
        /// User click on the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PatternControl_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            callBackClik(this);
        }


        /// <summary>
        /// Refresh the control
        /// </summary>
        public void Refresh()
        {
            if (_IsSelected)
            {
                brdMain.BorderBrush = Brushes.Red;
            }
            else
            {
                brdMain.BorderBrush = Brushes.Transparent;
            }

            if (Pattern == null)
            {
                return;
            }

            lblNumber.Text = Pattern.Number;
            lblName.Text = Pattern.Name;

            if (Pattern.Data == null)
            {
                return;
            }

            cnvPoints.Children.Clear();
            foreach (var d in Pattern.Data)
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
