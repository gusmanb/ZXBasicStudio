using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Emulator.Classes
{
    public class ZXBinaryBank
    {
        public ZXMemoryBank Bank { get; set; }
        public required byte[] Data { get; set; }
    }
    public enum ZXMemoryBank
    {
        Bank1 = 1,
        Bank2 = 2,
        Bank3 = 3,
        Bank4 = 4,
        Bank5 = 5,
        Bank6 = 6,
        Bank7 = 7
    }

}
