using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class JR_and_JR_cc_tests : InstructionsExecutionTestsBase
    {
        public static object[] jr_cc_Source =
        {
            new object[] {"Z", (byte)0x20, 0},
            new object[] {"C", (byte)0x30, 0},
            new object[] {"Z", (byte)0x28, 1},
            new object[] {"C", (byte)0x38, 1},
        };

        public static object[] jr_Source =
        {
            new object[] {null, (byte)0x18, 0},
        };

        [Test]
        [TestCaseSource("jr_cc_Source")]
        public void JR_cc_does_not_jump_if_flag_not_set(string flagName, byte opcode, int flagValue)
        {
            var instructionAddress = Fixture.Create<ushort>();

            SetFlag(flagName, !(Bit)flagValue);
            ExecuteAt(instructionAddress, opcode, nextFetches: new[] {Fixture.Create<byte>()});

            Assert.AreEqual(instructionAddress.Add(2), Registers.PC);
        }

        [Test]
        [TestCaseSource("jr_cc_Source")]
        public void JR_cc_returns_proper_T_states_if_no_jump_is_made(string flagName, byte opcode, int flagValue)
        {
            SetFlag(flagName, !(Bit)flagValue);
            var states = Execute(opcode);

            Assert.AreEqual(7, states);
        }

        [Test]
        [TestCaseSource("jr_cc_Source")]
        [TestCaseSource("jr_Source")]
        public void JR_cc_jumps_to_proper_address_if_flag_is_set_JR_jumps_always(string flagName, byte opcode, int flagValue)
        {
            var instructionAddress = Fixture.Create<ushort>();

            SetFlagIfNotNull(flagName, flagValue);
            ExecuteAt(instructionAddress, opcode, nextFetches: new byte[] {0x7F});
            Assert.AreEqual(instructionAddress.Add(129), Registers.PC);

            SetFlagIfNotNull(flagName, flagValue);
            ExecuteAt(instructionAddress, opcode, nextFetches: new byte[] {0x80});
            Assert.AreEqual(instructionAddress.Sub(126), Registers.PC);
        }

        private void SetFlagIfNotNull(string flagName, int flagValue)
        {
            if (flagName != null) SetFlag(flagName, flagValue);
        }
        
        [Test]
        [TestCaseSource("jr_cc_Source")]
        [TestCaseSource("jr_Source")]
        public void JR_and_JR_cc_returns_proper_T_states_if_jump_is_made(string flagName, byte opcode, int flagValue)
        {
            SetFlagIfNotNull(flagName, flagValue);
            var states = Execute(opcode);

            Assert.AreEqual(12, states);
        }

        [Test]
        [TestCaseSource("jr_cc_Source")]
        [TestCaseSource("jr_Source")]
        public void JR_and_JR_cc_do_not_modify_flags(string flagName, byte opcode, int flagValue)
        {
            Registers.F = Fixture.Create<byte>();
            SetFlagIfNotNull(flagName, flagValue);
            var value = Registers.F;

            Execute(opcode);

            Assert.AreEqual(value, Registers.F);
        }
    }
}