namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The EXX instruction.
        /// </summary>
        byte EXX()
        {
            FetchFinished();

            var tempBC = Registers.BC;
            var tempDE = Registers.DE;
            var tempHL = Registers.HL;

            Registers.BC = Registers.Alternate.BC;
            Registers.DE = Registers.Alternate.DE;
            Registers.HL = Registers.Alternate.HL;

            Registers.Alternate.BC = tempBC;
            Registers.Alternate.DE = tempDE;
            Registers.Alternate.HL = tempHL;

            return 4;
        }
    }
}
