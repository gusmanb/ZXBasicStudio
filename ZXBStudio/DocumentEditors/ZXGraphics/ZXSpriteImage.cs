using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public class ZXSpriteImage : IZXBitmap, IDisposable
    {
        #region Private fields
        private WriteableBitmap? bitmap;
        #endregion

        #region Public properties
        public bool IsEmpty { get; private set; }
        #endregion

        #region Constructors
        public ZXSpriteImage()
        {
            bitmap = new WriteableBitmap(new PixelSize(8, 8), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);

            Clear(Colors.White);
        }
        public ZXSpriteImage(Sprite Sprite, int FrameNumber)
        {
            bitmap = new WriteableBitmap(new PixelSize(Sprite.Width, Sprite.Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);

            RenderSprite(Sprite, FrameNumber);
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

        public unsafe void RenderSprite(Sprite Sprite, int FrameNumber)
        {
            try
            {
                if (bitmap == null)
                {
                    throw new ObjectDisposedException("ZXSpriteImage");
                }

                if(Sprite.Width==0 || Sprite.Height == 0)
                {
                    return;
                }

                if (bitmap.PixelSize.Width != Sprite.Width || bitmap.PixelSize.Height != Sprite.Height)
                {
                    bitmap.Dispose();
                    bitmap = new WriteableBitmap(new PixelSize(Sprite.Width, Sprite.Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);
                }

                using var lockData = bitmap.Lock();
                uint* data = (uint*)lockData.Address;

                var frame = Sprite.Patterns[FrameNumber];
                int index = 0;

                for (int y = 0; y < Sprite.Height; y++)
                {
                    for (int x = 0; x < Sprite.Width; x++)
                    {
                        int colorIndex = frame.RawData[index++];

                        PaletteColor color;

                        switch (Sprite.GraphicMode)
                        {
                            case GraphicsModes.ZXSpectrum:
                                {
                                    var attr = GetAttribute(Sprite, frame, x, y);
                                    if (colorIndex == 0)
                                        color = Sprite.Palette[attr.Paper];
                                    else
                                        color = Sprite.Palette[attr.Ink];
                                }
                                break;
                            case GraphicsModes.Monochrome:
                                if (colorIndex > Sprite.Palette.Length - 1)
                                {
                                    colorIndex = Sprite.Palette.Length - 1;
                                }
                                color = Sprite.Palette[colorIndex];
                                break;
                            default:
                                color = new PaletteColor { Red = 0xFF, Green = 0xFF, Blue = 0xFF };
                                break;
                        }

                        data[y * lockData.RowBytes / 4 + x] = ToRgba(color);

                    }
                }

                IsEmpty = false;
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Private functions
        uint ToRgba(PaletteColor Color)
        {
            return (uint)((255 << 24) | (Color.Blue << 16) | (Color.Green << 8) | Color.Red);
        }
        uint ToRgba(Color Color)
        {
            return (uint)((255 << 24) | (Color.B << 16) | (Color.G << 8) | Color.R);
        }
        private AttributeColor GetAttribute(Sprite Sprite, Pattern Pattern, int X, int Y)
        {
            int cW = Sprite.Width / 8;
            int cX = X / 8;
            int cY = Y / 8;
            int dir = (cY * cW) + cX;
            if (Pattern.Attributes == null)
            {
                Pattern.Attributes = new AttributeColor[(Sprite.Width + Sprite.Height) / 8];
                for (int n = 0; n < Pattern.Attributes.Length; n++)
                {
                    Pattern.Attributes[n] = new AttributeColor()
                    {
                        Attribute = 56  // Paper 7, ink 0
                    };
                }
            }
            if (dir > Pattern.Attributes.Length)
            {
                return new AttributeColor()
                {
                    Attribute = 56  // Paper 7, ink 0
                };
            }
            return Pattern.Attributes[dir];
        }

        #endregion

        #region IZXBitmap implementation
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
