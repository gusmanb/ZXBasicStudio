using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class SLL_tests : InstructionsExecutionTestsBase
    {
        static SLL_tests()
        {
            SLL_Source = GetBitInstructionsSource(0x30, includeLoadReg: true, loopSevenBits: false);
        }

        public static object[] SLL_Source;

        private byte offset;
        
        [Test]
        [TestCaseSource("SLL_Source")]
        public void SLL_shifts_byte_and_loads_register_correctly(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var values = new byte[] { 0x03, 0x07, 0x0F, 0x1F, 0x3F, 0x7F, 0xFF };
            SetupRegOrMem(reg, 0x01, offset);

            for(var i = 0; i < values.Length; i++)
            {
                Registers.CF = 0;
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(values[i], ValueOfRegOrMem(reg, offset));
                if(!string.IsNullOrEmpty(destReg))
                    Assert.AreEqual(values[i], ValueOfRegOrMem(destReg, offset));
            }
        }

        [Test]
        [TestCaseSource("SLL_Source")]
        public void SLL_sets_CF_from_bit_7(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            SetupRegOrMem(reg, (byte)(Fixture.Create<byte>() | 0x80), offset);
            Registers.CF = 0;
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(1, Registers.CF);

            SetupRegOrMem(reg, (byte)(Fixture.Create<byte>() & 0x7F), offset);
            Registers.CF = 1;
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, Registers.CF);
        }

        [Test]
        [TestCaseSource("SLL_Source")]
        public void SLL_resets_H_and_N(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            AssertResetsFlags(() => ExecuteBit(opcode, prefix, offset), opcode, prefix, "H", "N");
        }

        [Test]
        [TestCaseSource("SLL_Source")]
        public void SLL_sets_SF_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            SetupRegOrMem(reg, 0x20, offset);

            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, Registers.SF);

            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(1, Registers.SF);

            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, Registers.SF);
        }

        [Test]
        [TestCaseSource("SLL_Source")]
        public void SLL_sets_ZF_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            for(int i=0; i<256; i++) 
            {
                SetupRegOrMem(reg, (byte)i, offset);
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(ValueOfRegOrMem(reg, offset)==0, (bool)Registers.ZF);
            }
        }

        [Test]
        [TestCaseSource("SLL_Source")]
        public void SLL_sets_PV_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            for(int i=0; i<256; i++) 
            {
                SetupRegOrMem(reg, (byte)i, offset);
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(Parity[ValueOfRegOrMem(reg, offset)], Registers.PF);
            }
        }

        [Test]
        [TestCaseSource("SLL_Source")]
        public void SLL_sets_bits_3_and_5_from_result(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            foreach (var b in new byte[] {0x00, 0xD7, 0x28, 0xFF})
            {
                SetupRegOrMem(reg, b, offset);
                ExecuteBit(opcode, prefix, offset);
                var value = ValueOfRegOrMem(reg, offset);
                Assert.AreEqual(value.GetBit(3), Registers.Flag3);
                Assert.AreEqual(value.GetBit(5), Registers.Flag5);
            }
        }

        [Test]
        [TestCaseSource("SLL_Source")]
        public void SLL_returns_proper_T_states(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var states = ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(reg == "(HL)" ? 15 : reg.StartsWith("(I") ? 23 : 8, states);
        }
    }
}