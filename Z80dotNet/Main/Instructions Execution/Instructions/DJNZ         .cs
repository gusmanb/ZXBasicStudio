using System;

namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The DJNZ d instruction.
        /// </summary>
        byte DJNZ_d()
        {
            var offset = ProcessorAgent.FetchNextOpcode();
            FetchFinished();

            var oldValue = Registers.B;
            Registers.B = (byte)(oldValue - 1);

            if(oldValue == 1)
                return 8;

            Registers.PC = (ushort)(Registers.PC + (SByte)offset);
            return 13;
        }
    }
}
