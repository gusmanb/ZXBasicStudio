using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_rr_aa_tests : InstructionsExecutionTestsBase
    {
        static object[] LD_rr_aa_Source =
        {
            new object[] {"HL", (byte)0x2A, (byte?)null},
            new object[] {"DE", (byte)0x5B, (byte?)0xED},
            new object[] {"BC", (byte)0x4B, (byte?)0xED},
            new object[] {"SP", (byte)0x7B, (byte?)0xED},
            new object[] {"IX", (byte)0x2A, (byte?)0xDD},
            new object[] {"IY", (byte)0x2A, (byte?)0xFD},
        };

        [Test]
        [TestCaseSource("LD_rr_aa_Source")]
        public void LD_rr_aa_loads_value_from_memory(string reg, byte opcode, byte? prefix)
        {
            var address = Fixture.Create<ushort>();
            var oldValue = Fixture.Create<short>();
            var newValue = Fixture.Create<short>();

            SetReg(reg, oldValue);
            WriteShortToMemory(address, newValue);

            Execute(opcode, prefix, nextFetches: address.ToByteArray());

            Assert.AreEqual(newValue, ReadShortFromMemory(address));
            Assert.AreEqual(newValue, GetReg<short>(reg));
        }

        [Test]
        [TestCaseSource("LD_rr_aa_Source")]
        public void LD_rr_r_do_not_modify_flags(string reg, byte opcode, byte? prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("LD_rr_aa_Source")]
        public void LD_rr_r_returns_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(reg == "HL" ? 16 : 20, states);
        }
    }
}