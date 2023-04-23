## How Execution Works

The [Z80Processor class](../Main/Z80Processor.cs) simulates the processor execution in a syncrhonous way. You start the execution with any of the available control methods and then wait until the method returns (see the [execution stop conditions](StopConditions.md)). There are two execution modes, depending on the control method you use to start execution.

### Instruction execution loop mode

This mode is initiated when the `Start` method or the `Continue` method of [IZ80Processor](../Main/IZ80Processor.cs) is invoked. In this mode the processor just starts executing instructions in the order in which they are stored in memory, and does not stop until any of the [execution stopping conditions](StopConditions.md) applies. Control of the process is achieved by handling [the memory access events](MemoryAccessFlow.md) and/or [the instruction execution events](InstructionExecutionFlow.md).

The difference between `Start` and `Continue` is that the former does a processor reset prior to starting execution, while the later does not. A reset simply sets the PC, IFF1 and IFF2 registers, as well as the interrupt mode, to zero; and sets the AF and PC registers to FFFFh. There is also a separate `Reset` method that does the same without otherwise modifying the processor state.

_HINT:_ To start execution at a memory address other than zero, you can invoke `Reset`, then manually set `Registers.PC` to the desired value, then invoke `Continue`.

### Single instruction execution mode

Another way to get the processor started is to invoke the `ExecuteNextInstruction` method of [IZ80Processor](../Main/IZ80Processor.cs). This method will just execute the next instruction (as pointed by the PC register) and return. This may be useful for step-by-step debuggers.

The T states counters are appropriately updated in single instruction execution mode as they are in instruction execution loop mode (see [the state properties](State.md)). However the stop conditions do of course not apply in this mode.