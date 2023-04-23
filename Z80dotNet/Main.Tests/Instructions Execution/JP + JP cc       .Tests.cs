using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class JP_and_JP_cc_tests : InstructionsExecutionTestsBase
    {
        public static object[] JP_cc_Source =
        {
            new object[] {"Z", (byte)0xC2, 0},
            new object[] {"C", (byte)0xD2, 0},
            new object[] {"P", (byte)0xE2, 0},
            new object[] {"S", (byte)0xF2, 0},
            new object[] {"Z", (byte)0xCA, 1},
            new object[] {"C", (byte)0xDA, 1},
            new object[] {"P", (byte)0xEA, 1},
            new object[] {"S", (byte)0xFA, 1},
        };

        public static object[] JP_Source =
        {
            new object[] {null, (byte)0xC3, 0},
        };

        [Test]
        [TestCaseSource("JP_cc_Source")]
        public void JP_cc_does_not_jump_if_flag_not_set(string flagName, byte opcode, int flagValue)
        {
            var instructionAddress = Fixture.Create<ushort>();

            SetFlag(flagName, !(Bit)flagValue);
            ExecuteAt(instructionAddress, opcode);

            Assert.AreEqual(instructionAddress.Add(3), Registers.PC);
        }

        [Test]
        [TestCaseSource("JP_cc_Source")]
        public void JP_cc_returns_proper_T_states_if_no_jump_is_made(string flagName, byte opcode, int flagValue)
        {
            SetFlag(flagName, !(Bit)flagValue);
            var states = Execute(opcode);

            Assert.AreEqual(10, states);
        }

        [Test]
        [TestCaseSource("JP_cc_Source")]
        [TestCaseSource("JP_Source")]
        public void JP_cc_jumps_to_proper_address_if_flag_is_set_JP_jumps_always(string flagName, byte opcode, int flagValue)
        {
            var instructionAddress = Fixture.Create<ushort>();
            var jumpAddress = Fixture.Create<ushort>();
            
            SetFlagIfNotNull(flagName, flagValue);
            ExecuteAt(instructionAddress, opcode, nextFetches: new[] { jumpAddress.GetLowByte(), jumpAddress.GetHighByte() });

            Assert.AreEqual(jumpAddress, Registers.PC);
        }

        private void SetFlagIfNotNull(string flagName, int flagValue)
        {
            if (flagName != null) SetFlag(flagName, flagValue);
        }
        
        [Test]
        [TestCaseSource("JP_cc_Source")]
        [TestCaseSource("JP_Source")]
        public void JP_and_JP_cc_return_proper_T_states_if_jump_is_made(string flagName, byte opcode, int flagValue)
        {
            SetFlagIfNotNull(flagName, flagValue);
            var states = Execute(opcode);

            Assert.AreEqual(10, states);
        }

        [Test]
        [TestCaseSource("JP_cc_Source")]
        [TestCaseSource("JP_Source")]
        public void JP_and_JP_cc_do_not_modify_flags(string flagName, byte opcode, int flagValue)
        {
            Registers.F = Fixture.Create<byte>();
            SetFlagIfNotNull(flagName, flagValue);
            var value = Registers.F;

            Execute(opcode);

            Assert.AreEqual(value, Registers.F);
        }
    }
}