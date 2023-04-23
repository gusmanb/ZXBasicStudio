using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System;

namespace Konamiman.Z80dotNet.Tests
{
    public class Z80ProcessorTests_Configuration
    {
        private const int MemorySpaceSize = 65536;
        private const int PortSpaceSize = 256;

        Z80ProcessorForTests Sut { get; set; }
        Fixture Fixture { get; set; }

        [SetUp]
        public void Setup()
        {
            Fixture = new Fixture();

            Sut = new Z80ProcessorForTests();
        }

        [Test]
        public void Can_create_instances()
        {
            Assert.IsNotNull(Sut);
        }

        [Test]
        public void Has_proper_defaults()
        {
            Assert.AreEqual(4, Sut.ClockFrequencyInMHz);
            Assert.AreEqual(1, Sut.ClockSpeedFactor);

            Assert.IsTrue(Sut.AutoStopOnDiPlusHalt);
            Assert.IsFalse(Sut.AutoStopOnRetWithStackEmpty);
            Assert.AreEqual(0xFFFF.ToShort(), Sut.StartOfStack);

            Assert.IsInstanceOf<PlainMemory>(Sut.Memory);
            Assert.AreEqual(65536, Sut.Memory.Size);
            Assert.IsInstanceOf<PlainMemory>(Sut.PortsSpace);
            Assert.AreEqual(256, Sut.PortsSpace.Size);

            for(int i = 0; i < 65536; i++) {
                Assert.AreEqual(MemoryAccessMode.ReadAndWrite, Sut.GetMemoryAccessMode((ushort)i));
                Assert.AreEqual(0, Sut.GetMemoryWaitStatesForM1((ushort)i));
                Assert.AreEqual(0, Sut.GetMemoryWaitStatesForNonM1((ushort)i));
            }       
            for(int i = 0; i < 256; i++) {
                Assert.AreEqual(MemoryAccessMode.ReadAndWrite, Sut.GetPortAccessMode((byte) i));
                Assert.AreEqual(0, Sut.GetPortWaitStates((byte) i));
            }

            Assert.IsInstanceOf<Z80Registers>(Sut.Registers);

            Assert.IsInstanceOf<Z80InstructionExecutor>(Sut.InstructionExecutor);
            Assert.AreSame(Sut, Sut.InstructionExecutor.ProcessorAgent);
            Assert.IsInstanceOf<ClockSynchronizer>(Sut.ClockSynchronizer);

            Assert.AreEqual(StopReason.NeverRan, Sut.StopReason);
            Assert.AreEqual(ProcessorState.Stopped, Sut.State);
            Assert.IsFalse(Sut.IsHalted);
            Assert.IsNull(Sut.UserState);
        }

        [Test]
        public void Reset_sets_registers_properly()
        {
            Sut.Registers.IFF1 = 1;
            Sut.Registers.IFF1 = 1;
            Sut.Registers.PC = 1;
            Sut.Registers.AF = 0;
            Sut.Registers.SP = 0;
            Sut.InterruptMode = 1;
            Sut.SetIsHalted();
            
            Sut.Reset();

            Assert.AreEqual(0xFFFF.ToShort(), Sut.Registers.AF);
            Assert.AreEqual(0xFFFF.ToShort(), Sut.Registers.SP);
            Assert.AreEqual(0, Sut.Registers.PC);
            Assert.AreEqual(0, Sut.Registers.IFF1);
            Assert.AreEqual(0, Sut.Registers.IFF2);
            Assert.AreEqual(0, Sut.InterruptMode);

            Assert.AreEqual(0, Sut.TStatesElapsedSinceReset);

            Assert.False(Sut.IsHalted);
        }

        [Test]
        public void Interrupt_mode_can_be_set_to_0_1_or_2()
        {
            Sut.InterruptMode = 0;
            Assert.AreEqual(Sut.InterruptMode, 0);

            Sut.InterruptMode = 1;
            Assert.AreEqual(Sut.InterruptMode, 1);

            Sut.InterruptMode = 2;
            Assert.AreEqual(Sut.InterruptMode, 2);
        }

        [Test]
        public void Interrupt_mode_cannot_be_set_to_higher_than_2()
        {
            Assert.Throws<ArgumentException>(() => Sut.InterruptMode = 3);
        }

