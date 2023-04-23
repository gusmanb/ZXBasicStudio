namespace Konamiman.Z80dotNet.Tests
{
    public class Z80ProcessorForTests : Z80Processor
    {
        public void SetInstructionExecutionContextToNonNull()
        {
            executionContext = new InstructionExecutionContext();
        }

        public void SetInstructionExecutionContextToNull()
        {
            executionContext = null;
        }

        public bool MustFailIfNoInstructionFetchComplete { get; set; }

        protected override void FailIfNoInstructionFetchComplete()
        {
            if(MustFailIfNoInstructionFetchComplete)
                base.FailIfNoInstructionFetchComplete();
        }

        public void SetStartOFStack(short value)
        {
            StartOfStack = value;
        }

        public void SetIsHalted()
        {
            IsHalted = true;
        }
    }
}
