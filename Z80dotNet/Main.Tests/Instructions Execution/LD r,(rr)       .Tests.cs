using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public partial class LD_r_rr_tests : InstructionsExecutionTestsBase
    {
        public static object[] LD_r_rr_Source =
        {
            new object[] {"A", "BC", (byte) 0x0A},
            new object[] {"A", "DE", (byte) 0x1A},
            new object[] {"A", "HL", (byte) 0x7E},
            new object[] {"B", "HL", (byte) 0x46},
            new object[] {"C", "HL", (byte) 0x4E},
            new object[] {"D", "HL", (byte) 0x56},
            new object[] {"E", "HL", (byte) 0x5E},
            new object[] {"H", "HL", (byte) 0x66},
            new object[] {"L", "HL", (byte) 0x6E}
        };

        [Test]
        [TestCaseSource("LD_r_rr_Source")]
        public void LD_arr_r_loads_value_from_memory(string destReg, string srcPointerReg, byte opcode)
        {
            var isHorL = (destReg == "H" || destReg == "L");

            var address = Fixture.Create<ushort>();
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();

            SetReg(srcPointerReg, address.ToShort());
            ProcessorAgent.Memory[address] = newValue;
            if(!isHorL)
                SetReg(destReg, oldValue);

            Sut.Execute(opcode);

            Assert.AreEqual(newValue, GetReg<byte>(destReg));
        }

        [Test]
        [TestCaseSource("LD_r_rr_Source")]
        public void LD_r_rr_do_not_modify_flags(string destPointerReg, string srcReg, byte opcode)
        {
            AssertNoFlagsAreModified(opcode);
        }

        [Test]
        [TestCaseSource("LD_r_rr_Source")]
        public void LD_r_rr_returns_proper_T_states(string destPointerReg, string srcReg, byte opcode)
        {
            var states = Execute(opcode);
            Assert.AreEqual(7, states);
        }
    }
}