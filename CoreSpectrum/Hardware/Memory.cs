using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class Memory : IMemory
    {
        public byte[] MemoryArray { get { return memory; } }
        internal Memory(int Size, byte[] RomContent)
        {
            memory = new byte[Size];
            readTrigger = new bool[Size];
            writeTrigger = new bool[Size];

            if (RomContent != null && RomContent.Length > 0)
                SetContents(0, RomContent);
        }

        byte[] memory;
        bool[] readTrigger;
        bool[] writeTrigger;

        public event EventHandler<MemoryEventEventArgs>? MemoryEvent;

        public void MonitorMemoryRead(ushort Address) 
        {
            readTrigger[Address] = true;
        }
        public void MonitorMemoryWrite(ushort Address)
        {
            writeTrigger[Address] = true;
        }
        public void IgnoreMemoryRead(ushort Address)
        {
            readTrigger[Address] = false;
        }
        public void IgnoreMemoryWrite(ushort Address)
        {
            writeTrigger[Address] = false;
        }

        public byte this[int address]
        {
            get
            {

                if (readTrigger[address])
                {
                    if (MemoryEvent != null)
                    {
                        var args = new MemoryEventEventArgs { ValueToRead = memory[address], Address = (ushort)address };
                        MemoryEvent(this, args);
                        return args.ValueToRead;
                    }
                }

                return memory[address];
            }

            set
            {

                if (writeTrigger[address])
                {
                    if (MemoryEvent != null)
                    {
                        var args = new MemoryEventEventArgs { Write = true, ValueToWrite = value, Address = (ushort)address };
                        MemoryEvent(this, args);
                        memory[address] = args.ValueToWrite;
                        return;
                    }
                }

                memory[address] = value;
            }
        }

        public int Size
        {
            get
            {
                return memory.Length;
            }
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

    public class MemoryEventEventArgs : EventArgs
    {
        public bool Write { get; set; }
        public ushort Address { get; set; }
        public byte ValueToRead { get; set; }
        public byte ValueToWrite { get; set; }
    }
}
