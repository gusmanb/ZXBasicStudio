using System;

namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The JR d instruction.
        /// </summary>
        byte JR_d()
        {
            var offset = ProcessorAgent.FetchNextOpcode();
            FetchFinished();

            Registers.PC = (ushort)(Registers.PC + (SByte)offset);

            return 12;
        }
    }
}
