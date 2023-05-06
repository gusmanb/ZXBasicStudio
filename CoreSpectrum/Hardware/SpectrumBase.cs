using CoreSpectrum.Debug;
using CoreSpectrum.Enums;
using CoreSpectrum.Interfaces;
using CoreSpectrum.SupportClasses;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    public abstract class SpectrumBase: IDisposable
    {
        #region Variables

        protected readonly Breakpoint?[] _breakpoints = new Breakpoint?[65536];

        protected readonly Z80Processor _z80;
        protected readonly ISpectrumMemory _memory;
        protected readonly ULABase _ula;
        protected readonly TapePlayer _player;
        protected readonly MachineTimmings _timmings;

        protected volatile bool _pause;
        protected volatile bool _stop = true;
        protected volatile bool _turbo;
        protected volatile bool _renderOnturbo;
        protected volatile int _scanCycles;
        protected double _nextFrame;
        protected Stopwatch _sw = new Stopwatch();

        protected readonly double _ticksPerMilly;
        protected readonly double _ticksPerFrame;
        protected readonly double _millisPerFrame;
        protected readonly double _framesPerSecond;
        
        protected readonly List<ISynchronizedExecution> _syncExecs = new List<ISynchronizedExecution>();

        protected bool _threadAlive;

        private object locker = new object();

        #endregion

        #region Events

        public event EventHandler<EventArgs>? FrameRendered;
        public event EventHandler<BreakPointEventArgs>? BreakpointHit;
        public abstract event EventHandler? ProgramReady;

        #endregion

        #region Properties

        public bool Running { get { return !_stop; } }
        public bool Paused { get { return _pause; } }
        public bool TurboEnabled { get { return _turbo; } }
        public Z80Processor Z80 { get { return _z80; } }
        public ISpectrumMemory Memory { get { return _memory; } }
        public ULABase ULA { get { return _ula; } }
        public TapePlayer DataCorder { get { return _player; } }
        public MachineTimmings Timmings { get { return _timmings; } }

        #endregion

        protected SpectrumBase(byte[][] RomSet)
        {

            var hardware = GetHardware(RomSet);

            _timmings = hardware.Timmings;
            _memory = hardware.Memory;
            _ula = hardware.ULA;
            _player = new TapePlayer();
            _player.AudioChanged += TapePlayer_AudioChanged;

            _z80 = new Z80Processor();
            _z80.Memory = _memory;
            _z80.PortsSpace = _ula;
            _z80.RegisterInterruptSource(_ula);
            _z80.RegisterTStatesTarget(_player);
            _z80.RegisterTStatesTarget(_ula);

            _z80.BeforeInstructionFetch += z80_BeforeInstructionFetch;
            _z80.InstructionWaitStates += _z80_InstructionWaitStates;
            

            //Compute timmings
            _ticksPerMilly = Stopwatch.Frequency / 1000.0;
            _framesPerSecond = (double)_timmings.CpuClock / (double)(_timmings.TStatesPerScan * _timmings.ScansPerFrame);
            _millisPerFrame = (1.0 / _framesPerSecond) * 1000.0;
            _ticksPerFrame = _ticksPerMilly * _millisPerFrame;

        }

        #region Event handlers

        private void _z80_InstructionWaitStates(object? sender, Main.EventArgs.InstructionWaitStatesEventArgs e)
        {
            e.WaitStates = _ula.ContentionSource.GetContentionStates(e.InitialState, e.ExecutionStates, e.OpCode, e.MemoryAccesses, e.PortAccesses, _memory);
        }

        private void TapePlayer_AudioChanged(object? sender, AudioEventArgs e)
        {
            _ula.Ear = e.AudioLevel;
        }

        #endregion

        #region Machine initialization

        protected abstract MachineHardware GetHardware(byte[][] RomSet);

        #endregion

        #region Machine execution
        protected virtual void SpectrumCycle(object? State)
        {
            _threadAlive = true;
            _sw.Restart();

            long start = _sw.ElapsedTicks;
            _nextFrame = start + _ticksPerFrame;

            _scanCycles = 0;

            while (!_stop)
            {

                if (_pause)
                {
                    while (_pause && !_stop)
                        Thread.Sleep(1);

                    if (_stop)
                    {
                        _threadAlive = false;
                        return;
                    }

                    _nextFrame = _sw.ElapsedTicks;
                }

                lock(locker)
                    NextInstruction();
            }

            _sw.Stop();
            _threadAlive = false;
        }

        protected virtual void NextInstruction()
        {
            int instCycles = _z80.ExecuteNextInstruction();

            if (instCycles == 0)
            {
                Pause();
                return;
            }

            _scanCycles += instCycles;

            if (_scanCycles < _timmings.TStatesPerScan)
                return;

            _scanCycles -= _timmings.TStatesPerScan;

            _ula.ScanLine(_memory.GetVideoMemory(), _timmings.FirstScan, _timmings.ScansPerFrame);

            if (_ula.NewFrame && (!_turbo || _renderOnturbo))
            {
                if (FrameRendered != null)
                    FrameRendered(this, EventArgs.Empty);

                if (!_turbo)
                {
                    while (_nextFrame - _sw.ElapsedTicks > _ticksPerMilly)
                        Thread.Sleep(1);

                    while (_sw.ElapsedTicks < _nextFrame);
                    
                    _nextFrame += _ticksPerFrame;
                }
            }
        }

        #endregion

        #region Machine control

        public virtual void Start(bool backgroundThread = false)
        {
            lock (locker)
            {
                _pause = false;
                _stop = false;
                _turbo = false;
                _scanCycles = 0;
                _nextFrame = 0;
                _sw.Stop();

                _memory.ClearRAM();
                _ula.Reset();
                _z80.Restart();


                if (!backgroundThread)
                {
                    Thread th = new Thread(SpectrumCycle);
                    th.Start();
                }
                else
                    ThreadPool.QueueUserWorkItem(SpectrumCycle);

                foreach (var exec in _syncExecs)
                    exec.Start();
            }
        }

        public virtual void Stop()
        {
            lock (locker)
            {
                _stop = true;

                foreach (var exec in _syncExecs)
                    exec.Stop();

                while (_threadAlive)
                    Thread.Sleep(0);
            }
        }

        public virtual void Pause()
        {
            lock (locker)
            {

                if (_stop)
                    return;

                _pause = true;

                foreach (var exec in _syncExecs)
                    exec.Pause();
            }
        }

        public virtual void Resume()
        {
            lock (locker)
            {

                if (_stop)
                    return;

                _ula.ResetAudio();

                _pause = false;

                foreach (var exec in _syncExecs)
                    exec.Resume();
            }
        }

        public virtual void Reset()
        {
            lock (locker)
            {
                System.Diagnostics.Debug.WriteLine("Reset");

                _z80.Reset();

                foreach (var exec in _syncExecs)
                    exec.Reset();
            }
        }

        public virtual void Turbo(bool Enable, bool RenderOnTurbo = false)
        {
            lock (locker)
            {
                if (Enable)
                {
                    _turbo = true;
                    _renderOnturbo = RenderOnTurbo;
                    _ula.Turbo = true;
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
                    _ula.Turbo = false;

                    if (!paused && !_stop)
                        Resume();
                }

                foreach (var exec in _syncExecs)
                    exec.Turbo(Enable);
            }
        }

        public virtual void Step()
        {
            lock (locker)
            {
                NextInstruction();

                foreach (var exec in _syncExecs)
                    exec.Step();
            }
        }

        public abstract bool InjectProgram(ProgramImage Image);

        #endregion

        #region Synchronized execution

        public void RegisterSynchronized(ISynchronizedExecution Exec)
        {
            if(!_syncExecs.Contains(Exec))
                _syncExecs.Add(Exec);
        }

        public void ClearSynchronized()
        {
            _syncExecs.Clear();
        }

        #endregion

        #region Keyboard handling

        public virtual void PressKey(SpectrumKeys Key)
        {
            _ula.PressKey(Key);
        }

        public virtual void ReleaseKey(SpectrumKeys Key)
        {
            _ula.ReleaseKey(Key);
        }

        #endregion

        #region Breakpoint handling
        protected virtual void z80_BeforeInstructionFetch(object? sender, Konamiman.Z80dotNet.BeforeInstructionFetchEventArgs e)
        {
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

        public virtual void AddBreakpoint(Breakpoint BreakPoint)
        {
            _breakpoints[BreakPoint.Address] = BreakPoint;
        }

        public virtual void AddBreakpoints(IEnumerable<Breakpoint> BreakPoints)
        {
            foreach (var Breakpoint in BreakPoints)
                _breakpoints[Breakpoint.Address] = Breakpoint;
        }

        public virtual void RemoveBreakpoint(ushort Address)
        {
            _breakpoints[Address] = null;
        }

        public virtual void ClearBreakpoints()
        {
            Array.Fill(_breakpoints, null);
        }

        #endregion

        #region Machine initialization objects
        protected class MachineHardware
        {
            public required MachineTimmings Timmings { get; set; }
            public required ULABase ULA { get; set; }
            public required ISpectrumMemory Memory { get; set; }
        
        }
        #endregion

        protected abstract void Disposal();

        public virtual void Dispose()
        {
            Stop();
            FrameRendered = null;
            BreakpointHit = null;
            _syncExecs.Clear();
            _sw.Stop();
            _z80.UnregisterAllInterruptSources();
            _z80.UnregisterAllTStatesTargets();
            ClearSynchronized();
            Disposal();
        }
    }

    #region Machine information
    public struct MachineTimmings
    {
        public required int TStatesPerScan { get; set; }
        public required int ScansPerFrame { get; set; }
        public required byte FirstScan { get; set; }
        public required int CpuClock { get; set; }
        public required int IrqCycles { get; set; }
    }
    #endregion

    #region Event args
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
    #endregion
}
