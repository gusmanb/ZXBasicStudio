using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.SupportClasses
{
    public class TapeStream
    {
        const uint PILOT_T_STATES = 2168;
        const uint SYNC1_T_STATES = 667;
        const uint SYNC2_T_STATES = 735;
        const uint ZERO_T_STATES = 855;
        const uint ONE_T_STATES = 1710;
        const uint HEADER_PILOT_COUNT = 8063;
        const uint DATA_PILOT_COUNT = 3223;
        const uint TSTATES_PER_MILLI = 3500;
        List<TapeStreamPulse> _pulses = new List<TapeStreamPulse>();
        bool _currentSense = false;
        bool _isHeader = false;

        int _currentPulseIndex = -1;
        TapeStreamPulse _currentPulse;

        public bool LastState { get { return _pulses.LastOrDefault()?.Value ?? false; } }

        public bool IsHeader { get { return _isHeader; } }
        public TapeStream(bool InitialState) 
        {
            _currentSense = InitialState;
        }
        public TapeStream(bool InitialState, bool Header, byte[] Data, int Silence)
        {
            _currentSense = InitialState;
            AddPilot(Header);
            AddData(Data);
            AddSilence(Silence);
        }
        public TapeStream(bool InitialState, TapeStreamTimmings Timmings, byte[] Data, int Silence)
        {
            _currentSense = InitialState;
            AddPilot(Timmings);
            AddData(Timmings, Data);
            AddSilence(Silence);
        }
        public TapeStream(bool InitialState, TapeStreamTimmings Timmings, byte[] Data, int Silence, byte LastByteBits)
        {
            _currentSense = InitialState;
            AddPilot(Timmings);
            AddData(Timmings, Data, LastByteBits);
            AddSilence(Silence);
        }
        public TapeStream(bool InitialState, ulong Count)
        {
            _currentSense = InitialState;
            AddCalibration(Count);
        }
        public void AddPilot(bool Header)
        {
            _isHeader = Header;
            
            AddPulses(PILOT_T_STATES, Header ? HEADER_PILOT_COUNT : DATA_PILOT_COUNT);
            AddPulse(SYNC1_T_STATES);
            AddPulse(SYNC2_T_STATES);
        }

        public void AddCalibration(ulong Count)
        {
            _isHeader = false;

            AddPulses(PILOT_T_STATES, Count);
        }

        public void AddData(byte[] Data)
        {
            for (int buc = 0; buc < Data.Length; buc++)
                AddByte(Data[buc]);
        }
        public void AddByte(byte Data)
        {
            for (int buc = 7; buc > -1; buc--)
            {
                bool currentBit = (Data & (1 << buc)) != 0;
                if (currentBit)
                    AddPulses(ONE_T_STATES, 2);
                else
                    AddPulses(ZERO_T_STATES, 2);

            }
        }
        public void AddData(byte[] Data, ulong ZeroStates, ulong OneStates)
        {
            for (int buc = 0; buc < Data.Length; buc++)
                AddByte(Data[buc], ZeroStates, OneStates);
        }
        public void AddByte(byte Data, ulong ZeroStates, ulong OneStates)
        {
            for (int buc = 7; buc > -1; buc--)
            {
                bool currentBit = (Data & (1 << buc)) != 0;
                if (currentBit)
                    AddPulses(OneStates, 2);
                else
                    AddPulses(ZeroStates, 2);

            }
        }
        public void AddData(byte[] Data, ulong ZeroStates, ulong OneStates, byte LastByteBits)
        {
            for (int buc = 0; buc < Data.Length - 1; buc++)
                AddByte(Data[buc], ZeroStates, OneStates);

            AddByte(Data[Data.Length - 1], ZeroStates, OneStates, LastByteBits);
        }
        public void AddByte(byte Data, ulong ZeroStates, ulong OneStates, byte BitCount)
        {
            for (int buc = 7; buc > (8 - BitCount) - 1; buc--)
            {
                bool currentBit = (Data & (1 << buc)) != 0;
                if (currentBit)
                    AddPulses(OneStates, 2);
                else
                    AddPulses(ZeroStates, 2);

            }
        }
        public void AddPilot(TapeStreamTimmings Timmings)
        {
            AddPulses(Timmings.PulsePilotLength, Timmings.PilotPulseCount);
            AddPulse(Timmings.PulseSync1Length);
            AddPulse(Timmings.PulseSync2Length);
        }
        public void AddData(TapeStreamTimmings Timmings, byte[] Data)
        {
            for (int buc = 0; buc < Data.Length; buc++)
                AddByte(Timmings, Data[buc]);
        }
        public void AddByte(TapeStreamTimmings Timmings, byte Data)
        {
            for (int buc = 7; buc > -1; buc--)
            {
                bool currentBit = (Data & (1 << buc)) != 0;
                if (currentBit)
                    AddPulses(Timmings.PulseOneLength, 2);
                else
                    AddPulses(Timmings.PulseZeroLength, 2);
            }
        }
        public void AddData(TapeStreamTimmings Timmings, byte[] Data, byte LastByteBits)
        {
            for (int buc = 0; buc < Data.Length - 1; buc++)
                AddByte(Timmings, Data[buc]);

            AddByte(Timmings, Data[Data.Length - 1], LastByteBits);
        }
        public void AddByte(TapeStreamTimmings Timmings, byte Data, byte BitCount)
        {
            for (int buc = 7; buc > (8 - BitCount) - 1; buc--)
            {
                bool currentBit = (Data & (1 << buc)) != 0;
                if (currentBit)
                    AddPulses(Timmings.PulseOneLength, 2);
                else
                    AddPulses(Timmings.PulseZeroLength, 2);
            }
        }
        public void AddSilence(int milliseconds)
        {
            var lastPulse = _pulses.LastOrDefault();

            if (lastPulse != null)
            {
                TapeStreamPulse pulsePre = new TapeStreamPulse
                {
                    Value = !lastPulse.Value,
                    Length = TSTATES_PER_MILLI,
                    Start = lastPulse.Start + lastPulse.Length
                };
                _pulses.Add(pulsePre);
                lastPulse = pulsePre;
            }

            _currentSense = false;

            TapeStreamPulse pulse = new TapeStreamPulse
            {
                Value = _currentSense,
                Length = (ulong)(milliseconds * TSTATES_PER_MILLI),
                Start = lastPulse == null ? 0 : lastPulse.Start + lastPulse.Length
            };

            _pulses.Add(pulse);
        }
        public void AddPulse(ulong TStates)
        {
            _currentSense = !_currentSense;

            var lastPulse = _pulses.LastOrDefault();

            TapeStreamPulse pulse = new TapeStreamPulse
            {
                Value = _currentSense,
                Length = TStates,
                Start = lastPulse == null ? 0 : lastPulse.Start + lastPulse.Length
            };
            _pulses.Add(pulse);
         }
        public void AddPulses(ulong TStates, ulong Count)
        {
            var lastPulse = _pulses.LastOrDefault();

            for (ulong buc = 0; buc < Count; buc++)
            {
                _currentSense = !_currentSense;

                TapeStreamPulse pulse = new TapeStreamPulse
                {
                    Value = _currentSense,
                    Length = TStates,
                    Start = lastPulse == null ? 0 : lastPulse.Start + lastPulse.Length
                };
                _pulses.Add(pulse);
                lastPulse = pulse;
            }
        }

        public void AddPulses(ulong Count, ushort[] PulseLengths)
        {
            var lastPulse = _pulses.LastOrDefault();

            for (ulong buc = 0; buc < Count; buc++)
            {
                _currentSense = !_currentSense;

                TapeStreamPulse pulse = new TapeStreamPulse
                {
                    Value = _currentSense,
                    Length = PulseLengths[buc],
                    Start = lastPulse == null ? 0 : lastPulse.Start + lastPulse.Length
                };
                _pulses.Add(pulse);
                lastPulse = pulse;
            }
        }

        public bool GetValue(ulong TStates)
        {
            if (_currentPulse == null)
            {
                _currentPulse = _pulses[0];
                _currentPulseIndex = 0;
            }

            if (TStates < _currentPulse.Start)
            {
                _currentPulse = _pulses.Where(p => p.Start >= TStates && TStates < p.Start + p.Length).FirstOrDefault();

                if (_currentPulse == null)
                    _currentPulseIndex = _pulses.Count;
                else
                    _currentPulseIndex = _pulses.IndexOf(_currentPulse);
            }

            if (_currentPulseIndex >= _pulses.Count)
                return false;

            while (_currentPulse.Start + _currentPulse.Length < TStates)
            {
                _currentPulseIndex++;

                if (_currentPulseIndex >= _pulses.Count)
                    return false;
                
                _currentPulse = _pulses[_currentPulseIndex];
            }

            return _currentPulse == null ? false : _currentPulse.Value;
        }

        public ulong Length 
        {
            get
            {
                if (_pulses.Count == 0)
                    return 0;
                
                var last = _pulses.Last();

                return last.Start + last.Length;
            }
        }
        class TapeStreamPulse
        {
            public bool Value { get; set; }
            public ulong Length { get; set; }
            public ulong Start { get; set; }
        }
    }

    public class TapeStreamTimmings
    {
        public ushort PilotPulseCount { get; set; }
        public ushort PulsePilotLength { get; set; }
        public ushort PulseSync1Length { get; set; }
        public ushort PulseSync2Length { get; set; }
        public ushort PulseZeroLength { get; set; }
        public ushort PulseOneLength { get; set; }
    }
}
