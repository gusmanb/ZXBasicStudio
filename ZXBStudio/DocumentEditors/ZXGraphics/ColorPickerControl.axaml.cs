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


        /// <summary>
        /// Constructor
        /// </summary>
        public ColorPickerControl()
        {
            InitializeComponent();
        }


        public bool Inicialize(GraphicsModes graphicMode, PaletteColor[] palette, int primaryColor, int secondaryColor, bool bright, bool flash)
        {
            this.graphicMode = graphicMode;

            grdMonochrome.IsVisible = true;
            grdZXSpectrum.IsVisible = true;
            grdNext.IsVisible = true;
            switch (graphicMode)
            {
                case GraphicsModes.Monochrome:
                    grdMonochrome.IsVisible = true;
                    break;
                case GraphicsModes.ZXSpectrum:
                    grdZXSpectrum.IsVisible = true;
                    break;
                case GraphicsModes.Next:
                    grdNext.IsVisible = true;
                    break;

            }

            return true;
        }
    }
}
