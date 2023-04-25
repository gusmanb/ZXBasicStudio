using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXGraphics.neg
{
    /// <summary>
    /// Defines a file configuration
    /// </summary>
    public class FileTypeConfig
    {
        /// <summary>
        /// Type of the file
        /// </summary>
        public FileTypes FileType { get; set; }
        /// <summary>
        /// Filename with path
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Number of patterns in the file
        /// A patter is a graphic of 1 x 8 (8x8) for classic or (16x16) in Next
        /// </summary>
        public int NumerOfPatterns { get; set; }
        /// <summary>
        /// Index of the first item. 65 for GDUs, 32 for Foonts, 0 for others
        /// </summary>
        public int FirstIndex { get; set; }

    }
}
