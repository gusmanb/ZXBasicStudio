using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System;

namespace Konamiman.Z80dotNet.Tests
{
    public class Z80ProcessorTests_MemoryAccess
    {
        Z80ProcessorForTests Sut { get; set; }
        Fixture Fixture { get; set; }
        Mock<IMemory> Memory { get; set; }
        Mock<IMemory> Ports { get; set; }

        [SetUp]
        public void Setup()
        {
            Fixture = new Fixture();

            Sut = new Z80ProcessorForTests();
            Sut.SetInstructionExecutionContextToNonNull();

            Memory = new Mock<IMemory>();
            Sut.Memory = Memory.Object;
            Ports = new Mock<IMemory>();
            Sut.PortsSpace = Ports.Object;
        }

        [Test]
        public void Can_create_instances()
        {
            Assert.IsNotNull(Sut);
        }

        #region ReadFromMemory and ReadFromPort

        [Test]
        [TestCase(MemoryAccessMode.ReadAndWrite, false)]
        [TestCase(MemoryAccessMode.ReadOnly, false)]
        [TestCase(MemoryAccessMode.ReadAndWrite, true)]
        [TestCase(MemoryAccessMode.ReadOnly, true)]
        public void ReadFromMemory_accesses_memory_if_memory_mode_is_ReadAndWrite_or_ReadOnly(MemoryAccessMode accessMode, bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            memory.Setup(m => m[address]).Returns(value);
            SetAccessMode(address, accessMode, isPort);

            var actual = Read(address, isPort);

            Assert.AreEqual(value, actual);
            memory.Verify(m => m[address]);
        }

        private void SetAccessMode(byte address, MemoryAccessMode accessMode, bool isPort)
        {
            if(isPort)
                Sut.SetPortsSpaceAccessMode(address, 1, accessMode);
            else
                Sut.SetMemoryAccessMode(address, 1, accessMode);
        }

        [Test]
        [TestCase(MemoryAccessMode.NotConnected, false)]
        [TestCase(MemoryAccessMode.WriteOnly, false)]       
        [TestCase(MemoryAccessMode.NotConnected, true)]
        [TestCase(MemoryAccessMode.WriteOnly, true)]
        public void ReadFromMemory_does_not_access_memory_if_memory_mode_is_NotConnected_or_WriteOnly(MemoryAccessMode accessMode, bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            memory.Setup(m => m[address]).Throws<Exception>();
            SetAccessMode(address, accessMode, isPort);

            Read(address, isPort);
        }

        byte Read(byte address, bool isPort)
        {
            if(isPort)
                return Sut.ReadFromPort(address);
            else
                return Sut.ReadFromMemory(address);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ReadFromMemory_fires_Before_event_with_appropriate_address_and_value(bool isPort)
        {
            var address = Fixture.Create<byte>();

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeReadEventType(isPort))
                {
                    eventFired = true;
                    Assert.AreEqual(Sut, sender);
                    Assert.AreEqual(address, args.Address);
                    Assert.AreEqual(0xFF, args.Value);
                }
            };

            Read(address, isPort);

            Assert.IsTrue(eventFired);
        }

        MemoryAccessEventType BeforeReadEventType(bool isPort)
        {
            if(isPort)
                return MemoryAccessEventType.BeforePortRead;
            else
                return MemoryAccessEventType.BeforeMemoryRead;
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ReadFromMemory_fires_After_event_with_appropriate_address_and_value_if_memory_is_accessed(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            memory.Setup(m => m[address]).Returns(value);

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == AfterReadEventType(isPort))
                {
                    eventFired = true;
                    Assert.AreEqual(Sut, sender);
                    Assert.AreEqual(address, args.Address);
                    Assert.AreEqual(value, args.Value);
                }
            };

            var actual = Read(address, isPort);

