using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.neg
{
    /// <summary>
    /// Defines a pattern for char, gdu, sprite or tile
    /// </summary>
    public class Pattern
    {
        /// <summary>
        /// Index of the pattern
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Label for number to show (Usually the Id, except for Fonts that is Id+32)
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Name to Show
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Point data, usually 1x8 in classic mode
        /// </summary>
        public PointData[] Data { get; set; }
    }
}
