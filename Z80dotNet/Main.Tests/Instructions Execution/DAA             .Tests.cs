using NUnit.Framework;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class DAA_tests : InstructionsExecutionTestsBase
    {
        private const byte DAA_opcode = 0x27;

        public static object[] DAA_cases_Source =
        {//               N  C  H   A   added  C after
            new object[] {0, 0, 0, 0x09, 0x00, 0},
            new object[] {0, 0, 0, 0x90, 0x00, 0},

            new object[] {0, 0, 0, 0x0F, 0x06, 0},
            new object[] {0, 0, 0, 0x8A, 0x06, 0},

            new object[] {0, 0, 1, 0x03, 0x06, 0},
            new object[] {0, 0, 1, 0x90, 0x06, 0},

            new object[] {0, 0, 0, 0xA9, 0x60, 1},
            new object[] {0, 0, 0, 0xF0, 0x60, 1},

            new object[] {0, 0, 0, 0x9F, 0x66, 1},
            new object[] {0, 0, 0, 0xFA, 0x66, 1},

            new object[] {0, 0, 1, 0xA3, 0x66, 1},
            new object[] {0, 0, 1, 0xF0, 0x66, 1},

            new object[] {0, 1, 0, 0x09, 0x60, 1},
            new object[] {0, 1, 0, 0x20, 0x60, 1},

            new object[] {0, 1, 0, 0x0F, 0x66, 1},
            new object[] {0, 1, 0, 0x2A, 0x66, 1},

            new object[] {0, 1, 1, 0x03, 0x66, 1},
            new object[] {0, 1, 1, 0x30, 0x66, 1},

            new object[] {1, 0, 0, 0x09, 0x00, 0},
            new object[] {1, 0, 0, 0x90, 0x00, 0},

            new object[] {1, 0, 1, 0x0F, 0xFA, 0},
            new object[] {1, 0, 1, 0x86, 0xFA, 0},

            new object[] {1, 1, 0, 0x79, 0xA0, 1},
            new object[] {1, 1, 0, 0xF0, 0xA0, 1},

            new object[] {1, 1, 1, 0x6F, 0x9A, 1},
            new object[] {1, 1, 1, 0x76, 0x9A, 1}
        };

        [Test]
        [TestCaseSource("DAA_cases_Source")]
        public void DAA_generates_A_and_CF_correctly_based_on_input(int inputNF, int inputCF, int inputHF, int inputA, int addedValue, int outputC)
        {
            Setup(inputNF, inputCF, inputHF, inputA);

            Execute(DAA_opcode);

            Assert.AreEqual(((byte)inputA).Add(addedValue), Registers.A);
            Assert.AreEqual((Bit)outputC, Registers.CF);
        }

       
        [Test]
        [TestCaseSource("DAA_cases_Source")]
        public void DAA_returns_proper_T_states(int inputNF, int inputCF, int inputHF, int inputA, int addedValue, int outputC)
        {
            Setup(inputNF, inputCF, inputHF, inputA);

            var states = Execute(DAA_opcode);
            Assert.AreEqual(4, states);
        }

        [Test]
        public void DAA_covers_all_possible_combinations_of_flags_and_A()
        {
            for(var flagN=0; flagN<=1; flagN++)
            for(var flagC=0; flagC<=1; flagC++)
            for(var flagH=0; flagH<=1; flagH++)
            for(var valueOfA=0; valueOfA<=255; valueOfA++)
            {
                Setup(flagN, flagC, flagH, valueOfA);
                Execute(DAA_opcode);
            }
        }

        [Test]
        [TestCaseSource("DAA_cases_Source")]
        public void DAA_generates_PF_properly(int inputNF, int inputCF, int inputHF, int inputA, int addedValue, int outputC)
        {
            Setup(inputNF, inputCF, inputHF, inputA);

            Execute(DAA_opcode);

            Assert.AreEqual(Parity[Registers.A], Registers.PF);
        }

        [Test]
        [TestCaseSource("DAA_cases_Source")]
        public void DAA_generates_SF_properly(int inputNF, int inputCF, int inputHF, int inputA, int addedValue, int outputC)
        {
            Setup(inputNF, inputCF, inputHF, inputA);

            Execute(DAA_opcode);

            Assert.AreEqual(Registers.A.GetBit(7), Registers.SF);
        }

        [Test]
        [TestCaseSource("DAA_cases_Source")]
        public void DAA_generates_ZF_properly(int inputNF, int inputCF, int inputHF, int inputA, int addedValue, int outputC)
        {
            Setup(inputNF, inputCF, inputHF, inputA);

            Execute(DAA_opcode);

            Assert.AreEqual(Registers.A == 0 ? 1 : 0, Registers.ZF);
        }

        [Test]
        [TestCaseSource("DAA_cases_Source")]
        public void DAA_does_not_modify_NF(int inputNF, int inputCF, int inputHF, int inputA, int addedValue, int outputC)
        {
            Setup(inputNF, inputCF, inputHF, inputA);

            AssertDoesNotChangeFlags(DAA_opcode, null, "N");
        }

        [Test]
        [TestCaseSource("DAA_cases_Source")]
        public void DAA_sets_bits_3_and_5_from_of_result(int inputNF, int inputCF, int inputHF, int inputA, int addedValue, int outputC)
        {
            Setup(inputNF, inputCF, inputHF, inputA);

            Execute(DAA_opcode);

            Assert.AreEqual(Registers.Flag3, Registers.A.GetBit(3));
            Assert.AreEqual(Registers.Flag5, Registers.A.GetBit(5));
        }

        private void Setup(int inputNF, int inputCF, int inputHF, int inputA)
        {
            Registers.NF = inputNF;
            Registers.CF = inputCF;
            Registers.HF = inputHF;
            Registers.A = (byte)inputA;
        }
    }
}