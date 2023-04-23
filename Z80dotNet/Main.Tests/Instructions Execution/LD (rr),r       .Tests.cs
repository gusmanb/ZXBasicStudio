using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_arr_r_tests : InstructionsExecutionTestsBase
    {
        static object[] LD_rr_r_Source =
        {
            new object[] {"BC", "A", (byte)0x02},
            new object[] {"DE", "A", (byte)0x12},
            new object[] {"HL", "A", (byte)0x77},
            new object[] {"HL", "B", (byte)0x70},
            new object[] {"HL", "C", (byte)0x71},
            new object[] {"HL", "D", (byte)0x72},
            new object[] {"HL", "E", (byte)0x73},
            new object[] {"HL", "H", (byte)0x74},
            new object[] {"HL", "L", (byte)0x75}
        };

        [Test]
        [TestCaseSource("LD_rr_r_Source")]
        public void LD_arr_r_loads_value_in_memory(string destPointerReg, string srcReg, byte opcode)
        {
            var isHorL = (srcReg == "H" || srcReg == "L");

            var address = Fixture.Create<ushort>();
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();

            SetReg(destPointerReg, address.ToShort());
            ProcessorAgent.Memory[address] = oldValue;
            if(!isHorL)
                SetReg(srcReg, newValue);

            Sut.Execute(opcode);

            var expected = isHorL ? GetReg<byte>(srcReg) : newValue;
            Assert.AreEqual(expected, ProcessorAgent.Memory[address]);
        }

        [Test]
        [TestCaseSource("LD_rr_r_Source")]
        public void LD_rr_r_do_not_modify_flags(string destPointerReg, string srcReg, byte opcode)
        {
            AssertNoFlagsAreModified(opcode);
        }

        [Test]
        [TestCaseSource("LD_rr_r_Source")]
        public void LD_rr_r_returns_proper_T_states(string destPointerReg, string srcReg, byte opcode)
        {
            var states = Execute(opcode);
            Assert.AreEqual(7, states);
        }
    }
}