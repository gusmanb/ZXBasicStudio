using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Collections.Generic;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class SUB_SBC_CPI_CPD_CP_r_tests : InstructionsExecutionTestsBase
    {
        private const byte cpidrPrefix = 0xED;
        private readonly byte[] cpidrOpcodes = new byte[] {0xA1, 0xA9, 0xB1, 0xFF};

        static SUB_SBC_CPI_CPD_CP_r_tests()
        {
            var combinations = new List<object[]>();
            var CP_combinations = new List<object[]>();

            var registers = new[] {"B", "C", "D", "E", "H", "L", "(HL)", "n", "IXH", "IXL", "IYH", "IYL","(IX+n)","(IY+n)"};
            for(var src = 0; src<registers.Length; src++)
            {
                var reg = registers[src];
                var i = src;
                byte? prefix = null;

                ModifyTestCaseCreationForIndexRegs(reg, ref i, out prefix);

                var SUB_opcode = (byte)(i==7 ? 0xD6 : (i | 0x90));
                var SBC_opcode = (byte)(i==7 ? 0xDE : (i | 0x98));
                var CP_opcode = (byte)(i==7 ? 0xFE : (i | 0xB8));
                combinations.Add(new object[] {reg, SUB_opcode, 0, prefix});
                combinations.Add(new object[] {reg, SBC_opcode, 0, prefix});
                combinations.Add(new object[] {reg, SBC_opcode, 1, prefix});
                CP_combinations.Add(new object[] {reg, CP_opcode, 0, prefix});
            }

            SUB_SBC_A_r_Source = combinations.ToArray();
            CP_r_Source = CP_combinations.ToArray();

            CPID_R_Source = CPI_Source.Concat(CPD_Source).Concat(CPIR_Source).Concat(CPDR_Source).ToArray();
        }

        public static object[] SUB_SBC_A_r_Source;
        public static object[] CP_r_Source;
        public static object[] CPID_R_Source;

        public static object[] SUB_SBC_A_A_Source =
        {
            new object[] {"A", (byte)0x97, 0, (byte?)null},
            new object[] {"A", (byte)0x9F, 0, (byte?)null},
            new object[] {"A", (byte)0x9F, 1, (byte?)null},
        };

        public static object[] CP_A_Source =
        {
            new object[] {"A", (byte)0xBF, 0, (byte?)null},
        };

        public static object[] CPI_Source =
        {
            new object[] {"CPI", (byte)0xA1, 0, (byte?)null},
        };

        public static object[] CPD_Source =
        {
            new object[] {"CPD", (byte)0xA9, 0, (byte?)null},
        };

        public static object[] CPIR_Source =
        {
            new object[] {"CPIR", (byte)0xB1, 0, (byte?)null},
        };

        public static object[] CPDR_Source =
        {
            new object[] {"CPDR", (byte)0xFF, 0, (byte?)null}, //can't use B9 because it's "CP C" without prefix
        };


        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        [TestCaseSource("SUB_SBC_A_A_Source")]
        public void SUB_SBC_A_r_substracts_both_registers_with_or_without_carry(string src, byte opcode, int cf, byte? prefix)
        {
            var oldValue = Fixture.Create<byte>();
            var valueToSubstract = src=="A" ? oldValue : Fixture.Create<byte>();

            Setup(src, oldValue, valueToSubstract, cf);
            Execute(opcode, prefix);

            Assert.AreEqual(oldValue.Sub(valueToSubstract + cf), Registers.A);
        }

        [Test]
        [TestCaseSource("CP_r_Source")]
        [TestCaseSource("CP_A_Source")]
        [TestCaseSource("CPID_R_Source")]
        public void CPs_do_not_change_A(string src, byte opcode, int cf, byte? prefix)
        {
            var oldValue = Fixture.Create<byte>();
            var argument = Fixture.Create<byte>();

            Setup(src, oldValue, argument, cf);
            Execute(opcode, prefix);

            Assert.AreEqual(oldValue, Registers.A);
        }

        private void Setup(string src, byte oldValue, byte valueToSubstract, int cf = 0)
        {
            Registers.A = oldValue;
            Registers.CF = cf;

            if(src == "n") 
            {
                SetMemoryContentsAt(1, valueToSubstract);
            }
            else if(src == "(HL)" || src.StartsWith("CP")) 
            {
                var address = Fixture.Create<ushort>();
                ProcessorAgent.Memory[address] = valueToSubstract;
                Registers.HL = address.ToShort();
            }
            else if(src.StartsWith("(I")) 
            {
                var address = Fixture.Create<ushort>();
                var offset = Fixture.Create<byte>();
                var realAddress = address.Add(offset.ToSignedByte());
                ProcessorAgent.Memory[realAddress] = valueToSubstract;
                SetMemoryContentsAt(2, offset);
                SetReg(src.Substring(1,2), address.ToShort());
            }
            else if(src != "A")
            {
                SetReg(src, valueToSubstract);
            }
        }

        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        [TestCaseSource("CP_r_Source")]
        [TestCaseSource("CPID_R_Source")]
        public void SUB_SBC_CPs_set_SF_appropriately(string src, byte opcode, int cf, byte? prefix)
        {
            Setup(src, 0x02, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.SF);

            Setup(src, 0x01, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.SF);

            Setup(src, 0x00, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.SF);

            Setup(src, 0xFF, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.SF);
        }

        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        [TestCaseSource("CP_r_Source")]
        [TestCaseSource("CPID_R_Source")]
        public void SUB_SBC_CPs_set_ZF_appropriately(string src, byte opcode, int cf, byte? prefix)
        {
            Setup(src, 0x03, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.ZF);

            Setup(src, 0x02, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.ZF);

            Setup(src, 0x01, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.ZF);

            Setup(src, 0x00, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.ZF);
        }

        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        [TestCaseSource("CP_r_Source")]
        [TestCaseSource("CPID_R_Source")]
        public void SUB_SBC_CPs_set_HF_appropriately(string src, byte opcode, int cf, byte? prefix)
        {
            foreach(byte b in new byte[] { 0x11, 0x81, 0xF1 }) 
            {
                Setup(src, b, 1);
                Execute(opcode, prefix);
                Assert.AreEqual(0, Registers.HF);

                Setup(src, (byte)(b-1), 1);
                Execute(opcode, prefix);
                Assert.AreEqual(1, Registers.HF);

                Setup(src, (byte)(b-2), 1);
                Execute(opcode, prefix);
                Assert.AreEqual(0, Registers.HF);
            }
        }

        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        [TestCaseSource("CP_r_Source")]
        public void SUB_SBC_CP_r_sets_PF_appropriately(string src, byte opcode, int cf, byte? prefix)
        {
            //http://stackoverflow.com/a/8037485/4574

            TestPF(src, opcode, 127, 0, 0, prefix);
            TestPF(src, opcode, 127, 1, 0, prefix);
            TestPF(src, opcode, 127, 127, 0, prefix);
            TestPF(src, opcode, 127, 128, 1, prefix);
            TestPF(src, opcode, 127, 129, 1, prefix);
            TestPF(src, opcode, 127, 255, 1, prefix);
            TestPF(src, opcode, 128, 0, 0, prefix);
            TestPF(src, opcode, 128, 1, 1, prefix);
            TestPF(src, opcode, 128, 127, 1, prefix);
            TestPF(src, opcode, 128, 128, 0, prefix);
            TestPF(src, opcode, 128, 129, 0, prefix);
            TestPF(src, opcode, 128, 255, 0, prefix);
            TestPF(src, opcode, 129, 0, 0, prefix);
            TestPF(src, opcode, 129, 1, 0, prefix);
            TestPF(src, opcode, 129, 127, 1, prefix);
            TestPF(src, opcode, 129, 128, 0, prefix);
            TestPF(src, opcode, 129, 129, 0, prefix);
            TestPF(src, opcode, 129, 255, 0, prefix);
        }

        void TestPF(string src, byte opcode, int oldValue, int substractedValue, int expectedPF, byte? prefix)
        {
            Setup(src, (byte)oldValue, (byte)substractedValue);

            Execute(opcode, prefix);
            Assert.AreEqual(expectedPF, Registers.PF);
        }

        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        [TestCaseSource("SUB_SBC_A_A_Source")]
        [TestCaseSource("CP_r_Source")]
        [TestCaseSource("CPID_R_Source")]
        public void SUB_SBC_CPs_sets_NF(string src, byte opcode, int cf, byte? prefix)
        {
            AssertSetsFlags(opcode, null, "N");
        }

        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        [TestCaseSource("CP_r_Source")]
        public void SUB_SBC_CP_r_sets_CF_appropriately(string src, byte opcode, int cf, byte? prefix)
        {
            Setup(src, 0x01, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.CF);

            Setup(src, 0x00, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.CF);

            Setup(src, 0xFF, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.CF);
        }

        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        public void SUB_SBC_r_sets_bits_3_and_5_from_result(string src, byte opcode, int cf, byte? prefix)
        {
            Setup(src, (byte)(((byte)0).WithBit(3, 1).WithBit(5, 0) + 1), 1);
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.Flag3);
            Assert.AreEqual(0, Registers.Flag5);

            Setup(src, (byte)(((byte)0).WithBit(3, 0).WithBit(5, 1) + 1), 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.Flag3);
            Assert.AreEqual(1, Registers.Flag5);
        }

        [Test]
        [TestCaseSource("SUB_SBC_A_r_Source")]
        [TestCaseSource("SUB_SBC_A_A_Source")]
        [TestCaseSource("CP_r_Source")]
        public void SUB_SBC_CP_r_returns_proper_T_states(string src, byte opcode, int cf, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(
                (src == "(HL)" || src == "n") ? 7 :
                src.StartsWith("I") ? 8 :
                src.StartsWith(("(I")) ? 19 :
                4, states);
        }

        [Test]
        [TestCaseSource("CPID_R_Source")]
        public void CPI_CPD_CPIR_CPRD_do_not_change_C(string src, byte opcode, int cf, byte? prefix)
        {
            AssertDoesNotChangeFlags(opcode, cpidrPrefix, "C");
        }

        [Test]
        [TestCaseSource("CPID_R_Source")]
        public void CPI_CPD_CPIR_CPDR_reset_PF_if_BC_reaches_zero(string src, byte opcode, int cf, byte? prefix)
        {
            Registers.BC = 128;
            for(int i = 0; i <= 256; i++)
            {
                var oldBC = Registers.BC;
                Execute(opcode, prefix);
                Assert.AreEqual(oldBC.Dec(), Registers.BC);
                Assert.AreEqual(Registers.BC != 0, (bool)Registers.PF);
            }
        }

        [Test]
        [TestCaseSource("CPI_Source")]
        [TestCaseSource("CPIR_Source")]
        public void CPI_CPIR_increase_HL(string src, byte opcode, int cf, byte? prefix)
        {
            var address = Fixture.Create<short>();

            Registers.HL = address;

            Execute(opcode, prefix);

            Assert.AreEqual(address.Inc(), Registers.HL);
        }

        [Test]
        [TestCaseSource("CPD_Source")]
        [TestCaseSource("CPDR_Source")]
        public void CPD_CPDR_decrease_HL(string src, byte opcode, int cf, byte? prefix)
        {
            var address = Fixture.Create<short>();

            Registers.HL = address;

            Execute(opcode, prefix);

            Assert.AreEqual(address.Dec(), Registers.HL);
        }

        [Test]
        [TestCaseSource("CPID_R_Source")]
        public void CPI_CPD_CPIR_CPDR_decrease_BC(string src, byte opcode, int cf, byte? prefix)
        {
            var count = Fixture.Create<short>();

            Registers.BC = count;

            Execute(opcode, prefix);

            Assert.AreEqual(count.Dec(), Registers.BC);
        }

        [Test]
        [TestCaseSource("CPI_Source")]
        [TestCaseSource("CPD_Source")]
        public void CPI_CPD_return_proper_T_states(string src, byte opcode, int cf, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(16, states);
        }

        [Test]
        [TestCaseSource("CPID_R_Source")]
        public void CPI_CPD_set_Flag3_from_bit_3_of_A_minus_aHL_minus_HF_and_Flag5_from_bit_1(string src, byte opcode, int cf, byte? prefix)
        {
            var valueInMemory = Fixture.Create<byte>();
            var srcAddress = Fixture.Create<short>();

            for(int i = 0; i < 256; i++)
            {
                var valueOfA = (byte)i;
                Registers.A = valueOfA;
                Registers.HL = srcAddress;
                ProcessorAgent.Memory[srcAddress] = valueInMemory;

                Execute(opcode, prefix);

                var expected = valueOfA.Sub(valueInMemory).Sub((short)Registers.HF).GetLowByte();
                Assert.AreEqual(expected.GetBit(3), Registers.Flag3);
                Assert.AreEqual(expected.GetBit(1), Registers.Flag5);
            }
        }

        [Test]
        [TestCaseSource("CPIR_Source")]
        [TestCaseSource("CPDR_Source")]
        public void CPIR_CPDR_decrease_PC_if_Z_is_1_but_BC_is_not_0(string src, byte opcode, int cf, byte? prefix)
        {
            var dataAddress = Fixture.Create<ushort>();
            var runAddress = Fixture.Create<ushort>();
            var data = Fixture.Create<byte>();

            ProcessorAgent.Memory[dataAddress] = data;
            Registers.A = data;
            Registers.HL = dataAddress.ToShort();
            Registers.BC = Fixture.Create<short>();

            ExecuteAt(runAddress, opcode, cpidrPrefix);

            Assert.AreEqual(runAddress.Add(2), Registers.PC);
        }

        [Test]
        [TestCaseSource("CPIR_Source")]
        [TestCaseSource("CPDR_Source")]
        public void CPIR_CPDR_decrease_PC_if_BC_is_0_but_Z_is_not_0(string src, byte opcode, int cf, byte? prefix)
        {
            if(opcode==0xFF) opcode = 0xB9;

            var dataAddress = Fixture.Create<ushort>();
            var runAddress = Fixture.Create<ushort>();
            var data1 = Fixture.Create<byte>();
            var data2 = Fixture.Create<byte>();

            ProcessorAgent.Memory[dataAddress] = data1;
            Registers.A = data2;
            Registers.HL = dataAddress.ToShort();
            Registers.BC = 1;

            ExecuteAt(runAddress, opcode, cpidrPrefix);

            Assert.AreEqual(runAddress.Add(2), Registers.PC);
        }

        [Test]
        [TestCaseSource("CPIR_Source")]
        [TestCaseSource("CPDR_Source")]
        public void CPIR_CPDR_do_not_decrease_PC_if_BC_is_not_0_and_Z_is_0(string src, byte opcode, int cf, byte? prefix)
        {
            if(opcode==0xFF) opcode = 0xB9;

            var dataAddress = Fixture.Create<ushort>();
            var runAddress = Fixture.Create<ushort>();
            var data1 = Fixture.Create<byte>();
            var data2 = Fixture.Create<byte>();

            ProcessorAgent.Memory[dataAddress] = data1;
            Registers.A = data2;
            Registers.HL = dataAddress.ToShort();
            Registers.BC = 1000;

            ExecuteAt(runAddress, opcode, cpidrPrefix);

            Assert.AreEqual(runAddress, Registers.PC);
        }

        [Test]
        [TestCaseSource("CPIR_Source")]
        [TestCaseSource("CPDR_Source")]
        public void CPIR_CPDR_return_proper_T_states_depending_on_PC_being_decreased_or_not(string src, byte opcode, int cf, byte? prefix)
        {
            if(opcode==0xFF) opcode = 0xB9;

            var dataAddress = Fixture.Create<ushort>();
            var runAddress = Fixture.Create<ushort>();
            var data1 = Fixture.Create<byte>();
            var data2 = Fixture.Create<byte>();

            ProcessorAgent.Memory[dataAddress] = data1;
            Registers.A = data2;
            Registers.HL = dataAddress.ToShort();
            Registers.BC = 2;

            var states = ExecuteAt(runAddress, opcode, cpidrPrefix);
            Assert.AreEqual(21, states);

            Registers.HL = dataAddress.ToShort();
            states = ExecuteAt(runAddress, opcode, cpidrPrefix);
            Assert.AreEqual(16, states);
        }

        protected override int Execute(byte opcode, byte? prefix = null, params byte[] nextFetches)
        {
            if(cpidrOpcodes.Contains(opcode))
                return base.Execute(opcode == 0xFF ? (byte)0xB9 : opcode, cpidrPrefix, nextFetches);
            else
                return base.Execute(opcode, prefix, nextFetches);
        }
    }
}