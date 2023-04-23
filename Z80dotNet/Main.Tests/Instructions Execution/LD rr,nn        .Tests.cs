using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_rr_nn_tests : InstructionsExecutionTestsBase
    {
        public static object[] LD_rr_nn_Source =
        {
            new object[] {"BC", (byte)0x01, (byte?)null},
            new object[] {"DE", (byte)0x11, (byte?)null},
            new object[] {"HL", (byte)0x21, (byte?)null},
            new object[] {"SP", (byte)0x31, (byte?)null},
            new object[] {"IX", (byte)0x21, (byte?)0xDD},
            new object[] {"IY", (byte)0x21, (byte?)0xFD}
        };

        [Test]
        [TestCaseSource("LD_rr_nn_Source")]
        public void LD_rr_nn_loads_register_with_value(string reg, byte opcode, byte? prefix)
        {
            var oldValue = Fixture.Create<short>();
            var newValue = Fixture.Create<short>();

            SetReg(reg, oldValue);

            Execute(opcode, prefix, newValue.GetLowByte(), newValue.GetHighByte());

            Assert.AreEqual(newValue, GetReg<short>(reg));
        }

        [Test]
        [TestCaseSource("LD_rr_nn_Source")]
        public void LD_SP_nn_fires_FetchFinished_with_isLdSp_true(string reg, byte opcode, byte? prefix)
        {
            var eventFired = false;

            Sut.InstructionFetchFinished += (sender, e) =>
            {
                eventFired = true;
                Assert.True((reg=="SP" && e.IsLdSpInstruction) | (reg != "SP" && !e.IsLdSpInstruction));
            };

            Execute(opcode, prefix);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [TestCaseSource("LD_rr_nn_Source")]
        public void LD_rr_nn_do_not_modify_flags(string reg, byte opcode, byte? prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("LD_rr_nn_Source")]
        public void LD_rr_nn_returns_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(prefix.HasValue ? 14 : 10, states);
        }
    }
}
