using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class RRCA_tests : InstructionsExecutionTestsBase
    {
        private const byte RRCA_opcode = 0x0F;

        [Test]
        public void RRCA_rotates_byte_correctly()
        {
            var values = new byte[] { 0x82, 0x41, 0xA0, 0x50, 0x28, 0x14, 0x0A, 0x05 };
            Registers.A = 0x05;

            for(var i = 0; i < values.Length; i++)
            {
                Execute(RRCA_opcode);
                Assert.AreEqual(values[i], Registers.A);
            }
        }

        [Test]
        public void RRCA_sets_CF_correctly()
        {
            Registers.A = 0x06;

            Execute(RRCA_opcode);
            Assert.AreEqual(0, Registers.CF);

            Execute(RRCA_opcode);
            Assert.AreEqual(1, Registers.CF);

            Execute(RRCA_opcode);
            Assert.AreEqual(1, Registers.CF);

            Execute(RRCA_opcode);
            Assert.AreEqual(0, Registers.CF);
        }

        [Test]
        public void RRCA_resets_H_and_N()
        {
            AssertResetsFlags(RRCA_opcode, null, "H", "N");
        }

        [Test]
        public void RRCA_does_not_change_SF_ZF_PF()
        {
            AssertDoesNotChangeFlags(RRCA_opcode, null, "S", "Z", "P");
        }

        [Test]
        [TestCase(0x00)]
        [TestCase(0xD7)]
        [TestCase(0x28)]
        [TestCase(0xFF)]
        public void RRCA_sets_bits_3_and_5_from_A(int value)
        {
            Registers.A = (byte)value;
            Execute(RRCA_opcode);
            Assert.AreEqual(Registers.A.GetBit(3), Registers.Flag3);
            Assert.AreEqual(Registers.A.GetBit(5), Registers.Flag5);
        }

        [Test]
        public void RRCA_returns_proper_T_states()
        {
            var states = Execute(RRCA_opcode);
            Assert.AreEqual(4, states);
        }
    }
}