using System;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Base class for all the events triggered by the <see cref="IZ80Processor"/> class.
    /// </summary>
    public abstract class ProcessorEventArgs : EventArgs
    {
        /// <summary>
        /// User-defined state object that is passed from the <c>Before*</c> events to the matching
        /// <c>After*</c> events.
        /// </summary>
        /// <remarks>
        /// This property is always null at the beginning of the <c>Before*</c> events.
        /// The client code can set this property to any value in these events, and the value
        /// will be replicated in the same property of the corresponding <c>After*</c> event.</remarks>
        public object LocalUserState { get; set; }
    }
}