using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Debug
{
    public class Breakpoint
    {
        public string? Id { get; set; }
        public object? Tag { get; set; }
        public ushort Address { get; set; }
        public bool Temporary { get; set; }
        public bool Executed { get; set; }
    }
}
