using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class SpritePreviewControl : UserControl
    {
        #region Public properties

        /// <summary>
        /// Sprite data
        /// </summary>
        public Sprite SpriteData { get; set; }
        public int Zoom { get; set; } = 4;

        #endregion


        #region Private fields

        private int frameNumber = 0;
        private int speed = 1;
        private DispatcherTimer tmr;
        private Color emptyColor = new Color(255, 0x28, 0x28, 0x28);
        WriteableBitmap aspect = new WriteableBitmap(new PixelSize(32, 32), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);

        const int EMPTY_BITMAP = -1;

        /// <summary>
        /// Speeds in milliseconds
        /// </summary>
        private int[] speeds = new int[] { 600000, 1000, 500, 250, 200, 125, 100, 66, 50 };

        #endregion


        public SpritePreviewControl()
        {
            tmr = new DispatcherTimer(TimeSpan.FromMilliseconds(speeds[2]), DispatcherPriority.Normal, Refresh);
            InitializeComponent();
            imgPreview.Source = aspect;
        }


        /// <summary>
        /// Initializes the control
        /// </summary>
        /// <param name="spriteData">Data of the sprite, if is null, the "Add" icon is visible and no properties are shown</param>
        /// <param name="callBackCommand">CallBak for actions command, line "ADD", "CLONE", "DELETE" or "SELECTED"</param>
        /// <returns></returns>
        public bool Initialize(Sprite spriteData)
        {
            this.SpriteData = spriteData;

            this.cmbSpeed.SelectionChanged += CmbSpeed_SelectionChanged;
            tmr.Interval = TimeSpan.FromMilliseconds(speeds[speed]);

            return true;
        }


        public void Start()
        {
            tmr.Start();
        }


        public void Stop()
        {
            tmr.Stop();
        }


        private void CmbSpeed_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var s = cmbSpeed.SelectedIndex.ToInteger();
            if (s < 0 || s >= speeds.Length)
            {
                s = 2;
            }
            speed = s;
            tmr.Interval = TimeSpan.FromMilliseconds(speeds[speed]);
        }


        public void Refresh(object? sender = null, EventArgs e = null)
        {
            try
            {
                if (SpriteData == null || SpriteData.Patterns == null || SpriteData.Patterns.Count == 0)
                {
                    tmr.Stop();
                    // Delete background
                    UpdateBitmap(EMPTY_BITMAP);
                    imgPreview.InvalidateVisual();
                    return;
                }

                if (tmr == null)
                {
                    tmr = new DispatcherTimer(TimeSpan.FromMilliseconds(speeds[speed]), DispatcherPriority.Normal, Refresh);
                }

                tmr.Stop();

                if (SpriteData.Masked)
                {
                    frameNumber += 2;
                }
                else
                {
                    frameNumber++;
                }

                if (frameNumber >= SpriteData.Frames)
                {
                    frameNumber = 0;
                }

                imgPreview.Width = SpriteData.Width * Zoom;
                imgPreview.Height = SpriteData.Height * Zoom;

                UpdateBitmap(frameNumber);
                imgPreview.InvalidateVisual();

            }
            catch (Exception ex)
            {
            }
            tmr.Start();
        }

        private unsafe void UpdateBitmap(int frameNumber)
        {

            if (SpriteData == null)
                return;

            if(aspect == null || aspect.PixelSize.Width != SpriteData.Width || aspect.PixelSize.Height != SpriteData.Height)
            {
                aspect?.Dispose();
                aspect = new WriteableBitmap(new PixelSize(SpriteData.Width, SpriteData.Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Opaque);
                imgPreview.Source = aspect;
            }

            using var lockData = aspect.Lock();
            uint* data = (uint*)lockData.Address;

            if (frameNumber == -1)
            {
                for (int y = 0; y < SpriteData.Height; y++)
                {
                    for (int x = 0; x < SpriteData.Width; x++)
                    {
                        data[y * lockData.RowBytes / 4 + x] = emptyColor.ToUInt32();
                    }

                }

                return;
            }

            var frame = SpriteData.Patterns[frameNumber];
            int index = 0;

            for (int y = 0; y < SpriteData.Height; y++)
            {
                for (int x = 0; x < SpriteData.Width; x++)
                {
                    int colorIndex = frame.RawData[index++];

                    PaletteColor color;

                    switch (SpriteData.GraphicMode)
                    {
                        case GraphicsModes.ZXSpectrum:
                            {
                                var attr = GetAttribute(frame, x, y);
                                if (colorIndex == 0)
                                    color = SpriteData.Palette[attr.Paper];
                                else
                                    color = SpriteData.Palette[attr.Ink];
                            }
                            break;
                        case GraphicsModes.Monochrome:
                        case GraphicsModes.Next:
                            color = SpriteData.Palette[colorIndex];
                            break;
                        default:
                            color = new PaletteColor { Red = 0xFF, Green = 0xFF, Blue = 0xFF };
                            break;
                    }

                    data[y * lockData.RowBytes / 4 + x] = ToRgba(color);

                }
            }
        }

        uint ToRgba(PaletteColor color)
        {
            return (uint)((255 << 24) | (color.Blue << 16) | (color.Green << 8) | color.Red);
        }

        private AttributeColor GetAttribute(Pattern pattern, int x, int y)
        {
            int cW = SpriteData.Width / 8;
            int cX = x / 8;
            int cY = y / 8;
            return pattern.Attributes[(cY * cW) + cX];
        }

    }
}
