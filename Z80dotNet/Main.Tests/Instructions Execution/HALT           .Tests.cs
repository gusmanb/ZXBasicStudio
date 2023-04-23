using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class HALT_tests : InstructionsExecutionTestsBase
    {
        private const byte HALT_opcode = 0x76;

        [Test]
        public void HALT_fires_fetch_finished_with_isHalt_set()
        {
            var eventFired = false;

            Sut.InstructionFetchFinished += (sender, e) =>
            {
                eventFired = true;
                Assert.True(e.IsHaltInstruction);
            };

            Execute(HALT_opcode);

            Assert.IsTrue(eventFired);
        }

        [Test]
        public void HALT_does_not_modify_flags()
        {
            AssertDoesNotChangeFlags(HALT_opcode);
        }

        [Test]
        public void HALT_returns_proper_T_states()
        {
            var states = Execute(HALT_opcode);
            Assert.AreEqual(4, states);
        }
    }
}