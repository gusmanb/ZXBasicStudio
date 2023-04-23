using CoreSpectrum.SupportClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class TapePlayer
    {
        Tape? _tape;
        ulong _startTStates;
        ulong _offsetTStates;
        ulong _pauseTStates;
        bool _playing = false;
        bool _paused = false;
        Machine _machine;

        TimeSpan? _tapeLength;
        public TimeSpan? TapeLength { get { return _tapeLength; } }
        public TimeSpan? Position 
        { 
            get 
            {
                if (_tape == null)
                    return null;

                if(!_playing && !_paused)
                    return new TimeSpan(0, 0, (int)(_offsetTStates / 3500000.0));

                if (_paused)
                    return new TimeSpan(0, 0, (int)(((_pauseTStates - _startTStates) + _offsetTStates) / 3500000.0));

                return new TimeSpan(0, 0, (int)(((_machine._z80.TStatesElapsedSinceStart - _startTStates) + _offsetTStates) / 3500000.0));
            } 
        }

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

                return _tape.GetBlockIndex((_machine._z80.TStatesElapsedSinceStart - _startTStates) + _offsetTStates);
            }
        }

        public Tape? LoadedTape { get { return _tape; } }
        public bool Playing { get { return _playing; } }
        public bool Paused { get { return _paused; } }
        internal TapePlayer(Machine machine) 
        {
            _machine = machine;
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

            _startTStates = _machine._z80.TStatesElapsedSinceStart;
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

            _startTStates = _pauseTStates = _machine._z80.TStatesElapsedSinceStart;
            _offsetTStates = blocks[Block].Start;

            return true;
        }
        public bool Pause()
        {
            if (!_playing || _paused)
                return false;

            _pauseTStates = _machine._z80.TStatesElapsedSinceStart;
            _playing = false;
            _paused = true;

            return true;
        }
        public bool Resume()
        {
            if (_tape == null || _playing || !_paused)
                return false;

            _startTStates += _machine._z80.TStatesElapsedSinceStart - _pauseTStates;
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
            return true;
        }
        internal bool AudioOutput()
        {
            if (!_playing || _tape == null)
                return false;

            ulong TStates = _machine._z80.TStatesElapsedSinceStart + _offsetTStates;

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
