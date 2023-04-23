namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The LD (nn),A instruction.
        /// </summary>
        private byte LD_aa_A()
        {
            var address = (ushort)FetchWord();
            FetchFinished();

            ProcessorAgent.WriteToMemory(address, Registers.A);

            return 13;
        }
    }
}
