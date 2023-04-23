using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class JP_aHL_IX_IY_tests : InstructionsExecutionTestsBase
    {
        public static object[] JP_Source =
        {
            new object[] { "HL", (byte)0xE9, (byte?)null },
            new object[] { "IX", (byte)0xE9, (byte?)0xDD },
            new object[] { "IY", (byte)0xE9, (byte?)0xFD }
        };

        [Test]
        [TestCaseSource("JP_Source")]
        public void JP_aHL_IX_IY_jump_to_address_contained_in_HL(string reg, byte opcode, byte? prefix)
        {
            var instructionAddress = Fixture.Create<ushort>();
            var jumpAddress = Fixture.Create<short>();

            SetReg(reg, jumpAddress);
            ExecuteAt(instructionAddress, opcode, prefix);

            Assert.AreEqual(jumpAddress.ToUShort(), Registers.PC);
        }

        [Test]
        [TestCaseSource("JP_Source")]
        public void JP_aHL_IX_IY_do_not_change_flags(string reg, byte opcode, byte? prefix)
        {
            AssertNoFlagsAreModified(opcode, prefix);
        }

        [Test]
        [TestCaseSource("JP_Source")]
        public void JP_aHL_IX_IY_return_proper_T_states(string reg, byte opcode, byte? prefix)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(reg=="HL" ? 4 : 8, states);
        }
    }
}