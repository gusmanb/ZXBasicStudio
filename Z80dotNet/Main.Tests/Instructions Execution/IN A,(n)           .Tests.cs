using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class IN_A_n_tests : InstructionsExecutionTestsBase
    {
        private const byte IN_A_n_opcode = 0xDB;

        [Test]
        public void IN_A_n_reads_value_from_port()
        {
            var portNumber = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var oldValue = Fixture.Create<byte>();

            Registers.A = oldValue;
            SetPortValue(portNumber, value);

            Execute(IN_A_n_opcode, null, portNumber);

            Assert.AreEqual(value, Registers.A);
        }

        [Test]
        public void IN_A_n_does_not_modify_flags()
        {
            AssertDoesNotChangeFlags(IN_A_n_opcode);
        }

        [Test]
        public void IN_A_n_returns_proper_T_states()
        {
            var states = Execute(IN_A_n_opcode);
            Assert.AreEqual(11, states);
        }
    }
}