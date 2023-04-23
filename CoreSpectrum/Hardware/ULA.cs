using CoreSpectrum.Enums;
using CoreSpectrum.Interfaces;
using CoreSpectrum.SupportClasses;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class ULA : IIO, IZ80InterruptSource
    {
        int _currentLine = 0;
        int _flashFrames = 0;
        bool _screenIrq;
        private readonly Machine _machine;
        private readonly IAudioSampler _sampler;
        internal readonly IVideoRenderer _renderer;

        public IVideoRenderer Renderer { get { return _renderer; } }
        public IAudioSampler Sampler { get { return _sampler; } }


        byte[] keybSegments = new byte[] { 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, };
        private readonly Dictionary<SpectrumKeys, KeyInfo> keyInfos = new Dictionary<SpectrumKeys, KeyInfo>
        {
            [SpectrumKeys.Caps] = new KeyInfo { Row = 0, BitValue = 1 },
            [SpectrumKeys.Z] = new KeyInfo { Row = 0, BitValue = 2 },
            [SpectrumKeys.X] = new KeyInfo { Row = 0, BitValue = 4 },
            [SpectrumKeys.C] = new KeyInfo { Row = 0, BitValue = 8 },
            [SpectrumKeys.V] = new KeyInfo { Row = 0, BitValue = 16 },

            [SpectrumKeys.A] = new KeyInfo { Row = 1, BitValue = 1 },
            [SpectrumKeys.S] = new KeyInfo { Row = 1, BitValue = 2 },
            [SpectrumKeys.D] = new KeyInfo { Row = 1, BitValue = 4 },
            [SpectrumKeys.F] = new KeyInfo { Row = 1, BitValue = 8 },
            [SpectrumKeys.G] = new KeyInfo { Row = 1, BitValue = 16 },

            [SpectrumKeys.Q] = new KeyInfo { Row = 2, BitValue = 1 },
            [SpectrumKeys.W] = new KeyInfo { Row = 2, BitValue = 2 },
            [SpectrumKeys.E] = new KeyInfo { Row = 2, BitValue = 4 },
            [SpectrumKeys.R] = new KeyInfo { Row = 2, BitValue = 8 },
            [SpectrumKeys.T] = new KeyInfo { Row = 2, BitValue = 16 },

            [SpectrumKeys.D1] = new KeyInfo { Row = 3, BitValue = 1 },
            [SpectrumKeys.D2] = new KeyInfo { Row = 3, BitValue = 2 },
            [SpectrumKeys.D3] = new KeyInfo { Row = 3, BitValue = 4 },
            [SpectrumKeys.D4] = new KeyInfo { Row = 3, BitValue = 8 },
            [SpectrumKeys.D5] = new KeyInfo { Row = 3, BitValue = 16 },

            [SpectrumKeys.D0] = new KeyInfo { Row = 4, BitValue = 1 },
            [SpectrumKeys.D9] = new KeyInfo { Row = 4, BitValue = 2 },
            [SpectrumKeys.D8] = new KeyInfo { Row = 4, BitValue = 4 },
            [SpectrumKeys.D7] = new KeyInfo { Row = 4, BitValue = 8 },
            [SpectrumKeys.D6] = new KeyInfo { Row = 4, BitValue = 16 },

            [SpectrumKeys.P] = new KeyInfo { Row = 5, BitValue = 1 },
            [SpectrumKeys.O] = new KeyInfo { Row = 5, BitValue = 2 },
            [SpectrumKeys.I] = new KeyInfo { Row = 5, BitValue = 4 },
            [SpectrumKeys.U] = new KeyInfo { Row = 5, BitValue = 8 },
            [SpectrumKeys.Y] = new KeyInfo { Row = 5, BitValue = 16 },

            [SpectrumKeys.Enter] = new KeyInfo { Row = 6, BitValue = 1 },
            [SpectrumKeys.L] = new KeyInfo { Row = 6, BitValue = 2 },
            [SpectrumKeys.K] = new KeyInfo { Row = 6, BitValue = 4 },
            [SpectrumKeys.J] = new KeyInfo { Row = 6, BitValue = 8 },
            [SpectrumKeys.H] = new KeyInfo { Row = 6, BitValue = 16 },

            [SpectrumKeys.Space] = new KeyInfo { Row = 7, BitValue = 1 },
            [SpectrumKeys.Sym] = new KeyInfo { Row = 7, BitValue = 2 },
            [SpectrumKeys.M] = new KeyInfo { Row = 7, BitValue = 4 },
            [SpectrumKeys.N] = new KeyInfo { Row = 7, BitValue = 8 },
            [SpectrumKeys.B] = new KeyInfo { Row = 7, BitValue = 16 }
        };

        public event EventHandler? NmiInterruptPulse;
        public byte this[byte port, byte upperPart]
        {
            get
            {
                if ((port & 1) != 0)
                    return 0xff;

                byte value = ReadKeyboard(upperPart);

                return (byte)(value | (Ear ? 0x40 : 0));
            }

            set
            {
                if ((port & 1) != 0)
                    return;

                Border = (byte)(value & 7);
                byte newMic = (byte)((value & 0x18) >> 3);

                _mic = newMic;
                CreateAudioSample();
            }
        }

        public bool IntLineIsActive
        {
            get
            {
                if (_screenIrq)
                {
                    _screenIrq = false;
                    return true;
                }
                return false;
            }
        }

        bool _newFrame;
        public bool NewFrame
        {
            get
            {
                if (_newFrame)
                {
                    _newFrame = false;
                    return true;
                }
                return false;
            }
        }

        public byte? ValueOnDataBus => 255;

        /// <summary>
        /// Input
        /// </summary>
        public bool Ear 
        { 
            get 
            {
                return _machine._player.AudioOutput();
            } 
        }
        byte _mic;
        /// <summary>
        /// Output
        /// </summary>
        public byte Mic
        {
            get
            {
                return _machine._player.Playing ? (byte)(Ear ? 3 : 0) : _mic;
            }
        }
        public byte Border { get; private set; }
        public bool FlashInvert { get; private set; }
        internal ULA(Machine machine, IVideoRenderer renderer, IAudioSampler sampler)
        {
            _machine = machine;
            _sampler = sampler;
            _renderer = renderer;
        }
        public void PressKey(SpectrumKeys Key)
        {
            var info = keyInfos[Key];
            keybSegments[info.Row] = (byte)(keybSegments[info.Row] & ~info.BitValue);
        }
        public void ReleaseKey(SpectrumKeys Key)
        {
            var info = keyInfos[Key];
            keybSegments[info.Row] = (byte)(keybSegments[info.Row] | info.BitValue);
        }
        public void ScanLine()
        {
            _renderer.RenderLine(_machine._romram, Border, FlashInvert, _currentLine++);

            if(_machine._player.Playing)
                CreateAudioSample();

            if (_currentLine == 312)
            {
                _currentLine = 0;
                _screenIrq = true;
                _newFrame = true;
                _flashFrames++;
                
                if (_flashFrames >= 25)
                {
                    _flashFrames = 0;
                    FlashInvert = !FlashInvert;
                }
            }
        }
        private byte ReadKeyboard(byte LinesToRead)
        {
            byte result = 0xBF;

            if ((LinesToRead & 1) == 0)
                result &= keybSegments[0];

            if ((LinesToRead & 2) == 0)
                result &= keybSegments[1];

            if ((LinesToRead & 4) == 0)
                result &= keybSegments[2];

            if ((LinesToRead & 8) == 0)
                result &= keybSegments[3];

            if ((LinesToRead & 16) == 0)
                result &= keybSegments[4];

            if ((LinesToRead & 32) == 0)
                result &= keybSegments[5];

            if ((LinesToRead & 64) == 0)
                result &= keybSegments[6];

            if ((LinesToRead & 128) == 0)
                result &= keybSegments[7];

            return result;
        }
        internal void CreateAudioSample()
        {
            ulong sample = (_machine._z80.TStatesElapsedSinceStart << 2) | Mic;
            _sampler.AddSample(sample);
        }
        class KeyInfo
        {
            public int Row { get; set; }
            public int BitValue { get; set; }
        }
    }
}