        [Test]
        public void SetMemoryAccessMode_and_GetMemoryAccessMode_are_consistent()
        {
            Sut.SetMemoryAccessMode(0, 0x4000, MemoryAccessMode.NotConnected);
            Sut.SetMemoryAccessMode(0x4000, 0x4000, MemoryAccessMode.ReadAndWrite);
            Sut.SetMemoryAccessMode(0x8000, 0x4000, MemoryAccessMode.ReadOnly);
            Sut.SetMemoryAccessMode(0xC000, 0x4000, MemoryAccessMode.WriteOnly);

            Assert.AreEqual(MemoryAccessMode.NotConnected, Sut.GetMemoryAccessMode(0));
            Assert.AreEqual(MemoryAccessMode.NotConnected, Sut.GetMemoryAccessMode(0x3FFF));
            Assert.AreEqual(MemoryAccessMode.ReadAndWrite, Sut.GetMemoryAccessMode(0x4000));
            Assert.AreEqual(MemoryAccessMode.ReadAndWrite, Sut.GetMemoryAccessMode(0x7FFF));
            Assert.AreEqual(MemoryAccessMode.ReadOnly, Sut.GetMemoryAccessMode(0x8000));
            Assert.AreEqual(MemoryAccessMode.ReadOnly, Sut.GetMemoryAccessMode(0xBFFF));
            Assert.AreEqual(MemoryAccessMode.WriteOnly, Sut.GetMemoryAccessMode(0xC000));
            Assert.AreEqual(MemoryAccessMode.WriteOnly, Sut.GetMemoryAccessMode(0xFFFF));
        }

        [Test]
        public void SetMemoryAccessMode_works_when_address_plus_length_are_on_memory_size_boundary()
        {
           var value = Fixture.Create<MemoryAccessMode>();
           var length = Fixture.Create<byte>();

           Sut.SetMemoryAccessMode((ushort)(MemorySpaceSize - length), length, MemoryAccessMode.NotConnected);
        }

        [Test]
        public void SetMemoryAccessMode_fails_when_address_plus_length_are_beyond_memory_size_boundary()
        {
            var value = Fixture.Create<MemoryAccessMode>();
            var length = Fixture.Create<byte>();

            Assert.Throws<ArgumentException>(() => Sut.SetMemoryAccessMode((ushort)(MemorySpaceSize - length), length + 1, MemoryAccessMode.NotConnected));
        }

        [Test]
        public void SetMemoryAccessMode_fails_when_length_is_negative()
        {
            var value = Fixture.Create<MemoryAccessMode>();

            Assert.Throws<ArgumentException>(() => Sut.SetMemoryAccessMode(0, -1, value));
        }

        [Test]
        public void SetPortsSpaceAccessMode_and_GetPortsSpaceAccessMode_are_consistent()
        {
            Sut.SetPortsSpaceAccessMode(0, 64, MemoryAccessMode.NotConnected);
            Sut.SetPortsSpaceAccessMode(64, 64, MemoryAccessMode.ReadAndWrite);
            Sut.SetPortsSpaceAccessMode(128, 64, MemoryAccessMode.ReadOnly);
            Sut.SetPortsSpaceAccessMode(192, 64, MemoryAccessMode.WriteOnly);

            Assert.AreEqual(MemoryAccessMode.NotConnected, Sut.GetPortAccessMode(0));
            Assert.AreEqual(MemoryAccessMode.NotConnected, Sut.GetPortAccessMode(63));
            Assert.AreEqual(MemoryAccessMode.ReadAndWrite, Sut.GetPortAccessMode(64));
            Assert.AreEqual(MemoryAccessMode.ReadAndWrite, Sut.GetPortAccessMode(127));
            Assert.AreEqual(MemoryAccessMode.ReadOnly, Sut.GetPortAccessMode(128));
            Assert.AreEqual(MemoryAccessMode.ReadOnly, Sut.GetPortAccessMode(191));
            Assert.AreEqual(MemoryAccessMode.WriteOnly, Sut.GetPortAccessMode(192));
            Assert.AreEqual(MemoryAccessMode.WriteOnly, Sut.GetPortAccessMode(255));
        }

        [Test]
        public void SetPortsAccessMode_works_when_address_plus_length_are_on_ports_space_size_boundary()
        {
            var value = Fixture.Create<MemoryAccessMode>();
            var length = Fixture.Create<byte>();

            Sut.SetPortsSpaceAccessMode((byte)(PortSpaceSize - length), length, MemoryAccessMode.NotConnected);
        }

        [Test]
        public void SetPortsAccessMode_fails_when_address_plus_length_are_beyond_ports_space_size_boundary()
        {
            var value = Fixture.Create<MemoryAccessMode>();
            var length = Fixture.Create<byte>();

            Assert.Throws<ArgumentException>(() => Sut.SetPortsSpaceAccessMode((byte)(PortSpaceSize - length), length + 1, MemoryAccessMode.NotConnected));
        }

        [Test]
        public void SetPortsSpaceAccessMode_fails_when_length_is_negative()
        {
            var value = Fixture.Create<MemoryAccessMode>();

            Assert.Throws<ArgumentException>(() => Sut.SetPortsSpaceAccessMode(0, -1, value));
        }

