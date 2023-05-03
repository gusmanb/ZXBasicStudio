using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Interfaces
{
    public interface IAudioSource
    {
        int AudioThreshold { get; }
        event EventHandler<AudioEventArgs> AudioChanged;

    }

    public class AudioEventArgs : EventArgs
    {
        public required byte AudioLevel { get; set; }
    }
}
