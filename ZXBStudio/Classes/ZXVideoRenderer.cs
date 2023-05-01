using CoreSpectrum.Hardware;
using CoreSpectrum.Renderers;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using Avalonia.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Svg;

namespace ZXBasicStudio.Classes
{
    public class ZXVideoRenderer : PaletizedVideoRenderer<int>
    {
        private static readonly byte BRIGHT = 0xff, NORM = 0xd7;
        static int[] palette = new int[]
            {
                (int)Color.FromArgb(255, 0, 0, 0).ToUint32(),
                (int)Color.FromArgb(255, 0, 0, NORM).ToUint32(),
                (int)Color.FromArgb(255, NORM, 0, 0).ToUint32(),
                (int)Color.FromArgb(255, NORM, 0, NORM).ToUint32(),
                (int)Color.FromArgb(255, 0, NORM, 0).ToUint32(),
                (int)Color.FromArgb(255, 0, NORM, NORM).ToUint32(),
                (int)Color.FromArgb(255, NORM, NORM, 0).ToUint32(),
                (int)Color.FromArgb(255, NORM, NORM, NORM).ToUint32(),
                (int)Color.FromArgb(255, 0, 0, 0).ToUint32(),
                (int)Color.FromArgb(255, 0, 0, BRIGHT).ToUint32(),
                (int)Color.FromArgb(255, BRIGHT, 0, 0).ToUint32(),
                (int)Color.FromArgb(255, BRIGHT, 0, BRIGHT).ToUint32(),
                (int)Color.FromArgb(255, 0, BRIGHT, 0).ToUint32(),
                (int)Color.FromArgb(255, 0, BRIGHT, BRIGHT).ToUint32(),
                (int)Color.FromArgb(255, BRIGHT, BRIGHT, 0).ToUint32(),
                (int)Color.FromArgb(255, BRIGHT, BRIGHT, BRIGHT).ToUint32(),
            };

        public ZXVideoRenderer() : base(palette, false) { }

        public void DumpScreenMemory(SpectrumBase ZXMachine) 
        {
            var mem = ZXMachine.Memory.GetVideoMemory();

            for (int buc = 0; buc < 312; buc++)
                RenderLine(mem, 0, ZXMachine.Timmings.FirstScan, ZXMachine.ULA.FlashInvert, buc);
        }
    }
}
