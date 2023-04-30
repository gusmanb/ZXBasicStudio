using CoreSpectrum.Interfaces;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    /// <summary>
    /// This is a very simple contention source, it's very fast but not accurate, it slows down contended memory and ports always
    /// for the max possible value. Useful for inaccurate emulation but low resource usage
    /// </summary>
    public class SimpleContentionSource : IContentionSource
    {
        bool spectrum128k;

        public SimpleContentionSource(bool Spectrum128k)
        {
            spectrum128k = Spectrum128k;
        }


        public int GetContentionStates(ulong InitialState, int ExecutionStates, byte[] OpCode, ushort[] MemoryAccesses, (byte PortHi, byte PortLo)[] PortAccesses, IMemory Memory)
        {

            int states = 0;

            for (int i = 0; i < MemoryAccesses.Length; i++)
                states += GetMemoryContention(MemoryAccesses[i], Memory);

            for(int i = 0; i < PortAccesses.Length; i++)
                states += GetPortContention(PortAccesses[i]);

            return states;
        }

        private int GetMemoryContention(ushort Address, IMemory Memory)
        {
            if(spectrum128k) 
            {
                var mem = Memory as Memory128k;

                //Contention happens on low memory accesses and odd paged memory banks
                if ((Address > 16383 && Address < 32768) || (Address >= 0xC000 && (mem != null && (mem.Map.ActiveBank & 1) == 1))) 
                    return 7;
            }
            else 
            {
                //Contention happens on low memory
                if (Address > 16383 && Address < 32768)
                    return 6; 
            }

            return 0;
        }

        private int GetPortContention((byte PortHi, byte PortLo) Port)
        {
            //Four patterns, each one contends the execution different ammount of cycles.
            bool ulaHandled = (Port.PortLo & 1) == 0;
            bool ulaContended = Port.PortHi >= 0x40 && Port.PortHi <= 0x7F;

            if (!ulaHandled && !ulaContended)
                return 0;
            else if (ulaHandled && !ulaContended)
                return 6;
            else if (ulaHandled && ulaContended)
                return 12;
            else //!ulaHandled && ulaContended
                return 24;
            
        }

    }
}
