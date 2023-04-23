using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_aa_A_tests : InstructionsExecutionTestsBase
    {
        private const byte LD_aa_A_opcode = 0x32;

        [Test]
        public void LD_aa_A_loads_value_in_memory()
        {
            var address = Fixture.Create<ushort>();
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();

            Registers.A = newValue;
            ProcessorAgent.Memory[address] = oldValue;

            Execute(LD_aa_A_opcode, nextFetches: new[] {address.GetLowByte(), address.GetHighByte()});

            Assert.AreEqual(newValue, ProcessorAgent.Memory[address]);
        }

        [Test]
        public void LD_aa_A_does_not_modify_flags()
        {
            AssertNoFlagsAreModified(LD_aa_A_opcode);
        }

        [Test]
        public void LD_rr_r_returns_proper_T_states()
        {
            var states = Execute(LD_aa_A_opcode);
            Assert.AreEqual(13, states);
        }
    }
}