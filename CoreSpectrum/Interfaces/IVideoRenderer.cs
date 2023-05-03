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
        void RenderLine(Span<byte> Memory, byte FirstScan, byte BorderColor, bool FlashInvert, int LineNumber);
    }
}
