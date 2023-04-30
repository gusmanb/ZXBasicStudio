using CoreSpectrum.Interfaces;
using CoreSpectrum.SupportClasses;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class Spectrum48k : MachineBase
    {
        const ushort INJECT_PC = 0x12ac;
        private static readonly MachineTimmings Timmings48k = new MachineTimmings 
        { 
            CpuClock = 3500000,
            ScansPerFrame = 312,
            TStatesPerScan = 224,
            FirstScan = 63,
            IrqCycles = 32
        };

        bool _injecting = false;
        ProgramImage? _injectImage;

        public override event EventHandler? ProgramReady;

        public Spectrum48k(byte[][] RomSet, IVideoRenderer Renderer) : base(RomSet, Renderer) { }

        protected override MachineHardware GetHardware(byte[][] RomSet, IVideoRenderer Renderer)
        {

            if (RomSet == null || RomSet.Length != 1 || RomSet[0].Length != 16 * 1024)
                throw new ArgumentException("Spectrum 48k needs a 16Kb ROM.");

            var ula = new ULA48k(Timmings48k.CpuClock, 44100, Renderer);
            var memory = new Memory48k(RomSet);

            MachineHardware hardware = new MachineHardware
            { 
                ULA = ula, 
                Memory = memory, 
                Timmings = Timmings48k 
            };

            return hardware;
        }

        public override bool InjectProgram(ProgramImage Image)
        {
            Stop();

            foreach (var chunk in Image.Chunks)
            {
                if (chunk.Bank != 0)
                    return false;
            }

            _injecting = true;
            _injectImage = Image;

            Start();

            return true;
        }

        protected override void z80_BeforeInstructionFetch(object? sender, BeforeInstructionFetchEventArgs e)
        {
            if (_injecting)
            {
                if (_z80.Registers.PC == INJECT_PC)
                {
                    if (_injectImage != null)
                    {

                        foreach (var chunk in _injectImage.Chunks)
                            _memory.SetContents(chunk.Address, chunk.Data);

                        _z80.Registers.PC = _injectImage.Org;

                        var sp = (ushort)_z80.Registers.SP;
                        byte[] retAddr = BitConverter.GetBytes(INJECT_PC);
                        sp -= 2;
                        _z80.Registers.SP = unchecked((short)sp);
                        _memory.SetContents(sp, retAddr);

                        if(ProgramReady != null)
                            ProgramReady(this, EventArgs.Empty);
                    }

                    _injecting = false;
                    _injectImage = null;
                }
            }

            base.z80_BeforeInstructionFetch(sender, e);
        }
    }
}
