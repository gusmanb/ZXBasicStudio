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

namespace ZXBasicStudio.Emulator.Controls
{
    public partial class ZXScreen : UserControl
    {
        WriteableBitmap buffer = new WriteableBitmap(new PixelSize(416, 312), new Vector(72,72), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Opaque);

        PixelSize borderSize = new PixelSize(416, 312);
        PixelSize borderlessSize = new PixelSize(256, 192);

        Bitmap logoBitmap;

        bool isRunning;
        public bool IsRunning { get { return isRunning; } set { isRunning = value; InvalidateVisual(); } }

        DateTime? lastTurboUpdate;
        bool turbo = false;
        object turboLocker = new object();
        object drawLocker = new object();

        bool _borderless;
        bool _recreate = false;
        public bool Borderless 
        { 
            get { return _borderless; } 
            set { _borderless = value; _recreate = true; } 
        }

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

            if (_recreate)
            {
                lock (drawLocker)
                {
                    buffer.Dispose();
                    buffer = new WriteableBitmap(_borderless ? borderlessSize : borderSize, new Vector(72, 72), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Opaque);
                    _recreate = false;
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
            lock (drawLocker)
            {
                if (buffer == null)
                    return;

                if (IsRunning)
                {
                    var scale = Bounds.Width / buffer.PixelSize.Width;
                    if (buffer.PixelSize.Height * scale > Bounds.Height)
                        scale = Bounds.Height / buffer.PixelSize.Height;

                    var w = buffer.PixelSize.Width * scale;
                    var h = buffer.PixelSize.Height * scale;
                    var xOffset = (Bounds.Width - w) / 2.0;
                    var yOffset = (Bounds.Height - h) / 2.0;

                    var targetRect = new Rect(xOffset, yOffset, w, h);

                    context.DrawImage(buffer, new Rect(0, 0, buffer.PixelSize.Width, buffer.PixelSize.Height), targetRect, BitmapInterpolationMode.LowQuality);
                }
                else
                {
                    var scale = Bounds.Width / logoBitmap.PixelSize.Width;
                    if (logoBitmap.PixelSize.Height * scale > Bounds.Height)
                        scale = Bounds.Height / logoBitmap.Size.Height;

                    var w = logoBitmap.PixelSize.Width * scale;
                    var h = logoBitmap.PixelSize.Height * scale;
                    var xOffset = (Bounds.Width - w) / 2.0;
                    var yOffset = (Bounds.Height - h) / 2.0;

                    var targetRect = new Rect(xOffset, yOffset, w, h);

                    context.DrawImage(logoBitmap, new Rect(0, 0, logoBitmap.PixelSize.Width, logoBitmap.PixelSize.Height), targetRect, BitmapInterpolationMode.HighQuality);
                }
            }
        }
    }
}
