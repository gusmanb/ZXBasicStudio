using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.BuildSystem
{
    public class ZXBasicLocation
    {
        public required string File { get; set; }
        public ZXBasicLocationType LocationType { get; set; }
        public required string Name { get; set; }
        public int FirstLine { get; set; }
        public int LastLine { get; set; }
    }

    public enum ZXBasicLocationType
    {
        Sub,
        Function
    }
}
