using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.EventArgs
{
    public class InstructionWaitStatesEventArgs : System.EventArgs
    {
        public ulong InitialState { get; set; }
        public int ExecutionStates { get; set; }
        public byte[] OpCode { get; set; }
        public ushort[] MemoryAccesses { get; set; }
        public (byte PortHi, byte PortLo)[] PortAccesses { get; set; }
        public int WaitStates { get; set; }
    }
}
