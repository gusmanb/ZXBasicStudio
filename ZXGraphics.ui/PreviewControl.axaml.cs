using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using ZXGraphics.neg;
using Common;

namespace ZXGraphics.ui
{
    public partial class PreviewControl : UserControl
    {
        private Func<int, Pattern> callbackGetPattern = null;
        private DispatcherTimer tmr = null;

        private int currentFrame = 0;
        private int[] speeds = new int[] { 1000, 500, 250, 200, 125, 100, 66, 50 };
        private int[] zooms = new int[] { 1, 2, 4 };


        public PreviewControl()
        {
            InitializeComponent();

            cmbSpeed.SelectionChanged += CmbSpeed_SelectionChanged;
            cmbSpeed.SelectedIndex = 1;
            cmbZoom.SelectedItem = 2;
        }


        public bool Initialize(Func<int, Pattern> callbackGetPattern, int maxPatterns)
        {
            this.callbackGetPattern = callbackGetPattern;

            txtPreviewFirst.Maximum = maxPatterns;
            txtPreviewFrames.Maximum = maxPatterns;

            if (cmbZoom.SelectedIndex < 0)
            {
                cmbZoom.SelectedIndex = 2;
            }

            tmr = new DispatcherTimer();
            tmr.Interval = TimeSpan.FromMilliseconds(500);
            tmr.Tick += tmr_Tick;
            tmr.Start();

            return true;
        }


        private void tmr_Tick(object? sender, EventArgs e)
        {
            try
            {
                var first = txtPreviewFirst.Text.ToInteger();
                var numberOfFrames = txtPreviewFrames.Text.ToInteger() - 1;
                var width = txtPreviewWidth.Text.ToInteger();
                var height = txtPreviewHeigth.Text.ToInteger();
                var zoom = zooms[cmbZoom.SelectedIndex];

                var widthP = width * 8;
                var heightP = height * 8;
                var widthTotal = widthP * zoom;
                var heightTotal = heightP * zoom;

                cnvPreview.Width = widthTotal;
                cnvPreview.Height = heightTotal;
                brdPreview.Width = widthTotal + 2;
                brdPreview.Height = heightTotal + 2;

                cnvPreview.Children.Clear();
                int actual = currentFrame;
                for (int py = 0; py < height; py++)
                {
                    for (int px = 0; px < width; px++)
                    {
                        var pattern = callbackGetPattern(actual);
                        if (pattern != null)
                        {
                            foreach (var d in pattern.Data)
                            {
                                if (d.ColorIndex == 1)
                                {
                                    var r = new Rectangle();
                                    r.Width = zoom;
                                    r.Height = zoom;
                                    r.Fill = Brushes.Black;
                                    cnvPreview.Children.Add(r);
                                    Canvas.SetTop(r, ((py * 8) + d.Y) * zoom);
                                    Canvas.SetLeft(r, ((px * 8) + d.X) * zoom);
                                }
                            }
                        }
                        actual++;
                    }
                }

                if (currentFrame < first)
                {
                    currentFrame = first;
                }
                int paso = width * height;
                currentFrame += paso;
                if (currentFrame > (first + (numberOfFrames * paso)))
                {
                    currentFrame = first;
                }
            }
            catch (Exception ex)
            {

            }
        }



        private void CmbSpeed_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (tmr == null)
            {
                return;
            }

            var sel = cmbSpeed.SelectedIndex;
            int speed = speeds[sel];
            tmr.Interval = TimeSpan.FromMilliseconds(speed);
        }
    }
}
