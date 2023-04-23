## Memory access flow ##

The following is the chain of operations performed by [IZ80Processor](../Main/IZ80Processor.cs) whenever memory is read, either because opcodes are being fetched or as part of the processing of an instruction.

1. The `MemoryAccess` event is triggered with `MemoryAccessEventArgs.EventType` equal to `BeforeMemoryRead` and `Value` equal to FFh. _The code handling the event has the opportunity to set `MemoryAccessEventArgs.CancelMemoryAccess` to true and/or change the value_.
2. **If** the configured memory access mode for the affected address is `ReadAndWrite` or `ReadOnly`, **and** the `CancelMemoryAccess` property of the event has **not** been set to _true_, the value is read by accessing `Memory[address]`. Otherwise, the value set on the previous step is used as if it was actually obtained from memory.
3. The `MemoryAccess` event is triggered with `MemoryAccessEventArgs.EventType` equal to `AfterMemoryRead`. _The code handling the event has the opportunity to change the value_.
4. The value in `MemoryAccessEventArgs.Value` is assumed to be the obtained value and is processed appropriately.

As for the operations performed for a memory write, they are the following:

1. The `MemoryAccess` event is triggered with `MemoryAccessEventArgs.EventType` equal to `BeforeMemoryWrite`. _The code listening the event has the opportunity to either change the value to be written or set `MemoryAccessEventArgs.CancelMemoryAccess` to true_.
2. **If** the configured memory access mode for the affected address is `ReadAndWrite` or `WriteOnly`, **and** the `CancelMemoryAccess` property of the event has **not** been set to _true_, the value set on the previous step is written to `Memory[address]`.
3. The `MemoryAccess` event is triggered with `MemoryAccessEventArgs.EventType` equal to `AfterMemoryWrite`.

The code of the processor class takes care of the extra wait states needed for timing purposes.

The flow for accessing I/O ports is similar, but `PortsSpace` is used instead of `Memory` and the 'Port' members of `MemoryAccessEventArgs.EventType` are used instead of the 'Memory' members.

Note that [MemoryAccessEventArgs](../Main/EventArgs/MemoryAccessEventArgs.cs) inherits from [ProcessorEventArgs](../Main/EventArgs/ProcessorEventArgs.cs), which defines the `LocalUserState` property. This property is propagated from the 'before' event to the 'after' event and can be used by the events handling code at its convenience.
