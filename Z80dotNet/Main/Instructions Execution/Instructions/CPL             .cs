namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The CPL instruction.
        /// </summary>
        byte CPL()
        {
            FetchFinished();

            Registers.A = (byte)(Registers.A ^ 0xFF);

            Registers.HF = 1;
            Registers.NF = 1;
            SetFlags3and5From(Registers.A);

            return 4;
        }
    }
}
