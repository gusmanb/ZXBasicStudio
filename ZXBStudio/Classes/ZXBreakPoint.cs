using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXBreakPoint
    {
        public ZXBreakPoint(string File, int Line) 
        {
            this.File = File;
            this.Line = Line;
        }
        public string File { get; set; }
        public int Line { get; set; }
    }
}
