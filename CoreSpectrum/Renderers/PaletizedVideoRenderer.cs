using CoreSpectrum.Interfaces;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Renderers
{
    public class PaletizedVideoRenderer<T> : IVideoRenderer
    {
        bool _borderless;
        public bool Borderless { get { return _borderless; } set { _borderless = value; } }

        protected T[] _videoBuffer;
        protected T[] _videoBufferBorderless;
        protected T[] _palette;

        public T[] VideoBuffer { get { return _borderless ? _videoBufferBorderless : _videoBuffer; } }

        public PaletizedVideoRenderer(T[] palette, bool borderless)
        {
            _borderless = borderless;
            _videoBuffer = new T[312 * 416];
            _videoBufferBorderless = new T[256 * 192];
            _palette = (T[])palette.Clone();
        }

        public void RenderLine(IMemory memory, byte borderColor, bool flashInvert, int line)
        {
            if (_borderless)
                RenderBorderless(memory, flashInvert, line);
            else
                RenderBorder(memory, borderColor, flashInvert, line);
        }
        private void RenderBorder(IMemory memory, byte borderColor, bool flashInvert, int line)
        {
            int lineStart = line * 416;
            if (line < 64 || line >= 256) // top and bottom border
            {
                FillBorder(lineStart, 416, borderColor);
                return;
            }

            FillBorder(lineStart, 80, borderColor);
            FillBorder(lineStart + 336, 80, borderColor);

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

                T realPaper = _palette[invert ? ink : paper];
                T realInk = _palette[invert ? paper : ink];

                for (int bit = 128; bit > 0; bit >>= 1)
                    _videoBuffer[lineStart++] = (byt & bit) != 0 ? realInk : realPaper;
            }
        }
        private void RenderBorderless(IMemory memory, bool flashInvert, int line)
        {

            int lineStart = line * 416;
            if (line < 64 || line >= 256) // top and bottom border
            {
                return;
            }


            line -= 64;
            lineStart = line * 256;
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

                T realPaper = _palette[invert ? ink : paper];
                T realInk = _palette[invert ? paper : ink];

                for (int bit = 128; bit > 0; bit >>= 1)
                    _videoBufferBorderless[lineStart++] = (byt & bit) != 0 ? realInk : realPaper;
            }
        }
        private void FillBorder(int start, int length, byte color)
        {
            Array.Fill(_videoBuffer, _palette[color], start, length);
        }
    }
}
