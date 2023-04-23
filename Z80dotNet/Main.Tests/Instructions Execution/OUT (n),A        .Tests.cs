using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class OUT_A_n_tests : InstructionsExecutionTestsBase
    {
        private const byte OUT_A_n_opcode = 0xD3;

        [Test]
        public void OUT_A_n_reads_value_from_port()
        {
            var portNumber = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var oldValue = Fixture.Create<byte>();

            Registers.A = value;
            SetPortValue(portNumber, oldValue);

            Execute(OUT_A_n_opcode, null, portNumber);

            Assert.AreEqual(value, GetPortValue(portNumber));
        }

        [Test]
        public void OUT_A_n_does_not_modify_flags()
        {
            AssertDoesNotChangeFlags(OUT_A_n_opcode);
        }

        [Test]
        public void OUT_A_n_returns_proper_T_states()
        {
            var states = Execute(OUT_A_n_opcode);
            Assert.AreEqual(11, states);
        }
    }
}