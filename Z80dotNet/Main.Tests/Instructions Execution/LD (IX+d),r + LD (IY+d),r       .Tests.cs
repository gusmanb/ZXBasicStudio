using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public partial class LD_IX_IY_plus_d_r_tests : InstructionsExecutionTestsBase
    {
        public static object[] LD_Source =
        {
            new object[] {"A", "IX", (byte)0x77, (byte)0xDD},
            new object[] {"B", "IX", (byte)0x70, (byte)0xDD},
            new object[] {"C", "IX", (byte)0x71, (byte)0xDD},
            new object[] {"D", "IX", (byte)0x72, (byte)0xDD},
            new object[] {"E", "IX", (byte)0x73, (byte)0xDD},
            new object[] {"H", "IX", (byte)0x74, (byte)0xDD},
            new object[] {"L", "IX", (byte)0x75, (byte)0xDD},
            new object[] {"A", "IY", (byte)0x77, (byte)0xFD},
            new object[] {"B", "IY", (byte)0x70, (byte)0xFD},
            new object[] {"C", "IY", (byte)0x71, (byte)0xFD},
            new object[] {"D", "IY", (byte)0x72, (byte)0xFD},
            new object[] {"E", "IY", (byte)0x73, (byte)0xFD},
            new object[] {"H", "IY", (byte)0x74, (byte)0xFD},
            new object[] {"L", "IY", (byte)0x75, (byte)0xFD}
        };

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_IX_IY_plus_d_r_loads_value_from_memory(string srcReg, string destPointerReg, byte opcode, byte prefix)
        {
            var address = Fixture.Create<ushort>();
            var offset = Fixture.Create<byte>();
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();
            var actualAddress = address.Add(offset.ToSignedByte());

            SetReg(destPointerReg, address.ToShort());
            SetReg(srcReg, newValue);
            ProcessorAgent.Memory[actualAddress] = oldValue;

            Execute(opcode, prefix, offset);

            Assert.AreEqual(newValue, ProcessorAgent.Memory[actualAddress]);
        }

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_IX_IY_plus_d_r_do_not_modify_flags(string srcReg, string destPointerReg, byte opcode, byte prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_IX_IY_plus_d_r_return_proper_T_states(string srcReg, string destPointerReg, byte opcode, byte prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(19, states);
        }
    }
}