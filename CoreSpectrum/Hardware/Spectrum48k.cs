using CoreSpectrum.SupportClasses;
using Konamiman.Z80dotNet;

namespace CoreSpectrum.Hardware
{
    public class Spectrum48k : SpectrumBase
    {
        /// <summary>
        /// PC safe for program injection
        /// </summary>
        ushort _injectPC;
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

        public Spectrum48k(byte[][] RomSet, ushort InjectionAddress) : base(RomSet) 
        {
            _injectPC = InjectionAddress;
        }

        protected override MachineHardware GetHardware(byte[][] RomSet)
        {

            if (RomSet == null || RomSet.Length != 1 || RomSet[0].Length != 16 * 1024)
                throw new ArgumentException("Spectrum 48k needs a 16Kb ROM.");

            var ula = new ULA48k(Timmings48k.CpuClock, 44100, Timmings48k);
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

                if (chunk.Data.Length + chunk.Address > 0xFFFF)
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
                if (_z80.Registers.PC == _injectPC)
                {
                    if (_injectImage != null)
                    {
                        //Fill memory with chunks
                        foreach (var chunk in _injectImage.Chunks)
                            _memory.SetContents(chunk.Address, chunk.Data);

                        //PC at our program
                        _z80.Registers.PC = _injectImage.Org;

                        //Clear stack and store RET address (to return to 48k basic)
                        var sp = 0xFFFF;

                        byte[] retAddr = BitConverter.GetBytes(_injectPC);
                        sp -= 2;

                        _z80.Registers.SP = unchecked((short)sp);
                        _memory.SetContents(sp, retAddr);

                        //Notify that the program is ready
                        if (ProgramReady != null)
                            ProgramReady(this, EventArgs.Empty);
                    }

                    _injecting = false;
                    _injectImage = null;
                }
            }

            base.z80_BeforeInstructionFetch(sender, e);
        }

        protected override void Disposal()
        {
            ProgramReady = null;
        }
    }
}
