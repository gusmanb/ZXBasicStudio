using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    /// <summary>
    /// Creates a .tap file binary data
    /// </summary>
    public class TAPFile
    {
        List<ITAPBlock> _blocks = new List<ITAPBlock>();

        /// <summary>
        /// List of blocks to include in the tape
        /// </summary>
        public List<ITAPBlock> Blocks => _blocks;

        /// <summary>
        /// Serializes the tape to binary
        /// </summary>
        /// <returns>Teh tap file as binary data</returns>
        /// <exception cref="InvalidOperationException">Cannot serialize a tap file with no blocks</exception>
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
