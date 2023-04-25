using Konamiman.Z80dotNet;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class Memory128 : IMemory
    {

        const int MEMORY_TYPE_ROM = 0;
        const int MEMORY_TYPE_RAM = 1;

        byte[][] memoryPages = new byte[][] 
        {
            new byte[16 * 1024],
            new byte[16 * 1024],
            new byte[16 * 1024],
            new byte[16 * 1024],
            new byte[16 * 1024],
            new byte[16 * 1024],
            new byte[16 * 1024],
            new byte[16 * 1024]
        };

        byte[][] romPages = new byte[][] 
        {
            new byte[16 * 1024],
            new byte[16 * 1024]
        };

        byte[][][] compositeMemory;

        Memory128Map map = new Memory128Map();

        public Memory128Map Map { get { return map; } }

        public Memory128(byte[] Rom0, byte[] Rom1)
        {
            if (Rom0 == null || Rom0.Length != 16 * 1024)
                throw new InvalidDataException("ROM must be 16kb long");

            if (Rom1 == null || Rom1.Length != 16 * 1024)
                throw new InvalidDataException("ROM must be 16kb long");

            Buffer.BlockCopy(Rom0, 0, romPages[0], 0, Rom0.Length);
            Buffer.BlockCopy(Rom1, 0, romPages[1], 0, Rom1.Length);

            compositeMemory = new byte[][][]
            {
                romPages,
                memoryPages
            };
        }

        public byte this[int address]
        {
            get
            {
                MemoryIndex idx;
                Map.GetMemoryIndex(address, out idx);
                return compositeMemory[idx.MemoryType][idx.Segment][idx.Offset];
            }

            set
            {
                MemoryIndex idx;
                Map.GetMemoryIndex(address, out idx);
                compositeMemory[idx.MemoryType][idx.Segment][idx.Offset] = value;
            }
        }

        public int Size
        {
            get
            {
                return 64 * 1024;
            }
        }

        public byte[] GetContents(int startAddress, int length)
        {
            byte[] data = new byte[length];

            var ranges = map.GetMemoryRanges(startAddress, length);

            for (int buc = 0; buc < ranges.Length; buc++)
            {
                IndexedMemoryRange range = ranges[buc];
                Buffer.BlockCopy(compositeMemory[range.Index.MemoryType][range.Index.Segment],
                    range.Index.Offset,
                    data,
                    range.SourceAddress - startAddress,
                    range.Length);
            }

            return data;
        }

        public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
        {
            int realLength;

            if (length != null)
                realLength = length.Value;
            else if (startIndex == 0)
                realLength = contents.Length;
            else
                realLength = contents.Length - startIndex;

            var ranges = map.GetMemoryRanges(startAddress, realLength);

            for(int buc = 0; buc< ranges.Length; buc++) 
            {
                IndexedMemoryRange range = ranges[buc];
                Buffer.BlockCopy(contents, 
                    startIndex + (range.SourceAddress - startAddress),
                    compositeMemory[range.Index.MemoryType][range.Index.Segment],
                    range.Index.Offset,
                    range.Length);
            }
        }

        public class Memory128Map
        {

            const int fixedBankIndex = 2;
            const int fixedScreenIndex = 5;
            const int altScreenIndex = 7;

            int activeRom = 0;
            int activeBank = 0;
            int activeScreen = 0;

            MemoryRange romRange = new MemoryRange(0, 0x3FFF);
            MemoryRange screenRange = new MemoryRange(0x4000, 0x7FFF);
            MemoryRange fixedRamRange = new MemoryRange(0x8000, 0xBFFF);
            MemoryRange mappedRamRange = new MemoryRange(0xC000, 0xFFFF);

            public int ActiveROM { get { return activeRom; } }
            public int ActiveBank { get { return activeBank;} }
            public int ActiveScreen { get { return activeScreen;} }

            public void SetActiveRom(int RomNumber)
            {
                if (RomNumber < 0 || RomNumber > 1)
                    throw new IndexOutOfRangeException("Active rom can be 1 or 0");

                activeRom = RomNumber;

            }

            public void SetActiveBank(int BankNumber)
            {
                if (BankNumber < 0 || BankNumber > 1)
                    throw new IndexOutOfRangeException("Active bank can range from 0 to 7");

                activeBank = BankNumber;
            }

            public void SetActiveScreen(int ScreenNumber) 
            {
                if (ScreenNumber < 0 || ScreenNumber > 1)
                    throw new IndexOutOfRangeException("Active screen can be 1 or 0");

                activeScreen = ScreenNumber;
            }

            internal void GetMemoryIndex(int Address, out MemoryIndex Index)
            {
                if (romRange.Contains(Address))
                {
                    Index.MemoryType = MEMORY_TYPE_ROM;
                    Index.Segment = activeRom;
                    Index.Offset = Address;

                }
                else if (screenRange.Contains(Address))
                {
                    Index.MemoryType = MEMORY_TYPE_RAM;
                    Index.Segment = fixedScreenIndex;
                    Index.Offset = Address - screenRange.Start;
                }
                else if (fixedRamRange.Contains(Address))
                {
                    Index.MemoryType = MEMORY_TYPE_RAM;
                    Index.Segment = fixedBankIndex;
                    Index.Offset = Address - fixedRamRange.Start;
                }
                else if (mappedRamRange.Contains(Address))
                {
                    Index.MemoryType = MEMORY_TYPE_RAM;
                    Index.Segment = activeBank;
                    Index.Offset = Address - mappedRamRange.Start;
                }
                else
                    throw new IndexOutOfRangeException("Address is out of range");
            }

            internal void GetScreenIndex(int Address, out MemoryIndex Index) 
            {
                if(!screenRange.Contains(Address))
                    throw new IndexOutOfRangeException("Address is out of range");

                Index.MemoryType = MEMORY_TYPE_RAM;
                Index.Segment = activeScreen == 0 ? fixedScreenIndex : altScreenIndex;
                Index.Offset = Address - screenRange.Start;
            }

            internal IndexedMemoryRange[] GetMemoryRanges(int StartAddress, int Length)
            {
                List<IndexedMemoryRange> ranges = new List<IndexedMemoryRange>();

                while (Length > 0)
                {
                    IndexedMemoryRange range = new IndexedMemoryRange();
                    GetMemoryIndex(StartAddress, out range.Index);
                    range.SourceAddress = StartAddress;
                    int availableInRange = 16 * 1024 - range.Index.Offset;
                    if (availableInRange >= Length)
                    {
                        range.Length = Length;
                        Length = 0;
                    }
                    else
                    {
                        range.Length = availableInRange;
                        Length -= availableInRange;
                        StartAddress += availableInRange;
                    }

                    ranges.Add(range);
                }

                return ranges.ToArray();
            }
        }

        internal struct MemoryIndex
        {
            public int MemoryType;
            public int Segment;
            public int Offset;
        }

        internal struct IndexedMemoryRange
        {
            public MemoryIndex Index;
            public int SourceAddress;
            public int Length;
        }

        struct MemoryRange
        {
            public int Start;
            public int End;

            public MemoryRange(int Start, int End) 
            {
                this.Start = Start;
                this.End = End;
            }

            public bool Contains(int Address) => Address >= Start && Address <= End;

        }
    }
}
