using CoreSpectrum.Interfaces;
using CoreSpectrum.SupportClasses;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class Spectrum128k : SpectrumBase
    {
        const ushort LAST_K = 0x5C08;
        const ushort KEY_INPUT = 0x10A8;
        const ushort MAIN_2 = 0x12AC;
        const ushort MAIN_3_END = 0x12e0;
        const ushort CLEAR = 0x1EAF;
        const ushort CLEAR_END = 0x1EEC;
        const ushort BANKM = 0x5B5C;
        const byte KEY_RET = 0x0D;

        /// <summary>
        /// PC safe for switching to 48k ROM
        /// </summary>
        ushort _resetPC;

        private static readonly MachineTimmings Timmings128k = new MachineTimmings
        {
            CpuClock = 3546900,
            ScansPerFrame = 311,
            TStatesPerScan = 228,
            FirstScan = 63,
            IrqCycles = 32

        };

        bool _injecting = false;
        bool _on48mode = false;
        //bool _onClear = false;

        ProgramImage? _injectImage;

        public override event EventHandler? ProgramReady;

        public Spectrum128k(byte[][] RomSet, ushort ResetAddress) : base(RomSet) 
        {
            _resetPC = ResetAddress;
        }

        protected override MachineHardware GetHardware(byte[][] RomSet)
        {
            if (RomSet == null || RomSet.Length != 2 || RomSet[0].Length != 16 * 1024 || RomSet[1].Length != 16 * 1024)
                throw new ArgumentException("Spectrum 128k needs two 16Kb ROMs.");

            var memory = new Memory128k(RomSet[0], RomSet[1]);
            var ula = new ULA128k(Timmings128k.CpuClock, 44100, memory, Timmings128k);
            
            MachineHardware hardware = new MachineHardware
            {
                ULA = ula,
                Memory = memory,
                Timmings = Timmings128k
            };

            return hardware;
        }

        public override void Reset()
        {
            (_memory as Memory128k).Map.Reset();
            base.Reset();

        }

        public override void Start(bool backgroundThread = false)
        {
            (_memory as Memory128k).Map.Reset();
            base.Start(backgroundThread);
        }
        
        public override bool InjectProgram(ProgramImage Image)
        {
            Stop();

            foreach (var chunk in Image.Chunks)
            {
                if (chunk.Data.Length + chunk.Address > 0xFFFF)
                    return false;
            }

            _injecting = true;
            _injectImage = Image;
            _on48mode = false;

            base.Start();

            return true;
        }

        protected override void z80_BeforeInstructionFetch(object? sender, BeforeInstructionFetchEventArgs e)
        {
            if (_injecting)
            {
                //Injection on the 128k is a bit more convoluted.
                //First, we swap to the 48k ROM.
                //Then we wait until the machine reaches the KEYBOARD_INPUT routine.
                //There we inject a "return" key press.
                //Next we issue a CLEAR call and clear the memory to ORG-1.
                //Once the clear has finished we inject our program, set in the stack the return address to MAIN and jump to it.

                if (_injectImage == null)
                {
                    _injecting = false;
                    _injectImage = null;
                    _on48mode = false;

                    base.z80_BeforeInstructionFetch(sender, e);
                    return;
                }

                if (_z80.Registers.PC == _resetPC && !_on48mode) //Switch to 48k mode (without disabling banking)
                {
                    var mem = (Memory128k)_memory;

                    _on48mode = true;

                    mem.Map.SetActiveRom(1);
                    mem.Map.SetActiveBank(0);
                    mem.Map.SetActiveScreen(0);

                    _z80.Registers.PC = 0;
                    _on48mode = true;
                }
                else if (_z80.Registers.PC == KEY_INPUT && _on48mode)
                {
                    var flags = _memory.GetByte(((ushort)_z80.Registers.IY) + 1); //Get the FLAGS variable
                    flags |= (byte)(1 << 5); //Signal a key press
                    _memory.SetByte(((ushort)_z80.Registers.IY) + 1, flags); //Update flags
                    _memory.SetByte(LAST_K, KEY_RET); //Store a return in the LAST_K var
                }
                else if (_z80.Registers.PC == MAIN_3_END && _on48mode)
                {
                    _z80.Registers.BC = (short)(_injectImage.Org - 1); //Store in BC the clear address
                    _z80.ExecuteCall(CLEAR); //Call CLEAR
                }
                else if (_z80.Registers.PC == CLEAR_END && _on48mode) //Wait until CLEAR reaches its last instruction
                {
                    var mem = (Memory128k)_memory;

                    foreach (var chunk in _injectImage.Chunks) //Inject memory chunks
                    {
                        mem.Map.SetActiveBank(chunk.Bank);
                        _memory.SetContents(chunk.Address, chunk.Data);
                    }

                    mem.Map.SetActiveBank(_injectImage.InitialBank); //Swap to the specified memory bank
                    _z80.Registers.PC = _injectImage.Org;

                    //Update BANKM variable with the selected bank and ROM 1 enabled
                    var bankm = (byte)(_injectImage.InitialBank | (1 << 4));
                    _memory.SetByte(BANKM, bankm);

                    //Store return address in stack
                    ushort sp = (ushort)_z80.Registers.SP;
                    sp -= 2;
                    _z80.Registers.SP = unchecked((short)sp);
                    _memory.SetUshort(sp, MAIN_2);

                    //Signal program ready
                    if (ProgramReady != null)
                        ProgramReady(this, EventArgs.Empty);

                    _injecting = false;
                    _injectImage = null;
                    _on48mode = false;
                }
                
            }

            base.z80_BeforeInstructionFetch(sender, e);
        }

        protected override void Disposal()
        {
            ProgramReady = null;
            _injectImage = null;
        }
    }
}
