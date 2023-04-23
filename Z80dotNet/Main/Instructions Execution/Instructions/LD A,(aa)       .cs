namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The LD A,(nn) instruction.
        /// </summary>
        private byte LD_A_aa()
        {
            var address = (ushort)FetchWord();
            FetchFinished();

            Registers.A = ProcessorAgent.ReadFromMemory(address);

            return 13;
        }
    }
}
