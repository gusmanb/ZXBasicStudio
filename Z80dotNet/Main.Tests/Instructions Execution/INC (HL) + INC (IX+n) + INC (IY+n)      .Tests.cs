using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class INC_aHL_IX_IY_plus_n_tests : InstructionsExecutionTestsBase
    {
        public static object[] INC_Source =
        {
            new object[] { "HL", (byte)0x34, (byte?)null },
            new object[] { "IX", (byte)0x34, (byte?)0xDD },
            new object[] { "IY", (byte)0x34, (byte?)0xFD }
        };

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_increases_value_appropriately(string reg, byte opcode, byte? prefix)
        {
            var oldValue = Fixture.Create<byte>();
            var offset = reg == "HL" ? (byte)0 : Fixture.Create<byte>();
            var address = Setup(reg, oldValue, offset);

            if(reg == "HL")
                Execute(opcode, prefix);
            else
                Execute(opcode, prefix, offset);

            AssertMemoryContents(address, oldValue.Inc());
        }

        private ushort Setup(string reg, byte value, byte offset = 0)
        {
            var address = Fixture.Create<ushort>();
            var actualAddress = address.Add(offset.ToSignedByte());
            ProcessorAgent.Memory[actualAddress] = value;
            SetReg(reg, address.ToShort());
            return actualAddress;
        }

        private void AssertMemoryContents(ushort address, byte expected)
        {
            Assert.AreEqual(expected, ProcessorAgent.Memory[address]);
        }

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_sets_SF_appropriately(string reg, byte opcode, byte? prefix)
        {
            Setup(reg, 0xFD);

            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.SF);

            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.SF);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.SF);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.SF);
        }

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_sets_ZF_appropriately(string reg, byte opcode, byte? prefix)
        {
            Setup(reg, 0xFD);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.ZF);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.ZF);

            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.ZF);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.ZF);
        }

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_sets_HF_appropriately(string reg, byte opcode, byte? prefix)
        {
            foreach(byte b in new byte[] { 0x0E, 0x7E, 0xFE })
            {
                Setup(reg, b);

                Execute(opcode, prefix);
                Assert.AreEqual(0, Registers.HF);

                Execute(opcode, prefix);
                Assert.AreEqual(1, Registers.HF);

                Execute(opcode, prefix);
                Assert.AreEqual(0, Registers.HF);
            }
        }

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_sets_PF_appropriately(string reg, byte opcode, byte? prefix)
        {
            Setup(reg, 0x7E);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.PF);

            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.PF);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.PF);
        }

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_resets_NF(string reg, byte opcode, byte? prefix)
        {
            AssertResetsFlags(opcode, prefix, "N");
        }

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_does_not_change_CF(string reg, byte opcode, byte? prefix)
        {
            var randomValues = Fixture.Create<byte[]>();

            foreach (var value in randomValues)
            {
                Setup(reg, value);

                Registers.CF = 0;
                Execute(opcode, prefix);
                Assert.AreEqual(0, Registers.CF);

                Registers.CF = 1;
                Execute(opcode, prefix);
                Assert.AreEqual(1, Registers.CF);
            }
        }

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_sets_bits_3_and_5_from_result(string reg, byte opcode, byte? prefix)
        {
            Setup(reg, ((byte)0).WithBit(3, 1).WithBit(5, 0));
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.Flag3);
            Assert.AreEqual(0, Registers.Flag5);

            Setup(reg, ((byte)0).WithBit(3, 0).WithBit(5, 1));
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.Flag3);
            Assert.AreEqual(1, Registers.Flag5);
        }

        [Test]
        [TestCaseSource("INC_Source")]
        public void INC_aHL_IX_IY_plus_n_returns_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(reg=="HL" ? 11 : 23, states);
        }
    }
}
