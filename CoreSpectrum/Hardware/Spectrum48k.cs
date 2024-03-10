using CoreSpectrum.SupportClasses;
using Konamiman.Z80dotNet;

namespace CoreSpectrum.Hardware
{
    public class Spectrum48k : SpectrumBase
    {
        const ushort KEY_INPUT = 0x10A8;
        const ushort MAIN_2 = 0x12AC;
        const ushort MAIN_3_END = 0x12e0;
        const ushort CLEAR = 0x1EAF;
        const ushort CLEAR_END = 0x1EEC;
        const ushort LAST_K = 0x5C08;
        const byte KEY_RET = 0x0D;

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

        public Spectrum48k(byte[][] RomSet) : base(RomSet) 
        {
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
                /*
                if (_z80.Registers.PC == MAIN_2)
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
                        sp -= 2;
                        _z80.Registers.SP = unchecked((short)sp);
                        _memory.SetUshort(sp, MAIN_2);

                        //Notify that the program is ready
                        if (ProgramReady != null)
                            ProgramReady(this, EventArgs.Empty);
                    }

                    _injecting = false;
                    _injectImage = null;
                }*/

                if (_injectImage == null) //In case of having a null image, disable injection
                {
                    _injecting = false;
                    _injectImage = null;

                    base.z80_BeforeInstructionFetch(sender, e);
                    return;
                }

                if (_z80.Registers.PC == KEY_INPUT) //First entrance to keyboard routine, inject an "ENTER"
                {
                    var flags = _memory.GetByte(((ushort)_z80.Registers.IY) + 1); //Get the FLAGS variable
                    flags |= (byte)(1 << 5); //Signal a key press
                    _memory.SetByte(((ushort)_z80.Registers.IY) + 1, flags); //Update flags
                    _memory.SetByte(LAST_K, KEY_RET); //Store a return in the LAST_K var
                }
                else if (_z80.Registers.PC == MAIN_3_END) //"ENTER" has been processed and we can call the "CLEAR" routine
                {
                    _z80.Registers.BC = (short)(_injectImage.Org - 1); //Store in BC the clear address
                    _z80.ExecuteCall(CLEAR); //Call CLEAR
                }
                else if (_z80.Registers.PC == CLEAR_END) //Wait until CLEAR reaches its last instruction and inject our program5
                {

                    //Fill memory with chunks
                    foreach (var chunk in _injectImage.Chunks)
                        _memory.SetContents(chunk.Address, chunk.Data);

                    _z80.Registers.PC = _injectImage.Org;

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