            Assert.IsTrue(eventFired);
            Assert.AreEqual(value, actual);
        }

        MemoryAccessEventType AfterReadEventType(bool isPort)
        {
            if(isPort)
                return MemoryAccessEventType.AfterPortRead;
            else
                return MemoryAccessEventType.AfterMemoryRead;
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ReadFromMemory_fires_After_event_with_same_value_as_Before_event_if_memory_is_not_accessed(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var beforeEventFired = false;
            var afterEventFired = false;

            memory.Setup(m => m[address]).Throws<Exception>();
            SetAccessMode(address, MemoryAccessMode.NotConnected, isPort);

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeReadEventType(isPort))
                {
                    beforeEventFired = true;
                    Assert.AreEqual(address, args.Address);
                    args.Value = value;
                }
                if(args.EventType == AfterReadEventType(isPort))
                {
                    afterEventFired = true;
                    Assert.AreEqual(address, args.Address);
                    Assert.AreEqual(value, args.Value);
                }
            };

            var actual = Read(address, isPort);

            Assert.IsTrue(beforeEventFired);
            Assert.IsTrue(afterEventFired);
        }
        
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ReadFromMemory_returns_value_set_in_After_event(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var valueFromMemory = Fixture.Create<byte>();
            var valueFromEvent = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            memory.Setup(m => m[address]).Returns(valueFromMemory);

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == AfterReadEventType(isPort))
                {
                    eventFired = true;
                    args.Value = valueFromEvent;
                }
            };

            var actual = Read(address, isPort);

            Assert.IsTrue(eventFired);
            Assert.AreEqual(valueFromEvent, actual);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ReadFromMemory_does_not_access_memory_if_Cancel_is_set_from_Before_event(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            memory.Setup(m => m[address]).Throws<Exception>();

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeReadEventType(isPort)) 
                {
                    args.CancelMemoryAccess = true;
                    args.Value = value;
                }
            };

            var actual = Read(address, isPort);

            Assert.AreEqual(value, actual);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ReadFromMemory_propagates_Cancel_from_Before_to_after_event(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            memory.Setup(m => m[address]).Throws<Exception>();

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeReadEventType(isPort)) 
                {
                    args.CancelMemoryAccess = true;
                }
                else if(args.EventType == AfterReadEventType(isPort))
                {
                    eventFired = true;
                    Assert.IsTrue(args.CancelMemoryAccess);
                }
            };

            Read(address, isPort);

            Assert.IsTrue(eventFired);
        }
        
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ReadFromMemory_enters_with_null_LocalState(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeReadEventType(isPort))
                {
                    eventFired = true;
                    Assert.IsNull(args.LocalUserState);
                }
            };

            Read(address, isPort);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ReadFromMemory_passes_LocalState_from_Before_to_After(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var localUserState = Fixture.Create<object>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeReadEventType(isPort))
                {
                    args.LocalUserState = localUserState;
                }
                else if(args.EventType == AfterReadEventType(isPort))
                {
                    eventFired = true;
                    Assert.AreEqual(localUserState, args.LocalUserState);
                }
            };

            Read(address, isPort);

            Assert.IsTrue(eventFired);
        }

        #endregion

        #region FetchNextOpcode

        [Test]
        public void FetchNextOpcode_reads_from_address_pointed_by_PC()
        {
            var address = Fixture.Create<ushort>();
            var value = Fixture.Create<byte>();

            Memory.Setup(m => m[address]).Returns(value);
            Sut.Registers.PC = address;

            var actual = Sut.FetchNextOpcode();

            Assert.AreEqual(value, actual);
            Memory.Verify(m => m[address]);
        }

        [Test]
        public void FetchNextOpcode_increases_PC_by_one()
        {
            var address = Fixture.Create<ushort>();
            Sut.Registers.PC = address;

            Sut.FetchNextOpcode();

            Assert.AreEqual(address.ToShort().Inc(), Sut.Registers.PC);
        }

        #endregion

        #region WriteToMemory and WriteToPort

        [Test]
        [TestCase(MemoryAccessMode.ReadAndWrite, false)]
        [TestCase(MemoryAccessMode.WriteOnly, false)]
        [TestCase(MemoryAccessMode.ReadAndWrite, true)]
        [TestCase(MemoryAccessMode.WriteOnly, true)]
        public void WriteToMemory_accesses_memory_if_memory_mode_is_ReadAndWrite_or_WriteOnly(MemoryAccessMode accessMode, bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            SetAccessMode(address, accessMode, isPort);

            Write(address, value, isPort);

            memory.VerifySet(m => m[address] = value);
        }

        void Write(byte address, byte value, bool isPort)
        {
            if(isPort)
                Sut.WriteToPort(address, value);
            else
                Sut.WriteToMemory(address, value);
        }

        
        [Test]
        [TestCase(MemoryAccessMode.NotConnected, false)]
        [TestCase(MemoryAccessMode.ReadOnly, false)]       
        [TestCase(MemoryAccessMode.NotConnected, true)]
        [TestCase(MemoryAccessMode.ReadOnly, true)]
        public void WriteToMemory_does_not_access_memory_if_memory_mode_is_NotConnected_or_ReadOnly(MemoryAccessMode accessMode, bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            memory.SetupSet(m => m[address] = value).Throws<Exception>();
            SetAccessMode(address, accessMode, isPort);

            Write(address, value, isPort);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WriteMemory_fires_Before_event_with_appropriate_address_and_value(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeWriteEventType(isPort))
                {
                    eventFired = true;
                    Assert.AreEqual(Sut, sender);
                    Assert.AreEqual(address, args.Address);
                    Assert.AreEqual(value, args.Value);
                }
            };

            Write(address, value, isPort);

            Assert.IsTrue(eventFired);
        }

        MemoryAccessEventType BeforeWriteEventType(bool isPort)
        {
            if(isPort)
                return MemoryAccessEventType.BeforePortWrite;
            else
                return MemoryAccessEventType.BeforeMemoryWrite;
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WriteToMemory_fires_After_event_with_appropriate_address_and_value_if_memory_is_accessed(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == AfterWriteEventType(isPort))
                {
                    eventFired = true;
                    Assert.AreEqual(Sut, sender);
                    Assert.AreEqual(address, args.Address);
                    Assert.AreEqual(value, args.Value);
                }
            };

            Write(address, value, isPort);

            Assert.IsTrue(eventFired);
            memory.VerifySet(m => m[address] = value);
        }

        MemoryAccessEventType AfterWriteEventType(bool isPort)
        {
            if(isPort)
                return MemoryAccessEventType.AfterPortWrite;
            else
                return MemoryAccessEventType.AfterMemoryWrite;
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WriteToMemory_fires_After_event_with_same_value_as_Before_event_if_memory_is_not_accessed(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var originalValue = Fixture.Create<byte>();
            var modifiedValue = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var beforeEventFired = false;
            var afterEventFired = false;

            memory.SetupSet(m => m[address] = modifiedValue).Throws<Exception>();
            SetAccessMode(address, MemoryAccessMode.NotConnected, isPort);

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeWriteEventType(isPort))
                {
                    beforeEventFired = true;
                    Assert.AreEqual(address, args.Address);
                    args.Value = modifiedValue;
                }
                if(args.EventType == AfterWriteEventType(isPort))
                {
                    afterEventFired = true;
                    Assert.AreEqual(address, args.Address);
                    Assert.AreEqual(modifiedValue, args.Value);
                }
            };

            Write(address, originalValue, isPort);

            Assert.IsTrue(beforeEventFired);
            Assert.IsTrue(afterEventFired);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WriteToMemory_writes_value_set_in_Before_event(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var originalValue = Fixture.Create<byte>();
            var modifiedValue = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeWriteEventType(isPort))
                {
                    eventFired = true;
                    args.Value = modifiedValue;
                }
            };

            Write(address, originalValue, isPort);

            Assert.IsTrue(eventFired);
            memory.VerifySet(m => m[address] = modifiedValue);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WriteToMemory_does_not_access_memory_if_Cancel_is_set_from_Before_event(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            memory.SetupSet(m => m[address] = value).Throws<Exception>();

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeWriteEventType(isPort))
                {
                    eventFired = true;
                    args.CancelMemoryAccess = true;
                    args.Value = value;
                }
            };

            Write(address, value, isPort);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WriteToMemory_propagates_Cancel_from_Before_to_after_event(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            memory.SetupSet(m => m[address] = value).Throws<Exception>();

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeWriteEventType(isPort)) 
                {
                    args.CancelMemoryAccess = true;
                }
                else if(args.EventType == AfterWriteEventType(isPort))
                {
                    eventFired = true;
                    Assert.IsTrue(args.CancelMemoryAccess);
                }
            };

            Write(address, value, isPort);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WriteToMemory_enters_with_null_LocalState(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeWriteEventType(isPort))
                {
                    eventFired = true;
                    Assert.IsNull(args.LocalUserState);
                }
            };

            Write(address, value, isPort);

            Assert.IsTrue(eventFired);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WriteToMemory_passes_LocalState_from_Before_to_After(bool isPort)
        {
            var address = Fixture.Create<byte>();
            var localUserState = Fixture.Create<object>();
            var value = Fixture.Create<byte>();
            var memory = isPort ? Ports : Memory;

            var eventFired = false;

            Sut.MemoryAccess += (sender, args) =>
            {
                if(args.EventType == BeforeWriteEventType(isPort))
                {
                    args.LocalUserState = localUserState;
                }
                else if(args.EventType == AfterWriteEventType(isPort))
                {
                    eventFired = true;
                    Assert.AreEqual(localUserState, args.LocalUserState);
                }
            };

            Write(address, value, isPort);

            Assert.IsTrue(eventFired);
        }

        #endregion

        #region Other

        [Test]
        public void ReadAndWrite_operations_fail_if_not_execution_an_instruction()
        {
            var address = Fixture.Create<byte>();
            var value = Fixture.Create<byte>();

            Sut.SetInstructionExecutionContextToNull();

            Assert.Throws<InvalidOperationException>(() => Sut.ReadFromMemory(address));
            Assert.Throws<InvalidOperationException>(() => Sut.ReadFromPort(address));
            Assert.Throws<InvalidOperationException>(() => Sut.FetchNextOpcode());
            Assert.Throws<InvalidOperationException>(() => Sut.WriteToMemory(address, value));
            Assert.Throws<InvalidOperationException>(() => Sut.WriteToPort(address, value));
        }

        #endregion
    }
}
