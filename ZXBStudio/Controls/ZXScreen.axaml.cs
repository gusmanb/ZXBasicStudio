using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using ExCSS;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ZXBasicStudio.Controls
{
    public partial class ZXScreen : UserControl
    {
        WriteableBitmap buffer = new WriteableBitmap(new PixelSize(416, 312), new Vector(72,72), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Opaque);
        Bitmap logoBitmap;

        bool isRunning;
        public bool IsRunning { get { return isRunning; } set { isRunning = value; InvalidateVisual(); } }

        DateTime? lastTurboUpdate;
        bool turbo = false;
        object turboLocker = new object();

        public bool TurboEnabled 
        { 
            get { return turbo; } 
            set 
            {
                lock (turboLocker)
                {

                    if (value)
                        lastTurboUpdate = DateTime.Now;
                    else
                        lastTurboUpdate = null;

                    turbo = value;

                }
            }
        }

        private static readonly Rect videoScreenRect = new Rect(0, 0, 416, 312);
        public ZXScreen()
        {
            int[] tmpData = new int[416 * 312];
            Array.Fill(tmpData, unchecked((int)0xFFFFFFFF));
            RenderFrame(tmpData);
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            logoBitmap = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/logoSmall.png")));
            InitializeComponent();
        }

        public void RenderFrame(int[] VideoData)
        {
            lock (turboLocker)
            {
                if (lastTurboUpdate != null)
                {
                    TimeSpan elapsed = DateTime.Now - lastTurboUpdate.Value;
                    if (elapsed.TotalMilliseconds < 200)
                        return;

                    lastTurboUpdate = DateTime.Now;
                }
            }

            using (var surface = buffer.Lock())
            {
                Marshal.Copy(VideoData, 0, surface.Address, VideoData.Length);
            }

            Dispatcher.UIThread.InvokeAsync(new Action(() =>
            {
                InvalidateVisual();
            }));
        }

        public override void Render(DrawingContext context)
        {
            if (buffer == null)
                return;

            var scale = Bounds.Width / videoScreenRect.Width;
            if (videoScreenRect.Height * scale > Bounds.Height)
                scale = Bounds.Height / videoScreenRect.Height;

            var w = videoScreenRect.Width * scale;
            var h = videoScreenRect.Height * scale;
            var xOffset = (Bounds.Width - w) / 2.0;
            var yOffset = (Bounds.Height - h) / 2.0;

            var targetRect = new Rect(xOffset, yOffset, w, h);

            if(IsRunning)
                context.DrawImage(buffer, videoScreenRect, targetRect, BitmapInterpolationMode.LowQuality);
            else
                context.DrawImage(logoBitmap, videoScreenRect, targetRect, BitmapInterpolationMode.HighQuality);
        }
    }
}
