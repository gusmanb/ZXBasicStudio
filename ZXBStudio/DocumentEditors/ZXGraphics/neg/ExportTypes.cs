using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.neg
{
    /// <summary>
    /// Defines the pòssible export types for ZXGraphics
    /// </summary>
    public enum ExportTypes
    {
        None,

        // Fonts and UDGs/GDUs
        Bin,
        Tap,
        Asm,
        Dim,
        Data,

        // Sprites
        PutChars,
        GUSprite,
        FourSprites
    }
}
