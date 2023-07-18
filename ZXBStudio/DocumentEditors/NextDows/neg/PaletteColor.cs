using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.NextDows.neg
{
    /// <summary>
    /// Represents a color 
    /// </summary>
    public class PaletteColor
    {
        /// <summary>
        /// Index of the color into palette
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Red level
        /// </summary>
        public byte Red { get; set; }
        /// <summary>
        /// Green level
        /// </summary>
        public byte Green { get; set; }
        /// <summary>
        /// Blue level
        /// </summary>
        public byte Blue { get; set; }
        /// <summary>
        /// IBrush of the color
        /// </summary>
        public IBrush Brush { get; set; }
    }
}
