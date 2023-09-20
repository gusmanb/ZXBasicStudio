using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    /// <summary>
    /// RAW tape block, used to include already serialized tape blocks
    /// </summary>
    public class TAPRawBlock : ITAPBlock
    {
        byte[] _data;
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="Data">Data of the block</param>
        public TAPRawBlock(byte[] Data) 
        {
            _data = Data;
        }

        /// <summary>
        /// Serialize the data
        /// </summary>
        /// <returns>The binary data</returns>
        public byte[] Serialize()
        {
            return _data;
        }
    }
}
