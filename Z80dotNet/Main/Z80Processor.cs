using Main.Dependencies_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// The implementation of the <see cref="IZ80Processor"/> interface.
    /// </summary>
    public class Z80Processor : IZ80Processor, IZ80ProcessorAgent
    {
        private const int MemorySpaceSize = 65536;
        private const int PortSpaceSize = 65536;

        private const ushort NmiServiceRoutine = 0x66;
        private const byte NOP_opcode = 0x00;
        private const byte RST38h_opcode = 0xFF;

        public Z80Processor()
        {
            unchecked { StartOfStack =  (short)0xFFFF; }

            SetMemoryWaitStatesForM1(0, MemorySpaceSize, 0);
            SetMemoryWaitStatesForNonM1(0, MemorySpaceSize, 0);
            SetPortWaitStates(0, PortSpaceSize, 0);

            Memory = new PlainMemory(MemorySpaceSize);
            PortsSpace = new PlainIO();

            SetMemoryAccessMode(0, MemorySpaceSize, MemoryAccessMode.ReadAndWrite);
            SetPortsSpaceAccessMode(0, PortSpaceSize, MemoryAccessMode.ReadAndWrite);

            Registers = new Z80Registers();
            InterruptSources = new List<IZ80InterruptSource>();
            TStatesTargets = new List<ITStatesTarget>();

            InstructionExecutor = new Z80InstructionExecutor();

            executionContext = new InstructionExecutionContext();
        }

        #region Processor control

        private int ExecuteNextOpcode()
        {
            if(IsHalted) {
                executionContext.OpcodeBytes.Add(NOP_opcode);
                return InstructionExecutor.Execute(NOP_opcode);
            }

            return InstructionExecutor.Execute(FetchNextOpcode());
        }

        private int AcceptPendingInterrupt()
        {
            if(executionContext.IsEiOrDiInstruction)
                return 0;

            if(NmiInterruptPending) {
                IsHalted = false;
                Registers.IFF1 = 0;
                ExecuteCall(NmiServiceRoutine);
                return 11;
            }

            if(!InterruptsEnabled)
                return 0;

            var activeIntSource = InterruptSources.FirstOrDefault(s => s.IntLineIsActive);
            if(activeIntSource == null)
                return 0;

            Registers.IFF1 = 0;
            Registers.IFF2 = 0;
            IsHalted = false;

            switch(InterruptMode) {
                case 0:
                    var opcode = activeIntSource.ValueOnDataBus.GetValueOrDefault(0xFF);
                    InstructionExecutor.Execute(opcode);
                    return 13;
                case 1:
                    InstructionExecutor.Execute(RST38h_opcode);
                    return 13;
                case 2:
                    var pointerAddress = NumberUtils.CreateUshort(
                        lowByte: activeIntSource.ValueOnDataBus.GetValueOrDefault(0xFF),
                        highByte: Registers.I);
                    var callAddress = NumberUtils.CreateUshort(
                        lowByte: ReadFromMemoryInternal(pointerAddress),
                        highByte: ReadFromMemoryInternal((ushort)(pointerAddress + 1)));
                    ExecuteCall(callAddress);
                    return 19;
            }

            return 0;
        }

        public void ExecuteCall(ushort address)
        {
            var oldAddress = (short)Registers.PC;
            var sp = (ushort)(Registers.SP - 1);
            WriteToMemoryInternal(sp, oldAddress.GetHighByte());
            sp = (ushort)(sp - 1);
            WriteToMemoryInternal(sp, oldAddress.GetLowByte());

            Registers.SP = (short)sp;
            Registers.PC = address;
        }

        public void ExecuteRet()
        {
            var sp = (ushort)Registers.SP;
            var newPC = NumberUtils.CreateShort(ReadFromMemoryInternal(sp), ReadFromMemoryInternal((ushort)(sp + 1)));

            Registers.PC = (ushort)newPC;
            Registers.SP += 2;
        }

        private void ThrowIfNoFetchFinishedEventFired()
        {
            if (executionContext.FetchComplete)
                return;

            throw new InstructionFetchFinishedEventNotFiredException(
                instructionAddress: (ushort)(Registers.PC - executionContext.OpcodeBytes.Count),
                fetchedBytes: executionContext.OpcodeBytes.ToArray());
        }
        
        private bool InterruptsEnabled
        {
            get
            {
                return Registers.IFF1 == 1;
            }
        }
        
        void FireAfterInstructionExecutionEvent(int tStates)
        {
            AfterInstructionExecution?.Invoke(this, new AfterInstructionExecutionEventArgs(
                executionContext.OpcodeBytes.ToArray(),
                stopper: this,
                localUserState: executionContext.LocalUserStateFromPreviousEvent,
                tStates: tStates));
        }

        void InstructionExecutor_InstructionFetchFinished(object sender, InstructionFetchFinishedEventArgs e)
        {
            if(executionContext.FetchComplete)
                return;

            executionContext.FetchComplete = true;

            executionContext.IsRetInstruction = e.IsRetInstruction;
            executionContext.IsLdSpInstruction = e.IsLdSpInstruction;
            executionContext.IsHaltInstruction = e.IsHaltInstruction;
            executionContext.IsEiOrDiInstruction = e.IsEiOrDiInstruction;

            executionContext.SpAfterInstructionFetch = Registers.SP;

            var eventArgs = FireBeforeInstructionExecutionEvent();
            executionContext.LocalUserStateFromPreviousEvent = eventArgs.LocalUserState;
        }

        void FireBeforeInstructionFetchEvent()
        {
            var eventArgs = new BeforeInstructionFetchEventArgs(stopper: this);

            if(BeforeInstructionFetch != null) {
                executionContext.ExecutingBeforeInstructionEvent = true;
                try {
                    BeforeInstructionFetch(this, eventArgs);
                }
                finally {
                    executionContext.ExecutingBeforeInstructionEvent = false;
                }
            }

            executionContext.LocalUserStateFromPreviousEvent = eventArgs.LocalUserState;
        }

        BeforeInstructionExecutionEventArgs FireBeforeInstructionExecutionEvent()
        {
            var eventArgs = new BeforeInstructionExecutionEventArgs(
                executionContext.OpcodeBytes.ToArray(),
                executionContext.LocalUserStateFromPreviousEvent);

            if(BeforeInstructionExecution != null)
                BeforeInstructionExecution(this, eventArgs);

            return eventArgs;
        }

        public void Reset()
        {
            Registers.IFF1 = 0;
            Registers.IFF2 = 0;
            Registers.PC = 0;
            unchecked { Registers.AF = (short)0xFFFF; }
            unchecked { Registers.SP = (short)0xFFFF; }
            InterruptMode = 0;

            NmiInterruptPending = false;
            IsHalted = false;

            TStatesElapsedSinceReset = 0;
            StartOfStack = Registers.SP;
        }

        public int ExecuteNextInstruction()
        {
            executionContext.StartNewInstruction();

            var totalTStates = 0;

            FireBeforeInstructionFetchEvent();

            if (executionContext.MustStop)
                return totalTStates;

            var executionTStates = ExecuteNextOpcode();

            totalTStates = executionTStates + executionContext.AccummulatedMemoryWaitStates;
            TStatesElapsedSinceStart += (ulong)totalTStates;
            TStatesElapsedSinceReset += (ulong)totalTStates;

            UpdateTStatesTargets();

            ThrowIfNoFetchFinishedEventFired();

            FireAfterInstructionExecutionEvent(totalTStates);

            if (!IsHalted)
                IsHalted = executionContext.IsHaltInstruction;

            var interruptTStates = AcceptPendingInterrupt();
            totalTStates += interruptTStates;
            TStatesElapsedSinceStart += (ulong)interruptTStates;
            TStatesElapsedSinceReset += (ulong)interruptTStates;

            UpdateTStatesTargets();

            executionContext.StopReason = StopReason.ExecuteNextInstructionInvoked;

            return totalTStates;
        }

        private void UpdateTStatesTargets()
        {
            var tStates = TStatesElapsedSinceStart;

            for (int buc = 0; buc < TStatesTargets.Count; buc++)
                TStatesTargets[buc].TStates = tStates;


            //foreach (var target in TStatesTargets)
            //    target.TStates = TStatesElapsedSinceStart;
        }

        #endregion

        #region Information and state

        public ulong TStatesElapsedSinceStart { get; private set; }

        public ulong TStatesElapsedSinceReset { get; private set; }

        public object UserState { get; set; }

        public bool IsHalted { get; protected set; }

        private byte _InterruptMode;
        public byte InterruptMode
        {
            get
            {
                return _InterruptMode;
            }
            set
            {
                if(value > 2)
                    throw new ArgumentException("Interrupt mode can be set to 0, 1 or 2 only");

                _InterruptMode = value;
            }
        }

        public short StartOfStack { get; protected set; }

        #endregion

        #region Inside and outside world

        private IZ80Registers _Registers;
        public IZ80Registers Registers
        {
            get
            {
                return _Registers;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("Registers");

                _Registers = value;
            }
        }

        private IMemory _Memory;
        public IMemory Memory
        {
            get
            {
                return _Memory;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("Memory");

                _Memory = value;
            }
        }

        private MemoryAccessMode[] memoryAccessModes = new MemoryAccessMode[MemorySpaceSize];

        public void SetMemoryAccessMode(ushort startAddress, int length, MemoryAccessMode mode)
        {
             SetArrayContents(memoryAccessModes, startAddress, length, mode);
        }

        private void SetArrayContents<T>(T[] array, ushort startIndex, int length, T value)
        {
            if(length < 0)
                throw new ArgumentException("length can't be negative");
            if(startIndex + length > array.Length)
                throw new ArgumentException("start + length go beyond " + (array.Length - 1));

            var data = Enumerable.Repeat(value, length).ToArray();
            Array.Copy(data, 0, array, startIndex, length);
        }

        public MemoryAccessMode GetMemoryAccessMode(ushort address)
        {
            return memoryAccessModes[address];
        }

        private IIO _PortsSpace;
        public IIO PortsSpace
        {
            get
            {
                return _PortsSpace;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("PortsSpace");

                _PortsSpace = value;
            }
        }

        private MemoryAccessMode[] portsAccessModes = new MemoryAccessMode[PortSpaceSize];

        public void SetPortsSpaceAccessMode(byte startPort, int length, MemoryAccessMode mode)
        {
            SetArrayContents(portsAccessModes, startPort, length, mode);
        }

        public MemoryAccessMode GetPortAccessMode(ushort portNumber)
        {
            return portsAccessModes[portNumber];
        }

        private IList<IZ80InterruptSource> InterruptSources { get; set; }

        public void RegisterInterruptSource(IZ80InterruptSource source)
        {
            if(InterruptSources.Contains(source))
                return;

            InterruptSources.Add(source);
            source.NmiInterruptPulse += (sender, args) => NmiInterruptPending = true;
        }

        private readonly object nmiInterruptPendingSync = new object();
        private bool _nmiInterruptPending;
        private bool NmiInterruptPending
        {
            get
            {
                lock(nmiInterruptPendingSync) {
                    var value = _nmiInterruptPending;
                    _nmiInterruptPending = false;
                    return value;
                }
            }
            set
            {
                lock(nmiInterruptPendingSync) {
                    _nmiInterruptPending = value;
                }
            }
        }

        public IEnumerable<IZ80InterruptSource> GetRegisteredInterruptSources()
        {
            return InterruptSources.ToArray();
        }

        public void UnregisterAllInterruptSources()
        {
            foreach(var source in InterruptSources) {
                source.NmiInterruptPulse -= (sender, args) => NmiInterruptPending = true;
            }

            InterruptSources.Clear();
        }

        private IList<ITStatesTarget> TStatesTargets { get; set; }
        public void RegisterTStatesTarget(ITStatesTarget target)
        {
            if (TStatesTargets.Contains(target))
                return;

            TStatesTargets.Add(target);
        }
        public void UnregisterAllTStatesTargets()
        {
            TStatesTargets.Clear();
        }

        public IEnumerable<ITStatesTarget> GetRegisteredTStatesTarget()
        {
            return TStatesTargets.ToArray();
        }

        #endregion

        #region Configuration

        private byte[] memoryWaitStatesForM1 = new byte[MemorySpaceSize];

        public void SetMemoryWaitStatesForM1(ushort startAddress, int length, byte waitStates)
        {
            SetArrayContents(memoryWaitStatesForM1, startAddress, length, waitStates);
        }

        public byte GetMemoryWaitStatesForM1(ushort address)
        {
            return memoryWaitStatesForM1[address];
        }

        private byte[] memoryWaitStatesForNonM1 = new byte[MemorySpaceSize];

        public void SetMemoryWaitStatesForNonM1(ushort startAddress, int length, byte waitStates)
        {
            SetArrayContents(memoryWaitStatesForNonM1, startAddress, length, waitStates);
        }

        public byte GetMemoryWaitStatesForNonM1(ushort address)
        {
            return memoryWaitStatesForNonM1[address];
        }

        private byte[] portWaitStates = new byte[PortSpaceSize];

        public void SetPortWaitStates(ushort startPort, int length, byte waitStates)
        {
            SetArrayContents(portWaitStates, startPort, length, waitStates);
        }

        public byte GetPortWaitStates(ushort portNumber)
        {
            return portWaitStates[portNumber];
        }

        private IZ80InstructionExecutor _InstructionExecutor;
        public IZ80InstructionExecutor InstructionExecutor
        {
            get
            {
                return _InstructionExecutor;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("InstructionExecutor");

                if(_InstructionExecutor != null)
                    _InstructionExecutor.InstructionFetchFinished -= InstructionExecutor_InstructionFetchFinished;

                _InstructionExecutor = value;
                _InstructionExecutor.ProcessorAgent = this;
                _InstructionExecutor.InstructionFetchFinished += InstructionExecutor_InstructionFetchFinished;
            }
        }

        #endregion

        #region Events

        public event EventHandler<MemoryAccessEventArgs> MemoryAccess;

        public event EventHandler<BeforeInstructionFetchEventArgs> BeforeInstructionFetch;

        public event EventHandler<BeforeInstructionExecutionEventArgs> BeforeInstructionExecution;

        public event EventHandler<AfterInstructionExecutionEventArgs> AfterInstructionExecution;

        #endregion

        #region Members of IZ80ProcessorAgent

        public byte FetchNextOpcode()
        {
            FailIfNoExecutionContext();

            if(executionContext.FetchComplete)
                throw new InvalidOperationException("FetchNextOpcode can be invoked only before the InstructionFetchFinished event has been raised.");

            byte opcode;
            if (executionContext.PeekedOpcode == null)
            {
                var address = Registers.PC;
                opcode = ReadFromMemory(
                    address,
                    Memory,
                    GetMemoryAccessMode(address),
                    MemoryAccessEventType.BeforeMemoryRead,
                    MemoryAccessEventType.AfterMemoryRead,
                    GetMemoryWaitStatesForM1(address));
            }
            else
            {
                executionContext.AccummulatedMemoryWaitStates +=
                    GetMemoryWaitStatesForM1(executionContext.AddressOfPeekedOpcode);
                opcode = executionContext.PeekedOpcode.Value;
                executionContext.PeekedOpcode = null;
            }

            executionContext.OpcodeBytes.Add(opcode);
            Registers.PC++;
            return opcode;
        }
        
        public byte PeekNextOpcode()
        {
            FailIfNoExecutionContext();

            if(executionContext.FetchComplete)
                throw new InvalidOperationException("PeekNextOpcode can be invoked only before the InstructionFetchFinished event has been raised.");

            if (executionContext.PeekedOpcode == null)
            {
                var address = Registers.PC;
                var opcode = ReadFromMemory(
                    address,
                    Memory,
                    GetMemoryAccessMode(address),
                    MemoryAccessEventType.BeforeMemoryRead,
                    MemoryAccessEventType.AfterMemoryRead,
                    waitStates: 0);

                executionContext.PeekedOpcode = opcode;
                executionContext.AddressOfPeekedOpcode = Registers.PC;
                return opcode;
            }
            else
            {
                return executionContext.PeekedOpcode.Value;
            }
        }

        private void FailIfNoExecutionContext()
        {
            if(executionContext == null)
                throw new InvalidOperationException("This method can be invoked only when an instruction is being executed.");
        }

        public byte ReadFromMemory(ushort address)
        {
            FailIfNoExecutionContext();
            FailIfNoInstructionFetchComplete();

            return ReadFromMemoryInternal(address);
        }

        private byte ReadFromMemoryInternal(ushort address)
        {
            return ReadFromMemory(
                address,
                Memory,
                GetMemoryAccessMode(address),
                MemoryAccessEventType.BeforeMemoryRead,
                MemoryAccessEventType.AfterMemoryRead,
                GetMemoryWaitStatesForNonM1(address));
        }

        protected virtual void FailIfNoInstructionFetchComplete()
        {
            if(executionContext != null && !executionContext.FetchComplete)
                throw new InvalidOperationException("IZ80ProcessorAgent members other than FetchNextOpcode can be invoked only after the InstructionFetchFinished event has been raised.");
        }

        private byte ReadFromMemory(
            ushort address,
            IMemory memory,
            MemoryAccessMode accessMode,
            MemoryAccessEventType beforeEventType,
            MemoryAccessEventType afterEventType,
            byte waitStates)
        {
            if (MemoryAccess == null)
                return memory[address];

            var beforeEventArgs = FireMemoryAccessEvent(beforeEventType, address, 0xFF);

            byte value;
            if(!beforeEventArgs.CancelMemoryAccess && 
                (accessMode == MemoryAccessMode.ReadAndWrite || accessMode == MemoryAccessMode.ReadOnly))
                value = memory[address];
            else
                value = beforeEventArgs.Value;

            if(executionContext != null)
                executionContext.AccummulatedMemoryWaitStates += waitStates;

            var afterEventArgs = FireMemoryAccessEvent(
                afterEventType, 
                address,
                value,
                beforeEventArgs.LocalUserState,
                beforeEventArgs.CancelMemoryAccess);
            return afterEventArgs.Value;
        }

        private byte ReadFromPort(
            byte portLo,
            byte portHi,
            IIO io,
            MemoryAccessMode accessMode,
            MemoryAccessEventType beforeEventType,
            MemoryAccessEventType afterEventType,
            byte waitStates)
        {

            if (MemoryAccess == null)
                return io[portLo, portHi];

            var beforeEventArgs = FireMemoryAccessEvent(beforeEventType, portLo, 0xFF);

            byte value;
            if (!beforeEventArgs.CancelMemoryAccess &&
                (accessMode == MemoryAccessMode.ReadAndWrite || accessMode == MemoryAccessMode.ReadOnly))
                value = io[portLo, portHi];
            else
                value = beforeEventArgs.Value;

            if (executionContext != null)
                executionContext.AccummulatedMemoryWaitStates += waitStates;

            var afterEventArgs = FireMemoryAccessEvent(
            afterEventType,
                portLo,
                value,
                beforeEventArgs.LocalUserState,
                beforeEventArgs.CancelMemoryAccess);
            return afterEventArgs.Value;
        }

        MemoryAccessEventArgs FireMemoryAccessEvent(
            MemoryAccessEventType eventType,
            ushort address, 
            byte value, 
            object localUserState = null,
            bool cancelMemoryAccess = false)
        {
            var eventArgs = new MemoryAccessEventArgs(eventType, address, value, localUserState, cancelMemoryAccess);
            MemoryAccess?.Invoke(this, eventArgs);
            return eventArgs;
        }

        public void WriteToMemory(ushort address, byte value)
        {
            FailIfNoExecutionContext();
            FailIfNoInstructionFetchComplete();

            WriteToMemoryInternal(address, value);
        }

        private void WriteToMemoryInternal(ushort address, byte value)
        {
            WritetoMemory(
                address,
                value,
                Memory,
                GetMemoryAccessMode(address),
                MemoryAccessEventType.BeforeMemoryWrite,
                MemoryAccessEventType.AfterMemoryWrite,
                GetMemoryWaitStatesForNonM1(address));
        }

        private void WritetoMemory(
            ushort address,
            byte value,
            IMemory memory,
            MemoryAccessMode accessMode,
            MemoryAccessEventType beforeEventType,
            MemoryAccessEventType afterEventType,
            byte waitStates)
        {

            if (MemoryAccess == null)
            {
                memory[address] = value;
                return;
            }

            var beforeEventArgs = FireMemoryAccessEvent(beforeEventType, address, value);

            if(!beforeEventArgs.CancelMemoryAccess &&
                (accessMode == MemoryAccessMode.ReadAndWrite || accessMode == MemoryAccessMode.WriteOnly))
                memory[address] = beforeEventArgs.Value;

            if(executionContext != null)
                executionContext.AccummulatedMemoryWaitStates += waitStates;

            FireMemoryAccessEvent(
                afterEventType, 
                address, 
                beforeEventArgs.Value, 
                beforeEventArgs.LocalUserState,
                beforeEventArgs.CancelMemoryAccess);
        }

        private void WritetoPort(
            byte portLo,
            byte portHi,
            byte value,
            IIO io,
            MemoryAccessMode accessMode,
            MemoryAccessEventType beforeEventType,
            MemoryAccessEventType afterEventType,
            byte waitStates)
        {

            if (MemoryAccess == null)
            {
                io[portLo, portHi] = value;
                return;
            }

            var beforeEventArgs = FireMemoryAccessEvent(beforeEventType, portLo, value);

            if (!beforeEventArgs.CancelMemoryAccess &&
                (accessMode == MemoryAccessMode.ReadAndWrite || accessMode == MemoryAccessMode.WriteOnly))
                io[portLo, portHi] = beforeEventArgs.Value;

            if (executionContext != null)
                executionContext.AccummulatedMemoryWaitStates += waitStates;

            FireMemoryAccessEvent(
                afterEventType,
                portLo,
                beforeEventArgs.Value,
                beforeEventArgs.LocalUserState,
                beforeEventArgs.CancelMemoryAccess);
        }

        public byte ReadFromPort(byte portLo, byte portHi)
        {
            FailIfNoExecutionContext();
            FailIfNoInstructionFetchComplete();

            return ReadFromPort(
                portLo, 
                portHi,
                PortsSpace, 
                GetPortAccessMode(portLo),
                MemoryAccessEventType.BeforePortRead,
                MemoryAccessEventType.AfterPortRead,
                GetPortWaitStates(portLo));
        }

        public void WriteToPort(byte portLo, byte portHi, byte value)
        {
            FailIfNoExecutionContext();
            FailIfNoInstructionFetchComplete();

            WritetoPort(
                portLo,
                portHi,
                value,
                PortsSpace,
                GetPortAccessMode(portLo),
                MemoryAccessEventType.BeforePortWrite,
                MemoryAccessEventType.AfterPortWrite,
                GetPortWaitStates(portLo));
        }

        public void SetInterruptMode(byte interruptMode)
        {
            FailIfNoExecutionContext();
            FailIfNoInstructionFetchComplete();

            this.InterruptMode = interruptMode;
        }

        public void Stop(bool isPause = false)
        {
            FailIfNoExecutionContext();

            if (!executionContext.ExecutingBeforeInstructionEvent)
                FailIfNoInstructionFetchComplete();

            executionContext.StopReason =
                isPause ?
                StopReason.PauseInvoked :
                StopReason.StopInvoked;
        }

        IZ80Registers IZ80ProcessorAgent.Registers
        {
            get
            {
                return _Registers;
            }
        }

        #endregion

        #region Instruction execution context

        protected InstructionExecutionContext executionContext;

        #endregion
    }
}
