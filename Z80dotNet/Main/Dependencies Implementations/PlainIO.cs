using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konamiman.Z80dotNet
{
    public class PlainIO : IIO
    {
        public byte this[byte port, byte upperPart]
        {
            get
            {
                return 0;
            }

            set
            {
                
            }
        }
    }
}
