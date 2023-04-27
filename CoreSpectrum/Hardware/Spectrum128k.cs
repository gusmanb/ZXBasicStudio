using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class Spectrum128k : MachineBase
    {
        private static readonly MachineTimmings Timmings128k = new MachineTimmings
        {
            CpuClock = 3546900,
            ScansPerFrame = 311,
            TStatesPerScan = 228,
            FirstScan = 63
        };

        public Spectrum128k(byte[][] RomSet, IVideoRenderer Renderer) : base(RomSet, Renderer) { }

        protected override MachineHardware GetHardware(byte[][] RomSet, IVideoRenderer Renderer)
        {
            if (RomSet == null || RomSet.Length != 2 || RomSet[0].Length != 16 * 1024 || RomSet[1].Length != 16 * 1024)
                throw new ArgumentException("Spectrum 128k needs two 16Kb ROMs.");

            var memory = new Memory128k(RomSet[0], RomSet[1]);
            var ula = new ULA128k(Timmings128k.CpuClock, 44100, Renderer, memory);
            
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
    }
}
