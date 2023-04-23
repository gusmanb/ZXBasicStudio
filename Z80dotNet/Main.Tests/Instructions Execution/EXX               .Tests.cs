using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class EXX_tests : InstructionsExecutionTestsBase
    {
        private const byte EXX_opcode = 0xD9;

        [Test]
        public void EXX_exchanges_registers_correctly()
        {
            var BC = Fixture.Create<short>();
            var DE = Fixture.Create<short>();
            var HL = Fixture.Create<short>();
            var altBC = Fixture.Create<short>();
            var altDE = Fixture.Create<short>();
            var altHL = Fixture.Create<short>();

            Registers.BC = BC;
            Registers.DE = DE;
            Registers.HL = HL;
            Registers.Alternate.BC = altBC;
            Registers.Alternate.DE = altDE;
            Registers.Alternate.HL = altHL;

            Execute(EXX_opcode);

            Assert.AreEqual(altBC, Registers.BC);
            Assert.AreEqual(altDE, Registers.DE);
            Assert.AreEqual(altHL, Registers.HL);
            Assert.AreEqual(BC, Registers.Alternate.BC);
            Assert.AreEqual(DE, Registers.Alternate.DE);
            Assert.AreEqual(HL, Registers.Alternate.HL);
        }

        [Test]
        public void EXX_does_not_change_flags()
        {
            AssertNoFlagsAreModified(EXX_opcode);
        }

        [Test]
        public void EXX_returns_proper_T_states()
        {
            var states = Execute(EXX_opcode);
            Assert.AreEqual(4, states);
        }
    }
}