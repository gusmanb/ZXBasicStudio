using System;
using System.Linq;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a trivial memory implementation in which all the addresses are RAM 
    /// and the values written are simply read back. This is the default implementation
    /// of <see cref="IMemory"/>.
    /// </summary>
    public class PlainMemory : IMemory
    {
        private readonly byte[] memory;
        
        public PlainMemory(int size)
        {
            if(size < 1)
                throw new InvalidOperationException("Memory size must be greater than zero");

            memory = new byte[size];
            Size = size;
        }

        public int Size { get; private set; }

        public byte this[int address]
        {
            get
            {
                return memory[address];
            }
            set
            {
                memory[address] = value;
            }
        }

        public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
        {
            if(contents == null)
                throw new ArgumentNullException("contents");

            if (length == null)
                length = contents.Length;
            
            if((startIndex + length) > contents.Length)
                throw new IndexOutOfRangeException("startIndex + length cannot be greater than contents.length");

            if(startIndex < 0)
                throw new IndexOutOfRangeException("startIndex cannot be negative");

            if(startAddress + length > Size)
                throw new IndexOutOfRangeException("startAddress + length cannot go beyond the memory size");

            Array.Copy(
                sourceArray: contents,
                sourceIndex: startIndex,
                destinationArray: memory,
                destinationIndex: startAddress,
                length: length.Value
                );
            }

        public byte[] GetContents(int startAddress, int length)
        {
            if(startAddress >= memory.Length)
                throw new IndexOutOfRangeException("startAddress cannot go beyond memory size");

            if(startAddress + length > memory.Length)
                throw new IndexOutOfRangeException("startAddress + length cannot go beyond memory size");

            if(startAddress < 0)
                throw new IndexOutOfRangeException("startAddress cannot be negative");

            return memory.Skip(startAddress).Take(length).ToArray();
        }
    }
}
