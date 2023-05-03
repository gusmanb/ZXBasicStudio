using Avalonia.X11;
using Bufdio;
using Bufdio.Engines;
using CoreSpectrum.Hardware;
using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Controls;
using ZXBasicStudio.Emulator.Controls;
namespace ZXBasicStudio.Emulator.Classes
{
    public class ZXEmulatorAudio : ISynchronizedExecution, IDisposable
    {

        const int TIMER_TICK = 10;
        const int SAMPLE_RATE = 44100;
        const double LATENCY = 0.1f; //In s

        ISpectrumAudio? _audioSource;

        object _locker = new object();
        bool _pause = false;
        bool _turbo = false;
        bool _stop = true;

        float[] _buffer = new float[SAMPLE_RATE * 12];
        Timer? _audioTimer;
        IAudioEngine? _engine;

        public ISpectrumAudio? AudioSource
        {
            get { return _audioSource; }
            set
            {
                lock (_locker)
                    _audioSource = value;
            }
        }

        public ZXEmulatorAudio()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager("ZXBasicStudio.Resources.PortAudio", typeof(ZXEmulator).Assembly);

            string? libName;
            byte[]? lib = null;

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

            if (libName == null || lib == null)
                throw new InvalidProgramException("Missing required resources!");

            if (!File.Exists(libName))
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
            lock (_locker)
            {
                if (ZXOptions.Current.AudioDisabled || _pause || _turbo || _stop || _audioSource == null)
                    return;

                int samples = _audioSource.GetSamples(_buffer);

                if (samples != 0)
                {
                    Span<float> sampleSpan = new Span<float>(_buffer, 0, samples);
                    _engine?.Send(sampleSpan);
                }
                else
                    samples = 10;

                _audioTimer?.Change(TIMER_TICK, Timeout.Infinite);
            }
        }

        public void Pause()
        {
            lock (_locker)
            {
                if (ZXOptions.Current.AudioDisabled)
                    return;

                if (_audioTimer != null)
                    _audioTimer.Change(Timeout.Infinite, Timeout.Infinite);

                if (_engine != null)
                {
                    _engine.Dispose();
                    _engine = null;
                }

                _pause = true;
            }
        }

        public void Reset()
        {

        }

        public void Resume()
        {
            lock (_locker)
            {
                if (ZXOptions.Current.AudioDisabled)
                    return;

                _pause = false;

                if (_audioTimer == null || _engine != null || _turbo || _stop)
                    return;

                var options = new AudioEngineOptions(1, SAMPLE_RATE, LATENCY);
                _engine = new PortAudioEngine(options);
                _audioTimer.Change(TIMER_TICK, Timeout.Infinite);
            }
        }

        public void Start()
        {
            lock (_locker)
            {
                if (ZXOptions.Current.AudioDisabled)
                    return;

                Stop();

                var options = new AudioEngineOptions(1, SAMPLE_RATE, LATENCY);
                _engine = new PortAudioEngine(options);
                _audioTimer = new Timer(TimerCallback);
                _stop = false;
                _audioTimer.Change(TIMER_TICK, Timeout.Infinite);
            }
        }

        public void Step()
        {

        }

        public void Stop()
        {
            lock (_locker)
            {
                if (_audioTimer != null)
                {
                    _audioTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _audioTimer.Dispose();
                    _audioTimer = null;
                }

                _stop = true;

                if (_engine != null)
                {
                    _engine.Dispose();
                    _engine = null;
                }
            }
        }

        public void Turbo(bool Enable)
        {
            if (ZXOptions.Current.AudioDisabled)
                return;

            if (Enable)
            {
                lock (_locker)
                {
                    if (_audioTimer != null)
                        _audioTimer.Change(Timeout.Infinite, Timeout.Infinite);

                    if (_engine != null)
                    {
                        _engine.Dispose();
                        _engine = null;
                    }

                    _turbo = true;
                }
            }
            else
            {
                lock (_locker)
                {
                    _turbo = false;

                    if (_audioTimer == null || _engine != null || _pause || _stop)
                        return;

                    var options = new AudioEngineOptions(1, SAMPLE_RATE, LATENCY);
                    _engine = new PortAudioEngine(options);
                    _audioTimer.Change((int)(LATENCY * 1000), Timeout.Infinite);
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
