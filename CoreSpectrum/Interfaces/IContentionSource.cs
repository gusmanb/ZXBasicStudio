using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Interfaces
{
    internal interface IContentionSource
    {
        int GetContentionStates(ulong InitialState, int ExecutionStates, byte[] OpCode, ushort[] MemoryAccesses, (byte PortHi, byte PortLo)[] PortAccesses, IMemory Memory);
    }
}
