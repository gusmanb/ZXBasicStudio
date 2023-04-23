using NUnit.Framework;
using Ploeh.AutoFixture;
using System;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public class Z80InstructionsExecutor_core_test : InstructionsExecutionTestsBase
    {
        private const byte NOP_opcode = 0x00;
        private const byte LD_BC_nn_opcode = 0x01;
        private const byte ADD_HL_BC_opcode = 0x09;
        private const byte IN_B_C_opcode = 0x40;
        private const byte RLC_B_opcode = 0x00;

		[Test]
		public void InstructionsExecution_fire_FetchFinished_event_and_return_proper_T_states_count()
		{
		    var fetchFinishedEventsCount = 0;

		    Sut.InstructionFetchFinished += (sender, e) => fetchFinishedEventsCount++;

            SetMemoryContents(0);
			Assert.AreEqual(4, Execute(NOP_opcode));
		    SetMemoryContents(0, Fixture.Create<byte>(), Fixture.Create<byte>());
            Assert.AreEqual(10, Execute(LD_BC_nn_opcode));

            SetMemoryContents(0);
			Assert.AreEqual(8, Execute(0xCB, nextFetches: 0));

			Assert.AreEqual(15, Execute(ADD_HL_BC_opcode, 0xDD));
			Assert.AreEqual(15, Execute(ADD_HL_BC_opcode, 0xFD));

			Assert.AreEqual(12, Execute(IN_B_C_opcode, 0xED));

            Assert.AreEqual(23, Execute(0xCB, 0xDD, 0));
            Assert.AreEqual(23, Execute(0xCB, 0xFD, 0));
            
			Assert.AreEqual(8, fetchFinishedEventsCount);
        }

        [Test]
        public void Unsupported_instructions_just_return_8_TStates_elapsed()
        {
			var fetchFinishedEventsCount = 0;

			Sut.InstructionFetchFinished += (sender, e) => fetchFinishedEventsCount++;

			Assert.AreEqual(8, Execute(0x3F, 0xED));
			Assert.AreEqual(8, Execute(0xC0, 0xED));
			Assert.AreEqual(8, Execute(0x80, 0xED));
			Assert.AreEqual(8, Execute(0x9F, 0xED));

			Assert.AreEqual(4, fetchFinishedEventsCount);
        }

		[Test]
        public void Unsupported_instructions_invoke_overridable_method_ExecuteUnsopported_ED_Instruction()
		{
		    Sut = NewFakeInstructionExecutor();

		    Execute(0x3F, 0xED);
		    Execute(0xC0, 0xED);

			Assert.AreEqual(new Byte[] {0x3F, 0xC0}, ((FakeInstructionExecutor)Sut).UnsupportedExecuted);
        }

        [Test]
		public void Execute_increases_R_appropriately()
        {
			Registers.R = 0xFE;

			Execute(NOP_opcode);
			Assert.AreEqual(0xFF, Registers.R);

            Execute(LD_BC_nn_opcode, null, Fixture.Create<byte>(), Fixture.Create<byte>());
			Assert.AreEqual(0x80, Registers.R);

			Execute(RLC_B_opcode, 0xCB);
			Assert.AreEqual(0x82, Registers.R);

            Execute(ADD_HL_BC_opcode, 0xDD);
   			Assert.AreEqual(0x84, Registers.R);

            Execute(ADD_HL_BC_opcode, 0xFD);
			Assert.AreEqual(0x86, Registers.R);

			Execute(IN_B_C_opcode, 0xED);
			Assert.AreEqual(0x88, Registers.R);

            Execute(0xCB, 0xDD, 0);
            Assert.AreEqual(0x8A, Registers.R);

            Execute(0xCB, 0xFD, 0);
            Assert.AreEqual(0x8C, Registers.R);
        }

        [Test]
        public void DD_FD_not_followed_by_valid_opcode_are_trated_as_nops()
        {
            var fetchFinishedEventsCount = 0;

		    Sut.InstructionFetchFinished += (sender, e) => fetchFinishedEventsCount++;

            Assert.AreEqual(4, Execute(0xFD, 0xDD));
            Assert.AreEqual(4, Execute(0x01, 0xFD));
            Assert.AreEqual(10, Execute(0x01, null, Fixture.Create<byte>(), Fixture.Create<byte>()));

            Assert.AreEqual(3, fetchFinishedEventsCount);
        }
    }
}
