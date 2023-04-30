using System.Collections.Generic;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Internal class used to keep track of the current instruction execution.
    /// </summary>
    public class InstructionExecutionContext
    {
        public InstructionExecutionContext()
        {
            StopReason = StopReason.NotApplicable;
            OpcodeBytes = new List<byte>();
            MemoryAccesses = new List<ushort>();
            PortAccesses = new List<(byte PortHi, byte PortLo)>();
        }

        public StopReason StopReason
        {
            get;
            set;
        }

        public bool MustStop
        {
            get
            {
                return StopReason != StopReason.NotApplicable;
            }
        }

        public void StartNewInstruction()
        {
            OpcodeBytes.Clear();
            MemoryAccesses.Clear();
            PortAccesses.Clear();
            FetchComplete = false;
            PeekedOpcode = null;
            IsEiOrDiInstruction = false;
            StopReason = StopReason.NotApplicable;
            FetchCycles = 0;
        }

        public bool ExecutingBeforeInstructionEvent
        {
            get; 
            set;
        }

        public bool FetchComplete
        {
            get;
            set;
        }

        public List<byte> OpcodeBytes
        {
            get;
            set;
        }

        public bool IsRetInstruction
        {
            get;
            set;
        }

        public bool IsLdSpInstruction
        {
            get;
            set;
        }

        public bool IsHaltInstruction
        {
            get;
            set;
        }

        public bool IsEiOrDiInstruction
        {
            get;
            set;
        }

        public short SpAfterInstructionFetch
        {
            get;
            set;
        }

        public byte? PeekedOpcode
        {
            get; 
            set;
        }

        public ushort AddressOfPeekedOpcode
        {
            get;
            set;
        }

        public int FetchCycles
        {
            get; 
            set;
        }

        public List<ushort> MemoryAccesses
        {
            get; 
            set;
        }

        public List<(byte PortHi, byte PortLo)> PortAccesses
        {
            get;
            set;
        }
    }
}
