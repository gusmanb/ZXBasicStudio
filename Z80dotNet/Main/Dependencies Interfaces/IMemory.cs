namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents the memory that is visible by a processor.
    /// </summary>
    public interface IMemory
    {
        /// <summary>
        /// Returns the size of the memory.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Reads or writes one single memory address.
        /// </summary>
        /// <param name="address">Address to read or write</param>
        /// <returns>Data to write</returns>
        /// <exception cref="System.IndexOutOfRangeException"><c>address</c> is negative or beyond the memory size.</exception>
        byte this[int address] { get; set; }

        /// <summary>
        /// Sets a portion of the memory with the contents of a byte array.
        /// </summary>
        /// <param name="startAddress">First memory address that will be set</param>
        /// <param name="contents">New contents of the memory</param>
        /// <param name="startIndex">Start index for starting copying within the contens array</param>
        /// <param name="length">Length of the contents array that will be copied. If null,
        /// the whole array is copied.</param>
        /// <exception cref="System.IndexOutOfRangeException"><c>startAddress</c> + <c>length</c> (or <c>content.Length</c>)
        /// goes beyond the memory size, or <c>length</c> is greater that the actual length of <c>contents</c>.</exception>
        /// <exception cref="System.ArgumentNullException">contents is null</exception>
        void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null);

        /// <summary>
        /// Returns the contents of a portion of the memory as a byte array.
        /// </summary>
        /// <param name="startAddress">First memory address whose contents will be returned</param>
        /// <param name="length">Length of the portion to return</param>
        /// <returns>Current contents of the specified memory portion</returns>
        /// <exception cref="System.InvalidOperationException"><c>startAddress</c> + <c>length</c> (or <c>content.Length</c>)
        /// goes beyond the memory size.</exception>
        byte[] GetContents(int startAddress, int length);
    }
}
