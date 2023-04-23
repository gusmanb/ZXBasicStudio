namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Event triggered by the <see cref="IZ80Processor"/> class before an instruction is fetched.
    /// </summary>
    public class BeforeInstructionFetchEventArgs : ProcessorEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="stopper">An instance of <see cref="IExecutionStopper"/> that can be used
        /// by the event listener to request stop of the execution loop.</param>
        public BeforeInstructionFetchEventArgs(IExecutionStopper stopper)
        {
            this.ExecutionStopper = stopper;
        }

        /// <summary>
        /// Contains the instance of <see cref="IExecutionStopper"/> that allows the event consumer
        /// to ask termination of the processor execution.
        /// </summary>
        public IExecutionStopper ExecutionStopper { get; private set; }
    }
}