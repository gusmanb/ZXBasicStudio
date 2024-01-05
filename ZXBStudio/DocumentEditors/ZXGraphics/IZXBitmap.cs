using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics
{
    public interface IZXBitmap : IImage
    {
        public PixelSize PixelSize { get; }
    }
}
