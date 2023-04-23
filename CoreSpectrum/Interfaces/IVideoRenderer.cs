using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Interfaces
{
    public interface IVideoRenderer
    {
        void RenderLine(IMemory Memory, byte borderColor, bool FlashInvert, int LineNumber);
    }
}
