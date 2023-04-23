namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The EX DE,HL instruction
        /// </summary>
        byte EX_DE_HL()
        {
            FetchFinished();

            var temp = Registers.DE;
            Registers.DE = Registers.HL;
            Registers.HL = temp;

            return 4;
        }
    }
}
