namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a class that allows to perform a limited set of operations on an <see cref="IZ80Processor"/>.
    /// </summary>
    public interface IZ80ProcessorAgent : IExecutionStopper
    {
        /// <summary>
        /// Reads the next opcode byte from the memory location currently pointed by the PC register,
        /// then increments PC by one.
        /// </summary>
        /// <returns>The byte obtained from memory</returns>
        byte FetchNextOpcode();

        /// <summary>
        /// Reads the next opcode byte from the memory location currently pointed by the PC register,
        /// but does not modify PC.
        /// </summary>
        /// <returns>The byte obtained from memory</returns>
        /// <remarks>This method can be useful to handle 0xDD and 0xFD prefixes.
        /// If <see cref="IZ80InstructionExecutor.Execute"/> receives one of these bytes, it can use this method
        /// the check the next opcode byte. If both bytes do not form a supported instruction
        /// (for example, if the second byte is another 0xDD/0xFD byte), then <see cref="IZ80InstructionExecutor.Execute"/>
        /// simply returns; the first 0xDD/0xFD acts then as a NOP, and the
        /// second byte will be fetched again for the next invocation of <see cref="IZ80InstructionExecutor.Execute"/>.
        /// Otherwise, <see cref="FetchNextOpcode"/> in invoked in order to get PC properly incremented.
        /// </remarks>
        byte PeekNextOpcode();

        /// <summary>
        /// Reads one byte from memory.
        /// </summary>
        /// <param name="address">Memory address to read from</param>
        /// <returns>Obtained byte</returns>
        byte ReadFromMemory(ushort address);

        /// <summary>
        /// Writes one byte to memory.
        /// </summary>
        /// <param name="address">Memory address to write to</param>
        /// <param name="value">Value to write</param>
        void WriteToMemory(ushort address, byte value);

        /// <summary>
        /// Reads one byte from an I/O port.
        /// </summary>
        /// <param name="portNumber">Port number to read from</param>
        /// <returns>Obtained byte</returns>
        byte ReadFromPort(byte portNumber, byte upperData);

        /// <summary>
        /// Writes one byte to an I/O port.
        /// </summary>
        /// <param name="portNumber">Port number to write to</param>
        /// <param name="value">Value to write</param>
        void WriteToPort(byte portNumber, byte upperPart, byte value);

        /// <summary>
        /// Returns the current register set used by the processor.
        /// </summary>
        IZ80Registers Registers { get; }

        /// <summary>
        /// Changes the current interrupt mode.
        /// </summary>
        /// <param name="interruptMode">The new interrupt mode.</param>
        /// <exception cref="System.InvalidOperationException">Attempt to set a value other than 0, 1 or 2</exception>
        void SetInterruptMode(byte interruptMode);
    }
}
