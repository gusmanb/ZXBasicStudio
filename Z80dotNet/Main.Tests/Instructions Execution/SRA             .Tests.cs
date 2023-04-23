using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class SRA_tests : InstructionsExecutionTestsBase
    {
        static SRA_tests()
        {
            SRA_Source = GetBitInstructionsSource(0x28, includeLoadReg: true, loopSevenBits: false);
        }

        public static object[] SRA_Source;

        private byte offset;
        
        [Test]
        [TestCaseSource("SRA_Source")]
        public void SRA_shifts_negative_byte_and_loads_register_correctly(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var values = new byte[] { 0xC0, 0xE0, 0xF0, 0xF8, 0xFC, 0xFE, 0xFF };
            SetupRegOrMem(reg, 0x80, offset);

            for(var i = 0; i < values.Length; i++)
            {
                Registers.CF = 1;
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(values[i], ValueOfRegOrMem(reg, offset));
                if(!string.IsNullOrEmpty(destReg))
                    Assert.AreEqual(values[i], ValueOfRegOrMem(destReg, offset));
            }
        }

        [Test]
        [TestCaseSource("SRA_Source")]
        public void SRA_shifts_positive_byte_and_loads_register_correctly(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var values = new byte[] { 0x20, 0x10, 0x08, 0x04, 0x02, 0x01, 0 };
            SetupRegOrMem(reg, 0x40, offset);

            for(var i = 0; i < values.Length; i++)
            {
                Registers.CF = 1;
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(values[i], ValueOfRegOrMem(reg, offset));
            }
        }

        [Test]
        [TestCaseSource("SRA_Source")]
        public void SRA_sets_CF_from_bit_0(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            SetupRegOrMem(reg, (byte)(Fixture.Create<byte>() | 0x01), offset);
            Registers.CF = 0;
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(1, Registers.CF);

            SetupRegOrMem(reg, (byte)(Fixture.Create<byte>() & 0xFE), offset);
            Registers.CF = 1;
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, Registers.CF);
        }

        [Test]
        [TestCaseSource("SRA_Source")]
        public void SRA_resets_H_and_N(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            AssertResetsFlags(() => ExecuteBit(opcode, prefix, offset), opcode, prefix, "H", "N");
        }

        [Test]
        [TestCaseSource("SRA_Source")]
        public void SRA_sets_SF_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            SetupRegOrMem(reg, (byte)(Fixture.Create<byte>() | 0x80), offset);
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(1, Registers.SF);

            SetupRegOrMem(reg, (byte)(Fixture.Create<byte>() & 0x7F), offset);
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, Registers.SF);
        }

        [Test]
        [TestCaseSource("SRA_Source")]
        public void SRA_sets_ZF_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            for(int i=0; i<256; i++) 
            {
                SetupRegOrMem(reg, (byte)i, offset);
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(ValueOfRegOrMem(reg, offset)==0, (bool)Registers.ZF);
            }
        }

        [Test]
        [TestCaseSource("SRA_Source")]
        public void SRA_sets_PV_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            for(int i=0; i<256; i++) 
            {
                SetupRegOrMem(reg, (byte)i, offset);
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(Parity[ValueOfRegOrMem(reg, offset)], Registers.PF);
            }
        }

        [Test]
        [TestCaseSource("SRA_Source")]
        public void SRA_sets_bits_3_and_5_from_result(string reg, string destReg, byte opcode, byte? prefix, int bit)
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
        [TestCaseSource("SRA_Source")]
        public void SRA_returns_proper_T_states(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var states = ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(reg == "(HL)" ? 15 : reg.StartsWith("(I") ? 23 : 8, states);
        }
    }
}