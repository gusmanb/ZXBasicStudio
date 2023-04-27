using Main.Dependencies_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a Z80 processor class that can be used to develop processor simulators or computer emulators.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Z80 processor class is intended to be used in synchronous mode and controlled by events.
    /// You simply configure the instance, subscribe to the <see cref="IZ80Processor.MemoryAccess"/>,
    /// <see cref="IZ80Processor.BeforeInstructionExecution"/> and <see cref="IZ80Processor.AfterInstructionExecution"/> events
    /// and invoke the <see cref="IZ80Processor.Start"/> method.
    /// The method returns when the processor stops execution for whatever reason (see <see cref="IZ80Processor.StopReason"/>).
    /// </para>
    /// <para>
    /// The <see cref="IZ80Processor.MemoryAccess"/>,
    /// <see cref="IZ80Processor.BeforeInstructionExecution"/> and <see cref="IZ80Processor.AfterInstructionExecution"/> events
    /// provide full control of the memory and ports access and the instructions executions. During these events
    /// you can examine and alter the memory contents and even stop the processor execution.
    /// Extra control on the memory and registers can be achieved by using custom implementations of
    /// <see cref="IMemory"/> and <see cref="IZ80Registers"/>. The instruction execution engine itself can be
    /// replaced as well by a custom implementation, see the <see cref="InstructionExecutor"/> property.
    /// </para>
    /// <para>
    /// An alternative way of using the class is to use the <see cref="IZ80Processor.ExecuteNextInstruction"/> method.
    /// This method will simply execute the next instruction (as pointed by the PC register, see <see cref="IZ80Processor.Registers"/>)
    /// and then returns immediately. This can be useful to allow for step-by-step debugging of Z80 code.
    /// </para>
    /// <para>
    /// The default configuration when the class is instantiated is:
    /// <list type="bullet">
    /// <item><description><see cref="IZ80Processor.ClockFrequencyInMHz"/> = 4</description></item>
    /// <item><description><see cref="IZ80Processor.ClockSpeedFactor"/> = 1</description></item>
    /// <item><description><see cref="IZ80Processor.AutoStopOnDiPlusHalt"/> = true</description></item>
    /// <item><description><see cref="IZ80Processor.AutoStopOnRetWithStackEmpty"/> = false</description></item>
    /// <item><description>Memory and ports wait states: all zeros</description></item>
    /// <item><description><see cref="IZ80Processor.Memory"/> = an instance of <see cref="PlainMemory"/></description></item>
    /// <item><description><see cref="IZ80Processor.PortsSpace"/> = an instance of <see cref="PlainMemory"/></description></item>
    /// <item><description><see cref="IZ80Processor.Registers"/> = an instance of <see cref="Z80Registers"/></description></item>
    /// <item><description>Memory and ports access modes = all <see cref="MemoryAccessMode.ReadAndWrite"/></description></item>
    /// <item><description><see cref="IZ80Processor.InstructionExecutor"/> = an instance of <see cref="Z80InstructionExecutor"/></description></item>
    /// <item><description><see cref="ClockSynchronizer"/> = an instance of <see cref="ClockSynchronizer"/></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public interface IZ80Processor
    {
        #region Processor control

        /// <summary>
        /// Performs a reset (see <see cref="IZ80Processor.Reset"/>) and sets the processor in running state.
        /// This method cannot be invoked from an event handler.
        /// </summary>
        /// <remarks>
        /// The method will finish when the <see cref="IExecutionStopper.Stop"/> method is invoked from within an
        /// event handler, when a HALT instruction is executed with the interrupts disabled (only if
        /// <see cref="IZ80Processor.AutoStopOnDiPlusHalt"/> is <b>true</b>), or when a RET instruction is executed
        /// and the stack is empty (only if <see cref="IZ80Processor.AutoStopOnRetWithStackEmpty"/> is <b>true</b>).
        /// </remarks>
        /// <param name="userState">If this value is not null, it will be copied to the
        /// <see cref="IZ80Processor.UserState"/> property.
        /// </param>
        //void Start(object userState = null);

        /// <summary>
        /// Sets the processor in running state without first doing a reset, thus preserving the state of all the registers.
        /// This method cannot be invoked from an event handler.
        /// </summary>
        /// <exception cref="InvalidOperationException">The method is invoked from within an event handler.</exception>
        //void Continue();

        /// <summary>
        /// Resets all registers to its initial state. The running state of the processor is not modified.
        /// </summary>
        /// <para>
        /// This method sets the PC, IFF1, and IFF2 registers to 0, AF and SP to FFFFh,
        /// and the interrupt mode to 0.
        /// </para>
        /// <para>
        /// If the method is executed from a <see cref="IZ80Processor.MemoryAccess"/> event, the reset
        /// will be effective after the current instruction execution finishes.
        /// </para>
        void Reset();

        /// <summary>
        /// Executes the next instruction as pointed by the PC register, and then returns.
        /// This method cannot be invoked from an event handler.
        /// </summary>
        /// <remarks>
        /// <para>
        /// During the execution of this method, the <see cref="IZ80Processor.MemoryAccess"/>,
        /// <see cref="IZ80Processor.BeforeInstructionExecution"/> and <see cref="IZ80Processor.AfterInstructionExecution"/> 
        /// events will be triggered as usual.
        /// Altough not necessary, it is possible to invoke the <see cref="IExecutionStopper.Stop"/> method
        /// during the <see cref="AfterInstructionExecutionEventArgs"/> event,
        /// thus modifying the value of <see cref="IZ80Processor.StopReason"/>.
        /// </para>
        /// <para>
        /// This method will never issue a reset. A manual call to <see cref="IZ80Processor.Reset"/> is needed
        /// before the first invocation of this method if <see cref="IZ80Processor.Start"/> has never been invoked,
        /// unless register state is set manually.
        /// </para>
        /// </remarks>
        /// <returns>The total amount of T states required to execute the instruction,
        /// inclusing extra wait states.</returns>
        /// <exception cref="InvalidOperationException">The method is invoked from within an event handler.</exception>
        int ExecuteNextInstruction();

        #endregion

        #region Information and state

        /// <summary>
        /// Obtains the count of T states elapsed since the processor execution started.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is set to zero when the processor object is created, and when the
        /// <see cref="IZ80Processor.Start"/> method is invoked. It is not affected by the
        /// <see cref="IZ80Processor.Continue"/> and <see cref="IZ80Processor.Reset"/> methods.
        /// </para>
        /// <para>The value is updated after each relevant operation (memory access or instruction execution),
        /// both in running mode and in single instruction execution mode.</para>
        /// </remarks>
        ulong TStatesElapsedSinceStart { get; }

        /// <summary>
        /// Obtains the count of T states elapsed since the last reset.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is set to zero when the processor object is created, and when the
        /// <see cref="IZ80Processor.Start"/> method or the <see cref="IZ80Processor.Reset"/> are invoked.
        /// It is not affected by the <see cref="IZ80Processor.Continue"/> method.
        /// </para>
        /// <para>The value is updated after each relevant operation (memory access or instruction execution),
        /// both in running mode and in single instruction execution mode.</para>
        /// </remarks>
        ulong TStatesElapsedSinceReset { get; }

        /// <summary>
        /// Obtains the reason for the processor not being in the running state,
        /// that is, what triggered the last stop.
        /// </summary>
        //StopReason StopReason { get; }

        /// <summary>
        /// Obtains the current processor execution state.
        /// </summary>
        //ProcessorState State { get; }

        /// <summary>
        /// Contains an user-defined state object. This property exists for the client code convenience
        /// and can be set to any value, the class code will never access it.
        /// </summary>
        object UserState { get; set; }

        /// <summary>
        /// Returns true when a HALT instruction is executed, returns to false when an interrupt request arrives.
        /// </summary>
        bool IsHalted { get; }

        /// <summary>
        /// The current interrupt mode. It has always the value 0, 1 or 2.
        /// </summary>
        /// <exception cref="System.ArgumentException">Attempt to set a value other than 0, 1 or 2</exception>
        byte InterruptMode { get; set; }

        /// <summary>
        /// The current address where the stack starts. If <see cref="AutoStopOnDiPlusHalt"/> is <b>true</b>,
        /// execution will stop automatically when a <c>RET</c> instruction is executed with the SP register
        /// having this value.
        /// </summary>
        /// <remarks>
        /// This property is set to 0xFFFF when the class is first instantiated and when the <see cref="Start"/>
        /// and <see cref="Reset"/> methods are executed. Also, when a <c>LD SP,x</c> instruction is executed,
        /// this property is set to the new value of SP.
        /// </remarks>
        short StartOfStack { get; }

        #endregion

        #region Inside and outside world

        /// <summary>
        /// Gets or sets the register set used by the processor.
        /// </summary>
        IZ80Registers Registers { get; set; }

        /// <summary>
        /// Gets or sets the visible memory for the processor.
        /// </summary>
        IMemory Memory { get; set; }

        /// <summary>
        /// Sets the mode of a portion of the visible memory.
        /// </summary>
        /// <param name="startAddress">First memory address that will be set</param>
        /// <param name="length">Length of the memory portion that will be set</param>
        /// <param name="mode">New memory mode</param>
        /// <exception cref="System.ArgumentException"><c>startAddress</c> is less than 0, 
        /// or <c>startAddress</c> + <c>length</c> goes beyond 65535.</exception>
        void SetMemoryAccessMode(ushort startAddress, int length, MemoryAccessMode mode);

        /// <summary>
        /// Gets the access mode of a memory address.
        /// </summary>
        /// <param name="address">The address to check</param>
        /// <returns>The current memory access mode for the address</returns>
        /// <exception cref="System.ArgumentException"><c>address</c> is greater than 65536</exception>
        MemoryAccessMode GetMemoryAccessMode(ushort address);

        /// <summary>
        /// Gets or sets the visible ports space for the processor.
        /// </summary>
        IIO PortsSpace { get; set; }

        /// <summary>
        /// Sets the access mode of a portion of the visible ports space.
        /// </summary>
        /// <param name="startPort">First port that will be set</param>
        /// <param name="length">Length of the mports space that will be set</param>
        /// <param name="mode">New memory mode</param>
        /// <exception cref="System.ArgumentException"><c>startAddress</c> is less than 0, 
        /// or <c>startAddress</c> + <c>length</c> goes beyond 255.</exception>
        void SetPortsSpaceAccessMode(byte startPort, int length, MemoryAccessMode mode);

        /// <summary>
        /// Gets the access mode of a port.
        /// </summary>
        /// <param name="portNumber">The port number to check</param>
        /// <returns>The current memory access mode for the port</returns>
        /// <exception cref="System.ArgumentException"><c>portNumber</c> is greater than 255.</exception>
        MemoryAccessMode GetPortAccessMode(ushort portNumber);

        /// <summary>
        /// Registers a new interrupt source.
        /// </summary>
        /// <param name="source">Interrupt source to register</param>
        /// <remarks>
        /// After each instruction execution the processor will check if there is a non-maskable interrupt
        /// or a maskable interrupt (in that order) pending  from any of the registered sources,
        /// and process it as appropriate.
        /// </remarks>
        void RegisterInterruptSource(IZ80InterruptSource source);

        /// <summary>
        /// Retrieves a read-only collection of all the registered interrupt sources.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IZ80InterruptSource> GetRegisteredInterruptSources();

        /// <summary>
        /// Unregisters all the interrupt sources.
        /// </summary>
        /// <remarks>
        /// After this method is executed there is no way for the processor to receive an interrupt request,
        /// unless the <see cref="IZ80Processor.RegisterInterruptSource"/> method is invoked again.
        /// </remarks>
        void UnregisterAllInterruptSources();

        public void RegisterTStatesTarget(ITStatesTarget target);
        public void UnregisterAllTStatesTargets();
        IEnumerable<ITStatesTarget> GetRegisteredTStatesTarget();

        #endregion

        #region Configuration

        ///// <summary>
        ///// Gets or sets the clock frequency in MegaHertzs. 
        ///// This value cannot be changed while the processor is running or in single instruction execution mode.
        ///// </summary>
        ///// <exception cref="System.ArgumentException">The product of <see cref="IZ80Processor.ClockSpeedFactor"/>
        ///// by the new value gives a number that is smaller than 0.001 or greater than 100.</exception>
        ///// <exception cref="System.InvalidOperationException">The procesor is running or in single instruction execution mode.</exception>
        //decimal ClockFrequencyInMHz { get; set; }

        ///// <summary>
        ///// Gets or sets a value that is multiplied by the clock frequency to obtain the effective
        ///// clock frequency simulated by the processor.
        ///// </summary>
        ///// <exception cref="System.ArgumentException">The product of <see cref="IZ80Processor.ClockFrequencyInMHz"/>
        ///// by the new value gives a number that is smaller than 0.001 or greater than 100.</exception>
        //decimal ClockSpeedFactor { get; set; }

        ///// <summary>
        ///// Gets or sets a value that indicates whether the processor should stop automatically or not when a HALT
        ///// instruction is executed with interrupts disabled.
        ///// </summary>
        //bool AutoStopOnDiPlusHalt { get; set; }

        ///// <summary>
        ///// Gets or sets a value that indicates whether the processor should stop automatically when a RET
        ///// instruction is executed and the stack is empty.
        ///// </summary>
        ///// <remarks>
        ///// <para>"The stack is empty" means that the SP register has the value
        ///// it had when the <see cref="IZ80Processor.Start"/> method
        ///// was executed, or the value set by the last execution of a <c>LD SP,xx</c> instruction.
        ///// </para>
        ///// <para>
        ///// This setting is useful for testing simple programs, so that the processor stops automatically
        ///// as soon as the program finishes with a RET.
        ///// </para>
        ///// </remarks>
        //bool AutoStopOnRetWithStackEmpty { get; set; }

        /// <summary>
        /// Sets the wait states that will be simulated when accessing the visible memory
        /// during the M1 cycle for a given address range.
        /// </summary>
        /// <param name="startAddress">First memory address that will be configured</param>
        /// <param name="length">Length of the memory portion that will be configured</param>
        /// <param name="waitStates">New wait states</param>
        /// <exception cref="System.InvalidOperationException"><c>startAddress</c> + <c>length</c> goes beyond 65535.</exception>
        void SetMemoryWaitStatesForM1(ushort startAddress, int length, byte waitStates);

        /// <summary>
        /// Obtains the wait states that will be simulated when accessing the visible memory
        /// during the M1 cycle for a given address. 
        /// </summary>
        /// <param name="address">Address to het the wait states for</param>
        /// <returns>Current wait states during M1 for the specified address</returns>
        byte GetMemoryWaitStatesForM1(ushort address);

        /// <summary>
        /// Sets the wait states that will be simulated when accessing the visible memory
        /// outside the M1 cycle for a given address range.
        /// </summary>
        /// <param name="startAddress">First memory address that will be configured</param>
        /// <param name="length">Length of the memory portion that will be configured</param>
        /// <param name="waitStates">New wait states</param>
        /// <exception cref="System.InvalidOperationException"><c>startAddress</c> + <c>length</c> goes beyond 65535.</exception>
        void SetMemoryWaitStatesForNonM1(ushort startAddress, int length, byte waitStates);

        /// <summary>
        /// Obtains the wait states that will be simulated when accessing the visible memory
        /// outside the M1 cycle for a given address. 
        /// </summary>
        /// <param name="address">Address to het the wait states for</param>
        /// <returns>Current wait states outside M1 for the specified address</returns>
        byte GetMemoryWaitStatesForNonM1(ushort address);

        /// <summary>
        /// Sets the wait states that will be simulated when accessing the I/O ports.
        /// </summary>
        /// <param name="startPort">First port that will be configured</param>
        /// <param name="length">Length of the port range that will be configured</param>
        /// <param name="waitStates">New wait states</param>
        /// <exception cref="System.InvalidOperationException"><c>startAddress</c> + <c>length</c> goes beyond 255.</exception>
        void SetPortWaitStates(ushort startPort, int length, byte waitStates);

        /// <summary>
        /// Obtains the wait states that will be simulated when accessing the I/O ports
        /// for a given port. 
        /// </summary>
        /// <param name="portNumber">Port number to het the wait states for</param>
        /// <returns>Current wait states for the specified port</returns>
        byte GetPortWaitStates(ushort portNumber);

        /// <summary>
        /// Gets or set the instruction executor.
        /// </summary>
        IZ80InstructionExecutor InstructionExecutor { get; set; }

        /// <summary>
        /// Gets or sets the period waiter used to achieve proper timing on the execution flow.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// This property can be set to _null_, in this case no clock syncrhonization will be performed
        /// and the simulation will run at the maximum speed that the host system can provide.
        //IClockSynchronizer ClockSynchronizer { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Memory access event. Is is triggered before and after each memory and port read and write.
        /// </summary>
        event EventHandler<MemoryAccessEventArgs> MemoryAccess;

        /// <summary>
        /// Pre-instruction fetch event. It is triggered before the next instruction is fetched.
        /// </summary>
        event EventHandler<BeforeInstructionFetchEventArgs> BeforeInstructionFetch;

        /// <summary>
        /// Pre-instruction execution event. It is triggered before an instruction is executed.
        /// </summary>
        event EventHandler<BeforeInstructionExecutionEventArgs> BeforeInstructionExecution;

        /// <summary>
        /// Post-instruction execution event. It is triggered after an instruction is executed.
        /// </summary>
        event EventHandler<AfterInstructionExecutionEventArgs> AfterInstructionExecution;

        #endregion

        #region Utils

        /// <summary>
        /// Simulate the execution of a CALL instruction by pushing the current content of the PC register
        /// into the stack and setting it to the specified value.
        /// </summary>
        /// <param name="address">New value for the PC register</param>
        void ExecuteCall(ushort address);

        /// <summary>
        /// Simulate the execution of a RET instruction by setting the value of the PC register
        /// from the value popped from the stack.
        /// </summary>
        void ExecuteRet();

        #endregion
    }
}
