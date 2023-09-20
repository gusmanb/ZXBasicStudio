using CoreSpectrum.Interfaces;
using CoreSpectrum.SupportClasses;
using Main.Dependencies_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class TapePlayer : ITStatesTarget, IAudioSource
    {
        Tape? _tape;
        ulong _startTStates;
        ulong _offsetTStates;
        ulong _pauseTStates;
        bool _playing = false;
        bool _paused = false;
        ulong _tStates;
        bool _lastValue = false;
        TimeSpan? _tapeLength;

        public event EventHandler<AudioEventArgs>? AudioChanged;

        public TimeSpan? TapeLength { get { return _tapeLength; } }
        public TimeSpan? Position
        {
            get
            {
                if (_tape == null)
                    return null;

                if (!_playing && !_paused)
                    return new TimeSpan(0, 0, (int)(_offsetTStates / 3500000.0));

                if (_paused)
                    return new TimeSpan(0, 0, (int)(((_pauseTStates - _startTStates) + _offsetTStates) / 3500000.0));

                return new TimeSpan(0, 0, (int)(((TStates - _startTStates) + _offsetTStates) / 3500000.0));
            }
        }

        

        public ulong TStates { get { return _tStates; } set { _tStates = value; CheckAudio(); } }

        public int Block
        {
            get
            {
                if (_tape == null)
                    return -1;

                if (!_playing && !_paused)
                    return _tape.GetBlockIndex(_offsetTStates);

                if (_paused)
                    return _tape.GetBlockIndex((_pauseTStates - _startTStates) + _offsetTStates);

                return _tape.GetBlockIndex((TStates - _startTStates) + _offsetTStates);
            }
        }

        public Tape? LoadedTape { get { return _tape; } }
        public bool Playing { get { return _playing; } }
        public bool Paused { get { return _paused; } }

        public int AudioThreshold
        {
            get
            {
                return 1;
            }
        }

        internal TapePlayer()
        {
        }
        public void InsertTape(Tape tape)
        {
            _tape = tape;
            _startTStates = 0;
            _paused = false;
            _playing = false;
            _tapeLength = new TimeSpan(0, 0, (int)(tape.Length / 3500000.0));
        }
        public void EjectTape()
        {
            _tape = null;
            _startTStates = 0;
            _paused = false;
            _playing = false;
            _tapeLength = null;
        }
        public bool Play()
        {
            if (_tape == null || _playing)
                return false;

            _startTStates = TStates;
            _playing = true;
            _paused = false;
            return true;
        }
        public bool FfwRew(int Block)
        {
            if (_tape == null)
                return false;

            var blocks = _tape.Blocks;
            if (Block >= blocks.Length)
                return false;

            _startTStates = _pauseTStates = TStates;
            _offsetTStates = blocks[Block].Start;

            return true;
        }
        public bool Pause()
        {
            if (!_playing || _paused)
                return false;

            _pauseTStates = TStates;
            _playing = false;
            _paused = true;

            return true;
        }
        public bool Resume()
        {
            if (_tape == null || _playing || !_paused)
                return false;

            _startTStates += TStates - _pauseTStates;
            _paused = false;
            _playing = true;

            return true;
        }
        public bool Stop()
        {
            if (_tape == null || (!_playing && !_paused))
                return false;

            _startTStates = 0;
            _offsetTStates = 0;
            _paused = false;
            _playing = false;
            _lastValue = false;
            return true;
        }

        void CheckAudio()
        {
            if (!_playing || _tape == null || AudioChanged == null)
                return;

            ulong TStates = this.TStates + _offsetTStates;

            TStates -= _startTStates;

            if (TStates > _tape.Length)
            {
                Stop();
                return;
            }

            bool value = _tape.GetValue(TStates);

            if (value != _lastValue)
            {
                _lastValue = value;
                AudioChanged(this, new AudioEventArgs { AudioLevel = _lastValue ? (byte)1 : (byte)0 });
            }
        }

        internal bool AudioOutput()
        {
            if (!_playing || _tape == null)
                return false;

            ulong TStates = this.TStates + _offsetTStates;

            TStates -= _startTStates;

            if (TStates > _tape.Length)
            {
                Stop();
                return false;
            }

            return _tape.GetValue(TStates);
        }
    }
}
