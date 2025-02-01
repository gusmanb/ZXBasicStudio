using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    /// <summary>
    /// Control to show and manipulate imported image
    /// Fixed size to 400x400
    /// </summary>
    internal class ImageViewImportControl : Control
    {
        /// <summary>
        /// Zoom factor
        /// </summary>
        public int Zoom
        {
            get
            {
                return _Zoom;
            }
            set
            {
                _Zoom = value;
                this.InvalidateVisual();
            }
        }
        private int _Zoom = 1;

        /// <summary>
        /// Horizontal image offset
        /// </summary>
        public int OffsetX
        {
            get
            {
                return offsetX;
            }
            set
            {
                offsetX = value;
                this.InvalidateVisual();
            }
        }
        private int offsetX = 0;

        /// <summary>
        /// Vertical image offset
        /// </summary>
        public int OffsetY
        {
            get
            {
                return offsetY;
            }
            set
            {
                offsetY = value;
                this.InvalidateVisual();
            }
        }
        private int offsetY = 0;

        /// <summary>
        /// Width of the sprite window
        /// </summary>
        public int SpriteWidth
        {
            get
            {
                return _SpriteWidth;
            }
            set
            {
                _SpriteWidth = value;
                this.InvalidateVisual();
            }
        }
        private int _SpriteWidth = 16;

        /// <summary>
        /// Height of the sprite window
        /// </summary>
        public int SpriteHeight
        {
            get
            {
                return _SpriteHeight;
            }
            set
            {
                _SpriteHeight = value;
                this.InvalidateVisual();
            }
        }
        private int _SpriteHeight = 16;

        /// <summary>
        /// Import image data
        /// </summary>
        public SixLabors.ImageSharp.Image<Rgba32> imageData = null;

        private Pen penRed = new Pen(new SolidColorBrush(Colors.Red));
        private Brush brushGray = new SolidColorBrush(Colors.LightGray);
        private Brush brushWhite = new SolidColorBrush(Colors.White);
        private Brush brushMask = new SolidColorBrush(Color.FromArgb(175, 255, 255, 255));


        public ImageViewImportControl()
        {
            // Fixed size to 400x400
            this.Width = 400;
            this.Height = 400;

            this.PointerPressed += OnPointerPressed;
            this.PointerReleased += OnPointerReleased;
            this.PointerMoved += ImageViewImportControl_PointerMoved;
        }


        #region Render

        public async void LoadImage(IStorageFile file)
        {
            try
            {
                await using (var stream = await file.OpenReadAsync())
                {
                    imageData = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
                }
                this.InvalidateVisual();
            }
            catch (Exception ex)
            {
            }
        }


        public void Refresh()
        {
            this.InvalidateVisual();
        }


        public override void Render(DrawingContext context)
        {
            try
            {
                base.Render(context);

                int w = (int)this.Bounds.Width;
                int h = (int)this.Bounds.Height;
                int z8 = _Zoom * 8;

                // Background
                {
                    bool yPair = true;
                    for (int yy = 0; yy < 400; yy += z8)
                    {
                        bool pair = yPair;
                        yPair = !yPair;
                        for (int xx = 0; xx < 400; xx += z8)
                        {
                            int x1 = xx;
                            int x2 = x1 + z8;
                            int xz = z8;
                            int y1 = yy;
                            int y2 = y1 + z8;
                            int yz = z8;
                            if (x1 >= 400 || y1 >= 400)
                            {
                                continue;
                            }
                            if (x2 > 400)
                            {
                                xz = 400 - x1;
                            }
                            if (y2 > 400)
                            {
                                yz = 400 - y1;
                            }
                            Rect r0 = new Rect(x1, y1, xz, yz);
                            if (pair)
                            {
                                context.FillRectangle(brushGray, r0);
                            }
                            else
                            {
                                context.FillRectangle(brushWhite, r0);                             
                            }
                            pair = !pair;
                        }
                    }
                }

                // Image
                if (imageData != null)
                {
                    int iw = imageData.Size.Width;
                    int ih = imageData.Size.Height;
                    int cw = 0;
                    int ch = 0;
                    int z = _Zoom;

                    int yd = offsetY;
                    for (int y = 0; y < h; y += z)
                    {
                        int xd = offsetX;
                        for (int x = 0; x < w; x += z)
                        {
                            if (xd >= 0 && xd < iw &&
                                yd >= 0 && yd < ih)
                            {
                                try
                                {
                                    int xx = x;
                                    int yy = y;

                                    if ((xx + z) > w)
                                    {
                                        cw = w - xx;
                                    }
                                    else
                                    {
                                        cw = z;
                                    }
                                    if ((yy + z) > h)
                                    {
                                        ch = h - yy;
                                    }
                                    else
                                    {
                                        ch = z;
                                    }
                                    Rect r = new Rect(xx, yy, cw, ch);
                                    var pixel = imageData[xd, yd];
                                    var brush = new SolidColorBrush(Color.FromArgb(
                                        pixel.A, pixel.R, pixel.G, pixel.B));
                                    context.FillRectangle(brush, r);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            xd++;
                        }
                        yd++;
                    }
                }

                // Mask
                {
                    int x2 = SpriteWidth * _Zoom;
                    int y2 = SpriteHeight * _Zoom;

                    var r0 = new Rect(x2, 0, 400-x2, 400);
                    context.FillRectangle(brushMask, r0);
                    var r1 = new Rect(0, y2, x2, 400-y2);
                    context.FillRectangle(brushMask, r1);

                    context.DrawRectangle(penRed, new Rect(-1, -1, x2, y2));
                }              
            }
            catch (Exception ex)
            {

            }
        }

#endregion


        #region Mouse

        private bool mouseDown = false;
        private int mouseX = 0;
        private int mouseY = 0;


        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            mouseDown = false;
        }


        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            mouseDown = true;
            var pos = e.GetPosition((Control)sender);
            mouseX = (int)pos.X;
            mouseY = (int)pos.Y;
        }


        private void ImageViewImportControl_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (!mouseDown)
            {
                return;
            }

            var pos = e.GetPosition((Control)sender);
            var dX = mouseX - (int)pos.X;
            var dY = mouseY - (int)pos.Y;
            offsetX += (dX / _Zoom);
            offsetY += (dY / _Zoom);
            mouseX = (int)pos.X;
            mouseY = (int)pos.Y;
            this.InvalidateVisual();
        }

        #endregion
    }
}
