using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    /// <summary>
    /// Class to store a zx spectrum block header
    /// </summary>
    public class TAPHeader
    {
        /// <summary>
        /// Type of header
        /// </summary>
        public TAPHeaderType HeaderType { get; set; }
        /// <summary>
        /// FileName of the block
        /// </summary>
        public required string Filename { get; set; }
        /// <summary>
        /// Block's data size
        /// </summary>
        public ushort DataSize { get; set; }
        /// <summary>
        /// Parameter 1 of the header
        /// </summary>
        public ushort Param1 { get; set; }
        /// <summary>
        /// Parameter 2 of the header
        /// </summary>
        public ushort Param2 { get; set; }

        /// <summary>
        /// Serializes the header as binary data
        /// </summary>
        /// <returns>The header serialized in binary</returns>
        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.Add(0);
            data.Add((byte)HeaderType);
            data.AddRange(Encoding.ASCII.GetBytes(Filename.Substring(0, Math.Min(10, Filename.Length)).PadRight(10, ' ')));
            data.AddRange(BitConverter.GetBytes(DataSize));
            data.AddRange(BitConverter.GetBytes(Param1));
            data.AddRange(BitConverter.GetBytes(Param2));

            byte xSum = 0;

            for(int buc = 0; buc < data.Count; buc++)
                xSum ^= data[buc];

            data.Add(xSum);

            return data.ToArray();
        }
    }

    /// <summary>
    /// Type of tape headers
    /// </summary>
    public enum TAPHeaderType 
    {
        /// <summary>
        /// Basic progra,
        /// </summary>
        Program,
        /// <summary>
        /// Basic number array
        /// </summary>
        NumberArray,
        /// <summary>
        /// Basic char array
        /// </summary>
        CharArray,
        /// <summary>
        /// Code (can be also screen data)
        /// </summary>
        Code
    }
}
