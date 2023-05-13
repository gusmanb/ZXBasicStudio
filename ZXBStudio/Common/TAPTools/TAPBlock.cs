using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    public class TAPBlock : ITAPBlock
    {
        public TAPHeader Header { get; private set; }
        public TAPData Data { get; private set; }

        private TAPBlock(TAPHeader header, TAPData data)
        {
            Header = header;
            Data = data;
        }

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();

            byte[] headerData = Header.Serialize();
            data.AddRange(BitConverter.GetBytes((ushort)headerData.Length));
            data.AddRange(headerData);

            byte[] blockData = Data.Serialize();
            data.AddRange(BitConverter.GetBytes((ushort)blockData.Length));
            data.AddRange(blockData);

            return data.ToArray();
        }

        public static TAPBlock CreateBasicBlock(string BlockName, byte[] BasicData, ushort? AutoStartLine)
        {
            var header = new TAPHeader
            {
                HeaderType = TAPHeaderType.Program,
                Filename = BlockName,
                DataSize = (ushort)BasicData.Length,
                Param1 = AutoStartLine ?? 65535,
                Param2 = (ushort)BasicData.Length
            };

            var data = new TAPData { Data = BasicData };

            return new TAPBlock(header, data);
        }

        public static TAPBlock CreateScreensBlock(string BlockName, byte[] ScreenData)
        {
            if (ScreenData.Length != 6912)
                throw new ArgumentException("Screen data must be exactly 6912 bytes.");

            var header = new TAPHeader
            {
                HeaderType = TAPHeaderType.Code,
                Filename = BlockName,
                DataSize = 6912,
                Param1 = 16384,
                Param2 = 32768
            };

            var data = new TAPData { Data = ScreenData };

            return new TAPBlock(header, data);
        }

        public static TAPBlock CreateDataBlock(string BlockName, byte[] Data, ushort Address)
        {
            var header = new TAPHeader
            {
                HeaderType = TAPHeaderType.Code,
                Filename = BlockName,
                DataSize = (ushort)Data.Length,
                Param1 = Address,
                Param2 = 32768
            };

            var data = new TAPData { Data = Data };

            return new TAPBlock(header, data);
        }
    }
}
