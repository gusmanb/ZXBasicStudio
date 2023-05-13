using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    public class TAPFile
    {
        List<ITAPBlock> _blocks = new List<ITAPBlock>();
        public List<ITAPBlock> Blocks => _blocks;

        public byte[] Serialize()
        {
            if (_blocks.Count == 0)
                throw new InvalidOperationException("Cannot serialize a tap file with no blocks");

            List<byte> tapeData = new List<byte>();

            foreach (var block in _blocks)
                tapeData.AddRange(block.Serialize());

            return tapeData.ToArray();
        }
    }
}
