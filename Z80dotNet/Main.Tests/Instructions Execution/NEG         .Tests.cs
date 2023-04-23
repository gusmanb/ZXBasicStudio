using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Collections.Generic;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class NEG_tests : InstructionsExecutionTestsBase
    {
        private const byte opcode = 0x44;
        private const byte prefix = 0xED ;

        [Test]
        public void NEG_substracts_A_from_zero()
        {
            var oldValue = Fixture.Create<byte>();

            Registers.A = oldValue;
            Execute();

            var expected = (byte)((byte)0).Sub(oldValue);
            Assert.AreEqual(expected, Registers.A);
        }

        [Test]
        public void NEG_sets_SF_appropriately()
        {
            Registers.A = 2;
            Execute();
            Assert.AreEqual(1, Registers.SF);

            Registers.A = 1;
            Execute();
            Assert.AreEqual(1, Registers.SF);

            Registers.A = 0;
            Execute();
            Assert.AreEqual(0, Registers.SF);

            Registers.A = 0xFF;
            Execute();
            Assert.AreEqual(0, Registers.SF);

            Registers.A = 0x80;
            Execute();
            Assert.AreEqual(1, Registers.SF);
        }

        [Test]
        public void NEG_sets_ZF_appropriately()
        {
            Registers.A = 2;
            Execute();
            Assert.AreEqual(0, Registers.ZF);

            Registers.A = 1;
            Execute();
            Assert.AreEqual(0, Registers.ZF);

            Registers.A = 0;
            Execute();
            Assert.AreEqual(1, Registers.ZF);

            Registers.A = 0xFF;
            Execute();
            Assert.AreEqual(0, Registers.ZF);

            Registers.A = 0x80;
            Execute();
            Assert.AreEqual(0, Registers.ZF);
        }

        [Test]
        public void NEG_sets_HF_appropriately()
        {
            for(int i = 0; i <= 255; i++)
            {
                var b = (byte)i;
                Registers.A = b;
                Execute();
                var expected = (Bit)((b ^ Registers.A) & 0x10);
                Assert.AreEqual(expected, Registers.HF);
            }
        }

        [Test]
        public void NEG_sets_PF_appropriately()
        {
            for(int i = 0; i <= 255; i++)
            {
                var b = (byte)i;
                Registers.A = b;
                Execute();
                var expected = b == 0x80 ? 1 : 0;
                Assert.AreEqual(expected, Registers.PF);
            }
        }

        [Test]
        public void NEG_sets_NF()
        {
            AssertSetsFlags(opcode, prefix, "N");
        }

        [Test]
        public void NEG_sets_CF_appropriately()
        {
            for(int i = 0; i <= 255; i++)
            {
                var b = (byte)i;
                Registers.A = b;
                Execute();
                var expected = b == 0 ? 0 : 1;
                Assert.AreEqual(expected, Registers.CF);
            }
        }

        [Test]
        public void NEG_sets_bits_3_and_5_from_result()
        {
            Registers.A = 0x0F;
            Execute();
            Assert.AreEqual(0, Registers.Flag3);
            Assert.AreEqual(1, Registers.Flag5);

            Registers.A = 0xF1;
            Execute();
            Assert.AreEqual(1, Registers.Flag3);
            Assert.AreEqual(0, Registers.Flag5);
        }

        [Test]
        public void NEG_returns_proper_T_states()
        {
            var states = Execute();
            Assert.AreEqual(8, states);
        }

        private int Execute()
        {
            return Execute(opcode, prefix);
        }
    }
}