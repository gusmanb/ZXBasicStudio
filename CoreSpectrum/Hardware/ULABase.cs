using CoreSpectrum.Enums;
using CoreSpectrum.Interfaces;
using Konamiman.Z80dotNet;
using Main.Dependencies_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public abstract class ULABase : IIO, IZ80InterruptSource, ITStatesTarget
    {
        protected readonly Dictionary<SpectrumKeys, KeyInfo> keyInfos = new Dictionary<SpectrumKeys, KeyInfo>
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

        internal readonly ULASampler _sampler;
        protected readonly IVideoRenderer _renderer;

        private int _currentLine = 0;
        private int _flashFrames = 0;
        private bool _screenIrq;
        private byte _mic;
        private byte _ear;
        private bool _newFrame;
        protected byte[] keybSegments = new byte[] { 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, };

        public event EventHandler? NmiInterruptPulse;
        public abstract byte this[byte portLo, byte portHi] { get; set; }
        public virtual bool IntLineIsActive
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
        public virtual bool NewFrame
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
        public virtual byte? ValueOnDataBus => 255;
        public virtual ulong TStates { get; set; }
        /// <summary>
        /// Audio input
        /// </summary>
        public virtual byte Ear 
        { 
            get { return _ear; } 
            set 
            { 
                if (_ear != value) 
                { 
                    _ear = value; 
                    CreateAudioSample(); 
                } 
            } 
        }
        /// <summary>
        /// Audio output
        /// </summary>
        public virtual byte Mic
        {
            get
            {
                return _mic;
            }
            protected set 
            {
                if (value != _mic)
                {
                    _mic = value;
                    CreateAudioSample();
                }
            }
        }
        public virtual byte AudioOutput
        {
            get { return (byte)(_ear + _mic); }
        }
        public virtual byte Border { get; protected set; }
        public virtual bool FlashInvert { get; protected set; }
        internal virtual bool Turbo { get; set; }
        protected ULABase(int CpuClock, int AudioSamplingFrequency, IVideoRenderer renderer)
        {
            _sampler = new ULASampler(AudioSamplingFrequency, CpuClock);
            _renderer = renderer;
        }
        public virtual void PressKey(SpectrumKeys Key)
        {
            var info = keyInfos[Key];
            keybSegments[info.Row] = (byte)(keybSegments[info.Row] & ~info.BitValue);
        }
        public virtual void ReleaseKey(SpectrumKeys Key)
        {
            var info = keyInfos[Key];
            keybSegments[info.Row] = (byte)(keybSegments[info.Row] | info.BitValue);
        }
        public virtual void ScanLine(Span<byte> VideoMemory)
        {
            _renderer.RenderLine(VideoMemory, Border, FlashInvert, _currentLine++);

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
        public virtual int GetSamples(float[] Buffer)
        {
            return _sampler.GetSamples(TStates, Buffer);
        }
        protected virtual byte ReadKeyboard(byte LinesToRead)
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
        protected virtual void CreateAudioSample()
        {
            if (Turbo)
                return;

            _sampler.AddSample(TStates, AudioOutput);
        }
        protected class KeyInfo
        {
            public int Row { get; set; }
            public int BitValue { get; set; }
        }
    }
}
