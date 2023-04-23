using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class RLCA_tests : InstructionsExecutionTestsBase
    {
        private const byte RLCA_opcode = 0x07;

        [Test]
        public void RLCA_rotates_byte_correctly()
        {
            var values = new byte[] {0xA, 0x14, 0x28, 0x50, 0xA0, 0x41, 0x82, 0x05};
            Registers.A = 0x05;

            for(var i = 0; i < values.Length; i++)
            {
                Execute(RLCA_opcode);
                Assert.AreEqual(values[i], Registers.A);
            }
        }

        [Test]
        public void RLCA_sets_CF_correctly()
        {
            Registers.A = 0x60;

            Execute(RLCA_opcode);
            Assert.AreEqual(0, Registers.CF);

            Execute(RLCA_opcode);
            Assert.AreEqual(1, Registers.CF);

            Execute(RLCA_opcode);
            Assert.AreEqual(1, Registers.CF);

            Execute(RLCA_opcode);
            Assert.AreEqual(0, Registers.CF);
        }

        [Test]
        public void RLCA_resets_H_and_N()
        {
            AssertResetsFlags(RLCA_opcode, null, "H", "N");
        }

        [Test]
        public void RLCA_does_not_change_SF_ZF_PF()
        {
            AssertDoesNotChangeFlags(RLCA_opcode, null, "S", "Z", "P");
        }

        [Test]
        [TestCase(0x00)]
        [TestCase(0xD7)]
        [TestCase(0x28)]
        [TestCase(0xFF)]
        public void RLCA_sets_bits_3_and_5_from_A(int value)
        {
            Registers.A = (byte)value;
            Execute(RLCA_opcode);
            Assert.AreEqual(Registers.A.GetBit(3), Registers.Flag3);
            Assert.AreEqual(Registers.A.GetBit(5), Registers.Flag5);
        }

        [Test]
        public void RLCA_returns_proper_T_states()
        {
            var states = Execute(RLCA_opcode);
            Assert.AreEqual(4, states);
        }
    }
}