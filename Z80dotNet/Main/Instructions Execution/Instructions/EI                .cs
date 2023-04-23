namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The EI instruction.
        /// </summary>
        byte EI()
        {
            FetchFinished(isEiOrDi: true);

            Registers.IFF1 = 1;
            Registers.IFF2 = 1;

            return 4;
        }
    }
}
