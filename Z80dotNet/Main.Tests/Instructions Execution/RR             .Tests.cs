using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class RR_tests : InstructionsExecutionTestsBase
    {
        static RR_tests()
        {
            RR_Source = GetBitInstructionsSource(0x18, includeLoadReg: true, loopSevenBits: false);
        }

        public static object[] RR_Source;

        private byte offset;

        [SetUp]
        public void Setup()
        {
            offset = Fixture.Create<byte>();
        }
        
        [Test]
        [TestCaseSource("RR_Source")]
        public void RR_rotates_byte_and_loads_register_correctly(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var values = new byte[] { 0x60, 0x30, 0x18, 0xC, 0x6, 0x3, 0x1, 0x0 };
            SetupRegOrMem(reg, 0xC0, offset);

            for(var i = 0; i < values.Length; i++)
            {
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(values[i], ValueOfRegOrMem(reg, offset) & 0x7F);
                if(!string.IsNullOrEmpty(destReg))
                    Assert.AreEqual(values[i], ValueOfRegOrMem(destReg, offset) & 0x7F);
            }
        }

        [Test]
        [TestCaseSource("RR_Source")]
        public void RR_sets_bit_7_from_CF(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            SetupRegOrMem(reg, (byte)(Fixture.Create<byte>() | 0X80), offset);
            Registers.CF = 0;
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, ValueOfRegOrMem(reg, offset).GetBit(7));

            SetupRegOrMem(reg, (byte)(Fixture.Create<byte>() & 0x7f), offset);
            Registers.CF = 1;
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(1, ValueOfRegOrMem(reg, offset).GetBit(7));
        }

        [Test]
        [TestCaseSource("RR_Source")]
        public void RR_sets_CF_correctly(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            SetupRegOrMem(reg, 0x06, offset);

            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, Registers.CF);

            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(1, Registers.CF);

            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(1, Registers.CF);

            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, Registers.CF);
        }

        [Test]
        [TestCaseSource("RR_Source")]
        public void RR_resets_H_and_N(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            AssertResetsFlags(() => ExecuteBit(opcode, prefix, offset), opcode, prefix, "H", "N");
        }

        [Test]
        [TestCaseSource("RR_Source")]
        public void RR_sets_SF_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            Registers.CF = 1;
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(1, Registers.SF);

            Registers.CF = 0;
            ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(0, Registers.SF);
        }

        [Test]
        [TestCaseSource("RR_Source")]
        public void RR_sets_ZF_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            for(int i=0; i<256; i++) 
            {
                SetupRegOrMem(reg, (byte)i, offset);
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(ValueOfRegOrMem(reg, offset)==0, (bool)Registers.ZF);
            }
        }

        [Test]
        [TestCaseSource("RR_Source")]
        public void RR_sets_PV_appropriately(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            for(int i=0; i<256; i++) 
            {
                SetupRegOrMem(reg, (byte)i, offset);
                ExecuteBit(opcode, prefix, offset);
                Assert.AreEqual(Parity[ValueOfRegOrMem(reg, offset)], Registers.PF);
            }
        }

        [Test]
        [TestCaseSource("RR_Source")]
        public void RR_sets_bits_3_and_5_from_result(string reg, string destReg, byte opcode, byte? prefix, int bit)
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
        [TestCaseSource("RR_Source")]
        public void RR_returns_proper_T_states(string reg, string destReg, byte opcode, byte? prefix, int bit)
        {
            var states = ExecuteBit(opcode, prefix, offset);
            Assert.AreEqual(reg == "(HL)" ? 15 : reg.StartsWith("(I") ? 23 : 8, states);
        }
    }
}