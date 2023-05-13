using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    public class TAPRawBlock : ITAPBlock
    {
        byte[] _data;
        public TAPRawBlock(byte[] Data) 
        {
            _data = Data;
        }

        public byte[] Serialize()
        {
            return _data;
        }
    }
}
