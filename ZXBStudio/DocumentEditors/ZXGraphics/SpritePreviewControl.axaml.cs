using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        #endregion


        #region Private fields

        private int frameNumber = 0;
        private int speed = 1;
        private DispatcherTimer tmr = null;

        /// <summary>
        /// Speeds in milliseconds
        /// </summary>
        private int[] speeds = new int[] { 600000, 1000, 500, 250, 200, 125, 100, 66, 50 };

        #endregion


        public SpritePreviewControl()
        {
            InitializeComponent();
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
            if (tmr == null)
            {
                tmr = new DispatcherTimer(TimeSpan.FromMilliseconds(speeds[2]), DispatcherPriority.Normal, Refresh);
                cmbSpeed.SelectedIndex = 2;
            }

            return true;
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


        public void Refresh(object? sender=null, EventArgs e=null)
        {
            try
            {
                if (SpriteData == null || SpriteData.Patterns == null || SpriteData.Patterns.Count == 0)
                {
                    if (tmr != null)
                    {
                        tmr.Stop();
                        tmr = null;
                    }

                    // Delete background
                    {
                        var r = new Rectangle();
                        r.Width = cnvPreview.Width * 4;
                        r.Height = cnvPreview.Height * 4;
                        r.Fill = new SolidColorBrush(new Color(255, 0x28, 0x28, 0x28));
                        cnvPreview.Children.Add(r);
                        Canvas.SetTop(r, 0);
                        Canvas.SetLeft(r, 0);
                    }

                    return;
                }

                if (tmr == null)
                {
                    tmr = new DispatcherTimer(TimeSpan.FromMilliseconds(speeds[speed]), DispatcherPriority.Normal, Refresh);
                }
                tmr.Stop();

                frameNumber++;
                if (frameNumber >= SpriteData.Frames)
                {
                    frameNumber = 0;
                }
                cnvPreview.Width = SpriteData.Width * 4;
                cnvPreview.Height = SpriteData.Height * 4;

                cnvPreview.Children.Clear();
                for (int y = 0; y < SpriteData.Height; y++)
                {
                    for (int x = 0; x < SpriteData.Width; x++)
                    {
                        int colorIndex = 0;

                        var frame = SpriteData.Patterns[frameNumber];
                        var p = frame.Data.FirstOrDefault(d => d.X == x && d.Y == y);
                        if (p != null)
                        {
                            colorIndex = p.ColorIndex;
                        }

                        var r = new Rectangle();
                        r.Width = 4;
                        r.Height = 4;

                        var palette = SpriteData.Palette[colorIndex];
                        r.Fill = new SolidColorBrush(new Color(255, palette.Red, palette.Green, palette.Blue));
                        //var r2 = r.Clonar<Rectangle>();

                        cnvPreview.Children.Add(r);
                        Canvas.SetTop(r, y * 4);
                        Canvas.SetLeft(r, x * 4);
                    }
                }
            }
            catch(Exception ex) 
            {
            }
            if (tmr != null)
            {
                tmr.Start();
            }
        }        

    }
}
