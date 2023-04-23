## Execution Stop Conditions

Once [the processor execution has started](HowExecutionWorks.md), it will stop (and the invoked execution starter method will return) only when one of the execution stop conditions is met. This section explains what are the possible stop conditions.

Note that no matter what, execution always stops after an instruction has been executed completely, including the update of registers and T states counters. An instruction will never be aborted in mid-execution.

### Invoking the Stop method

There is a [IExecutionStopper interface](../Main/Dependencies%20Interfaces/IExecutionStopper.cs) defined with a single method, `Stop`, that allows to manually request the processor execution to be stopped after the current instruction has finished executing. There are two points where this method is accessible:

* The `Execute` method of [IZ80InstructionsExecutor](../Main/Dependencies%20Interfaces/IZ80InstructionExecutor.cs) can invoke `Stop` through its `ProcessorAgent` property ([IZ80ProcessorAgent](../Main/Dependencies%20Interfaces/IZ80ProcessorAgent.cs) derives from [IExecutionStopper](../Main/Dependencies%20Interfaces/IExecutionStopper.cs)). Note however that the default implementation of  [IZ80InstructionsExecutor](../Main/Dependencies%20Interfaces/IZ80InstructionExecutor.cs) does never call `Stop`, however you can [implement your own version](Dependencies.md) that stops the execution under whatever circumstances you deem appropriate.

* When handling [the before instruction fetch event](InstructionExecutionFlow.md) you can invoke `Stop` through the `ExecutionStopper` property of [BeforeInstructionFetchEventArgs](../Main/EventArgs/BeforeInstructionFetchEventArgs.cs). 

* When handling [the after instruction execution event](InstructionExecutionFlow.md) you can invoke `Stop` through the `ExecutionStopper` property of [AfterInstructionExecutionEventArgs](../Main/EventArgs/AfterInstructionExecutionEventArgs.cs). 


### Auto stop on RET with stack empty

If the `AutoStopOnRetWithStackEmpty` property is [configured](Configuration.md) with a value of _true_, execution will stop after a return instruction (`RET`, `RETI` or `RETN`) is executed if the stack was empty prior to its execution.

"The stack is empty" means that the value of the SP register is equal to the value returned by the `StartOfStack` property of [IZ80Processor](../Main/IZ80Processor.cs). The value of this property is set according to these rules:

* It is FFFFh when the processor class is instantiated.

* It is set to FFFFh by the `Start` and `Reset` methods.

* After a stack load instruction is executed (`LD SP,...`), it is set to the current value of the SP register.
 
This feature allows to use the processor class to exercise small pieces of Z80 code. Simply load the processor memory with your program, ending with a `RET` instruction; invoke `Start`, and check the state of memory and registers after it returns.

### Auto stop on HALT with interrupts disabled

If the `AutoStopOnDiPlusHalt` property is [configured](Configuration.md) with a value of _true_, execution will stop after a `HALT` instruction is executed if interrupts were disabled prior to its execution.

This property is _true_ by default and it is usually not a good idea to set it to _false_. Note that if this property is _false_ on a system that has no non-maskable [interrupt sources](Interrupts.md) registered and a `HALT` instruction is executed with the interrupts disabled, the only way to stop execution is to manually invoke the `Stop` method as explained above.

### Unhandled exception

Last but not least, execution will stop (and will propagate to the code that initiated it) if an exception is thrown and does not get handled. [IZ80Processor](../Main/IZ80Processor.cs) does not handle any exception thrown by [IZ80InstructionsExecutor](../Main/Dependencies%20Interfaces/IZ80InstructionExecutor.cs) or by the event handling code.


Note that an [InstructionFetchFinishedEventNotFiredException](../Main/Custom%20Exceptions/InstructionFetchFinishedEventNotFiredException.cs) will be thrown if the `Execute` method of [IZ80InstructionsExecutor](../Main/Dependencies%20Interfaces/IZ80InstructionExecutor.cs) returns without having fired the `InstructionFetchFinished` event.