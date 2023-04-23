using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class CCF_tests : InstructionsExecutionTestsBase
    {
        private const byte CCF_opcode = 0x3F;

        [Test]
        public void CCF_complements_CF_correctly()
        {
            Registers.CF = 0;
            Execute(CCF_opcode);
            Assert.AreEqual(1, Registers.CF);

            Registers.CF = 1;
            Execute(CCF_opcode);
            Assert.AreEqual(0, Registers.CF);
        }

        [Test]
        public void CCF_sets_H_as_previous_carry()
        {
            Registers.CF = 0;
            Registers.HF = 1;
            Execute(CCF_opcode);
            Assert.AreEqual(0, Registers.HF);

            Registers.CF = 1;
            Registers.HF = 0;
            Execute(CCF_opcode);
            Assert.AreEqual(1, Registers.HF);
        }

        [Test]
        public void CCF_resets_N()
        {
            AssertResetsFlags(CCF_opcode, null, "N");
        }

        [Test]
        public void CCF_does_not_change_SF_ZF_PF()
        {
            AssertDoesNotChangeFlags(CCF_opcode, null, "S", "Z", "P");
        }

        [Test]
        [TestCase(0x00)]
        [TestCase(0x0F)]
        [TestCase(0xF0)]
        [TestCase(0xFF)]
        public void CCF_sets_bits_3_and_5_from_A(int value)
        {
            Registers.A = (byte)value;
            Execute(CCF_opcode);
            Assert.AreEqual(Registers.A.GetBit(3), Registers.Flag3);
            Assert.AreEqual(Registers.A.GetBit(5), Registers.Flag5);
        }

        [Test]
        public void CCF_returns_proper_T_states()
        {
            var states = Execute(CCF_opcode);
            Assert.AreEqual(4, states);
        }
    }
}