using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DebuggingTools.Memory.Classes
{
    public class ZXMemoryRange
    {
        public required int StartAddress { get; set; }
        public required int EndAddress { get; set; }

        public bool Contains(int Address) => Address >= StartAddress && Address <= EndAddress;
    }
}
