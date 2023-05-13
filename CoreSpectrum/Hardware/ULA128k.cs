using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CoreSpectrum.Hardware.Ay8912;

namespace CoreSpectrum.Hardware
{
    public class ULA128k : ULABase
    {

        const byte PAGE_MASK = 0x07;
        const byte DISPLAY_MASK = 0x08;
        const byte ROM_MASK = 0x10;
        const byte DISABLE_MAP_MASK = 0x20;

        Memory128k _mem;
        AYSampler _aySampler;

        byte _activeAyReg = 0;
        bool _disableMapping = false;

        byte[] _ayRegs = new byte[18];

        //SimpleContentionSource contention = new SimpleContentionSource(true);
        AccurateContentionSource contention = new AccurateContentionSource(true);

        internal override IContentionSource ContentionSource
        {
            get
            {
                return contention;
            }
        }

        public ULA128k(int CpuClock, int AudioSamplingFrequency, Memory128k Memory) : base(CpuClock, AudioSamplingFrequency) 
        { 
            _mem = Memory;
            _aySampler = new AYSampler(AudioSamplingFrequency, CpuClock);
        }
        public override byte this[byte portLo, byte portHi]
        {
            get
            {
                if ((portLo & 0x01) == 0)
                {

                    byte value = ReadKeyboard(portHi);

                    return (byte)(value | (_ear > 0 ? 0x40 : 0));
                }
                else if ((portHi & 0xC0) == 0xC0 && (portLo & 0x02) == 0)
                {
                    if (_activeAyReg > 17)
                        return 0xFF;

                    return _ayRegs[_activeAyReg];
                }
                else
                    return 0xFF;
            }

            set
            {
                if ((portLo & 0x01) == 0)
                {

                    Border = (byte)(value & 7);
                    byte newMic = (byte)((value & 0x18) >> 3);

                    Mic = newMic;
                    CreateAudioSample();
                }
                else if ((portHi & 0xC0) == 0xC0 && (portLo & 0x02) == 0)
                {
                    _activeAyReg = value;
                }
                else if ((portHi & 0xC0) == 0x80 && (portLo & 0x02) == 0)
                {
                    if (_activeAyReg > 17)
                        return;

                    _ayRegs[_activeAyReg] = value;
                    _aySampler.AddSample(TStates, _activeAyReg, value);
                }
                else if ((portHi & 0x80) == 0 && (portLo & 0x02) == 0)
                {

                    if (_disableMapping)
                        return;

                    if ((value & DISABLE_MAP_MASK) == DISABLE_MAP_MASK)
                        _disableMapping = true;

                    _mem.Map.SetActiveBank(value & PAGE_MASK);
                    _mem.Map.SetActiveScreen((value & DISPLAY_MASK) >> 3);
                    _mem.Map.SetActiveRom((value & ROM_MASK) >> 4);
                }
            }
        }

        public override void ResetAudio(bool FullReset = false)
        {
            base.ResetAudio(FullReset);
            _aySampler.ResetSampler(TStates, FullReset);
        }

        public override int GetSamples(float[] Buffer)
        {
            var tStates = TStates;

            int ULASamplesGenerated = _sampler.GetSamples(tStates, Buffer);
            int AYSamplesGenerated = _aySampler.GetSamples(tStates, Buffer);

            //if (ULASamplesGenerated != AYSamplesGenerated)
            //    throw new Exception("Audio desync!!!");

            return ULASamplesGenerated;
        }

        public override void Reset()
        {
            _activeAyReg = 0;
            _disableMapping = false;
            _ayRegs = new byte[18];
            base.Reset();
        }
    }
}
