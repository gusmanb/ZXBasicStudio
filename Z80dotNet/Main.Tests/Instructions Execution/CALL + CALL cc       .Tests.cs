using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class CALL_and_CALL_cc_tests : InstructionsExecutionTestsBase
    {
        public static object[] CALL_cc_Source =
        {
            new object[] {"Z", (byte)0xC4, 0},
            new object[] {"C", (byte)0xD4, 0},
            new object[] {"P", (byte)0xE4, 0},
            new object[] {"S", (byte)0xF4, 0},
            new object[] {"Z", (byte)0xCC, 1},
            new object[] {"C", (byte)0xDC, 1},
            new object[] {"P", (byte)0xEC, 1},
            new object[] {"S", (byte)0xFC, 1},
        };

        public static object[] CALL_Source =
        {
            new object[] {null, (byte)0xCD, 0},
        };

        [Test]
        [TestCaseSource("CALL_cc_Source")]
        public void CALL_cc_does_not_jump_if_flag_not_set(string flagName, byte opcode, int flagValue)
        {
            var instructionAddress = Fixture.Create<ushort>();

            SetFlag(flagName, !(Bit)flagValue);
            ExecuteAt(instructionAddress, opcode);

            Assert.AreEqual(instructionAddress.Add(3), Registers.PC);
        }

        [Test]
        [TestCaseSource("CALL_cc_Source")]
        public void CALL_cc_returns_proper_T_states_if_no_jump_is_made(string flagName, byte opcode, int flagValue)
        {
            SetFlag(flagName, !(Bit)flagValue);
            var states = Execute(opcode);

            Assert.AreEqual(10, states);
        }

        [Test]
        [TestCaseSource("CALL_cc_Source")]
        [TestCaseSource("CALL_Source")]
        public void CALL_cc_pushes_SP_and_jumps_to_proper_address_if_flag_is_set_CALL_jumps_always(string flagName, byte opcode, int flagValue)
        {
            var instructionAddress = Fixture.Create<ushort>();
            var callAddress = Fixture.Create<ushort>();
            var oldSP = Fixture.Create<short>();
            Registers.SP = oldSP;
            
            SetFlagIfNotNull(flagName, flagValue);
            ExecuteAt(instructionAddress, opcode, nextFetches: new[] { callAddress.GetLowByte(), callAddress.GetHighByte() });

            Assert.AreEqual(callAddress, Registers.PC);
            Assert.AreEqual(oldSP.Sub(2), Registers.SP);
            Assert.AreEqual(instructionAddress.Add(3).ToShort(), ReadShortFromMemory(Registers.SP.ToUShort()));
        }

        private void SetFlagIfNotNull(string flagName, int flagValue)
        {
            if (flagName != null) SetFlag(flagName, flagValue);
        }
        
        [Test]
        [TestCaseSource("CALL_cc_Source")]
        [TestCaseSource("CALL_Source")]
        public void CALL_and_CALL_cc_return_proper_T_states_if_jump_is_made(string flagName, byte opcode, int flagValue)
        {
            SetFlagIfNotNull(flagName, flagValue);
            var states = Execute(opcode);

            Assert.AreEqual(17, states);
        }

        [Test]
        [TestCaseSource("CALL_cc_Source")]
        [TestCaseSource("CALL_Source")]
        public void CALL_and_CALL_cc_do_not_modify_flags(string flagName, byte opcode, int flagValue)
        {
            Registers.F = Fixture.Create<byte>();
            SetFlagIfNotNull(flagName, flagValue);
            var value = Registers.F;

            Execute(opcode);

            Assert.AreEqual(value, Registers.F);
        }
    }
}