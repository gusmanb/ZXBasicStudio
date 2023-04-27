using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
        ayemu_ay_t _ay;

        byte _activeAyReg = 0;
        bool _disableMapping = false;

        public ULA128k(int CpuClock, int AudioSamplingFrequency, IVideoRenderer Renderer, Memory128k Memory) : base(CpuClock, AudioSamplingFrequency, Renderer) 
        { 
            _mem = Memory;
            _ay = new ayemu_ay_t();
            Ay8912.ayemu_init(_ay);
            Ay8912.ayemu_set_sound_format(_ay, 44100, 1, 16);
            Ay8912.ayemu_set_stereo(_ay, ayemu_stereo_t.AYEMU_MONO, null);
            Ay8912.ayemu_set_chip_type(_ay, ayemu_chip_t.AYEMU_AY, null);
            Ay8912.ayemu_reset(_ay);
        }
        public override byte this[byte portLo, byte portHi]
        {
            get
            {
                if ((portLo & 0x01) == 0)
                {

                    byte value = ReadKeyboard(portHi);

                    return (byte)(value | (AudioOutput > 0 ? 0x40 : 0));
                }
                else if ((portHi & 0xC0) == 0xC0 && (portLo & 0x02) == 0)
                {
                    byte regValue = Ay8912.ayemu_get_reg(_ay, _activeAyReg);
                    return regValue;
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
                    CreateAYSample(value);
                }
                else if ((portHi & 0x80) == 0 && (portLo & 0x02) == 0)
                {

                    if (_disableMapping)
                        return;

                    if ((value & DISABLE_MAP_MASK) == DISABLE_MAP_MASK)
                    {
                        _disableMapping = true;
                        _mem.Map.SetActiveBank(0);
                        _mem.Map.SetActiveScreen(0);
                        //_mem.Map
                    }
                }
            }
        }

        void CreateAYSample(byte Value)
        {
            AYSample sample = new AYSample();
            sample.Register = _activeAyReg;
            sample.Value = Value;
            sample.TStates = TStates;
        }

        struct AYSample
        {
            public byte Register;
            public byte Value;
            public ulong TStates;
        }
    }
}
