namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The NOP2 instruction (equivalent to two NOPs, used for unsupported instructions).
        /// </summary>
        byte NOP2()
        {
            FetchFinished();
            return 8;
        }
    }
}
