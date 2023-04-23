namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents the access mode for a certain memory address.
    /// </summary>
    public enum MemoryAccessMode
    {
        /// <summary>
        /// Read and write. The registered <see cref="IMemory"/> object will be accessed
        /// for both reading and writing from and to memory.
        /// </summary>
        ReadAndWrite,

        /// <summary>
        /// Read only. The registered <see cref="IMemory"/> object will be accessed
        /// only for reading to memory.
        /// </summary>
        ReadOnly,

        /// <summary>
        /// Write only. The registered <see cref="IMemory"/> object will be accessed
        /// only for writing to memory. The read value will always be FFh.
        /// </summary>
        WriteOnly,

        /// <summary>
        /// Not connected. The registered <see cref="IMemory"/> object will never be accessed
        /// for the affected memory address. The read value will always be FFh.
        /// </summary>
        NotConnected
    }
}