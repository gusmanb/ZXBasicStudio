using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_A_aa_tests : InstructionsExecutionTestsBase
    {
        private const byte LD_A_aa_opcode = 0x3A;

        [Test]
        public void LD_A_aa_loads_value_from_memory()
        {
            var address = Fixture.Create<ushort>();
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();

            Registers.A = oldValue;
            ProcessorAgent.Memory[address] = newValue;

            Execute(LD_A_aa_opcode, nextFetches: new[] {address.GetLowByte(), address.GetHighByte()});

            Assert.AreEqual(newValue, Registers.A);
        }

        [Test]
        public void LD_A_aa_does_not_modify_flags()
        {
            AssertNoFlagsAreModified(LD_A_aa_opcode);
        }

        [Test]
        public void LD_rr_r_returns_proper_T_states()
        {
            var states = Execute(LD_A_aa_opcode);
            Assert.AreEqual(13, states);
        }
    }
}