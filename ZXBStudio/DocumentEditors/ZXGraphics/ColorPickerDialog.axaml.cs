using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Common;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;
using ZXBasicStudio.Extensions;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic;
using static System.Net.Mime.MediaTypeNames;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public partial class ColorPickerDialog : Window
    {
        private AttributeColor attributeColor = null;
        private PaletteColor[] palette = null;
        private GraphicsModes graphicMode = GraphicsModes.Monochrome;
        private Action<string, AttributeColor> callBackAction = null;


        public ColorPickerDialog()
        {
            InitializeComponent();
        }


        public bool Initialize(AttributeColor attributeColor, PaletteColor[] palette, GraphicsModes graphicMode, Action<string, AttributeColor> callBackAction)
        {
            this.attributeColor = attributeColor;
            this.palette = palette;
            this.graphicMode = graphicMode;
            this.callBackAction = callBackAction;

            DrawColors();
            return true;
        }


        private void DrawColors()
        {
            DrawPalette(cnvInk);
            DrawPalette(cnvPaper);
        }

        private void DrawPalette(Canvas cnv)
        {
            int rows = 0;
            int cols = 0;

            switch (graphicMode)
            {
                case GraphicsModes.Monochrome:
                    rows = 1;
                    cols = 2;
                    break;
                case GraphicsModes.ZXSpectrum:
                    rows = 2;
                    cols = 8;
                    break;
            }

            int cellW = (int)(((this.Width - 200) / 2) / cols);
            int cellH = (int)(((this.Height - 100) / 2) / rows);
            int idx = 0;

            cnv.Children.Clear();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var r = new Rectangle();
                    r.Width = cellW-2;
                    r.Height = cellH-2;

                    var pal = palette[idx];
                    r.Fill = new SolidColorBrush(new Color(255, pal.Red, pal.Green, pal.Blue));
                    cnv.Children.Add(r);
                    Canvas.SetTop(r, row * cellH);
                    Canvas.SetLeft(r, col * cellW);
                    idx++;
                }
            }
        }
    }
}
