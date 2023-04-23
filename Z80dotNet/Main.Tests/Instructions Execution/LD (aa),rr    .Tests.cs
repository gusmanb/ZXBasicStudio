using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_aa_rr_tests : InstructionsExecutionTestsBase
    {
        static object[] LD_aa_rr_Source =
        {
            new object[] {"HL", (byte)0x22, (byte?)null},
            new object[] {"DE", (byte)0x53, (byte?)0xED},
            new object[] {"BC", (byte)0x43, (byte?)0xED},
            new object[] {"SP", (byte)0x73, (byte?)0xED},
            new object[] {"IX", (byte)0x22, (byte?)0xDD},
            new object[] {"IY", (byte)0x22, (byte?)0xFD},
        };

        [Test]
        [TestCaseSource("LD_aa_rr_Source")]
        public void LD_aa_rr_loads_value_in_memory(string reg, byte opcode, byte? prefix)
        {
            var address = Fixture.Create<ushort>();
            var oldValue = Fixture.Create<short>();
            var newValue = Fixture.Create<short>();

            SetReg(reg, newValue);
            WriteShortToMemory(address, oldValue);

            Execute(opcode, prefix, nextFetches: address.ToByteArray());

            Assert.AreEqual(newValue, ReadShortFromMemory(address));
        }

        [Test]
        [TestCaseSource("LD_aa_rr_Source")]
        public void LD_rr_r_do_not_modify_flags(string reg, byte opcode, byte? prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("LD_aa_rr_Source")]
        public void LD_rr_r_returns_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(reg == "HL" ? 16 : 20, states);
        }
    }
}