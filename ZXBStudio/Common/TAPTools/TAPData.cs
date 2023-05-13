using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    public class TAPData
    {
        public required byte[] Data { get; set; }

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
