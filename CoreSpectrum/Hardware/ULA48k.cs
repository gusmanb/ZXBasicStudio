using CoreSpectrum.Enums;
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
    public class ULA48k : ULABase
    {
        SimpleContentionSource contention = new SimpleContentionSource(false);

        internal override IContentionSource ContentionSource
        {
            get
            {
                return contention;
            }
        }

        public ULA48k(int CpuClock, int AudioSamplingFrequency, IVideoRenderer Renderer) : base(CpuClock, AudioSamplingFrequency, Renderer) { }
        public override byte this[byte portLo, byte portHi]
        {
            get
            {
                if ((portLo & 1) != 0)
                    return 0xff;

                byte value = ReadKeyboard(portHi);

                return (byte)(value | (_ear > 0 ? 0x40 : 0));
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
