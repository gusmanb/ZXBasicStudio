using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konamiman.Z80dotNet
{
    public interface IIO
    {
        byte this[byte portLo, byte portHi] { get; set; }
    }
}
