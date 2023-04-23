using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.SupportClasses
{
    public class Tape
    {
        List<TimedTapeBlock> _blocks = new List<TimedTapeBlock>();
        public TimedTapeBlock[] Blocks { get { return _blocks.ToArray(); } }
        public ulong Length { get; private set; }
        public Tape(TapeBlock[] Blocks)
        {
            foreach (var block in Blocks)
            {
                var len = block.Length;

                _blocks.Add(new TimedTapeBlock { Start = Length, Length = len, Block = block });
                Length += len;
            }

        }

        public bool GetValue(ulong TStates)
        {
            var block = _blocks.FirstOrDefault(b => b.Start <= TStates && TStates < b.Start + b.Length);

            if (block == null)
                return false;

            TStates -= block.Start;
            return block.Block.Stream.GetValue(TStates);
        }

        public int GetBlockIndex(ulong TStates)
        {
            for (int buc = 0; buc < _blocks.Count; buc++)
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
            public TapeBlock Block { get; set; }
        }
    }
}
