using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.SupportClasses
{
    public class TAPFile
    {
        public static Tape Load(string FileName)
        {
            byte[] data = File.ReadAllBytes(FileName);

            List<TapeBlock> blocks = new List<TapeBlock>();

            int pos = 0;

            while (pos < data.Length)
            {
                int len = Word(data, pos);
                pos += 2;

                TapeBlock block = new TapeBlock(blocks.LastOrDefault()?.Stream?.LastState ?? false, data, pos, len);
                pos += len;

                blocks.Add(block);
            }
            return new Tape(blocks.ToArray());

        }

        private static ushort Word(byte[] data, int offset)
        {
            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }
    }
}
