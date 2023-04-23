using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_r_n_tests : InstructionsExecutionTestsBase
    {
        public static object[] LD_r_n_Source =
        {
            new object[] {"A", (byte)0x3E, (byte?)null},
            new object[] {"B", (byte)0x06, (byte?)null},
            new object[] {"C", (byte)0x0E, (byte?)null},
            new object[] {"D", (byte)0x16, (byte?)null},
            new object[] {"E", (byte)0x1E, (byte?)null},
            new object[] {"H", (byte)0x26, (byte?)null},
            new object[] {"L", (byte)0x2E, (byte?)null},
            new object[] {"IXH", (byte)0x26, (byte?)0xDD},
            new object[] {"IXL", (byte)0x2E, (byte?)0xDD},
            new object[] {"IYH", (byte)0x26, (byte?)0xFD},
            new object[] {"IYL", (byte)0x2E, (byte?)0xFD},
        };

        [Test]
        [TestCaseSource("LD_r_n_Source")]
        public void LD_r_n_loads_register_with_value(string reg, byte opcode, byte? prefix)
        {
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();

            SetReg(reg, oldValue);

            Execute(opcode, prefix, newValue);

            Assert.AreEqual(newValue, GetReg<byte>(reg));
        }

        [Test]
        [TestCaseSource("LD_r_n_Source")]
        public void LD_r_n_do_not_modify_flags(string reg, byte opcode, byte? prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("LD_r_n_Source")]
        public void LD_r_n_returns_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(IfIndexRegister(reg, 11, @else: 7), states);
        }
    }
}