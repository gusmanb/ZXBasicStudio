using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    /// <summary>
    /// Class to store/serialize tape data
    /// </summary>
    public class TAPData
    {
        /// <summary>
        /// Data to serialize
        /// </summary>
        public required byte[] Data { get; set; }

        /// <summary>
        /// Serializes the data in binary form
        /// </summary>
        /// <returns>The serialized data</returns>
        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.Add(0xFF);
            data.AddRange(Data);

            byte xSum = 0;

            for (int buc = 0; buc < data.Count; buc++)
                xSum ^= data[buc];

            data.Add(xSum);

            return data.ToArray();
        }
    }
}
