using System;
using Konamiman.Z80dotNet.Tests.InstructionsExecution;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class OUT_C_r : InstructionsExecutionTestsBase
    {
        public static object[] OUT_C_r_Source =
        {
            new Object[] { "A", (byte)0x79 },
            new Object[] { "B", (byte)0x41 },
            new Object[] { "C", (byte)0x49 },
            new Object[] { "D", (byte)0x51 },
            new Object[] { "E", (byte)0x59 },
            new Object[] { "H", (byte)0x61 },
            new Object[] { "L", (byte)0x69 }
        };

        public static object[] OUT_C_0_Source =
        {
            new Object[] { "0", (byte)0x71 },
        
        };

        [Test]
        [TestCaseSource("OUT_C_r_Source")]
        [TestCaseSource("OUT_C_0_Source")]
        public void OUT_C_r_writes_value_to_port(string reg, byte opcode)
        {
            var portNumber = Fixture.Create<byte>();
            var value = reg == "C" ? portNumber : Fixture.Create<byte>();
            var oldValue = Fixture.Create<byte>();

            if(reg != "0" && reg != "C")
                SetReg(reg, value);
            SetPortValue(portNumber, oldValue);

            Execute(opcode, portNumber, value);

            Assert.AreEqual(reg == "0" ? 0 : value, GetPortValue(portNumber));
        }

        [Test]
        [TestCaseSource("OUT_C_r_Source")]
        [TestCaseSource("OUT_C_0_Source")]
        public void OUT_C_r_does_not_change_flags(string reg, byte opcode)
        {
            AssertDoesNotChangeFlags(opcode, 0xED);
        }

        [Test]
        [TestCaseSource("OUT_C_r_Source")]
        [TestCaseSource("OUT_C_0_Source")]
        public void OUT_C_r_returns_proper_T_states(string reg, byte opcode)
        {
            var portNumber = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var states = Execute(opcode, portNumber, value);
            Assert.AreEqual(12, states);
        }

        private int Execute(byte opcode, byte portNumber, byte value)
        {
            Registers.C = portNumber;
            SetPortValue(portNumber, value);
            return Execute(opcode, 0xED);
        }
    }
}
