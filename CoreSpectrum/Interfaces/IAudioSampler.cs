using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Interfaces
{
    public interface IAudioSampler
    {
        void AddSample(ulong TStates, byte Value);
        void Play();
        void Pause();
        void Resume(ulong TStates);
        void Stop();
    }
}
