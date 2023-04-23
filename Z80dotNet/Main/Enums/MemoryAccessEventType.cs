namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents the type of a memory access event. 
    /// </summary>
    public enum MemoryAccessEventType
    {
        /// <summary>
        /// A memory address is going to be read.
        /// </summary>
        BeforeMemoryRead,

        /// <summary>
        /// A memory address has been read.
        /// </summary>
        AfterMemoryRead,
        
        /// <summary>
        /// A memory address is going to be written.
        /// </summary>
        BeforeMemoryWrite,
        
        /// <summary>
        /// A memory address has been written.
        /// </summary>
        AfterMemoryWrite,
        
        /// <summary>
        /// A port is going to be read.
        /// </summary>
        BeforePortRead,
                
        /// <summary>
        /// A port has been read.
        /// </summary>
        AfterPortRead,
                
        /// <summary>
        /// A port is going to be written.
        /// </summary>
        BeforePortWrite,
                        
        /// <summary>
        /// A port has been written.
        /// </summary>
        AfterPortWrite
    }
}