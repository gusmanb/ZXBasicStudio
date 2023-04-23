using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public partial class LD_r_IX_IY_plus_d_tests : InstructionsExecutionTestsBase
    {
        public static object[] LD_Source =
        {
            new object[] {"A", "IX", (byte)0x7E, (byte)0xDD},
            new object[] {"B", "IX", (byte)0x46, (byte)0xDD},
            new object[] {"C", "IX", (byte)0x4E, (byte)0xDD},
            new object[] {"D", "IX", (byte)0x56, (byte)0xDD},
            new object[] {"E", "IX", (byte)0x5E, (byte)0xDD},
            new object[] {"H", "IX", (byte)0x66, (byte)0xDD},
            new object[] {"L", "IX", (byte)0x6E, (byte)0xDD},
            new object[] {"A", "IY", (byte)0x7E, (byte)0xFD},
            new object[] {"B", "IY", (byte)0x46, (byte)0xFD},
            new object[] {"C", "IY", (byte)0x4E, (byte)0xFD},
            new object[] {"D", "IY", (byte)0x56, (byte)0xFD},
            new object[] {"E", "IY", (byte)0x5E, (byte)0xFD},
            new object[] {"H", "IY", (byte)0x66, (byte)0xFD},
            new object[] {"L", "IY", (byte)0x6E, (byte)0xFD}
        };

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_r_IX_IY_plus_d_loads_value_from_memory(string destReg, string srcPointerReg, byte opcode, byte prefix)
        {
            var address = Fixture.Create<ushort>();
            var offset = Fixture.Create<byte>();
            var oldValue = Fixture.Create<byte>();
            var newValue = Fixture.Create<byte>();
            var actualAddress = address.Add(offset.ToSignedByte());

            SetReg(srcPointerReg, address.ToShort());
            SetReg(destReg, oldValue);
            ProcessorAgent.Memory[actualAddress] = newValue;

            Execute(opcode, prefix, offset);

            Assert.AreEqual(newValue, GetReg<byte>(destReg));
        }

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_r_IX_IY_plus_d_do_not_modify_flags(string destReg, string srcPointerReg, byte opcode, byte prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("LD_Source")]
        public void LD_r_IX_IY_plus_d_return_proper_T_states(string destReg, string srcPointerReg, byte opcode, byte prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(19, states);
        }
    }
}