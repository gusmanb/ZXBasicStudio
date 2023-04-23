using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class POP_rr_tests : InstructionsExecutionTestsBase
    {
        public static object[] POP_rr_Source =
        {
            new object[] {"BC", (byte)0xC1, (byte?)null},
            new object[] {"DE", (byte)0xD1, (byte?)null},
            new object[] {"HL", (byte)0xE1, (byte?)null},
            new object[] {"AF", (byte)0xF1, (byte?)null},
            new object[] {"IX", (byte)0xE1, (byte?)0xDD},
            new object[] {"IY", (byte)0xE1, (byte?)0xFD},
        };

        [Test]
        [TestCaseSource("POP_rr_Source")]
        public void POP_rr_loads_register_with_value_and_increases_SP(string reg, byte opcode, byte? prefix)
        {
            var instructionAddress = Fixture.Create<ushort>();
            var value = Fixture.Create<ushort>();
            var oldSP = Fixture.Create<short>();

            Registers.SP = oldSP;
            SetMemoryContentsAt(oldSP.ToUShort(), value.GetLowByte());
            SetMemoryContentsAt(oldSP.ToUShort().Inc(), value.GetHighByte());

            ExecuteAt(instructionAddress, opcode, prefix);

            Assert.AreEqual(value, GetReg<short>(reg));
            Assert.AreEqual(oldSP.Add(2), Registers.SP);
        }

        [Test]
        [TestCaseSource("POP_rr_Source")]
        public void POP_rr_do_not_modify_flags_unless_AF_is_popped(string reg, byte opcode, byte? prefix)
        {
            if(reg!="AF")
                AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("POP_rr_Source")]
        public void POP_rr_returns_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(reg.StartsWith("I") ? 14 : 10, states);
        }
    }
}
