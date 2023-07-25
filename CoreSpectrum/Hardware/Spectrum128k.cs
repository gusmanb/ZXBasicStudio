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
        /// <summary>
        /// PC safe for switching to 48k ROM
        /// </summary>
        ushort _resetPC;

        /// <summary>
        /// PC safe for injection (after switching to 48k mode)
        /// </summary>
        ushort _injectPC;

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
        ProgramImage? _injectImage;

        public override event EventHandler? ProgramReady;

        public Spectrum128k(byte[][] RomSet, ushort ResetAddress, ushort InjectAddress) : base(RomSet) 
        {
            _resetPC = ResetAddress;
            _injectPC = InjectAddress;
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
                if (_z80.Registers.PC == _injectPC && _on48mode)
                {
                    if (_injectImage != null)
                    {
                        var mem = (Memory128k)_memory;

                        foreach (var chunk in _injectImage.Chunks)
                        {
                            mem.Map.SetActiveBank(chunk.Bank);
                            _memory.SetContents(chunk.Address, chunk.Data);
                        }

                        var bankm = _memory.GetContents(0x5B5C, 1)[0];
                        bankm |= 1 << 4;
                        _memory.SetContents(0x5B5C, new byte[] { bankm });

                        mem.Map.SetActiveBank(_injectImage.InitialBank);
                        _z80.Registers.PC = _injectImage.Org;
                        ushort sp = (ushort)(_injectImage.Org - 1);//_z80.Registers.SP;//0xFFFF;
                        byte[] retAddr = BitConverter.GetBytes(_injectPC);
                        sp -= 2;
                        _z80.Registers.SP = unchecked((short)sp);
                        _memory.SetContents(sp, retAddr);

                        if (ProgramReady != null)
                            ProgramReady(this, EventArgs.Empty);
                    }

                    _injecting = false;
                    _injectImage = null;
                    _on48mode = false;
                }
                else if (_z80.Registers.PC == _resetPC)
                {
                    
                    var mem = (Memory128k)_memory;

                    _on48mode = true;
                    mem.Map.SetActiveRom(1);
                    mem.Map.SetActiveBank(0);
                    mem.Map.SetActiveScreen(0);
                    _z80.Registers.PC = 0;
                    _on48mode = true;
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
