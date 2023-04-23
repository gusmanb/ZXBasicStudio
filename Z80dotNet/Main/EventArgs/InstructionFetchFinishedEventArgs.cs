using System;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Event args for the <see cref="IZ80InstructionExecutor.InstructionFetchFinished"/> event.
    /// </summary>
    public class InstructionFetchFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value that indicates if the instruction that has been executed was
        /// a return instruction (RET, conditional RET, RETI or RETN)
        /// </summary>
        public bool IsRetInstruction { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the instruction that has been executed was
        /// a stack load (LD SP,xx) instruction
        /// </summary>
        public bool IsLdSpInstruction { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the instruction that has been executed was
        /// a HALT instruction
        /// </summary>
        public bool IsHaltInstruction { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the instruction that has been executed was
        /// an EI instruction or a DI instruction
        /// </summary>
        public bool IsEiOrDiInstruction { get; set; }
    }
}
