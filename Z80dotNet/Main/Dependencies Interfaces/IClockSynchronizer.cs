
namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a class that helps on synchronizing with a simulated clock
    /// by introducing delays that last for a specified amount of clock cycles.
    /// </summary>
    public interface IClockSynchronizer
    {
        /// <summary>
        /// Gets or sets the clock speed of the simulated system, in MHz.
        /// </summary>
        decimal EffectiveClockFrequencyInMHz { get; set; }

        /// <summary>
        /// Starts the internal clock used to keep track of time.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the internal clock used to keep track of time.
        /// </summary>
        /// <remarks>
        /// The class should still work even if this method is never invoked (that is, consecutive calls
        /// to <see cref="Start"/> should be allowed). However invoking this method is convenient
        /// in order to clean up resources.
        /// </remarks>
        void Stop();

        /// <summary>
        /// Signals that a certain number of clock cycles have elapsed in the simulated system.
        /// </summary>
        /// <param name="periodLengthInCycles">Amount of period cycles to simulate</param>
        /// <remarks>This method will do its best to accurately reproduce the simulated system's clock speed
        /// by pausing the current thread for the specified amount of time. However, depending on the host system's
        /// clock accuracy the method may need to accummulate several clock cycles across different method invocations
        /// before actually pausing the thread.</remarks>
        void TryWait(int periodLengthInCycles);
    }
}
