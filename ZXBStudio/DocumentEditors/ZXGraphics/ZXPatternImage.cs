using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ShimSkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public class ZXPatternImage : IZXBitmap, IDisposable
    {
        #region Private fields
        private WriteableBitmap? bitmap;
        #endregion

        #region Public properties
        public bool IsEmpty { get; private set; }
        #endregion

        #region Constructors
        public ZXPatternImage()
        {
            bitmap = new WriteableBitmap(new PixelSize(8, 8), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);

            Clear(Colors.White);
        }
        public ZXPatternImage(Pattern Pattern, int Width, int Height)
        {
            bitmap = new WriteableBitmap(new PixelSize(Width, Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);

            RenderPattern(Pattern, Width, Height);
        }
        #endregion

        #region Public functions
        public unsafe void Clear(Color ClearColor)
        {
            if (bitmap == null) //disposed
                return;

            using var lockData = bitmap.Lock();
            uint* data = (uint*)lockData.Address;
            uint color = ToRgba(ClearColor);

            for (int y = 0; y < bitmap.PixelSize.Height; y++)
            {
                for (int x = 0; x < bitmap.PixelSize.Width; x++)
                {
                    data[y * lockData.RowBytes / 4 + x] = color;
                }

            }

            IsEmpty = true;
        }

        public unsafe void RenderPattern(Pattern Pattern, int Width, int Height)
        {

            if (bitmap == null) //disposed
                throw new ObjectDisposedException("ZXSpriteImage");

            if (bitmap.PixelSize.Width != Width || bitmap.PixelSize.Height != Height)
            {
                bitmap.Dispose();
                bitmap = new WriteableBitmap(new PixelSize(Width, Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);
            }

            using var lockData = bitmap.Lock();
            uint* data = (uint*)lockData.Address;

            int index = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int colorIndex = Pattern.RawData[index++];
                    data[y * lockData.RowBytes / 4 + x] = ToRgba(colorIndex == 1 ? Colors.Black : Colors.White);

                }
            }

            IsEmpty = false;
        }
        #endregion

        #region Private functions
        uint ToRgba(Color Color)
        {
            return (uint)((255 << 24) | (Color.B << 16) | (Color.G << 8) | Color.R);
        }
        #endregion

        #region IIMage implementation
        public Size Size
        {
            get
            {
                return bitmap.Size;
            }
        }

        public PixelSize PixelSize
        {
            get
            {
                return bitmap.PixelSize;
            }
        }

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
            ((IImage)bitmap).Draw(context, sourceRect, destRect);
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            if (bitmap == null)
                return;

            bitmap.Dispose();
            bitmap = null;
            IsEmpty = true;
        }
        #endregion
    }
}
