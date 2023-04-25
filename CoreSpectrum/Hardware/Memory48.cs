using Konamiman.Z80dotNet;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class Memory48 : IMemory
    {
        byte[] memory = new byte[64 * 1024];

        public Memory48(byte[] RomContent)
        {

            if (RomContent == null || RomContent.Length != 16 * 1024)
                throw new InvalidDataException("ROM must be 16kb long");

            Buffer.BlockCopy(RomContent, 0, memory, 0, RomContent.Length);
        }

        public byte this[int address]
        {
            get => memory[address];
            set => memory[address] = value;
        }

        public int Size
        {
            get => memory.Length;
        }

        public byte[] GetContents(int startAddress, int length)
        {
            byte[] vs = new byte[length];
            Buffer.BlockCopy(memory, startAddress, vs, 0, length);
            return vs;
        }

        public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
        {
            Buffer.BlockCopy(contents, startIndex, memory, startAddress, length ?? contents.Length);
        }
    }
}
