using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using ZXBasicStudio.Common;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    /// <summary>
    /// Preview panel
    /// </summary>
    public partial class PreviewControl : UserControl
    {
        /// <summary>
        /// Delegate for get pattern
        /// </summary>
        private Func<int, Pattern> callbackGetPattern = null;

        /// <summary>
        /// Timer for animation
        /// </summary>
        private DispatcherTimer tmr = null;

        /// <summary>
        /// Actual frame
        /// </summary>
        private int currentFrame = 0;
        /// <summary>
        /// Speeds in milliseconds
        /// </summary>
        private int[] speeds = new int[] { 1000, 500, 250, 200, 125, 100, 66, 50 };
        /// <summary>
        /// Zoom values
        /// </summary>
        private int[] zooms = new int[] { 1, 2, 4 };


        /// <summary>
        /// Constructor
        /// </summary>
        public PreviewControl()
        {
            InitializeComponent();

            cmbSpeed.SelectionChanged += CmbSpeed_SelectionChanged;
            cmbSpeed.SelectedIndex = 1;
            cmbZoom.SelectedItem = 2;
        }


        /// <summary>
        /// Initializes the preview panel
        /// </summary>
        /// <param name="callbackGetPattern"></param>
        /// <param name="maxPatterns"></param>
        /// <returns></returns>
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


        public void Start()
        {
            if (tmr == null)
            {
                tmr = new DispatcherTimer();
                tmr.Interval = TimeSpan.FromMilliseconds(500);
                tmr.Tick += tmr_Tick;
            }
            tmr.Start();
        }


        public void Stop()
        {
            if (tmr != null)
            {
                tmr.Stop();
            }
        }


        /// <summary>
        /// Timer tick event to refresh preview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// Speed changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbSpeed_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (tmr == null)
            {
                return;
            }

            var sel = cmbSpeed.SelectedIndex;
            if (sel < 0)
            {
                sel = 2;
            }
            int speed = speeds[sel];
            tmr.Interval = TimeSpan.FromMilliseconds(speed);
        }
    }
}
