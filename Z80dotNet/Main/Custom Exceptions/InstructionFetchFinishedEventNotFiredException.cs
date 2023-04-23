using System;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Exception that is thrown by <see cref="IZ80Processor"/> when the <see cref="IZ80InstructionExecutor.Execute"/>
    /// method of the <see cref="IZ80InstructionExecutor"/> instance used returns without having fired the
    /// <see cref="IZ80InstructionExecutor.InstructionFetchFinished"/> event.
    /// </summary>
    public class InstructionFetchFinishedEventNotFiredException : Exception
    {
        /// <summary>
        /// The memory address of the first opcode byte fetched for the instruction that caused the exception.
        /// </summary>
        public ushort InstructionAddress { get; set; }

        /// <summary>
        /// The opcode bytes of the instruction that caused the exception.
        /// </summary>
        public byte[] FetchedBytes { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="instructionAddress">The memory address of the first opcode byte fetched for the instruction that caused the exception.</param>
        /// <param name="fetchedBytes">The opcode bytes of the instruction that caused the exception.</param>
        /// <param name="message">Message for the exception.</param>
        /// <param name="innerException">Inner exception.</param>
        public InstructionFetchFinishedEventNotFiredException(
            ushort instructionAddress,
            byte[] fetchedBytes,
            string message = null,
            Exception innerException = null)
        : base(message ?? "IZ80InstructionExecutor.Execute returned without having fired the InstructionFetchFinished event.", innerException)
        {
            this.InstructionAddress = instructionAddress;
            this.FetchedBytes = fetchedBytes;
        }
    }
}
