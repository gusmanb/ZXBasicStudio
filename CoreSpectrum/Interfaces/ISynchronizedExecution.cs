using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Interfaces
{
    public interface ISynchronizedExecution
    {
        void Start();

        void Stop();

        void Pause();

        void Resume();

        void Reset();

        void Turbo(bool Enable);

        void Step();
    }
}
