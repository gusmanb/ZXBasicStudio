using CoreSpectrum.Hardware;
using CoreSpectrum.Renderers;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using Avalonia.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void DumpScreenMemory(Machine ZXMachine, int[] OutputBuffer) 
        {
            Array.Copy(_videoBuffer, OutputBuffer, _videoBuffer.Length);

            for (int line = 64; line < 256; line++)
                DumpPixels(ZXMachine.ROMRAM, ZXMachine.ULA.FlashInvert, line, OutputBuffer);
        }

        private void DumpPixels(IMemory memory, bool flashInvert, int line, int[] outputBuffer)
        {
            int lineStart = line * 416;
            if (line < 64 || line >= 256) // top and bottom border
            {
                return;
            }

            lineStart += 80;
            line -= 64;
            int charY = 0x5800 + ((line >> 3) << 5);
            int lineAddr = ((line & 0x07) << 8) | ((line & 0x38) << 2) | ((line & 0xC0) << 5) | 0x4000;

            for (int charX = 0; charX < 32; charX++)
            {
                byte att = memory[charY + charX];
                int ink = att & 0x07;
                int paper = (att & 0x38) >> 3;
                if ((att & 0x40) != 0) { ink += 8; paper += 8; }
                bool flash = (att & 0x80) != 0;
                bool invert = flash && flashInvert;
                byte byt = memory[lineAddr++];

                int realPaper = _palette[invert ? ink : paper];
                int realInk = _palette[invert ? paper : ink];

                for (int bit = 128; bit > 0; bit >>= 1)
                    outputBuffer[lineStart++] = (byt & bit) != 0 ? realInk : realPaper;
            }
        }
    }
}
