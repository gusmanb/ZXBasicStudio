namespace Konamiman.Z80dotNet
{
    public partial class Z80InstructionExecutor
    {
        /// <summary>
        /// The IN A,(n) instruction.
        /// </summary>
        byte IN_A_n()
        {
            var portNumber = ProcessorAgent.FetchNextOpcode();
            FetchFinished();

            Registers.A = ProcessorAgent.ReadFromPort(portNumber, Registers.A);
            return 11;
        }
    }
}
