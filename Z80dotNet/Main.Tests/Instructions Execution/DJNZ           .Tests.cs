using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class DJNZ_tests : InstructionsExecutionTestsBase
    {
        private const byte DJNZ_opcode = 0x10;

        [Test]
        public void DJNZ_decreases_B()
        {
            var value = Fixture.Create<byte>();
            Registers.B = value;

            Execute(DJNZ_opcode);

            Assert.AreEqual(value.Dec(), Registers.B);
        }

        [Test]
        public void DJNZ_does_not_jump_if_B_decreases_to_zero()
        {
            var instructionAddress = Fixture.Create<ushort>();

            Registers.B = 1;
            ExecuteAt(instructionAddress, DJNZ_opcode, nextFetches: new[] {Fixture.Create<byte>()});

            Assert.AreEqual(instructionAddress + 2, Registers.PC);
        }

        [Test]
        public void DJNZ_returns_proper_T_states_when_no_jump_is_done()
        {
            Registers.B = 1;
            var states = Execute(DJNZ_opcode);

            Assert.AreEqual(8, states);
        }

        [Test]
        public void DJNZ_jumps_to_proper_address_if_B_does_not_decrease_to_zero()
        {
            var instructionAddress = Fixture.Create<ushort>();

            Registers.B = 0;
            ExecuteAt(instructionAddress, DJNZ_opcode, nextFetches: new byte[] {0x7F});
            Assert.AreEqual(instructionAddress.Add(129), Registers.PC);

            Registers.B = 0;
            ExecuteAt(instructionAddress, DJNZ_opcode, nextFetches: new byte[] {0x80});
            Assert.AreEqual(instructionAddress.Sub(126), Registers.PC);
        }

        [Test]
        public void DJNZ_returns_proper_T_states_when_jump_is_done()
        {
            Registers.B = 0;
            var states = Execute(DJNZ_opcode);

            Assert.AreEqual(13, states);
        }

        [Test]
        public void DJNZ_does_not_modify_flags()
        {
            AssertNoFlagsAreModified(DJNZ_opcode);
        }
    }
}