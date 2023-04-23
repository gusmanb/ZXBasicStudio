using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_aHL_n_tests : InstructionsExecutionTestsBase
    {
        private const byte LD_aHL_n_opcode = 0x36;

        [Test]
        public void LD_aHL_n_loads_value_in_memory()
        {
            var address = Fixture.Create<ushort>();
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();

            ProcessorAgent.Memory[address] = oldValue;
            Registers.HL = address.ToShort();

            Execute(LD_aHL_n_opcode, null, newValue);

            Assert.AreEqual(newValue, ProcessorAgent.Memory[address]);
        }

        [Test]
        public void LD_aHL_n_does_not_modify_flags()
        {
            AssertNoFlagsAreModified(LD_aHL_n_opcode);
        }

        [Test]
        public void LD_aHL_n_returns_proper_T_states()
        {
            var states = Execute(LD_aHL_n_opcode);
            Assert.AreEqual(10, states);
        }
    }
}