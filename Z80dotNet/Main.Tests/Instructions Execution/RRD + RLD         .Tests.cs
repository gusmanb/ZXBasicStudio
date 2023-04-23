using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class RRD_RLD_tests : InstructionsExecutionTestsBase
    {
        private const byte prefix = 0xED;

        public static object[] RRD_RLD_Source =
        {
            new object[] {(byte)0x67, "R"},
            new object[] {(byte)0x6F, "L"}
        };

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_moves_data_appropriately(byte opcode, string direction)
        {
            var oldHLContents = "1001 0110".AsBinaryByte();
            var oldAValue = "0011 1010".AsBinaryByte();
            var address = Setup(oldHLContents, oldAValue);
            
            Execute(opcode, prefix);

            byte expectedHLContents, expectedAValue;
            if(direction == "R") {
                expectedHLContents = "1010 1001".AsBinaryByte();
                expectedAValue = "0011 0110".AsBinaryByte();
            }
            else {
                expectedHLContents = "0110 1010".AsBinaryByte();
                expectedAValue = "0011 1001".AsBinaryByte();
            }

            AssertMemoryContents(address, expectedHLContents);
            Assert.AreEqual(expectedAValue, Registers.A);
        }

        private ushort Setup(byte HLcontents, byte Avalue)
        {
            Registers.A = Avalue;
            var address = Fixture.Create<ushort>();
            ProcessorAgent.Memory[address] = HLcontents;
            Registers.HL = address.ToShort();
            return address;
        }

        private void AssertMemoryContents(ushort address, byte expected)
        {
            Assert.AreEqual(expected, ProcessorAgent.Memory[address]);
        }

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_sets_SF_appropriately(byte opcode, string direction)
        {
            for(int i=0; i<=255; i++)
            {
                var b = (byte)i;
                Registers.A = b;
                Execute(opcode, prefix);
                Assert.AreEqual(b >= 128, (bool)Registers.SF);
            }
        }

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_sets_ZF_appropriately(byte opcode, string direction)
        {
            for(int i=0; i<=255; i++)
            {
                var b = (byte)i;
                Setup(b, 0);
                Execute(opcode, prefix);
                Assert.AreEqual(Registers.A == 0, (bool)Registers.ZF);
            }
        }

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_resets_HF(byte opcode, string direction)
        {
            AssertResetsFlags(opcode, prefix, "H");
        }

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_sets_PF_as_parity(byte opcode, string direction)
        {
            for(int i=0; i<=255; i++)
            {
                var b = (byte)i;
                Setup(Fixture.Create<byte>(), b);
                Execute(opcode, prefix);
                Assert.AreEqual(Parity[Registers.A], Registers.PF);
            }
        }

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_resets_NF(byte opcode, string direction)
        {
            AssertResetsFlags(opcode, prefix, "N");
        }

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_does_not_chance_CF(byte opcode, string direction)
        {
            AssertDoesNotChangeFlags(opcode, prefix, "C");
        }

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_sets_bits_3_and_5_from_result(byte opcode, string direction)
        {
            for(int i=0; i<=255; i++)
            {
                var b = (byte)i;
                Setup(Fixture.Create<byte>(), b);
                Execute(opcode, prefix);
                Assert.AreEqual(Registers.A.GetBit(3), Registers.Flag3);
                Assert.AreEqual(Registers.A.GetBit(5), Registers.Flag5);
            }
        }

        [Test]
        [TestCaseSource("RRD_RLD_Source")]
        public void RRD_RLD_returns_proper_T_states(byte opcode, string direction)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(18, states);
        }
    }
}
