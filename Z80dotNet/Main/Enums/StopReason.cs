namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a reason for the processor not being running.
    /// </summary>
    public enum StopReason
    {
        /// <summary>
        /// The processor is currently running, so no stop reason applies.
        /// </summary>
        NotApplicable,

        /// <summary>
        /// The processor has never been running since it was instantiated.
        /// </summary>
        NeverRan,

        /// <summary>
        /// The processor was running and finished with an invocation of <see cref="IExecutionStopper.Stop"/>
        /// with <c>isPause=false</c>.
        /// </summary>
        StopInvoked,

        /// <summary>
        /// The processor was running and finished with an invocation of <see cref="IExecutionStopper.Stop"/>
        /// with <c>isPause=true</c>.
        /// </summary>
        PauseInvoked,

        /// <summary>
        /// The <see cref="IZ80Processor.ExecuteNextInstruction"/> was invoked and finished normally
        /// (the <see cref="IExecutionStopper.Stop"/> method was not invoked).
        /// </summary>
        ExecuteNextInstructionInvoked,

        /// <summary>
        /// A HALT instruction was encountered when interrupts were disabled,
        /// and <see cref="IZ80Processor.AutoStopOnDiPlusHalt"/> property was set to true.
        /// </summary>
        DiPlusHalt,

        /// <summary>
        /// A RET instruction was encountered, the stack is empty, and the
        /// <see cref="IZ80Processor.AutoStopOnRetWithStackEmpty"/> property was set to true.
        /// </summary>
        RetWithStackEmpty,

        /// <summary>
        /// An exception was thrown and not handled during he execution of the
        /// <see cref="IZ80Processor.Start"/> method, the <see cref="IZ80Processor.Continue"/> method
        /// or the <see cref="IZ80Processor.ExecuteNextInstruction"/> method.
        /// </summary>
        ExceptionThrown
    }
}