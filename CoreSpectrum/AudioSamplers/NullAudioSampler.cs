using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.AudioSamplers
{
    public class NullAudioSampler : IAudioSampler
    {
        public void AddSample(ulong Sample)
        {
            
        }

        public void Pause()
        {
            
        }
        public void Resume(ulong TStates)
        {

        }
        public void Play()
        {
            
        }

        public void Stop() { }
    }
}
