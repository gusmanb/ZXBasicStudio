namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a class that can pause the current thread for a specific amount of time,
    /// based on a clock frequency and a number of cycles elapsed.
    /// </summary>
    public interface IExecutionStopper
    {
        /// <summary>
        /// Stops the processor execution, causing the <see cref="IZ80Processor.Start"/> method to return.
        /// </summary>
        /// <param name="isPause">If <b>true</b>, the <see cref="IZ80Processor.StopReason"/> property of the
        /// processor classs will return <see cref="Konamiman.Z80dotNet.StopReason.PauseInvoked"/> after the method returns.
        /// If <b>false</b>, it will return <see cref="Konamiman.Z80dotNet.StopReason.StopInvoked"/>.</param>
        void Stop(bool isPause = false);
    }
}