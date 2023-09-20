using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Interfaces
{
    public interface ISpectrumMemory : IMemory
    {
        void ClearRAM();
        Span<byte> GetVideoMemory();
    }
}
