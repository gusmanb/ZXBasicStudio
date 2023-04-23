using System.CodeDom.Compiler;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Konamiman.Z80dotNet.Tests.InstructionsExecution
{
    public abstract class InstructionsExecutionTestsBase
    {
        protected Z80InstructionExecutor Sut { get; set; }
        protected FakeProcessorAgent ProcessorAgent { get; set; }
        protected IZ80Registers Registers { get; set; }
        protected Fixture Fixture { get; set; }

        [SetUp]
        public void Setup()
        {
            Sut = new Z80InstructionExecutor();
            Sut.ProcessorAgent = ProcessorAgent = new FakeProcessorAgent();
            Registers = ProcessorAgent.Registers;
            Sut.InstructionFetchFinished += (s, e) => { };

            Fixture = new Fixture();
        }

        #region Auxiliary methods

        protected int nextFetchesAddress;

        protected void SetPortValue(byte portNumber, byte value)
        {
            ProcessorAgent.Ports[portNumber] = value;
        }

        protected byte GetPortValue(byte portNumber)
        {
            return ProcessorAgent.Ports[portNumber];
        }

        protected void SetMemoryContents(params byte[] opcodes)
        {
            SetMemoryContentsAt(0, opcodes);
        }
        
        protected void SetMemoryContentsAt(ushort address, params byte[] opcodes)
        {
            Array.Copy(opcodes, 0, ProcessorAgent.Memory, address, opcodes.Length);
            nextFetchesAddress = address + opcodes.Length;
        }

        protected void ContinueSettingMemoryContents(params byte[] opcodes)
        {
			Array.Copy(opcodes, 0, ProcessorAgent.Memory, nextFetchesAddress, opcodes.Length);
            nextFetchesAddress += opcodes.Length;
        }

		protected FakeInstructionExecutor NewFakeInstructionExecutor()
        {
			var sut = new FakeInstructionExecutor();
            sut.ProcessorAgent = ProcessorAgent = new FakeProcessorAgent();
		    Registers = ProcessorAgent.Registers;
			sut.InstructionFetchFinished += (s, e) => { };
            return sut;
        }

        protected T GetReg<T>(string name)
        {
            return (T)RegProperty(name).GetValue(Registers, null); 
        }

        protected Bit GetFlag(string name)
        {
            if(name.Length == 1)
                name += "F";

            return (Bit)RegProperty(name).GetValue(Registers, null); 
        }

        protected void SetReg(string regName, byte value)
        {
            RegProperty(regName).SetValue(Registers, value, null);
        }

        protected void SetReg(string regName, short value)
        {
            RegProperty(regName).SetValue(Registers, value, null);
        }

        protected void SetFlag(string flagName, Bit value)
        {
            if(flagName.Length == 1)
                flagName += "F";

            RegProperty(flagName).SetValue(Registers, value, null);
        }

        protected PropertyInfo RegProperty(string name)
        {
            return typeof(Z80Registers).GetProperty(name);
        }

        protected virtual int Execute(byte opcode, byte? prefix = null, params byte[] nextFetches)
        {
            return ExecuteAt(0, opcode, prefix, nextFetches);
        }

        protected int ExecuteAt(ushort address, byte opcode, byte? prefix = null, params byte[] nextFetches)
        {
            Registers.PC = address.Inc();   //Inc needed to simulate the first fetch made by the enclosing IZ80Processor
            if (prefix == null)
            {
                SetMemoryContentsAt(address.Inc(), nextFetches);
                return Sut.Execute(opcode);
            }
            else
            {
                SetMemoryContentsAt(address.Inc(), opcode);
                ContinueSettingMemoryContents(nextFetches);

                return Sut.Execute(prefix.Value);
            }

        }

        protected object IfIndexRegister(string regName, object value, object @else)
        {
            return regName.StartsWith("IX") || regName.StartsWith("IY") ? value : @else;
        }

        protected void AssertNoFlagsAreModified(byte opcode, byte? prefix = null)
        {
            var value = Fixture.Create<byte>();
            Registers.F = value;
            Execute(opcode, prefix);

            Assert.AreEqual(value, Registers.F);
        }

        protected void AssertSetsFlags(byte opcode, byte? prefix = null, params string[] flagNames)
        {
            AssertSetsFlags(null, opcode, prefix, flagNames);
        }

        protected void AssertSetsFlags(Action executor, byte opcode, byte? prefix = null, params string[] flagNames)
        {
            AssertSetsOrResetsFlags(opcode, 1, prefix, executor, flagNames);
        }

        protected void AssertResetsFlags(byte opcode, byte? prefix = null, params string[] flagNames)
        {
            AssertResetsFlags(null, opcode, prefix, flagNames);
        }

        protected void AssertResetsFlags(Action executor, byte opcode, byte? prefix = null, params string[] flagNames)
        {
            AssertSetsOrResetsFlags(opcode, 0, prefix, executor, flagNames);
        }

        protected void AssertSetsOrResetsFlags(byte opcode, Bit expected, byte? prefix = null, Action executor = null, params string[] flagNames)
        {
            if(executor == null)
                executor = () => Execute(opcode, prefix);

            var randomValues = Fixture.Create<byte[]>();

            foreach (var value in randomValues)
            {
                foreach (var flag in flagNames)
                    SetFlag(flag, !expected);

                Registers.A = value;

                executor();

                foreach (var flag in flagNames)
                    Assert.AreEqual(expected, GetFlag(flag));
            }
        }

        protected void AssertDoesNotChangeFlags(byte opcode, byte? prefix = null, params string[] flagNames)
        {
            AssertDoesNotChangeFlags(null, opcode, prefix, flagNames);
        }

        protected void AssertDoesNotChangeFlags(Action executor, byte opcode, byte? prefix = null, params string[] flagNames)
        {
            if(executor == null)
                executor = () => Execute(opcode, prefix);

            if(flagNames.Length == 0)
                flagNames = new[] {"C", "H", "S", "Z", "P", "N", "Flag3", "Flag5"};
            
            var randomFlags = flagNames.ToDictionary(keySelector: x => x, elementSelector: x => Fixture.Create<Bit>());

            foreach(var flag in flagNames)
                SetFlag(flag, randomFlags[flag]);

            for(var i = 0; i <= Fixture.Create<byte>(); i++)
            {
                executor();

                foreach(var flag in flagNames)
                    Assert.AreEqual(randomFlags[flag], GetFlag(flag));
            }
        }

        protected void WriteShortToMemory(ushort address, short value)
        {
            ProcessorAgent.Memory[address] = value.GetLowByte();
            ProcessorAgent.Memory[address + 1] = value.GetHighByte();
        }

        protected short ReadShortFromMemory(ushort address)
        {
            return NumberUtils.CreateShort(ProcessorAgent.Memory[address], ProcessorAgent.Memory[address + 1]);
        }

        protected void SetupRegOrMem(string reg, byte value, byte offset = 0)
        {
            if(reg == "(HL)") {
                var address = Fixture.Create<ushort>();
                if(address < 10) address += 10;
                ProcessorAgent.Memory[address] = value;
                Registers.HL = address.ToShort();
            }
            else if(reg.StartsWith(("(I"))) {
                var regName = reg.Substring(1, 2);
                var address = Fixture.Create<ushort>();
                if(address < 1000) address += 1000;
                var realAddress = address.Add(offset.ToSignedByte());
                ProcessorAgent.Memory[realAddress] = value;
                SetReg(regName, address.ToShort());
            }
            else {
                SetReg(reg, value);
            }
        }

        protected byte ValueOfRegOrMem(string reg, byte offset = 0)
        {
            if(reg == "(HL)") {
                return ProcessorAgent.Memory[Registers.HL];
            }
            else if (reg.StartsWith(("(I"))) {
                var regName = reg.Substring(1, 2);
                var address = GetReg<short>(regName).Add(offset.ToSignedByte());
                return ProcessorAgent.Memory[address];
            }
            else
            {
                return GetReg<byte>(reg);
            }
        }
        
        protected static object[] GetBitInstructionsSource(byte baseOpcode, bool includeLoadReg = true, bool loopSevenBits = false)
        {
            var bases = new[]
            {
                new object[] {"A", 7},
                new object[] {"B", 0},
                new object[] {"C", 1},
                new object[] {"D", 2},
                new object[] {"E", 3},
                new object[] {"H", 4},
                new object[] {"L", 5},
                new object[] {"(HL)", 6}
            };

            var sources = new List<object[]>();
            var bitsCount = loopSevenBits ? 7 : 0;
            for (var bit = 0; bit <= bitsCount; bit++)
            {
                foreach (var instr in bases)
                {
                    var reg = (string)instr[0];
                    var regCode = (int)instr[1];
                    var opcode = baseOpcode | (bit << 3) | regCode;
                    //srcReg, dest, opcode, prefix, bit
                    sources.Add(new object[] {reg, (string)null, (byte)opcode, (byte?)null, bit});
                }

                foreach (var instr in bases)
                {
                    var destReg = (string)instr[0];
                    if(destReg == "(HL)") destReg = "";
                    if(destReg != "" && !includeLoadReg) continue;
                    var regCode = baseOpcode | (bit << 3) | (int)instr[1];
                    foreach(var reg in new[] { "(IX+n)", "(IY+n)" }) 
                    {
                        //srcReg, dest, opcode, prefix, bit
                        sources.Add(new object[] {reg, destReg, (byte?)regCode, reg[2]=='X' ? (byte?)0xDD : (byte?)0xFD, bit});
                    }
                }
            }

            return sources.ToArray();
        }

        public static void ModifyTestCaseCreationForIndexRegs(string regName, ref int regNamesArrayIndex, out byte? prefix)
        {
            prefix = null;

            switch(regName) {
                case "IXH":
                    regNamesArrayIndex = 4;
                    prefix = 0xDD;
                    break;
                case "IXL":
                    regNamesArrayIndex = 5;
                    prefix = 0xDD;
                    break;
                case "IYH":
                    regNamesArrayIndex = 4;
                    prefix = 0xFD;
                    break;
                case "IYL":
                    regNamesArrayIndex = 5;
                    prefix = 0xFD;
                    break;
                case "(IX+n)":
                    regNamesArrayIndex = 6;
                    prefix = 0xDD;
                    break;
                case "(IY+n)":
                    regNamesArrayIndex = 6;
                    prefix = 0xFD;
                    break;
                }
        }
        
        protected int ExecuteBit(byte opcode, byte? prefix = null, byte? offset = null)
        {
            if (prefix == null)
                return Execute(opcode, 0xCB);
            else
                return Execute(0xCB, prefix, offset.Value, opcode);
        }

        #endregion

        #region ParityTable

        protected byte[] Parity = new byte[] { 
	        1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
	        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
	        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
	        1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
	        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
	        1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
	        1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
	        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
	        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
	        1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
	        1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
	        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
	        1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
	        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
	        0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
	        1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1 };
        #endregion

        #region Fake classes

        protected class FakeInstructionExecutor : Z80InstructionExecutor
		{
		    public List<byte> UnsupportedExecuted = new List<byte>();

		    protected override int ExecuteUnsopported_ED_Instruction(byte secondOpcodeByte)
		    {
				UnsupportedExecuted.Add(secondOpcodeByte);
		        return base.ExecuteUnsopported_ED_Instruction(secondOpcodeByte);
		    }
        }

        protected class FakeProcessorAgent : IZ80ProcessorAgent
        {
            public FakeProcessorAgent()
            {
                Registers = new Z80Registers();
                Memory = new byte[65536];
                Ports = new byte[256];
            }

            public byte[] Memory { get; set; }

            public byte[] Ports { get; set; }

            public ushort MemoryPointer { get; set; }

            public byte CurrentInterruptMode { get; private set; }

            public byte FetchNextOpcode()
            {
                return Memory[Registers.PC++];
            }

            public byte PeekNextOpcode()
            {
                return Memory[Registers.PC];
            }

            public byte ReadFromMemory(ushort address)
            {
                return Memory[address];
            }

            public void WriteToMemory(ushort address, byte value)
            {
                Memory[address] = value;
            }

            public byte ReadFromPort(byte portNumber)
            {
                return Ports[portNumber];
            }

            public void WriteToPort(byte portNumber, byte value)
            {
                Ports[portNumber] = value;
            }

            public IZ80Registers Registers { get; set; }

            public void SetInterruptMode(byte interruptMode)
            {
                CurrentInterruptMode = interruptMode;
            }

            public void Stop(bool isPause = false)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}