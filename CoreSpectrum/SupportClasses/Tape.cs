using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.SupportClasses
{
    public class Tape
    {
        TimedTapeBlock[] _blocks;
        public TimedTapeBlock[] Blocks { get { return _blocks.ToArray(); } }
        TimedTapeBlock? currentBlock;
        int currentBlockIndex = 0;
        public ulong Length { get; private set; }
        public Tape(TapeBlock[] Blocks)
        {

            if (Blocks == null || Blocks.Length < 1)
                throw new ArgumentException("At least one block must be provided.");

            _blocks = new TimedTapeBlock[Blocks.Length];

            for (int buc = 0; buc < _blocks.Length; buc++)
            {
                var cb = Blocks[buc];
                var len = cb.Length;
                _blocks[buc] = new TimedTapeBlock { Start = Length, Length = len, Block = cb };
                Length += len;
            }

        }

        public bool GetValue(ulong TStates)
        {
            if (currentBlock == null || currentBlock.Start > TStates)
            {
                currentBlock = _blocks[0];
                currentBlockIndex = 0;
            }

            while (currentBlockIndex < _blocks.Length && !(currentBlock.Start <= TStates && TStates < currentBlock.Start + currentBlock.Length))
            {
                currentBlockIndex += 1;
                if (currentBlockIndex >= _blocks.Length)
                    currentBlock = null;
                else
                    currentBlock = _blocks[currentBlockIndex];
            }

            if (currentBlock == null)
                return false;

            if (currentBlock.Block.Stream == null)
                return false;

            TStates -= currentBlock.Start;
            return currentBlock.Block.Stream.GetValue(TStates);
        }

        public int GetBlockIndex(ulong TStates)
        {
            for (int buc = 0; buc < _blocks.Length; buc++)
            {
                var block = _blocks[buc];
                if (block.Start <= TStates && TStates < block.Start + block.Length)
                    return buc;
            }

            return -1;
        }

        public class TimedTapeBlock
        {
            public ulong Start { get; set; }
            public ulong Length { get; set; }
            public required TapeBlock Block { get; set; }
        }
    }
}
