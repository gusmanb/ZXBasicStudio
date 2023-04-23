using CoreSpectrum.Debug;
using CoreSpectrum.Enums;
using CoreSpectrum.Interfaces;
using CoreSpectrum.SupportClasses;
using Konamiman.Z80dotNet;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace CoreSpectrum.Hardware
{
    public class Machine
    {
        private const int CyclesPerScan = 224;

        internal readonly Z80Processor _z80;
        internal readonly Memory _romram;
        internal readonly ULA _ula;
        internal readonly TapePlayer _player;

        private bool _pause;
        private bool _stop = true;
        private bool _turbo;
        private bool _renderOnturbo;

        private readonly double ticksPerMilly;
        private readonly double ticksPerFrame;
        private readonly double millisPerFrame = 19.968;
        private readonly IAudioSampler _sampler;

        private readonly Breakpoint?[] _breakpoints = new Breakpoint?[65536];

        public double TicksPerMilly => ticksPerMilly;
        public double TicksPerFrame => ticksPerFrame;
        public double MillisPerFrame => millisPerFrame;

        public event EventHandler<SpectrumFrameArgs>? FrameRendered;

        public event EventHandler<BreakPointEventArgs>? BreakpointHit;

        public byte Mic { get { return _ula.Mic; } }
        public bool Ear { get { return _ula.Ear; } }
        public bool Running { get { return !_stop; } }
        public bool Paused { get { return _pause; } }
        public bool TurboEnabled { get { return _turbo; } }
        public Z80Processor Z80 { get { return _z80; } }
        public Memory ROMRAM { get { return _romram; } }
        public ULA ULA { get { return _ula; } }
        public TapePlayer DataCorder { get { return _player; } }

        public Machine(byte[] romContent, IVideoRenderer renderer, IAudioSampler sampler)
        {
            _romram = new Memory(64 * 1024, romContent);
            _ula = new ULA(this, renderer, sampler);
            _z80 = new Z80Processor();
            _z80.Memory = _romram;
            _z80.PortsSpace = _ula;
            _z80.RegisterInterruptSource(_ula);
            _z80.SetMemoryAccessMode(0, 16384, MemoryAccessMode.ReadOnly);
            _player = new TapePlayer(this);
            _sampler = sampler;

            ticksPerMilly = Stopwatch.Frequency / 1000.0;
            ticksPerFrame = ticksPerMilly * millisPerFrame;
            _z80.BeforeInstructionFetch += _z80_BeforeInstructionFetch;
        }

        private void _z80_BeforeInstructionFetch(object? sender, Konamiman.Z80dotNet.BeforeInstructionFetchEventArgs e)
        {

            if (_z80.Registers.PC == 32768)
                _z80.Registers.PC = 32768;

            var bp = _breakpoints[_z80.Registers.PC];
            if (bp == null)
                return;

            if (bp.Executed)
            {
                bp.Executed = false;
                return;
            }

            if (bp.Temporary)
                _breakpoints[_z80.Registers.PC] = null;
            else
                bp.Executed = true;

            if (BreakpointHit != null)
            {
                var args = new BreakPointEventArgs(bp);
                BreakpointHit(this, args);

                if (args.StopExecution)
                    e.ExecutionStopper.Stop(true);
            }

        }

        public void AddBreakpoint(Breakpoint BreakPoint)
        {
            _breakpoints[BreakPoint.Address] = BreakPoint;
        }

        public void AddBreakpoints(IEnumerable<Breakpoint> BreakPoints)
        {
            foreach (var Breakpoint in BreakPoints)
                _breakpoints[Breakpoint.Address] = Breakpoint;
        }

        public void RemoveBreakpoint(ushort Address)
        {
            _breakpoints[Address] = null;
        }

        public void ClearBreakpoints()
        {
            Array.Fill(_breakpoints, null);
        }

        public void Start(bool backgroundThread = false)
        {
            _pause = false;
            _stop = false;
            _z80.Reset();
            _z80.Reset();

            if (!backgroundThread)
            {
                Thread th = new Thread(SpectrumCycle);
                th.Start();
            }
            else
                Task.Run(() => SpectrumCycle(null));

            _sampler.Play();
        }

        public void Stop()
        {
            _stop = true;
            _sampler.Stop();
        }

        public void Pause()
        {
            if (_stop)
                return;

            _pause = true;
            _sampler.Pause();
        }

        public void Resume()
        {
            if (_stop)
                return;

            _pause = false;
            _sampler.Resume(_z80.TStatesElapsedSinceStart);
        }

        public void Reset()
        {
            _z80.Reset();
        }

        public void Turbo(bool Enable, bool RenderOnTurbo = false)
        {
            if (Enable)
            {
                _sampler.Pause();
                _turbo = true;
                _renderOnturbo = RenderOnTurbo;
            }
            else
            {
                bool paused = _pause;

                if (!paused && !_stop)
                {
                    Pause();
                    Thread.Sleep(10);
                }

                _turbo = false;

                if(!paused && !_stop)
                    Resume();
            }
        }

        public void Step()
        {
            NextInstruction();
        }

        public void PressKey(SpectrumKeys Key)
        {
            _ula.PressKey(Key);
        }

        public void ReleaseKey(SpectrumKeys Key)
        {
            _ula.ReleaseKey(Key);
        }

        public void LoadZ80Program(Z80File program)
        {
            _romram.SetContents(program.StartAddress, program.Data, 0x4000, program.EndAddress - program.StartAddress);
            var regs = _z80.Registers;

            regs.AF = (short)program.Header.AF;
            regs.BC = (short)program.Header.BC;
            regs.DE = (short)program.Header.DE;
            regs.HL = (short)program.Header.HL;

            regs.Alternate.AF = (short)program.Header.AFP;
            regs.Alternate.BC = (short)program.Header.BCP;
            regs.Alternate.DE = (short)program.Header.DEP;
            regs.Alternate.HL = (short)program.Header.HLP;

            regs.IX = (short)program.Header.IX;
            regs.IY = (short)program.Header.IY;

            regs.IFF1 = program.Header.IFFStatus;
            regs.I = program.Header.I;
            regs.R = program.Header.R;

            regs.SP = (short)program.Header.SP;
            regs.PC = program.Header.PC;
        }

        int scanCycles;
        double nextFrame;
        Stopwatch sw = new Stopwatch();
        void SpectrumCycle(object? State)
        {
            sw.Restart();
            
            long start = sw.ElapsedTicks;
            nextFrame = start + ticksPerFrame;

            scanCycles = 0;

            while(!_stop)
            {

                if (_pause)
                {
                    while (_pause && !_stop)
                        Thread.Sleep(1);

                    if (_stop)
                        return;

                    nextFrame = sw.ElapsedTicks;
                    _sampler.Resume(_z80.TStatesElapsedSinceStart);
                }

                NextInstruction();
            }

            sw.Stop();
            
        }

        void NextInstruction()
        {
            int instCycles = _z80.ExecuteNextInstruction();

            if (instCycles == 0)
            {
                Pause();
                return;
            }

            scanCycles += instCycles;

            if (scanCycles < CyclesPerScan)
                return;

            scanCycles -= CyclesPerScan;

            _ula.ScanLine();

            if (_ula.NewFrame && (!_turbo || _renderOnturbo))
            {
                if (FrameRendered != null)
                    FrameRendered(this, new SpectrumFrameArgs(_ula._renderer));

                if (!_turbo)
                {
                    while (nextFrame - sw.ElapsedTicks > ticksPerMilly * 2)
                        Thread.Sleep(1);

                    while (sw.ElapsedTicks < nextFrame - ticksPerMilly) ;

                    nextFrame += ticksPerFrame;
                }
            }
        }

        public class SpectrumFrameArgs : EventArgs
        {
            public SpectrumFrameArgs(IVideoRenderer renderer) 
            {
                VideoRenderer = renderer;
            }
            public IVideoRenderer VideoRenderer { get; set; }
        }

        public class SpectrumAudioSampleArgs : EventArgs
        {
            public byte Sample { get; set; }
        }

        public class BreakPointEventArgs : EventArgs
        {
            public BreakPointEventArgs(Breakpoint breakpoint)
            {
                Breakpoint = breakpoint;
            }

            public Breakpoint Breakpoint { get; set; }
            public bool StopExecution { get; set; }
        }
    }
}