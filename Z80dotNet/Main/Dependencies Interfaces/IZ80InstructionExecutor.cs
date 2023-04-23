using System;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a class that can execute Z80 instructions.
    /// </summary>
    public interface IZ80InstructionExecutor
    {
        /// <summary>
        /// The <see cref="IZ80ProcessorAgent"/> that the <see cref="Execute"/> method will use in order
        /// to interact with the processor.
        /// </summary>
        IZ80ProcessorAgent ProcessorAgent { get; set; }

        /// <summary>
        /// Executes the next instruction.
        /// </summary>
        /// <param name="firstOpcodeByte">First byte of the opcode for the instruction to execute</param>
        /// <returns>Total number of T states elapsed when executing the instruction,
        /// not including extra memory wait states.</returns>
        /// <remarks>
        /// <para>
        /// The execution flow for this method should be as follows:
        /// </para>
        /// <list type="number">
        /// <item><description>If needed, extra opcode bytes are fetched by using the 
        /// <see cref="IZ80ProcessorAgent.FetchNextOpcode"/> method on <see cref="ProcessorAgent"/>.</description></item>
        /// <item><description>The <see cref="InstructionFetchFinished"/> event is triggered.</description></item>
        /// <item><description>The instruction is processed by accessing the <see cref="ProcessorAgent"/> members as appropriate.</description></item>
        /// <item><description>The method terminates, returning the total count of T states required for the instruction
        /// execution, not including any extra memory or port wait states (but including the automatically
        /// inserted wait state used for port access).</description></item>
        /// </list>
        /// <para>
        /// The PC register will point to the address after the supplied opcode byte when this method is invoked,
        /// and each subsequent call to <see cref="IZ80ProcessorAgent.FetchNextOpcode"/> will further increment
        /// PC by one. This has to be taken in account when implementing the relative jump instructions (DJNZ and JR).
        /// </para>
        /// </remarks>
        int Execute(byte firstOpcodeByte);

        /// <summary>
        /// Event triggered when the instruction opcode has been fully fetched.
        /// </summary>
        event EventHandler<InstructionFetchFinishedEventArgs> InstructionFetchFinished;
    }
}
