using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXCodeLine
    {
        public ZXCodeLine(ZXFileType FileType, string File, int LineNumber, ushort Address) 
        {
            this.FileType = FileType;
            this.File = File;
            this.LineNumber = LineNumber;
            this.Address = Address;
        }
        public string File { get; set; }
        public ZXFileType FileType { get; set; }
        public int LineNumber { get; set; }
        public ushort Address { get; set; }
    }

    public enum ZXFileType
    {
        Basic,
        Assembler
    }
}
