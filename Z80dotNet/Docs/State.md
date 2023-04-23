## Checking state

[IZ80Processor](../Main/IZ80Processor.cs) contains a handful of properties that allow to check the current state of the simulated processor. These properties can be checked when the processor is stopped, or when handling [the instruction execution events](InstructionExecutionFlow.md) and [the memory access events](MemoryAccessFlow.md) while the processor is running and executing instructions. The properties are:

* **`TStatesElapsedSinceStart`**: The number of T states elapsed since the execution started with the `Start` method. This is the sum of the states consumed by the instructions themselves and the extra wait states introduced by memory and ports access (the amount of wait states required by memory and ports must be configured before starting execution, see the [documentation about configuration](Configuration.md)).

* **`TStatesElapsedSinceReset`**: Same as `TStatesElapsedSinceStart`, but this one counts the T states elapsed since the processor was reset, either with the `Start` method or with the `Reset` method.

* **`StopReason`**: A member of the [StopReason enumeration](../Main/Enums/StopReason.cs) that indicates why the processor exited the execution loop. The value of this property changes every time that execution starts or stops.

* **`State`**: A member of the [ProcessorState enumeration](../Main/Enums/ProcessorState.cs) that indicates whether the processor is stopped, paused or running.

* **`IsHalted`**: A boolean value that becomes _true_ when a `HALT` instruction is executed and returns to _false_ when a maskable interrupt is processed.

* **`InterruptMode`**: The current interrupt mode as set by an `IM n` instruction.

* **`StartOfStack`**: The current start of stack address as set by a reset or by a `LD SP` instruction. This value is relevant for the auto stop on empty stack functionality, see [execution stop conditions](StopConditions.md).