using Bufdio;
using Bufdio.Engines;
using CoreSpectrum.Hardware;
using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZXBasicStudio.Controls;

namespace ZXBasicStudio.Classes
{
    public class BufdioAudioSampler : IAudioSampler, IDisposable
    {
        const int SAMPLE_RATE = 44100;
        const int CPU_CLOCK = 3500000;
        const double LATENCY = 0.1f; //In s
        const double SAMPLE_T_STATES = (double)CPU_CLOCK / (double)SAMPLE_RATE;
        const float AMPLITUDE = 1;
        const int timerTick = 10;
        const int BUFFER_SIZE = 10000;
        Timer? audioTimer;

        IAudioEngine? engine;

        private static readonly float[] audioOutSpkLevels = { 0, AMPLITUDE / 3, AMPLITUDE / 1.5f, AMPLITUDE };

        ulong nextChange = 0;
        ulong latency = 0;
        float nextValue = 0;
        float currentValue = 0;
        bool _pause = false;

        private volatile ulong[] statesBuffer = new ulong[BUFFER_SIZE];
        private volatile byte[] valuesBuffer = new byte[BUFFER_SIZE];
        private volatile int readPos = 0;
        private volatile int writePos = 0;
        float[] outBuffer = new float[SAMPLE_RATE * 10];
        private ulong cTicks;
        double rest = 0;

        object locker = new object();
        //public Machine? Machine { get; set; }
        public MachineBase? Machine { get; set; }

        public BufdioAudioSampler() 
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager("ZXBasicStudio.Resources.PortAudio", typeof(ZXEmulator).Assembly);

            string libName;
            byte[] lib = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                libName = resources.GetString("linux_lib");
                lib = resources.GetObject("lib_linux") as byte[];
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                libName = resources.GetString("osx_lib");
                lib = resources.GetObject("lib_osx") as byte[];
            }
            else
            {
                libName = resources.GetString("win_lib");
                lib = resources.GetObject("lib_win") as byte[];
            }

            if(!File.Exists(libName))
                File.WriteAllBytes(libName, lib);

            string file = Path.GetFullPath(libName);
            Console.WriteLine($"Lib audio: {file}");

            try
            {
                BufdioLib.InitializePortAudio(file);
            }
            catch { ZXOptions.Current.AudioDisabled = true; }
        }

        private void TimerCallback(object? args)
        {
            lock (locker)
            {
                if (ZXOptions.Current.AudioDisabled || _pause || Machine == null)
                    return;

                int i = 0;

                while (cTicks < Machine.Z80.TStatesElapsedSinceStart && i < outBuffer.Length)
                    outBuffer[i++] = NextSample();

                if (i != 0)
                {
                    Span<float> samples = new Span<float>(outBuffer, 0, i);
                    engine?.Send(samples);
                }

                audioTimer.Change(timerTick, Timeout.Infinite);
            }
        }

        public void AddSample(ulong TStates, byte Value)
        {
            if (ZXOptions.Current.AudioDisabled)
                return;

            statesBuffer[writePos] = TStates;
            valuesBuffer[writePos++] = Value;

            if (writePos >= BUFFER_SIZE)
                writePos = 0;

        }

        private float NextSample()
        {
            float sum = 0;
            int ticksThisSample = (int)SAMPLE_T_STATES;

            rest += SAMPLE_T_STATES - ticksThisSample;

            if (rest >= 1)
            {
                rest -= 1;
                ticksThisSample++;
            }

            for (int loop = 0; loop < ticksThisSample; loop++)
            {
                cTicks++;
                NextTickSample();
                sum += currentValue;
            }

            return sum / ticksThisSample;
        }

        private void NextTickSample()
        {
            if (ZXOptions.Current.AudioDisabled)
                return;

            if (nextChange == 0 || cTicks >= nextChange)
            {
                nextChange = 0;
                currentValue = nextValue;

                while (nextChange <= cTicks && readPos != writePos)
                {
                    nextChange = statesBuffer[readPos] + latency;
                    nextValue = audioOutSpkLevels[valuesBuffer[readPos++]];

                    if (readPos >= BUFFER_SIZE)
                        readPos = 0;
                }
            }
        }

        public void Pause()
        {
            lock (locker)
            {
                if (ZXOptions.Current.AudioDisabled)
                    return;

                if (audioTimer == null || engine == null)
                    return;

                if (engine != null)
                {
                    engine.Dispose();
                    engine = null;
                }
                _pause = true;
                audioTimer.Change(Timeout.Infinite, Timeout.Infinite);
                readPos = 0;
                writePos = 0;
                cTicks = 0;
            }
        }

        public void Resume(ulong TStates)
        {
            lock (locker)
            {
                if (ZXOptions.Current.AudioDisabled)
                    return;

                if (audioTimer == null || engine != null)
                    return;

                var options = new AudioEngineOptions(1, SAMPLE_RATE, LATENCY);
                engine = new PortAudioEngine(options);
                cTicks = TStates;
                _pause = false;
                audioTimer.Change((int)(LATENCY * 1000), Timeout.Infinite);
            }
        }

        public void Play()
        {
            lock (locker)
            {
                if (ZXOptions.Current.AudioDisabled)
                    return;

                Stop();

                var options = new AudioEngineOptions(1, SAMPLE_RATE, LATENCY);
                engine = new PortAudioEngine(options);
                audioTimer = new Timer(TimerCallback);
                _pause = false;
                audioTimer.Change((int)(LATENCY * 1000), Timeout.Infinite);
            }
        }

        public void Stop() 
        {
            lock (locker)
            {
                if (audioTimer != null)
                {
                    audioTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    audioTimer.Dispose();
                    audioTimer = null;
                }

                _pause = true;
                readPos = 0;
                writePos = 0;
                cTicks = 0;

                if (engine != null)
                {
                    engine.Dispose();
                    engine = null;
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
