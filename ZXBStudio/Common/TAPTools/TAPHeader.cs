using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    public class TAPHeader
    {
        public TAPHeaderType HeaderType { get; set; }
        public required string Filename { get; set; }
        public ushort DataSize { get; set; }
        public ushort Param1 { get; set; }
        public ushort Param2 { get; set; }

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

    public enum TAPHeaderType 
    {
        Program,
        NumberArray,
        CharArray,
        Code
    }
}
