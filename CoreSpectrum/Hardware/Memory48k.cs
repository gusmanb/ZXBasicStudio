﻿using CoreSpectrum.Interfaces;
using Konamiman.Z80dotNet;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class Memory48k : ISpectrumMemory
    {
        byte[] memory = new byte[64 * 1024];

        public byte this[int address]
        {
            get => memory[address];
            set 
            { 
                if (address < 16384) 
                    return;

                memory[address] = value; 
            }
        }

        public int Size
        {
            get => memory.Length;
        }

        public Memory48k(byte[][] RomSet)
        {
            if (RomSet == null || RomSet.Length != 1 || RomSet[0].Length != 16 * 1024)
                throw new InvalidDataException("Spectrum 48k ROM set must contain a single 16Kb ROM");

            Buffer.BlockCopy(RomSet[0], 0, memory, 0, RomSet[0].Length);
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

        public void SetUshort(int startAddress, ushort value)
        {
            SetContents(startAddress, BitConverter.GetBytes(value));
        }

        public ushort GetUshort(int startAddress)
        {
            return BitConverter.ToUInt16(GetContents(startAddress, 2));
        }

        public void SetByte(int startAddress, byte value)
        {
            SetContents(startAddress, new byte[] { value });
        }

        public byte GetByte(int startAddress)
        {
            return GetContents(startAddress, 1)[0];
        }


        public Span<byte> GetVideoMemory()
        {
            return new Span<byte>(memory, 0x4000, 6912);
        }

        public void ClearRAM()
        {
            Array.Fill<byte>(memory, 0x00, 16384, 48 * 1024);
        }
    }
}
