using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.neg
{
    /// <summary>
    /// Lists the file types handled by ZXGraphics
    /// </summary>
    public enum FileTypes
    {
        /// <summary>
        /// Not defined = None
        /// </summary>
        Undefined = 99,

        /// <summary>
        /// GDU or UDG
        /// </summary>
        UDG = 0,
        /// <summary>
        /// Estándar font
        /// </summary>
        Font = 1,
        /// <summary>
        ///  Sprite format
        /// </summary>
        Sprite = 2,
        /// <summary>
        /// Tiles format
        /// </summary>
        Tile = 3,
        /// <summary>
        /// Maping data
        /// </summary>
        Map = 4,
    }
}
