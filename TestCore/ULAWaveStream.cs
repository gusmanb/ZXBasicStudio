using CoreSpectrum.Hardware;
using CoreSpectrum.Interfaces;
using MicroLibrary;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCore
{
    public class ULAWaveStream : WaveStream, IAudioSampler
    {

        const int SAMPLE_RATE = 70000;
        const int CPU_CLOCK = 3500000;
        const long SAMPLE_T_STATES = CPU_CLOCK / SAMPLE_RATE;

        private static readonly WaveFormat audioOutWaveFormat = new WaveFormat(SAMPLE_RATE, 8, 1);
        private static readonly byte[] audioOutSpkLevels = { 128, 144, 160, 176 };

        List<byte> dbuf = new List<byte>();

        long samples = 0;
        long nextChange = -1;
        long latency = 0;
        byte nextValue = 0;
        byte currentValue = 0;
        private volatile ulong[] sampleBuffer = new ulong[10000];
        private volatile int readPos = 0;
        private volatile int writePos = 0;
        private readonly WaveOutEvent waveOut;
        public override WaveFormat WaveFormat
        {
            get
            {
                return audioOutWaveFormat;
            }
        }

        public override long Length
        {
            get
            {
                return long.MaxValue;
            }
        }

        public override long Position { get => samples; set => throw new NotImplementedException(); }

        public ULAWaveStream()
        {
            waveOut = new WaveOutEvent { NumberOfBuffers = 4, DesiredLatency = 75 };
            waveOut.Init(this);
            waveOut.Play();
        }

        public void AddSample(ulong sample)
        {
            sampleBuffer[writePos++] = sample;

            if (writePos >= sampleBuffer.Length)
                writePos = 0;

        }

        private void NextSample()
        {
            samples++;
            long states = samples * SAMPLE_T_STATES;

            if (nextChange == -1 || states >= nextChange)
            {
                nextChange = -1;
                currentValue = nextValue;

                ulong next;

                while (nextChange <= states && readPos != writePos)
                {
                    next = sampleBuffer[readPos++];

                    if (readPos >= sampleBuffer.Length)
                        readPos = 0;

                    nextChange = (long)(next >> 2) + latency;
                    nextValue = audioOutSpkLevels[next & 2];
                }
            }
        }

        bool first = false;
        bool playing = false;

        public override int Read(byte[] buffer, int offset, int count)
        {

            if (!playing)
            {
                Array.Fill<byte>(buffer, 128, offset, count);
                return count;
            }

            for (int i = 0; i < count; i++)
            {
                NextSample();
                buffer[i] = currentValue;
            }

            return count;
        }

        public void Play()
        {
            samples = ((long)(0 / SAMPLE_T_STATES)) - 3500;
            playing = true;
        }

        public void Resume(ulong TStates)
        {
            samples = ((long)(TStates / SAMPLE_T_STATES)) - 3500;
            playing = true;
        }

        public void Pause()
        {
            playing = false;
        }

        public void Stop()
        {
            playing = false;
        }
    }
}
