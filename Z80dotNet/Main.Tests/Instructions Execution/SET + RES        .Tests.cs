using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class SET_tests : InstructionsExecutionTestsBase
    {
        static SET_tests()
        {
            SET_Source = GetBitInstructionsSource(0xC0, includeLoadReg: true, loopSevenBits: true);
            RES_Source = GetBitInstructionsSource(0x80, includeLoadReg: true, loopSevenBits: true);
        }

        public static object[] SET_Source;
        public static object[] RES_Source;
        
        private byte offset;

        [SetUp]
        public void Setup()
        {
            offset = Fixture.Create<byte>();
        }

        [Test]
        [TestCaseSource("SET_Source")]
        public void SET_sets_bit_correctly(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var value = Fixture.Create<byte>().WithBit(bit, 0);
            SetupRegOrMem(reg, value, offset);
            ExecuteBit(opcode, prefix, offset);
            var expected = value.WithBit(bit, 1);
            var actual = ValueOfRegOrMem(reg, offset);
            Assert.AreEqual(expected, actual);
            if(!string.IsNullOrEmpty(destReg))
                Assert.AreEqual(expected, ValueOfRegOrMem(destReg, actual));
        }

        [Test]
        [TestCaseSource("RES_Source")]
        public void RES_resets_bit_correctly(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var value = Fixture.Create<byte>().WithBit(bit, 1);
            SetupRegOrMem(reg, value, offset);
            ExecuteBit(opcode, prefix, offset);
            var expected = value.WithBit(bit, 0);
            var actual = ValueOfRegOrMem(reg, offset);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCaseSource("SET_Source")]
        [TestCaseSource("RES_Source")]
        public void SET_RES_do_not_change_flags(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            AssertDoesNotChangeFlags(() => ExecuteBit(opcode, prefix, offset), opcode, prefix);
        }

        [Test]
        [TestCaseSource("SET_Source")]
        [TestCaseSource("RES_Source")]
        public void SET_RES_return_proper_T_states(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var states = ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(reg == "(HL)" ? 15 : reg.StartsWith("(I") ? 23 : 8, states);
        }
    }
}