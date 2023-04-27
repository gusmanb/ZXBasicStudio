using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class Spectrum48k : MachineBase
    {
        private static readonly MachineTimmings Timmings48k = new MachineTimmings 
        { 
            ProcessorSpeed = 3500000,
            ScansPerFrame = 312,
            TStatesPerScan = 224
        };

        public Spectrum48k(byte[][] RomSet, IVideoRenderer Renderer, IAudioSampler Sampler) : base(RomSet, Renderer, Sampler) { }

        protected override MachineHardware GetHardware(byte[][] RomSet, IVideoRenderer Renderer, IAudioSampler Sampler)
        {
            var ula = new ULA48k(Renderer, Sampler);
            var memory = new Memory48k(RomSet);

            MachineHardware hardware = new MachineHardware
            { 
                ULA = ula, 
                Memory = memory, 
                Timmings = Timmings48k 
            };

            return hardware;
        }
    }
}
