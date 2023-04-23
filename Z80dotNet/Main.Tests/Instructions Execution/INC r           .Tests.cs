using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class INC_r_tests : InstructionsExecutionTestsBase
    {
        public static object[] INC_r_Source =
        {
            new object[] {"A",   (byte)0x3C, (byte?)null},
            new object[] {"B",   (byte)0x04, (byte?)null},
            new object[] {"C",   (byte)0x0C, (byte?)null},
            new object[] {"D",   (byte)0x14, (byte?)null},
            new object[] {"E",   (byte)0x1C, (byte?)null},
            new object[] {"H",   (byte)0x24, (byte?)null},
            new object[] {"L",   (byte)0x2C, (byte?)null},
            new object[] {"IXH", (byte)0x24, (byte?)0xDD},
            new object[] {"IXL", (byte)0x2C, (byte?)0xDD},
            new object[] {"IYH", (byte)0x24, (byte?)0xFD},
            new object[] {"IYL", (byte)0x2C, (byte?)0xFD}
        };

        [Test]
        [TestCaseSource("INC_r_Source")]
        public void INC_r_increases_value_appropriately(string reg, byte opcode, byte? prefix)
        {
            SetReg(reg, 0xFE);
            Execute(opcode, prefix);
            Assert.AreEqual(0xFF, GetReg<byte>(reg));

            Execute(opcode, prefix);
            Assert.AreEqual(0x00, GetReg<byte>(reg));

            Execute(opcode, prefix);
            Assert.AreEqual(0x01, GetReg<byte>(reg));
        }

        [Test]
        [TestCaseSource("INC_r_Source")]
        public void INC_r_sets_SF_appropriately(string reg, byte opcode, byte? prefix)
        {
            SetReg(reg, 0xFD);

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
        [TestCaseSource("INC_r_Source")]
        public void INC_r_sets_ZF_appropriately(string reg, byte opcode, byte? prefix)
        {
            SetReg(reg, 0xFD);

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
        [TestCaseSource("INC_r_Source")]
        public void INC_r_sets_HF_appropriately(string reg, byte opcode, byte? prefix)
        {
            foreach(byte b in new byte[] { 0x0E, 0x7E, 0xFE }) 
            {
                SetReg(reg, b);

                Execute(opcode, prefix);
                Assert.AreEqual(0, Registers.HF);

                Execute(opcode, prefix);
                Assert.AreEqual(1, Registers.HF);

                Execute(opcode, prefix);
                Assert.AreEqual(0, Registers.HF);
            }
        }

        [Test]
        [TestCaseSource("INC_r_Source")]
        public void INC_r_sets_PF_appropriately(string reg, byte opcode, byte? prefix)
        {
            SetReg(reg, 0x7E);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.PF);

            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.PF);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.PF);
        }

        [Test]
        [TestCaseSource("INC_r_Source")]
        public void INC_r_resets_NF(string reg, byte opcode, byte? prefix)
        {
            AssertResetsFlags(opcode, prefix, "N");
        }

        [Test]
        [TestCaseSource("INC_r_Source")]
        public void INC_r_does_not_change_CF(string reg, byte opcode, byte? prefix)
        {
            var randomValues = Fixture.Create<byte[]>();

            foreach (var value in randomValues)
            {
                SetReg(reg, value);

                Registers.CF = 0;
                Execute(opcode, prefix);
                Assert.AreEqual(0, Registers.CF);

                Registers.CF = 1;
                Execute(opcode, prefix);
                Assert.AreEqual(1, Registers.CF);
            }
        }

        [Test]
        [TestCaseSource("INC_r_Source")]
        public void INC_r_sets_bits_3_and_5_from_result(string reg, byte opcode, byte? prefix)
        {
            SetReg(reg, ((byte)0).WithBit(3, 1).WithBit(5, 0));
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.Flag3);
            Assert.AreEqual(0, Registers.Flag5);

            SetReg(reg, ((byte)0).WithBit(3, 0).WithBit(5, 1));
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.Flag3);
            Assert.AreEqual(1, Registers.Flag5);
        }

        [Test]
        [TestCaseSource("INC_r_Source")]
        public void INC_r_returns_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(IfIndexRegister(reg, 8, @else: 4), states);
        }
    }
}
