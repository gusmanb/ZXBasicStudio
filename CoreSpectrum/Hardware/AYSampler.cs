using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CoreSpectrum.Hardware.Ay8912;

namespace CoreSpectrum.Hardware
{
    public class AYSampler
    {
        int SAMPLE_RATE;
        int CPU_CLOCK;
        double SAMPLE_T_STATES;
        const int BUFFER_SIZE = 10000;
        private ulong cTicks;
        double rest = 0;
        private volatile int readPos = 0;
        private volatile int writePos = 0;
        ulong nextChange = 0;

        ayemu_ay_t _ay;
        AYSample[] _samples = new AYSample[BUFFER_SIZE];

        internal AYSampler(int SampleRate = 44100, int CpuClock = 3546900)
        {
            _ay = new ayemu_ay_t();
            Ay8912.ayemu_init(_ay);
            Ay8912.ayemu_set_sound_format(_ay, SampleRate, 1, 16);
            Ay8912.ayemu_set_stereo(_ay, ayemu_stereo_t.AYEMU_MONO, null);
            Ay8912.ayemu_set_chip_type(_ay, ayemu_chip_t.AYEMU_AY, null);
            Ay8912.ayemu_reset(_ay);
            SAMPLE_RATE = SampleRate;
            CPU_CLOCK = CpuClock;
            SAMPLE_T_STATES = CPU_CLOCK / (double)SAMPLE_RATE;
        }

        public void AddSample(ulong TStates, byte Register, byte Value)
        {
            _samples[writePos].TStates = TStates;
            _samples[writePos].Register = Register;
            _samples[writePos].Value = Value;
            writePos++;

            if (writePos >= _samples.Length)
                writePos = 0;
        }

        //AY samples are added to the ULA samples
        public int GetSamples(ulong TStates, float[] Buffer)
        {
            int i = 0;

            while (cTicks < TStates && i < Buffer.Length)
                Buffer[i++] += NextSample();

            return i;

        }

        public void ResetSampler(ulong TStates, bool ResetChip)
        {
            if (ResetChip)
                ayemu_reset(_ay);
            else
            {
                while (readPos != writePos)
                {
                    ayemu_set_reg(_ay, _samples[readPos].Register, _samples[readPos].Value);
                    readPos++;
                    if(readPos >= _samples.Length)
                        readPos = 0;
                }
            }

            readPos = 0;
            writePos = 0;
            cTicks = TStates;
            rest = 0;
            nextChange = 0;
        }

        private float NextSample()
        {
            ulong ticksThisSample = (ulong)SAMPLE_T_STATES;

            rest += SAMPLE_T_STATES - ticksThisSample;

            if (rest >= 1)
            {
                rest -= 1;
                ticksThisSample++;
            }

            ulong targetTicks = cTicks + ticksThisSample;

            while (readPos != writePos && nextChange < targetTicks)
            {
                ayemu_set_reg(_ay, _samples[readPos].Register, _samples[readPos].Value);

                readPos++;
                
                if (readPos >= BUFFER_SIZE)
                    readPos = 0;

                if (readPos != writePos)
                    nextChange = _samples[readPos].TStates;
                else
                    nextChange = 0;
            }

            cTicks = targetTicks;
            return ayemu_gen_sample(_ay);
        }

        struct AYSample
        {
            public byte Register;
            public byte Value;
            public ulong TStates;
        }
    }
}
