using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.neg
{
    /// <summary>
    /// Export configuration settings
    /// </summary>
    public class ExportConfig
    {
        /// <summary>
        /// Default export type
        /// </summary>
        public ExportTypes ExportType { get; set; }
        /// <summary>
        /// Generate export on every Pre-Build
        /// </summary>
        public bool AutoExport { get; set; }
        /// <summary>
        /// Exported filename
        /// </summary>
        public string ExportFilePath { get; set; }
        /// <summary>
        /// Label name
        /// </summary>
        public string LabelName { get; set; }
        /// <summary>
        /// Filename for the data block inside .tap file
        /// </summary>
        public string ZXFileName { get; set; }
        /// <summary>
        /// Memory address in the ZX Spectrum
        /// </summary>
        public int ZXAddress { get; set; }
        /// <summary>
        /// Array base for DIM export
        /// 0=0, 1=1, 2=From project settings
        /// </summary>
        public int ArrayBase { get; set; }
    }
}
