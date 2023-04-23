using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Collections.Generic;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class XOR_r_tests : InstructionsExecutionTestsBase
    {
        static XOR_r_tests()
        {
            var combinations = new List<object[]>();

            var registers = new[] {"B", "C", "D", "E", "H", "L", "(HL)", "n", "IXH", "IXL", "IYH", "IYL","(IX+n)","(IY+n)"};
            for(var src = 0; src<registers.Length; src++)
            {
                var reg = registers[src];
                var i = src;
                byte? prefix = null;

                ModifyTestCaseCreationForIndexRegs(reg, ref i, out prefix);

                var opcode = (byte)(i==7 ? 0xEE : (i | 0xA8));
                combinations.Add(new object[] {reg, opcode, prefix});
            }

            XOR_r_Source = combinations.ToArray();
        }

        public static object[] XOR_r_Source;

        public static object[] XOR_A_Source =
        {
            new object[] {"A", (byte)0xAF, (byte?)null},
        };


        [Test]
        [TestCaseSource("XOR_r_Source")]
        public void XOR_r_xors_both_registers(string src, byte opcode, byte? prefix)
        {
            var oldValue = Fixture.Create<byte>();
            var valueToXor = Fixture.Create<byte>();

            Setup(src, oldValue, valueToXor);
            Execute(opcode, prefix);

            Assert.AreEqual(oldValue ^ valueToXor, Registers.A);
        }

        [Test]
        [TestCaseSource("XOR_A_Source")]
        public void XOR_A_resets_A(string src, byte opcode, byte? prefix)
        {
            var value = Fixture.Create<byte>();

            Registers.A = value;
            Execute(opcode, prefix);

            Assert.AreEqual(0, Registers.A);
        }

        private void Setup(string src, byte oldValue, byte valueToXor)
        {
            Registers.A = oldValue;

            if(src == "n") 
            {
                SetMemoryContentsAt(1, valueToXor);
            }
            else if(src == "(HL)") 
            {
                var address = Fixture.Create<ushort>();
                ProcessorAgent.Memory[address] = valueToXor;
                Registers.HL = address.ToShort();
            }
            else if(src.StartsWith("(I")) 
            {
                var address = Fixture.Create<ushort>();
                var offset = Fixture.Create<byte>();
                var realAddress = address.Add(offset.ToSignedByte());
                ProcessorAgent.Memory[realAddress] = valueToXor;
                SetMemoryContentsAt(2, offset);
                SetReg(src.Substring(1,2), address.ToShort());
            }
            else if(src != "A")
            {
                SetReg(src, valueToXor);
            }
        }

        [Test]
        [TestCaseSource("XOR_r_Source")]
        public void XOR_r_sets_SF_appropriately(string src, byte opcode, byte? prefix)
        {
            ExecuteCase(src, opcode, 0xFF, 0x00, prefix);
            Assert.AreEqual(1, Registers.SF);

            ExecuteCase(src, opcode, 0xFF, 0x7F, prefix);
            Assert.AreEqual(1, Registers.SF);

            ExecuteCase(src, opcode, 0xFF, 0xF0, prefix);
            Assert.AreEqual(0, Registers.SF);
        }

        private void ExecuteCase(string src, byte opcode, int oldValue, int valueToAnd, byte? prefix)
        {
            Setup(src, (byte)oldValue, (byte)valueToAnd);
            Execute(opcode, prefix);
        }

        [Test]
        [TestCaseSource("XOR_r_Source")]
        public void XOR_r_sets_ZF_appropriately(string src, byte opcode, byte? prefix)
        {
            ExecuteCase(src, opcode, 0xFF, 0x00, prefix);
            Assert.AreEqual(0, Registers.ZF);

            ExecuteCase(src, opcode, 0xFF, 0x7F, prefix);
            Assert.AreEqual(0, Registers.ZF);

            ExecuteCase(src, opcode, 0xFF, 0xFF, prefix);
            Assert.AreEqual(1, Registers.ZF);
        }

        [Test]
        [TestCaseSource("XOR_r_Source")]
        public void XOR_r_sets_PF_appropriately(string src, byte opcode, byte? prefix)
        {
            ExecuteCase(src, opcode, 0xFF, 0x7E, prefix);
            Assert.AreEqual(1, Registers.PF);

            ExecuteCase(src, opcode, 0xFF, 0x7F, prefix);
            Assert.AreEqual(0, Registers.PF);

            ExecuteCase(src, opcode, 0xFF, 0x80, prefix);
            Assert.AreEqual(0, Registers.PF);

            ExecuteCase(src, opcode, 0xFF, 0x81, prefix);
            Assert.AreEqual(1, Registers.PF);
        }

        [Test]
        [TestCaseSource("XOR_r_Source")]
        [TestCaseSource("XOR_A_Source")]
        public void XOR_r_resets_NF_CF_HF(string src, byte opcode, byte? prefix)
        {
            AssertResetsFlags(opcode, null, "N", "C", "H");
        }

        [Test]
        [TestCaseSource("XOR_r_Source")]
        public void XOR_r_sets_bits_3_and_5_from_result(string src, byte opcode, byte? prefix)
        {
            var value = ((byte)0).WithBit(3, 1).WithBit(5, 0);
            Setup(src, value, 0);
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.Flag3);
            Assert.AreEqual(0, Registers.Flag5);

            value = ((byte)0).WithBit(3, 0).WithBit(5, 1);
            Setup(src, value, 0);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.Flag3);
            Assert.AreEqual(1, Registers.Flag5);
        }

        [Test]
        [TestCaseSource("XOR_r_Source")]
        [TestCaseSource("XOR_A_Source")]
        public void XOR_r_returns_proper_T_states(string src, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(
                (src == "(HL)" || src == "n") ? 7 :
                src.StartsWith("I") ? 8 :
                src.StartsWith(("(I")) ? 19 :
                4, states);
        }
    }
}