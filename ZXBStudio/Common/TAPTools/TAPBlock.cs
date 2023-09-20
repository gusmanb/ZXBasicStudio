using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.TAPTools
{
    /// <summary>
    /// Standard tape block
    /// </summary>
    public class TAPBlock : ITAPBlock
    {
        /// <summary>
        /// Tape header describing the content of the block
        /// </summary>
        public TAPHeader Header { get; private set; }
        /// <summary>
        /// Binary data of the block
        /// </summary>
        public TAPData Data { get; private set; }

        private TAPBlock(TAPHeader header, TAPData data)
        {
            Header = header;
            Data = data;
        }
        /// <summary>
        /// Serialize the block as a byte array
        /// </summary>
        /// <returns>The block as a byte array</returns>
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

        /// <summary>
        /// Create a block that contains a basic program
        /// </summary>
        /// <param name="BlockName">Name of the block</param>
        /// <param name="BasicData">Basic data in binary form</param>
        /// <param name="AutoStartLine">Line number to autostart the program</param>
        /// <returns>A new tape block</returns>
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

        /// <summary>
        /// Create a block that contains SCREEN data
        /// </summary>
        /// <param name="BlockName">Name of the block</param>
        /// <param name="ScreenData">Screen data in binary form</param>
        /// <returns>A new tape block</returns>
        /// <exception cref="ArgumentException">Screen data must be exactly 6912 bytes</exception>
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

        /// <summary>
        /// Create a block of CODE data
        /// </summary>
        /// <param name="BlockName">Name of the block</param>
        /// <param name="Data">Code data in binary form</param>
        /// <param name="Address">Address of the block</param>
        /// <returns>A new tape block</returns>
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
