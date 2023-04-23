## Dependencies

The [Z80Processor class](../Main/Z80Processor.cs) delegates part of its work to other classes. These dependencies take the form of interfaces that are accessible via public properties; this way it is possible to create custom versions of the dependencies by just creating classes that implement the proper interfaces, then assigning instances of these to the proper properties before using the processor class.

This section describes these dependencies in detail. "The default implementation" is the type that will be used by a new instance of the Z80ProcessorClass unless a different object is plugged by setting the appropriate property. 

### IMemory

The [IMemory interface](../Main/Dependencies%20Interfaces/IMemory.cs) represents the memory or ports space visible by the processor. It just contains an indexer to read and write single bytes, and a couple of auxiliary methods for reading and writing larger blocks. 

The default implementation is [PlainMemory](../Main/Dependencies%20Implementations/PlainMemory.cs), a very simple class that just uses an array to store the information without further processing.

[IZ80Processor](../Main/IZ80Processor.cs) holds two instances of this interface, one for the memory (`Memory`) and another one for the ports space (`PortsSpace`).

_NOTE_: The Z80Processor class may not access the memory objects at all for certain memory positions, depending on how the memory access mode is configured. See the documentation about [configuration](Configuration.md) 

### IZ80Registers, IMainZ80Registers

The [IZ80Registers interface](../Main/Dependencies%20Interfaces/IZ80Registers.cs), which inherits from [IMainZ80Registers](../Main/Dependencies%20Interfaces/IMainZ80Registers.cs), represents the complete Z80 register set, including all the general purpose registers (AF, BC, DE, HL, IX, IY, alternate) and the special registers (PC, SP, I, R, IFF0, IFF1).

The default implementation is [Z80Registers](../Main/Dependencies%20Implementations/Z80Registers.cs), which simply stores the 16 bit registers in the exposed properties and uses bit and byte manipulation operations to manage the flags and the 8 bit registers (A, B, etc).

[IZ80Processor](../Main/IZ80Processor.cs) holds one instance of IZ80Registers in the `Registers` property.

### IZ80InstructionExecutor

The [IZ80InstructionExecutor interface](../Main/Dependencies%20Interfaces/IZ80InstructionExecutor.cs) represents the class that knows how to execute the actual Z80 instructions. It contains just one method, `Execute`, that receives the first byte of the opcode for the instruction to be executed and returns the count of T states consumed by the instruction execution. It also has a `ProcessorAgent` property that holds an instance of [IZ80ProcessorAgent](../Main/Dependencies%20Interfaces/IZ80ProcessorAgent.cs), an interface that allows the instruction executor to interact with the processor by fetching further opcode bytes and reading/writing memory and ports (proper setting of the `ProcessorAgent` property is managed the Z80Processor class itself).

The default implementation is [Z80InstructionsExecutor](../Main/Instructions%20Execution), a rather complex class that contains the necessary code to properly execute all the Z80 instructions.

[IZ80Processor](../Main/IZ80Processor.cs) holds one instance of IZ80InstructionExecutor in the `InstructionExecutor ` property.

### IClockSynchronizer

The [IClockSynchronizer](../Main/Dependencies%20Interfaces/IClockSynchronizer.cs) represents the class that takes care of real time synchronization when the processor is running. It contains a `TryWait` method that receives a number of elapsed T states, and an `EffectiveClockFrequencyInMHz` property that allows converting T states to time. Z80Processor executes `TryWait` after finishing the execution of each instruction, which ideally causes the thread that is running the simulation to pause for the exact time required (T states divided by clock frequency). However in most host systems it is difficult to actually pause a thread for such a small amount of time, so workarounds are required.

The default implementation is [ClockSynchronizer](../Main/Dependencies%20Implementations/ClockSynchronizer.cs), a class that indeed uses a workaround: its `TryWait` method just increases an internal counter of T states, and the actual thread pausing is performed only when this counter reaches a certain suitable value.

[IZ80Processor](../Main/IZ80Processor.cs) holds one instance of IZ80InstructionExecutor in the `ClockSynchronizer ` property. This property can be set to _null_, in this case no clock syncrhonization will be performed and the simulation will run at the maximum speed that the host system can provide. This is useful if you are not interested on simulating the exact speed of an existing system, but just run Z80 code as quickly as possible.

_NOTE:_ Real time syncrhonization is not performed when the processor is running in single instruction execution mode. See the documentation about [the execution control methods](HowExecutionWorks.md).