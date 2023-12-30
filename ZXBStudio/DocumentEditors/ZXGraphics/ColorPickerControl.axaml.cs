using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ZXBasicStudio.Common;
using Avalonia.Input;
using System.Text;
using System.Security.Cryptography;
using ZXBasicStudio.Extensions;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    /// <summary>
    /// Main editor control for ZXGraphics
    /// </summary>
    public partial class ColorPickerControl : UserControl
    {
        private GraphicsModes graphicMode = GraphicsModes.Monochrome;
        private Action<string, int> callBackCommand = null;
        private PaletteColor[] palette = null;
        private int selectedIndexColor = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public ColorPickerControl()
        {
            InitializeComponent();
        }


        public bool Inicialize(GraphicsModes graphicMode, PaletteColor[] palette, int selectedIndexColor, Action<string, int> callBackCommand)
        {
            this.graphicMode = graphicMode;
            this.palette = palette;
            this.callBackCommand = callBackCommand;
            this.selectedIndexColor = selectedIndexColor;

            grdMonochrome.IsVisible = false;
            grdZXSpectrum.IsVisible = false;
            grdNext.IsVisible = false;
            switch (graphicMode)
            {
                case GraphicsModes.Monochrome:
                    grdMonochrome.IsVisible = true;
                    break;
                case GraphicsModes.ZXSpectrum:
                    DrawZXSpectrum();
                    grdZXSpectrum.IsVisible = true;
                    break;
                case GraphicsModes.Next:
                    grdNext.IsVisible = true;
                    break;

            }

            return true;
        }
       
        /// <summary>
        /// Draw colors for ZXSpectrum palette
        /// </summary>
        private void DrawZXSpectrum()
        {
            cnvZXSpectrum.Children.Clear();
            // Draw all colors
            int index = 0;
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var p = palette[index];
                    var b = new SolidColorBrush(new Color(255, p.Red, p.Green, p.Blue));
                    DrawRectangle(cnvZXSpectrum, x * 20, y * 20, 18, 18, Brushes.White, b, index);
                    index++;

                }
            }

            // Draw selected color (ink)
            {
                int y = selectedIndexColor / 8;
                int x = selectedIndexColor % 8;
                DrawRectangle(cnvZXSpectrum, (x * 20) - 2, (y * 20) - 2, 22, 22, Brushes.Red, null, -1);
            }
        }


        private void DrawRectangle(Canvas canvas, int x, int y, int w, int h, IBrush? borderBrush, IBrush? colorBrush, int tag)
        {
            var r = new Rectangle();
            r.Width = w;
            r.Height = h;
            r.Tag = tag.ToString();
            if (borderBrush != null)
            {
                r.Stroke = borderBrush;
                r.StrokeThickness = 1;
            }
            if (colorBrush != null)
            {
                r.Fill = colorBrush;
            }
            canvas.Children.Add(r);
            Canvas.SetTop(r, y);
            Canvas.SetLeft(r, x);

            r.Tapped += R_Tapped;
        }


        private void R_Tapped(object? sender, TappedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            Rectangle r = (Rectangle)sender;

            int color = r.Tag.ToInteger();
            if (color >= 0)
            {
                selectedIndexColor = color;
                this.IsVisible = false;

                callBackCommand?.Invoke("SELECT", selectedIndexColor);
            }
        }
    }
}
