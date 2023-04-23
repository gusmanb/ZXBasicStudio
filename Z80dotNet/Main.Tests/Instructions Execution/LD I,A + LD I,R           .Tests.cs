using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_I_R_A_tests : InstructionsExecutionTestsBase
    {
        private const byte prefix = 0xED;

        public static object[] LD_I_R_A_Source =
        {
            new object[] {"I", (byte)0x47},
            new object[] {"R", (byte)0x4F}
        };

        [Test]
        [TestCaseSource("LD_I_R_A_Source")]
        public void LD_I_R_A_loads_value_correctly(string reg, byte opcode)
        {
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();
            SetReg(reg, oldValue);
            Registers.A = newValue;

            Execute(opcode, prefix);

            Assert.AreEqual(newValue, GetReg<byte>(reg));
        }

        [Test]
        [TestCaseSource("LD_I_R_A_Source")]
        public void LD_I_R_A_does_not_modify_flags(string reg, byte opcode)
        {
            AssertDoesNotChangeFlags(opcode, prefix);
        }

        [Test]
        [TestCaseSource("LD_I_R_A_Source")]
        public void LD_I_R_A_returns_proper_T_states(string reg, byte opcode)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(9, states);
        }
    }
}