using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class LD_IX_IY_plus_n_tests : InstructionsExecutionTestsBase
    {
        public static object[] LD_Source =
        {
            new object[] { "IX", (byte)0x36, (byte)0xDD },
            new object[] { "IY", (byte)0x36, (byte)0xFD }
        };

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_IX_IY_plus_n_loads_value_in_memory(string reg, byte opcode, byte prefix)
        {
            var address = Fixture.Create<ushort>();
            var offset = Fixture.Create<byte>();
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();
            var actualAddress = address.Add(offset.ToSignedByte());

            ProcessorAgent.Memory[actualAddress] = oldValue;
            SetReg(reg, address.ToShort());

            Execute(opcode, prefix, offset, newValue);

            Assert.AreEqual(newValue, ProcessorAgent.Memory[actualAddress]);
        }

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_IX_IY_plus_n_does_not_modify_flags(string reg, byte opcode, byte prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_IX_IY_plus_n_returns_proper_T_states(string reg, byte opcode, byte prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(19, states);
        }
    }
}