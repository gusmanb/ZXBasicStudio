using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class INI_IND_INIR_INDR_tests : InstructionsExecutionTestsBase
    {
        private const byte prefix = 0xED;

        static INI_IND_INIR_INDR_tests()
        {
            All_Source =
                INI_Source
                    .Concat(IND_Source)
                    .Concat(INIR_Source)
                    .Concat(INDR_Source)
                    .Concat(OUTI_Source)
                    .Concat(OUTD_Source)
                    .Concat(OTIR_Source)
                    .Concat(OTDR_Source)
                    .ToArray();
        }

        public static object[] All_Source;

        public static object[] INI_Source =
        {
            new object[] { (byte)0xA2 }
        };

        public static object[] IND_Source =
        {
            new object[] { (byte)0xAA }
        };

        public static object[] INIR_Source =
        {
            new object[] { (byte)0xB2 }
        };

        public static object[] INDR_Source =
        {
            new object[] { (byte)0xBA }
        };

        public static object[] OUTI_Source =
        {
            new object[] { (byte)0xA3 }
        };

        public static object[] OUTD_Source =
        {
            new object[] { (byte)0xAB }
        };

        public static object[] OTIR_Source =
        {
            new object[] { (byte)0xB3 }
        };

        public static object[] OTDR_Source =
        {
            new object[] { (byte)0xBB }
        };

        [Test]
        [TestCaseSource("INI_Source")]
        [TestCaseSource("IND_Source")]
        [TestCaseSource("INIR_Source")]
        [TestCaseSource("INDR_Source")]
        public void INI_IND_INIR_INDR_read_value_from_port_aC_into_aHL(byte opcode)
        {
            var portNumber = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var oldValue = Fixture.Create<byte>();
            var address = Fixture.Create<ushort>();

            Registers.HL = address.ToShort();
            ProcessorAgent.Memory[address] = oldValue;

            ExecuteWithPortSetup(opcode, portNumber, value);

            var actual = ProcessorAgent.Memory[address];
            Assert.AreEqual(value, actual);
        }

        [Test]
        [TestCaseSource("OUTI_Source")]
        [TestCaseSource("OUTD_Source")]
        [TestCaseSource("OTIR_Source")]
        [TestCaseSource("OTDR_Source")]
        public void OUTI_OUTD_OTIR_OTDR_write_value_from_aHL_into_port_aC(byte opcode)
        {
            var portNumber = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var oldValue = Fixture.Create<byte>();
            var address = Fixture.Create<ushort>();

            Registers.HL = address.ToShort();
            ProcessorAgent.Memory[address] = value;

            ExecuteWithPortSetup(opcode, portNumber, oldValue);

            var actual = ProcessorAgent.Ports[portNumber];
            Assert.AreEqual(value, actual);
        }

        [Test]
        [TestCaseSource("INI_Source")]
        [TestCaseSource("INIR_Source")]
        [TestCaseSource("OUTI_Source")]
        [TestCaseSource("OTIR_Source")]
        public void INI_INIR_OUTI_OTIR_increase_HL(byte opcode)
        {
            var address = Fixture.Create<ushort>();

            Registers.HL = address.ToShort();

            ExecuteWithPortSetup(opcode);

            var expected = address.Inc();
            Assert.AreEqual(expected, Registers.HL);
        }

        [Test]
        [TestCaseSource("IND_Source")]
        [TestCaseSource("INDR_Source")]
        [TestCaseSource("OUTD_Source")]
        [TestCaseSource("OTDR_Source")]
        public void IND_INDR_OUTD_OTDR_decrease_HL(byte opcode)
        {
            var address = Fixture.Create<ushort>();

            Registers.HL = address.ToShort();

            ExecuteWithPortSetup(opcode);

            var expected = address.Dec();
            Assert.AreEqual(expected, Registers.HL);
        }

        [Test]
        [TestCaseSource("All_Source")]
        public void INI_IND_INIR_INDR_OUTI_OUTD_OTIR_OTDR_decrease_B(byte opcode)
        {
            var counter = Fixture.Create<byte>();

            Registers.B = counter;

            ExecuteWithPortSetup(opcode);

            var expected = counter.Dec();
            Assert.AreEqual(expected, Registers.B);
        }

        [Test]
        [TestCaseSource("All_Source")]
        public void INI_IND_INIR_INDR_OUTI_OUTD_OTIR_OTDR_set_Z_if_B_reaches_zero(byte opcode)
        {
            for (int i = 0; i < 256; i++)
            {
                byte b = (byte)i;
                Registers.B = b;

                ExecuteWithPortSetup(opcode);

                Assert.AreEqual(b-1 == 0, (bool)Registers.ZF);
            }
        }

        [Test]
        [TestCaseSource("All_Source")]
        public void INI_IND_INIR_INDR_OUTI_OUTD_OTIR_OTDR_set_NF(byte opcode)
        {
            AssertSetsFlags(opcode, prefix, "N");
        }

        [Test]
        [TestCaseSource("All_Source")]
        public void INI_IND_INIR_INDR_OUTI_OUTD_OTIR_OTDR_do_not_change_CF(byte opcode)
        {
            AssertDoesNotChangeFlags(opcode, prefix, "C");
        }

        [Test]
        [TestCaseSource("All_Source")]
        public void INI_IND_INIR_INDR_OUTI_OUTD_OTIR_OTDR_set_bits_3_and_5_from_result_of_decrementing_B(byte opcode)
        {
            Registers.B = ((byte)1).WithBit(3, 1).WithBit(5, 0);
            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.Flag3);
            Assert.AreEqual(0, Registers.Flag5);

            Registers.B = ((byte)1).WithBit(3, 0).WithBit(5, 1);
            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.Flag3);
            Assert.AreEqual(1, Registers.Flag5);
        }

        [Test]
        [TestCaseSource("All_Source")]
        public void INI_IND_INIR_INDR_OUTI_OUTD_OTIR_OTDR_set_SF_appropriately(byte opcode)
        {
            Registers.B = 0x02;

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.SF);

            Execute(opcode, prefix);
            Assert.AreEqual(0, Registers.SF);

            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.SF);

            Execute(opcode, prefix);
            Assert.AreEqual(1, Registers.SF);
        }

        [Test]
        [TestCaseSource("INI_Source")]
        [TestCaseSource("IND_Source")]
        [TestCaseSource("OUTI_Source")]
        [TestCaseSource("OUTD_Source")]
        public void INI_IND_OUTI_OUTD_return_proper_T_states(byte opcode)
        {
            var states = Execute(opcode, prefix);
            Assert.AreEqual(16, states);
        }

        [Test]
        [TestCaseSource("INIR_Source")]
        [TestCaseSource("INDR_Source")]
        [TestCaseSource("OTIR_Source")]
        [TestCaseSource("OTDR_Source")]
        public void INIR_INDR_OTIR_OTDR_decrease_PC_by_two_if_counter_does_not_reach_zero(byte opcode)
        {
            var dataAddress = Fixture.Create<ushort>();
            var runAddress = Fixture.Create<ushort>();
            var portNumber = Fixture.Create<byte>();
            var oldData = Fixture.Create<byte>();
            var data = Fixture.Create<byte>();

            ProcessorAgent.Memory[dataAddress] = oldData;
            SetPortValue(portNumber, data);
            Registers.HL = dataAddress.ToShort();
            Registers.B = Fixture.Create<byte>();

            ExecuteAt(runAddress, opcode, prefix);

            Assert.AreEqual(runAddress, Registers.PC);
        }

        [Test]
        [TestCaseSource("INIR_Source")]
        [TestCaseSource("INDR_Source")]
        [TestCaseSource("OTIR_Source")]
        [TestCaseSource("OTDR_Source")]
        public void INIR_INDR_OTIR_OTDR_do_not_decrease_PC_by_two_if_counter_reaches_zero(byte opcode)
        {
            var dataAddress = Fixture.Create<ushort>();
            var runAddress = Fixture.Create<ushort>();
            var portNumber = Fixture.Create<byte>();
            var oldData = Fixture.Create<byte>();
            var data = Fixture.Create<byte>();

            ProcessorAgent.Memory[dataAddress] = oldData;
            SetPortValue(portNumber, data);
            Registers.HL = dataAddress.ToShort();
            Registers.B = 1;

            ExecuteAt(runAddress, opcode, prefix);

            Assert.AreEqual(runAddress.Add(2), Registers.PC);
        }

        [Test]
        [TestCaseSource("INIR_Source")]
        [TestCaseSource("INDR_Source")]
        [TestCaseSource("OTIR_Source")]
        [TestCaseSource("OTDR_Source")]
        public void INIR_INDR_OTIR_OTDR_return_proper_T_states_depending_of_value_of_B(byte opcode)
        {
            Registers.B = 128;
            for(int i = 0; i <= 256; i++)
            {
                var oldB = Registers.B;
                var states = Execute(opcode, prefix);
                Assert.AreEqual(oldB.Dec(), Registers.B);
                Assert.AreEqual(Registers.B == 0 ? 16 : 21, states);
            }
        }

        private int ExecuteWithPortSetup(byte opcode, byte portNumber = 0, byte portValue = 0)
        {
            Registers.C = portNumber;
            SetPortValue(portNumber, portValue);
            return Execute(opcode, prefix);
        }
    }
}