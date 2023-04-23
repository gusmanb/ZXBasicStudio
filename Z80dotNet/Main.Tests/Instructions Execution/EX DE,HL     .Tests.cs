using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class EX_DE_HL_tests : InstructionsExecutionTestsBase
    {
        private const byte EX_DE_HL_opcode = 0xEB;

        [Test]
        public void EX_DE_HL_exchanges_the_AF_registers()
        {
            var DE = Fixture.Create<short>();
            var HL = Fixture.Create<short>();

            Registers.DE = DE;
            Registers.HL = HL;

            Execute(EX_DE_HL_opcode);

            Assert.AreEqual(HL, Registers.DE);
            Assert.AreEqual(DE, Registers.HL);
        }

        [Test]
        public void EX_DE_HL_does_not_change_flags()
        {
            AssertNoFlagsAreModified(EX_DE_HL_opcode);
        }

        [Test]
        public void EX_DE_HL_returns_proper_T_states()
        {
            var states = Execute(EX_DE_HL_opcode);
            Assert.AreEqual(4, states);
        }
    }

}