        [Test]
        public void SetMemoryWaitStatesForM1_and_GetMemoryWaitStatesForM1_are_consistent()
        {
            var value1 = Fixture.Create<byte>();
            var value2 = Fixture.Create<byte>();

            Sut.SetMemoryWaitStatesForM1(0, 0x8000, value1);
            Sut.SetMemoryWaitStatesForM1(0x8000, 0x8000, value2);

            Assert.AreEqual(value1, Sut.GetMemoryWaitStatesForM1(0));
            Assert.AreEqual(value1, Sut.GetMemoryWaitStatesForM1(0x7FFF));
            Assert.AreEqual(value2, Sut.GetMemoryWaitStatesForM1(0x8000));
            Assert.AreEqual(value2, Sut.GetMemoryWaitStatesForM1(0xFFFF));
        }

        [Test]
        public void SetMemoryWaitStatesForM1_works_when_address_plus_length_are_in_memory_size_boundary()
        {
            var value = Fixture.Create<byte>();
            var length = Fixture.Create<byte>();

            Sut.SetMemoryWaitStatesForM1((ushort)(MemorySpaceSize - length), length, value);
        }

        [Test]
        public void SetMemoryWaitStatesForM1_fails_when_address_plus_length_are_beyond_memory_size_boundary()
        {
            var value = Fixture.Create<byte>();
            var length = Fixture.Create<byte>();

            Assert.Throws<ArgumentException>(
                () => Sut.SetMemoryWaitStatesForM1((ushort)(MemorySpaceSize - length), length + 1, value));
        }

        [Test]
        public void SetMemoryWaitStatesForM1_fails_when_length_is_negative()
        {
            var value = Fixture.Create<byte>();

            Assert.Throws<ArgumentException>(
                () => Sut.SetMemoryWaitStatesForM1(0, -1, value));
        }
        
        [Test]
        public void SetMemoryWaitStatesForNonM1_and_GetMemoryWaitStatesForNonM1_are_consistent()
        {
            var value1 = Fixture.Create<byte>();
            var value2 = Fixture.Create<byte>();

            Sut.SetMemoryWaitStatesForNonM1(0, 0x8000, value1);
            Sut.SetMemoryWaitStatesForNonM1(0x8000, 0x8000, value2);

            Assert.AreEqual(value1, Sut.GetMemoryWaitStatesForNonM1(0));
            Assert.AreEqual(value1, Sut.GetMemoryWaitStatesForNonM1(0x7FFF));
            Assert.AreEqual(value2, Sut.GetMemoryWaitStatesForNonM1(0x8000));
            Assert.AreEqual(value2, Sut.GetMemoryWaitStatesForNonM1(0xFFFF));
        }

        [Test]
        public void SetMemoryWaitStatesForNonM1_works_when_address_plus_length_are_in_memory_size_boundary()
        {
            var value = Fixture.Create<byte>();
            var length = Fixture.Create<byte>();

            Sut.SetMemoryWaitStatesForNonM1((ushort)(MemorySpaceSize - length), length, value);
        }

        [Test]
        public void SetMemoryWaitStatesForNonM1_fails_when_address_plus_length_are_beyond_memory_size_boundary()
        {
            var value = Fixture.Create<byte>();
            var length = Fixture.Create<byte>();

            Assert.Throws<ArgumentException>(
                () => Sut.SetMemoryWaitStatesForNonM1((ushort)(MemorySpaceSize - length), length + 1, value));
        }

        [Test]
        public void SetMemoryWaitStatesForNonM1_fails_when_length_is_negative()
        {
            var value = Fixture.Create<byte>();

            Assert.Throws<ArgumentException>(
                () => Sut.SetMemoryWaitStatesForNonM1(0, -1, value));
        }
        
        [Test]
        public void SetPortWaitStates_and_GetPortWaitStates_are_consistent()
        {
            var value1 = Fixture.Create<byte>();
            var value2 = Fixture.Create<byte>();

            Sut.SetPortWaitStates(0, 128, value1);
            Sut.SetPortWaitStates(128, 128, value2);

            Assert.AreEqual(value1, Sut.GetPortWaitStates(0));
            Assert.AreEqual(value1, Sut.GetPortWaitStates(127));
            Assert.AreEqual(value2, Sut.GetPortWaitStates(128));
            Assert.AreEqual(value2, Sut.GetPortWaitStates(255));
        }

        [Test]
        public void SetPortWaitStates_works_when_address_plus_length_are_in_memory_size_boundary()
        {
            var value = Fixture.Create<byte>();
            var length = Fixture.Create<byte>();

            Sut.SetPortWaitStates((ushort)(PortSpaceSize - length), length, value);
        }

        [Test]
        public void SetPortWaitStates_fails_when_address_plus_length_are_beyond_memory_size_boundary()
        {
            var value = Fixture.Create<byte>();
            var length = Fixture.Create<byte>();

            Assert.Throws<ArgumentException>(
                () => Sut.SetPortWaitStates((ushort)(PortSpaceSize - length), length + 1, value));
        }

