using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_A_I_R_tests : InstructionsExecutionTestsBase
    {
        private const byte prefix = 0xED;

        public static object[] LD_A_R_I_Source =
        {
            new object[] {"I", (byte)0x57},
            new object[] {"R", (byte)0x5F}
        };

        [Test]
        [TestCaseSource("LD_A_R_I_Source")]
        public void LD_A_I_R_loads_value_correctly(string reg, byte opcode)
        {
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();
            Registers.A = oldValue;
            SetReg(reg, newValue);

            Execute(opcode, prefix);

            //Account for R being increased on instruction execution
            if(reg == "R")
                newValue = newValue.Inc7Bits().Inc7Bits();

            Assert.AreEqual(newValue, Registers.A);
        }

        [Test]
        [TestCaseSource("LD_A_R_I_Source")]
        public void LD_A_I_R_returns_proper_T_states(string reg, byte opcode)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(9, states);
        }

        [Test]
        [TestCaseSource("LD_A_R_I_Source")]
        public void LD_A_I_R_sets_SF_properly(string reg, byte opcode)
        {
            for(int i=0; i<=255; i++)
            {
                var b = (byte)i;
                SetReg(reg, b);
                Execute(opcode, prefix);
                Assert.AreEqual(b >= 128, (bool)Registers.SF);
            }
        }

        [Test]
        [TestCaseSource("LD_A_R_I_Source")]
        public void LD_A_I_R_sets_ZF_properly(string reg, byte opcode)
        {
            for(int i=0; i<=255; i++)
            {
                var b = (byte)i;
                SetReg(reg, b);
                Execute(opcode, prefix);

                //Account for R being increased on instruction execution
                if(reg == "R")
                    b = b.Inc7Bits().Inc7Bits();

                Assert.AreEqual(b == 0, (bool)Registers.ZF);
            }
        }

        [Test]
        [TestCaseSource("LD_A_R_I_Source")]
        public void LD_A_I_R_sets_PF_from_IFF2(string reg, byte opcode)
        {
            SetReg(reg, Fixture.Create<byte>());

            Registers.IFF2 = 0;
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.PF);

            Registers.IFF2 = 1;
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.PF);
        }

        [Test]
        [TestCaseSource("LD_A_R_I_Source")]
        public void LD_A_I_R_resets_HF_and_NF_properly(string reg, byte opcode)
        {
            AssertResetsFlags(opcode, prefix, "H", "N");
        }

        [Test]
        [TestCaseSource("LD_A_R_I_Source")]
        public void LD_A_I_R_does_not_change_CF(string reg, byte opcode)
        {
            AssertDoesNotChangeFlags(opcode, prefix, "C");
        }

        [Test]
        [TestCaseSource("LD_A_R_I_Source")]
        public void LD_A_I_R_sets_flags_3_5_from_I(string reg, byte opcode)
        {
            SetReg(reg, ((byte)1).WithBit(3, 1).WithBit(5, 0));
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.Flag3);
            Assert.AreEqual(0, Registers.Flag5);

            SetReg(reg, ((byte)1).WithBit(3, 0).WithBit(5, 1));
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.Flag3);
            Assert.AreEqual(1, Registers.Flag5);
        }
    }
}