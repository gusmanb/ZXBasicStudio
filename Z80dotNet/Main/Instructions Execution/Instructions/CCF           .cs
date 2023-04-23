namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The CCF instruction.
        /// </summary>
        byte CCF()
        {
            FetchFinished();

            var oldCF = Registers.CF;
            Registers.NF = 0;
            Registers.HF = oldCF;
            Registers.CF = !oldCF;
            SetFlags3and5From(Registers.A);

            return 4;
        }
    }
}