        [Test]
        public void SetPortWaitStates_fails_when_length_is_negative()
        {
            var value = Fixture.Create<byte>();

            Assert.Throws<ArgumentException>(
                () => Sut.SetPortWaitStates(0, -1, value));
        }

        [Test]
        public void Can_set_Memory_to_non_null_value()
        {
            var value = new Mock<IMemory>().Object;
            Sut.Memory = value;
            Assert.AreEqual(value, Sut.Memory);
        }

        [Test]
        public void Cannot_set_Memory_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => Sut.Memory = null);
        }

        [Test]
        public void Can_set_Registers_to_non_null_value()
        {
            var value = new Mock<IZ80Registers>().Object;
            Sut.Registers = value;
            Assert.AreEqual(value, Sut.Registers);
        }

        [Test]
        public void Cannot_set_Registers_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => Sut.Registers = null);
        }

        [Test]
        public void Can_set_PortsSpace_to_non_null_value()
        {
            var value = new Mock<IMemory>().Object;
            Sut.PortsSpace = value;
            Assert.AreEqual(value, Sut.PortsSpace);
        }

        [Test]
        public void Cannot_set_PortsSpace_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => Sut.PortsSpace = null);
        }

        [Test]
        public void Can_set_InstructionExecutor_to_non_null_value()
        {
            var value = new Mock<IZ80InstructionExecutor>().Object;
            Sut.InstructionExecutor = value;
            Assert.AreEqual(value, Sut.InstructionExecutor);
        }

        [Test]
        public void Cannot_set_InstructionExecutor_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => Sut.InstructionExecutor = null);
        }

        [Test]
        public void Sets_InstructionExecutor_agent_to_self()
        {
            var mock = new Mock<IZ80InstructionExecutor>();
            Sut.InstructionExecutor = mock.Object;

            mock.VerifySet(m => m.ProcessorAgent = Sut);
        }

        [Test]
        public void Can_set_ClockSynchronizationHelper_to_non_null_value()
        {
            var value = new Mock<IClockSynchronizer>().Object;
            Sut.ClockSynchronizer = value;
            Assert.AreEqual(value, Sut.ClockSynchronizer);
        }

        [Test]
        public void Can_set_ClockSynchronizationHelper_to_null()
        {
            Sut.ClockSynchronizer = null;
        }

        [Test]
        public void Sets_ClockSynchronizationHelper_clockSpeed_to_processor_speed_by_speed_factor()
        {
            var mock = new Mock<IClockSynchronizer>();
            Sut.ClockFrequencyInMHz = 2;
            Sut.ClockSpeedFactor = 3;
            Sut.ClockSynchronizer = mock.Object;

            mock.VerifySet(m => m.EffectiveClockFrequencyInMHz = 2 * 3);
        }

        [Test]
        public void Can_set_clock_speed_and_clock_factor_combination_up_to_100_MHz()
        {
            Sut.ClockFrequencyInMHz = 20;
            Sut.ClockSpeedFactor = 5;

            Assert.AreEqual(Sut.ClockFrequencyInMHz, 20);
            Assert.AreEqual(Sut.ClockSpeedFactor, 5);
        }

        [Test]
        public void Cannot_set_clock_speed_and_clock_factor_combination_over_100_MHz()
        {
            Sut.ClockFrequencyInMHz = 1;
            Sut.ClockSpeedFactor = 1;

            Assert.Throws<ArgumentException>(() =>
            {
                Sut.ClockFrequencyInMHz = 1;
                Sut.ClockSpeedFactor = 101;
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Sut.ClockSpeedFactor = 1;
                Sut.ClockFrequencyInMHz = 101;
            });
        }

        [Test]
        public void Can_set_clock_speed_and_clock_factor_combination_down_to_1_KHz()
        {
            Sut.ClockFrequencyInMHz = 0.1M;
            Sut.ClockSpeedFactor = 0.01M;

            Assert.AreEqual(Sut.ClockFrequencyInMHz, 0.1M);
            Assert.AreEqual(Sut.ClockSpeedFactor, 0.01M);
        }

        [Test]
        public void Cannot_set_clock_speed_and_clock_factor_combination_under_1_KHz()
        {
            Sut.ClockFrequencyInMHz = 1;
            Sut.ClockSpeedFactor = 1;

            Assert.Throws<ArgumentException>(() =>
            {
                Sut.ClockFrequencyInMHz = 1;
                Sut.ClockSpeedFactor = 0.0009M;
            });

            Assert.Throws<ArgumentException>(() =>
            {
                Sut.ClockSpeedFactor = 0.0009M;
                Sut.ClockFrequencyInMHz = 1;
            });
        }
    }
}
