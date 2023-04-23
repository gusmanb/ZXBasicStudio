using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class EX_SP_HL_IX_IY_tests : InstructionsExecutionTestsBase
    {
        private const byte EX_SP_HL_opcode = 0xE3;

        public static object[] EX_Source =
        {
            new object[] {"HL", (byte)0xE3, (byte?)null},
            new object[] {"IX", (byte)0xE3, (byte?)0xDD},
            new object[] {"IY", (byte)0xE3, (byte?)0xFD},
        };

        [Test]
        [TestCaseSource("EX_Source")]
        public void EX_SP_HL_IX_IY_exchanges_reg_and_pushed_value(string reg, byte opcode, byte? prefix)
        {
            var regValue = Fixture.Create<short>();
            var pushedValue = Fixture.Create<short>();
            var SP = Fixture.Create<ushort>();

            SetReg(reg, regValue);
            Registers.SP = SP.ToShort();
            WriteShortToMemory(SP, pushedValue);

            Execute(opcode, prefix);

            Assert.AreEqual(regValue, ReadShortFromMemory(SP));
            Assert.AreEqual(pushedValue, GetReg<short>(reg));
        }

        [Test]
        [TestCaseSource("EX_Source")]
        public void EX_SP_HL_IX_IY_do_not_change_flags(string reg, byte opcode, byte? prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("EX_Source")]
        public void EX_SP_HL_return_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(reg == "HL" ? 19 : 23, states);
        }
    }
}