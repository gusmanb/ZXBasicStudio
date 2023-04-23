using Konamiman.Z80dotNet.Tests.InstructionsExecution;
using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class IM_n_tests : InstructionsExecutionTestsBase
    {
        private const byte prefix = 0xED;

        public static object[] IM_n_Source =
        {
            new object[] {(byte)0, (byte)0x46},
            new object[] {(byte)1, (byte)0x56},
            new object[] {(byte)2, (byte)0x5E},
        };

        [Test]
        [TestCaseSource("IM_n_Source")]
        public void IM_n_changes_interrupt_mode_appropriately(byte newMode, byte opcode)
        {
            var oldMode = (byte)((newMode + 1) % 3);
            
            Sut.ProcessorAgent.SetInterruptMode(oldMode);

            Execute(opcode, prefix);

            Assert.AreEqual(newMode, ((FakeProcessorAgent)Sut.ProcessorAgent).CurrentInterruptMode);
        }

        [Test]
        [TestCaseSource("IM_n_Source")]
        public void IM_n_does_not_modify_flags(byte newMode, byte opcode)
        {
            AssertDoesNotChangeFlags(opcode, prefix);
        }

        [Test]
        [TestCaseSource("IM_n_Source")]
        public void IM_n_returns_proper_T_states(byte newMode, byte opcode)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(8, states);
        }
    }
}
