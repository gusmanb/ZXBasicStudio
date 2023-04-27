using CoreSpectrum.Interfaces;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Renderers
{
    public class IndexedVideoRenderer : IVideoRenderer
    {
        bool _borderless;

        public bool Borderless { get { return _borderless; } set { _borderless = value; } }

        protected byte[] _videoBuffer;
        protected byte[] _videoBufferBorderless;
        protected byte[] _borderSample;

        public byte[] VideoBuffer { get { return _borderless ? _videoBufferBorderless : _videoBuffer; } }

        public byte[] BorderSample { get { return _borderSample; } }

        public IndexedVideoRenderer(bool borderless)
        {
            _borderless = borderless;
            _videoBuffer = new byte[312 * 416];
            _videoBufferBorderless = new byte[256 * 192];
            _borderSample = new byte[312];
        }

        public void RenderLine(Span<byte> videoMemory, byte firstScan, byte borderColor, bool flashInvert, int line)
        {
            if (_borderless)
                RenderBorderless(videoMemory, firstScan, flashInvert, line);
            else
                RenderBorder(videoMemory, firstScan, borderColor, flashInvert, line);
        }
        private void RenderBorder(Span<byte> videoMemory, byte firstScan, byte borderColor, bool flashInvert, int line)
        {
            int lineStart = line * 416;
            if (line < firstScan || line >= firstScan + 192) // top and bottom border, CORRECT FOR 128k
            {
                FillBorder(lineStart, 416, borderColor);
                return;
            }

            FillBorder(lineStart, 80, borderColor);
            FillBorder(lineStart + 336, 80, borderColor);

            lineStart += 80;
            line -= firstScan;
            int charY = 0x1800 + ((line >> 3) << 5);
            int lineAddr = ((line & 0x07) << 8) | ((line & 0x38) << 2) | ((line & 0xC0) << 5);

            for (int charX = 0; charX < 32; charX++)
            {
                byte att = videoMemory[charY + charX];
                int ink = att & 0x07;
                int paper = (att & 0x38) >> 3;
                if ((att & 0x40) != 0) { ink += 8; paper += 8; }
                bool flash = (att & 0x80) != 0;
                bool invert = flash && flashInvert;
                byte byt = videoMemory[lineAddr++];

                byte realPaper = (byte)(invert ? ink : paper);
                byte realInk = (byte)(invert ? paper : ink);

                for (int bit = 128; bit > 0; bit >>= 1)
                    _videoBuffer[lineStart++] = (byt & bit) != 0 ? realInk : realPaper;
            }
        }
        private void RenderBorderless(Span<byte> videoMemory, byte firstScan, bool flashInvert, int line)
        {
            if (line < firstScan || line >= firstScan + 192) // top and bottom border, CORRECT FOR 128k
            {
                return;
            }


            line -= firstScan;
            int lineStart = line * 256;
            int charY = 0x1800 + ((line >> 3) << 5);
            int lineAddr = ((line & 0x07) << 8) | ((line & 0x38) << 2) | ((line & 0xC0) << 5);

            for (int charX = 0; charX < 32; charX++)
            {
                byte att = videoMemory[charY + charX];
                int ink = att & 0x07;
                int paper = (att & 0x38) >> 3;
                if ((att & 0x40) != 0) { ink += 8; paper += 8; }
                bool flash = (att & 0x80) != 0;
                bool invert = flash && flashInvert;
                byte byt = videoMemory[lineAddr++];

                byte realPaper = (byte)(invert ? ink : paper);
                byte realInk = (byte)(invert ? paper : ink);

                for (int bit = 128; bit > 0; bit >>= 1)
                    _videoBufferBorderless[lineStart++] = (byt & bit) != 0 ? realInk : realPaper;
            }
        }
        private void FillBorder(int start, int length, byte color)
        {
            Array.Fill(_videoBuffer, color, start, length);
        }
    }
}
