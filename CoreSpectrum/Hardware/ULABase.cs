using CoreSpectrum.Enums;
using CoreSpectrum.Interfaces;
using Konamiman.Z80dotNet;
using Main.Dependencies_Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public abstract class ULABase : IIO, IZ80InterruptSource, ITStatesTarget, ISpectrumAudio
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
        protected IVideoRenderer? _renderer;

        private int _currentLine = 0;
        private int _flashFrames = 0;
        private bool _screenIrq;
        protected byte _mic;
        protected byte _ear;
        protected ulong _tStates;
        protected ulong _irqStates;

        private bool _newFrame;
        protected byte[] keybSegments = new byte[] { 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, };

        public event EventHandler? NmiInterruptPulse;
        public abstract byte this[byte portLo, byte portHi] { get; set; }
        internal abstract IContentionSource ContentionSource { get; }
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
        public virtual ulong TStates 
        { 
            get 
            { 
                return _tStates; 
            } 
            set 
            { 
                _tStates = value; 

                if (_screenIrq && _tStates > _irqStates) 
                    _screenIrq = false; 
            } 
        }
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
        public virtual IVideoRenderer? Renderer { get { return _renderer; } set { _renderer = value; } }

        internal virtual bool Turbo { get; set; }
        protected ULABase(int CpuClock, int AudioSamplingFrequency)
        {
            _sampler = new ULASampler(AudioSamplingFrequency, CpuClock);
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
        public virtual void ClearIRQ()
        {
            _screenIrq = false;
        }
        public virtual void ScanLine(Span<byte> VideoMemory, byte FirstScan, int ScansPerFrame)
        {
            if(_renderer != null)
                _renderer.RenderLine(VideoMemory, FirstScan, Border, FlashInvert, _currentLine);

            _currentLine++;

            if (_currentLine == ScansPerFrame)
            {
                _currentLine = 0;
                _screenIrq = true;
                _irqStates = _tStates + 32;
                _newFrame = true;
                _flashFrames++;

                if (_flashFrames >= 16)
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
        public virtual void Reset()
        {
            _currentLine = 0;
            _flashFrames = 0;
            _screenIrq = false;
            _mic = 0;
            _ear = 0;
            _tStates = 0;
            _irqStates = 0;
            _newFrame = false;
            keybSegments = new byte[] { 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, 0xbf, };
            ResetAudio(true);
    }
        public virtual void ResetAudio(bool FullReset = false)
        {
            _sampler.ResetSampler(TStates);
        }
        protected class KeyInfo
        {
            public int Row { get; set; }
            public int BitValue { get; set; }
        }
    }
}
