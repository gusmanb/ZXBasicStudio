using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class DEC_rr_tests : InstructionsExecutionTestsBase
    {
        public static object[] DEC_rr_Source =
        {
            new object[] {"BC", (byte)0x0B, null},
            new object[] {"DE", (byte)0x1B, null},
            new object[] {"HL", (byte)0x2B, null},
            new object[] {"SP", (byte)0x3B, null},
            new object[] {"IX", (byte)0x2B, (byte?)0xDD},
            new object[] {"IY", (byte)0x2B, (byte?)0xFD},
        };

        [Test]
        [TestCaseSource("DEC_rr_Source")]
        public void DEC_rr_decreases_register(string reg, byte opcode, byte? prefix)
        {
            SetReg(reg, 0);
            Execute(opcode, prefix);
            Assert.AreEqual(0xFFFF.ToShort(), GetReg<short>(reg));
        }

        [Test]
        [TestCaseSource("DEC_rr_Source")]
        public void DEC_rr_do_not_modify_flags(string reg, byte opcode, byte? prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("DEC_rr_Source")]
        public void DEC_rr_returns_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(IfIndexRegister(reg, 10, @else: 6), states);
        }
    }
}
