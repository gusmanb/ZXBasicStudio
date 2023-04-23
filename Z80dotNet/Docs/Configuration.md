## Configuration

The [Z80Processor class](../Main/Z80Processor.cs) can be configured by using a set of methods and properties that are in place for this purpose. The class constructor sets the configuration to  values that are useful in most cases, so the processor object is ready to be used as soon as it is instantiated; however in some cases it may be necessary to apply custom configuration. This section describes the available configuration members.

_NOTE:_ Remember that plugging [custom dependencies](Dependencies.md) is an alternative (or complementary) way to customize the behavior of the processor class.

* **`AutoStopOnDiPlusHalt`**:  When this value is true, the processor execution will stop automatically if a HALT instruction is executed when interrupts are disabled. See the [execution stop conditions](StopConditions.md) documentation for details. The default value is _true_.

* **`AutoStopOnRetWithStackEmpty `**: When this value is true, the processor execution will stop automatically if a RET instruction is executed when the stack is empty. See the [execution stop conditions](StopConditions.md) documentation for details. The default value is _false_.

* **`ClockFrequencyInMhz`** and **`ClockSpeedFactor`**: These two values, when multiplied, hold the clock speed at which the simulated processor runs; this information is only relevant for [the clock synchronizer](../Main/Dependencies%20Interfaces/IClockSynchronizer.cs). There are two properties to help in the development of computer emulators, so it is possible to fix the clock frequency to the value for the real system but allow the user to tune the speed factor as appropriate. The default value is 4 (MHz) for the clock frequency and 1 for the speed factor.

* **`SetMemoryAccessMode`** and **`GetMemoryAccessMode`** methods allow to set and get the memory access mode for a certain range of memory addresses (see the [memory access workflow](MemoryAccessFlow.md) for details). The default value is `MemoryAccessMode.ReadAndWrite` for the entire memory.

* **`SetMemoryWaitStatesForM1`** and **`GetMemoryWaitStatesForM1`** methods allow to configure the amount of extra T states that will be counted for timing purposes when the memory is read during the M1 state (when fetching the instruction opcode bytes). The default value is zero for the entire memory.

* **`SetMemoryWaitStatesForNonM1`** and **`GetMemoryWaitStatesForNonM1`** methods allow to configure the amount of extra T states that will be counted for timing purposes when the memory is read or written during the execution of an instruction but outside the M1 state. The default value is zero for the entire memory.

* **`SetPortsSpaceAccessMode`** and **`GetPortsSpaceAccessMode`** methods allow to set and get the ports space access mode for a certain range of port numbers (see the [memory access workflow](MemoryAccessFlow.md) for details). The default value is `MemoryAccessMode.ReadAndWrite` for the entire ports space.

* **`SetPortWaitStates`** and **`GetPortWaitStates`** methods allow to configure the amount of extra T states that will be counted for timing purposes when a port is read or written during the execution of an instruction (not including the extra T state that is always added by the Z80 itself). The default value is zero for the entire ports space.

* **`UserState`**: This is a convenience property that can be used by the client code to store just anything. The code of the processor class will never access this property for anything. The default value is _null_.
