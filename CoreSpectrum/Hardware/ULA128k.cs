using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class ULA128k : ULABase
    {
        public ULA128k(IVideoRenderer Renderer, IAudioSampler Sampler) : base(Renderer, Sampler) 
        {
            
        }
        public override byte this[byte portLo, byte portHi]
        {
            get
            {
                if ((portLo & 1) != 0)
                    return 0xff;

                byte value = ReadKeyboard(portHi);

                return (byte)(value | (AudioOutput > 0 ? 0x40 : 0));
            }

            set
            {
                if ((portLo & 1) != 0)
                    return;

                Border = (byte)(value & 7);
                byte newMic = (byte)((value & 0x18) >> 3);

                Mic = newMic;
                CreateAudioSample();
            }
        }
    }
}
