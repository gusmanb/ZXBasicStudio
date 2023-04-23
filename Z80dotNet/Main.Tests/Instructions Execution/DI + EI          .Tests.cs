using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class DI_EI_tests : InstructionsExecutionTestsBase
    {
        private const byte DI_opcode = 0xF3;
        private const byte EI_opcode = 0xFB;

        [Test]
        public void DI_resets_IFF()
        {
            Registers.IFF1 = 1;
            Registers.IFF2 = 1;

            Execute(DI_opcode);

            Assert.AreEqual(0, Registers.IFF1);
            Assert.AreEqual(0, Registers.IFF2);
        }

        [Test]
        public void EI_sets_IFF()
        {
            Registers.IFF1 = 0;
            Registers.IFF2 = 0;

            Execute(EI_opcode);

            Assert.AreEqual(1, Registers.IFF1);
            Assert.AreEqual(1, Registers.IFF2);
        }

        [Test]
        [TestCase(EI_opcode)]
        [TestCase(DI_opcode)]
        public void EI_fires_FetchFinished_with_isEiOrDi_true(byte opcode)
        {
            var eventFired = false;

            Sut.InstructionFetchFinished += (sender, e) =>
            {
                eventFired = true;
                Assert.True(e.IsEiOrDiInstruction);
            };

            Execute(opcode);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [TestCase(DI_opcode)]
        [TestCase(EI_opcode)]
        public void DI_EI_do_not_change_flags(byte opcode)
        {
            AssertNoFlagsAreModified(opcode);
        }

        [Test]
        [TestCase(DI_opcode)]
        [TestCase(EI_opcode)]
        public void DI_EI_return_proper_T_states(byte opcode)
        {
            var states = Execute(opcode);
            Assert.AreEqual(4, states);
        }
    }
}