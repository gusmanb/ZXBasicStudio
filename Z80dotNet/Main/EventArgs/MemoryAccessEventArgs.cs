namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Event triggered by the <see cref="IZ80Processor"/> class before and after a memory or port access.
    /// </summary>
    public class MemoryAccessEventArgs : ProcessorEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="eventType">Type of event being processed</param>
        /// <param name="address">Memory or port address being accessed</param>
        /// <param name="value">Initial value for the <see cref="Value"/> property</param>
        /// <param name="localUserState">Initial value for the <see cref="ProcessorEventArgs.LocalUserState"/> property</param>
        /// <param name="cancelMemoryAccess">Initial value for the <see cref="CancelMemoryAccess"/> property</param>
        public MemoryAccessEventArgs(
            MemoryAccessEventType eventType,
            ushort address,
            byte value,
            object localUserState = null,
            bool cancelMemoryAccess = false)
        {
            this.EventType = eventType;
            this.Address = address;
            this.Value = value;
            this.LocalUserState = localUserState;
            this.CancelMemoryAccess = cancelMemoryAccess;
        }

        /// <summary>
        /// Gets the type of event being processed. 
        /// </summary>
        public MemoryAccessEventType EventType { get; private set; }

        /// <summary>
        /// Gets the memory or port address being accessed.
        /// </summary>
        public ushort Address { get; private set; }

        /// <summary>
        /// Gets or sets the value that has been read, will be written, or has been written.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The underlying memory manager (the <see cref="IZ80Processor.Memory"/>
        /// property or the <see cref="IZ80Processor.PortsSpace"/> property of the <see cref="IZ80Processor"/>
        /// that triggered the event) will not be accessed, and the value of this property when the <c>After*</c>
        /// event starts will be the same as when the matching <c>Before*</c> method finished
        /// (see <see cref="EventType"/>), in the following cases:
        /// </para>
        /// <list type="bullet">
        /// <item><description>The access is a memory or port read, and the memory mode for the address
        /// (see <see cref="IZ80Processor.SetMemoryAccessMode"/>, <see cref="IZ80Processor.GetMemoryAccessMode"/>,
        /// <see cref="IZ80Processor.SetPortsSpaceAccessMode"/>, <see cref="IZ80Processor.GetPortAccessMode"/>)
        /// is <see cref="MemoryAccessMode.NotConnected"/> or <see cref="MemoryAccessMode.WriteOnly"/>.
        /// </description></item>
        /// <item><description>The access is a memory or port write, and the memory mode for the address
        /// is <see cref="MemoryAccessMode.NotConnected"/> or <see cref="MemoryAccessMode.ReadOnly"/>.
        /// </description></item>
        /// <item><description>The <see cref="CancelMemoryAccess"/> property is set to <b>true</b> during
        /// the <c>Before*</c> event.
        /// </description></item>
        /// </list>
        /// <para>The value of this property at the beginning of a <c>Before*</c> event for read is always 0xFF.</para>
        /// </remarks>
        public byte Value { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether access to the underlying memory manager should be cancelled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this property is set to <b>true</b> during a <c>Before*</c> event (see <see cref="EventType"/>),
        /// then the underlying memory manager (the <see cref="IZ80Processor.Memory"/>
        /// property or the <see cref="IZ80Processor.PortsSpace"/> property of the <see cref="IZ80Processor"/> that triggered the event)
        /// will not be accessed. Instead, the matching <c>After*</c> event will be triggered directly, having a
        /// <see cref="Value"/> equal to the one set in the <c>Before*</c> event.
        /// </para>
        /// <para>
        /// The value of this property when the <c>After*</c> event is triggered is the same that the matching 
        /// <c>Before*</c> event had when it ended,
        /// so it is possible to check whether the memory access was cancelled or not. Changing the value
        /// of this property during the <c>After*</c> event has no effect.
        /// </para>
        /// </remarks>
        public bool CancelMemoryAccess { get; set; }
    }
}