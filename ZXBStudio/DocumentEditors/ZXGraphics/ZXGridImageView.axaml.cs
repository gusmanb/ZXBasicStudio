using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Diagnostics;
using ZXBasicStudio.DocumentEditors.ZXGraphics;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class ZXGridImageView : UserControl
    {
        private WriteableBitmap? gridImage;
        private IZXBitmap? backgroundImage;
        private SKColor gridColor = new SKColor(0x00, 0x00, 0x00, 0xFF);
        private int zoom = 4;
        public int Zoom
        {
            get => zoom;
            set
            {
                zoom = value;

                if (backgroundImage != null)
                {
                    this.Width = (int)(backgroundImage.PixelSize.Width * Zoom + backgroundImage.PixelSize.Width + 1);
                    this.Height = (int)(backgroundImage.PixelSize.Height * Zoom + backgroundImage.PixelSize.Height + 1);
                }

                InvalidateVisual();
            }
        }
        public SKColor GridColor
        {
            get => gridColor;
            set
            {
                gridColor = value;
                InvalidateVisual();
            }
        }

        public IZXBitmap? BackgroundImage
        {
            get => backgroundImage;
            set
            {
                backgroundImage = value;

                if (backgroundImage != null)
                {
                    this.Width = (int)(backgroundImage.PixelSize.Width * Zoom + backgroundImage.PixelSize.Width + 1);
                    this.Height = (int)(backgroundImage.PixelSize.Height * Zoom + backgroundImage.PixelSize.Height + 1);
                }

                InvalidateVisual();
            }
        }

        public ZXGridImageView()
        {
            InitializeComponent();
        }

        private void EnsureGrid()
        {
            if (backgroundImage == null)
                return;

            int w = (int)(backgroundImage.PixelSize.Width * Zoom + backgroundImage.PixelSize.Width + 1);
            int h = (int)(backgroundImage.PixelSize.Height * Zoom + backgroundImage.PixelSize.Height + 1);

            if (gridImage != null)
            {
                if (gridImage.PixelSize.Width == w && gridImage.PixelSize.Height == h)
                    return;

                gridImage.Dispose();

                gridImage = new WriteableBitmap(new PixelSize(w, h), new Vector(72, 72), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Opaque);
            }
            else
            {
                gridImage = new WriteableBitmap(new PixelSize(w, h), new Vector(72, 72), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Opaque);
            }

            using var lockBits = gridImage.Lock();
            var info = new SKImageInfo(lockBits.Size.Width, lockBits.Size.Height, lockBits.Format.ToSkColorType(), SKAlphaType.Premul);

            using var surface = SKSurface.Create(info, lockBits.Address, lockBits.RowBytes);
            var canvas = surface.Canvas;

            //Clear grid
            canvas.Clear(new SKColor(0xFF, 0xFF, 0xFF, 0x00));

            using var paint = new SKPaint { Color = gridColor, StrokeWidth = 1, IsAntialias = false };

            //Draw vertical lines
            for (int x = 0; x < gridImage.PixelSize.Width + 1; x += Zoom + 1)
                canvas.DrawLine(new SKPoint(x, 0), new SKPoint(x, gridImage.PixelSize.Height), paint);

            //Draw horizontal lines
            for (int y = 0; y < gridImage.PixelSize.Height + 1; y += Zoom + 1)
                canvas.DrawLine(new SKPoint(0, y), new SKPoint(gridImage.PixelSize.Width, y), paint);

        }

        public override void Render(DrawingContext context)
        {
            EnsureGrid();

            if (backgroundImage == null || gridImage == null)
            {
                base.Render(context);
                return;
            }

            context.DrawImage(backgroundImage, new Rect(0, 0, gridImage.Size.Width, gridImage.Size.Height));

            if(zoom > 4)
                context.DrawImage(gridImage, new Rect(0, 0, gridImage.Size.Width, gridImage.Size.Height));
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Debug.WriteLine($"OnPointerPressed: {e.GetPosition(this)}");
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e); 
            Debug.WriteLine($"OnPointerMoved: {this.Width}/{this.Height} : {e.GetPosition(this)}");
        }
    }
}