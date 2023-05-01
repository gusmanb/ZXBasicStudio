using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public class ULASampler
    {
        int SAMPLE_RATE;
        int CPU_CLOCK;
        double SAMPLE_T_STATES;
        float AMPLITUDE = 1;
        const int BUFFER_SIZE = 10000;

        private readonly float[] audioOutSpkLevels;

        ulong nextChange = 0;
        float nextValue = 0;
        float currentValue = 0;

        private volatile ulong[] statesBuffer = new ulong[BUFFER_SIZE];
        private volatile byte[] valuesBuffer = new byte[BUFFER_SIZE];
        private volatile int readPos = 0;
        private volatile int writePos = 0;
        private ulong cTicks;
        double rest = 0;

        internal ULASampler(int SampleRate = 44100, int CpuClock = 3500000, float Amplitude = 1)
        {
            SAMPLE_RATE = SampleRate;
            CPU_CLOCK = CpuClock;
            SAMPLE_T_STATES = CPU_CLOCK / (double)SAMPLE_RATE;
            audioOutSpkLevels = new float[] { 0, AMPLITUDE / 3, AMPLITUDE / 1.5f, AMPLITUDE, AMPLITUDE, AMPLITUDE, AMPLITUDE };
        }

        public int GetSamples(ulong TStates, float[] Buffer)
        {
            int i = 0;

            while (cTicks < TStates && i < Buffer.Length)
                Buffer[i++] = NextSample();

            return i;

        }

        public void AddSample(ulong TStates, byte Value)
        {
            statesBuffer[writePos] = TStates;
            valuesBuffer[writePos++] = Value;

            if (writePos >= BUFFER_SIZE)
                writePos = 0;
        }

        public void ResetSampler(ulong TStates)
        {
            readPos = 0;
            writePos = 0;
            cTicks = TStates;
            rest = 0;
            nextChange = 0;
            nextValue = 0;
            currentValue = 0;
        }

        private float NextSample()
        {
            float sum = 0;
            ulong ticksThisSample = (ulong)SAMPLE_T_STATES;

            rest += SAMPLE_T_STATES - ticksThisSample;

            if (rest >= 1)
            {
                rest -= 1;
                ticksThisSample++;
            }

            ulong consumedTicks = 0;
            ulong ticksLeft = ticksThisSample;

            while (ticksLeft > 0)
            {
                consumedTicks = NextTickSample(ticksLeft);

                if (consumedTicks == 0)
                    continue;

                ticksLeft -= consumedTicks;
                cTicks += consumedTicks;
                sum += currentValue * consumedTicks;
            }

            return sum / ticksThisSample;
        }

        private ulong NextTickSample(ulong MaxTicks)
        {

            if (nextChange == 0 || cTicks >= nextChange)
            {
                nextChange = 0;
                currentValue = nextValue;

                while (nextChange <= cTicks && readPos != writePos)
                {
                    nextChange = statesBuffer[readPos];
                    nextValue = audioOutSpkLevels[valuesBuffer[readPos++]];

                    if (readPos >= BUFFER_SIZE)
                        readPos = 0;
                }
            }

            if (nextChange == 0)
                return MaxTicks;

            if (nextChange < cTicks + MaxTicks)
                return nextChange - cTicks;
            else
                return MaxTicks;
        }
    }
}
