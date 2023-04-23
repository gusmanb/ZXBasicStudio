using System;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Default implementation of <see cref="IZ80InstructionExecutor"/>.
    /// </summary>
    public partial class Z80InstructionExecutor : IZ80InstructionExecutor
    {
        private IZ80Registers Registers;

        public IZ80ProcessorAgent ProcessorAgent { get; set; }

        public Z80InstructionExecutor()
        {
            Initialize_CB_InstructionsTable();
            Initialize_DD_InstructionsTable();
            Initialize_DDCB_InstructionsTable();
            Initialize_ED_InstructionsTable();
            Initialize_FD_InstructionsTable();
            Initialize_FDCB_InstructionsTable();
            Initialize_SingleByte_InstructionsTable();
            GenerateParityTable();
        }

        public int Execute(byte firstOpcodeByte)
        {
            Registers = ProcessorAgent.Registers;

            switch(firstOpcodeByte)
            {
                case 0xCB:
                    return Execute_CB_Instruction();
                case 0xDD:
                    return Execute_DD_Instruction();
                case 0xED:
                    return Execute_ED_Instruction();
                case 0xFD:
                    return Execute_FD_Instruction();
                default:
                    return Execute_SingleByte_Instruction(firstOpcodeByte);
            }
        }

        private int Execute_CB_Instruction()
        {
            Inc_R();
            Inc_R();
            return CB_InstructionExecutors[ProcessorAgent.FetchNextOpcode()]();
        }

        private int Execute_ED_Instruction()
        {
            Inc_R();
            Inc_R();
            var secondOpcodeByte = ProcessorAgent.FetchNextOpcode();
            if (IsUnsupportedInstruction(secondOpcodeByte))
                return ExecuteUnsopported_ED_Instruction(secondOpcodeByte);
            else if(secondOpcodeByte >= 0xA0)
                return ED_Block_InstructionExecutors[secondOpcodeByte - 0xA0]();
            else
                return ED_InstructionExecutors[secondOpcodeByte - 0x40]();
        }

        private static bool IsUnsupportedInstruction(byte secondOpcodeByte)
        {
            return
                secondOpcodeByte < 0x40 ||
                secondOpcodeByte.Between(0x80, 0x9F) || 
                secondOpcodeByte.Between(0xA4, 0xA7) || 
                secondOpcodeByte.Between(0xAC, 0xAF) || 
                secondOpcodeByte.Between(0xB4, 0xB7) || 
                secondOpcodeByte.Between(0xBC, 0xBF) || 
                secondOpcodeByte > 0xBF;
        }

        /// <summary>
        /// Executes an unsupported ED instruction, that is, an instruction whose opcode is
        /// ED xx, where xx is 00-3F, 80-9F, A4-A7, AC-AF, B4-B7, BC-BF or C0-FF.
        /// </summary>
        /// <param name="secondOpcodeByte">The opcode byte fetched after the 0xED.</param>
        /// <returns>The total amount of T states required for the instruction execution.</returns>
        /// <remarks>You can override this method in derived classes in order to implement a custom
        /// behavior for these unsupported instructions (for example, to implement the multiplication
        /// instructions of the R800 processor).</remarks>
        protected virtual int ExecuteUnsopported_ED_Instruction(byte secondOpcodeByte)
        {
            return NOP2();
        }

        private int Execute_SingleByte_Instruction(byte firstOpcodeByte)
        {
            Inc_R();
            return SingleByte_InstructionExecutors[firstOpcodeByte]();
        }

        public event EventHandler<InstructionFetchFinishedEventArgs> InstructionFetchFinished;

        #region Auxiliary methods

        private void FetchFinished(bool isRet = false, bool isHalt = false, bool isLdSp = false, bool isEiOrDi = false)
        {
            InstructionFetchFinished(this, new InstructionFetchFinishedEventArgs()
            {
                IsRetInstruction = isRet,
                IsHaltInstruction = isHalt,
                IsLdSpInstruction = isLdSp,
                IsEiOrDiInstruction = isEiOrDi
            });
        }

        private void Inc_R()
        {
            ProcessorAgent.Registers.R = ProcessorAgent.Registers.R.Inc7Bits();
        }

        private short FetchWord()
        {
            return NumberUtils.CreateShort(
                lowByte: ProcessorAgent.FetchNextOpcode(),
                highByte: ProcessorAgent.FetchNextOpcode());
        }

        private void WriteShortToMemory(ushort address, short value)
        {
            ProcessorAgent.WriteToMemory(address, value.GetLowByte());
            ProcessorAgent.WriteToMemory((ushort)(address + 1), value.GetHighByte());
        }

        private short ReadShortFromMemory(ushort address)
        {
            return NumberUtils.CreateShort(
                ProcessorAgent.ReadFromMemory(address),
                ProcessorAgent.ReadFromMemory((ushort)(address + 1)));
        }

        private void SetFlags3and5From(byte value)
        {
            const int Flags_3_5 = 0x28;

            Registers.F = (byte)((Registers.F & ~Flags_3_5) | (value & Flags_3_5));
        }

        #endregion
    }
}
