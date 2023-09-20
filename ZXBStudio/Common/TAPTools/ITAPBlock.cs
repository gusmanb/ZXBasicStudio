using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    /// <summary>
    /// Interface that must be implemented by any kind of tape block
    /// </summary>
    public interface ITAPBlock
    {
        /// <summary>
        /// Serializes the block as a byte array
        /// </summary>
        /// <returns>The block as a byte array</returns>
        byte[] Serialize();
    }
}
