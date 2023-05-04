using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
using Avalonia.Threading;
using CoreSpectrum.AudioSamplers;
using CoreSpectrum.Debug;
using CoreSpectrum.Enums;
using CoreSpectrum.Hardware;
using CoreSpectrum.Interfaces;
using CoreSpectrum.Renderers;
using CoreSpectrum.SupportClasses;
using Fizzler;
using Konamiman.Z80dotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Emulator.Classes;

namespace ZXBasicStudio.Emulator.Controls
{
    public partial class ZXEmulator : UserControl
    {
        ZXVideoRenderer renderer;
        ZXEmulatorAudio audio;
        SpectrumBase machine;
        ZXEmulatorKeyboardMapper mapper;

        public bool Running { get { return machine.Running; } }
        public bool Paused { get { return machine.Paused; } }
        public bool Borderless 
        { 
            get { return renderer.Borderless; } 
            set 
            {
                bool pause = Running && !Paused;

                if (pause)
                    Pause();

                Thread.Sleep(100);

                renderer.Borderless = value; 
                emuScr.Borderless = value;

                if (pause)
                    Resume();
            } 
        }
        public bool TurboEnabled 
        { 
            get { return machine.TurboEnabled; } 
            set 
            {
                if (value)
                {
                    emuScr.TurboEnabled = true;
                    machine.Turbo(true, true);
                }
                else
                {
                    machine.Turbo(false, false);
                    emuScr.TurboEnabled = false;
                }
            } 
        }
        public bool DirectMode { get; set; }
        public ZXSpectrumModelDefinition? ModelDefinition { get; private set; }
        public IZ80Registers Registers { get { return machine.Z80.Registers; } }
        public ISpectrumMemory Memory { get { return machine.Memory; } }

        public TapePlayer Datacorder { get { return machine.DataCorder; } }

        public ulong TStates { get { return machine.Z80.TStatesElapsedSinceReset; } }

        public bool EnableKeyMapping { get { return mapper.Active; } set { mapper.Active = value; } }

        #region Events

        public event EventHandler<BreakpointEventArgs>? Breakpoint;
        public event EventHandler? ProgramReady;
        public event EventHandler<ExceptionEventArgs>? ExceptionTrapped;

        #endregion

        public ZXEmulator()
        {
            renderer = new ZXVideoRenderer();
            audio = new ZXEmulatorAudio();
            mapper = new ZXEmulatorKeyboardMapper();
            SetModel(ZXSpectrumModel.Spectrum48k);

            if(machine == null)
                throw new ArgumentException("Unknown Spectrum model!");

            machine.RegisterSynchronized(audio);
            machine.FrameRendered += Machine_FrameRendered;
            machine.BreakpointHit += Machine_BreakpointHit;
            InitializeComponent();
            emuKeyb.KeyPressed += EmuKeyb_KeyPressed;
            btnKeyb.Click += BtnKeyb_Click;

        }

        public void SetModel(ZXSpectrumModel Model)
        {
            SpectrumBase speccy;

            ModelDefinition = ZXSpectrumModelDefinitions.GetDefinition(Model);

            if (ModelDefinition == null)
                throw new ArgumentException("Unknown Spectrum model!");

            if (Model == ZXSpectrumModel.Spectrum48k)
                speccy = new Spectrum48k(ModelDefinition.RomSet, ModelDefinition.InjectAddress);
            else
                speccy = new Spectrum128k(ModelDefinition.RomSet, ModelDefinition.ResetAddress, ModelDefinition.InjectAddress);

            if (machine != null)
                machine.Dispose();

            machine = speccy;
            machine.RegisterSynchronized(audio);
            machine.RegisterSynchronized(mapper);
            machine.ULA.Renderer = renderer;
            machine.FrameRendered += Machine_FrameRendered;
            machine.BreakpointHit += Machine_BreakpointHit;
            machine.ProgramReady += Machine_ProgramReady;
            audio.AudioSource = machine.ULA;
            mapper.Emulator = machine;
        }

        private void Machine_ProgramReady(object? sender, EventArgs e)
        {
            if(ProgramReady != null)
                ProgramReady(this, EventArgs.Empty);
        }

        private void BtnKeyb_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (btnKeyb.Content?.ToString() == "<")
            {
                btnKeyb.Content = ">";
                grdEmulator.ColumnDefinitions = new ColumnDefinitions("*,16,650");
                brdKeyb.IsVisible = true;
            }
            else
            {
                btnKeyb.Content = "<";
                grdEmulator.ColumnDefinitions = new ColumnDefinitions("*,16,0");
                brdKeyb.IsVisible = false;
            }
        }

        private void EmuKeyb_KeyPressed(object? sender, ZXKeyboardEventArgs e)
        {
            if (!machine.Running)
                return;

            Task.Run(async () =>
            {
                var sk = e.Key.SpectrumKey;
                bool? newSymbol = null;
                bool? newCaps = null;

                if (sk == CoreSpectrum.Enums.SpectrumKeys.Sym)
                {
                    if (emuKeyb.SymbolModifier)
                    {
                        newSymbol = false;
                        SendKeyUp(sk);
                    }
                    else if (emuKeyb.CapsModifier)
                    {
                        newCaps = false;
                        newSymbol = false;
                        SendKeyDown(sk);
                        await Task.Delay(30);
                        SendKeyUp(sk);
                        SendKeyUp(CoreSpectrum.Enums.SpectrumKeys.Caps);
                    }
                    else
                    {
                        newSymbol = true;
                        SendKeyDown(sk);
                    }

                }
                else if (sk == CoreSpectrum.Enums.SpectrumKeys.Caps)
                {
                    if (emuKeyb.CapsModifier)
                    {
                        newCaps = false;
                        SendKeyUp(sk);
                    }
                    else if (emuKeyb.SymbolModifier)
                    {
                        newCaps = false;
                        newSymbol = false;
                        SendKeyDown(sk);
                        await Task.Delay(30);
                        SendKeyUp(sk);
                        SendKeyUp(CoreSpectrum.Enums.SpectrumKeys.Sym);
                    }
                    else
                    {
                        newCaps = true;
                        SendKeyDown(sk);
                    }
                }
                else
                {
                    SendKeyDown(sk);
                    await Task.Delay(30);
                    SendKeyUp(sk);
                }

                if (newSymbol != null || newCaps != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (newSymbol != null)
                            emuKeyb.SymbolModifier = newSymbol.Value;

                        if (newCaps != null)
                            emuKeyb.CapsModifier = newCaps.Value;
                    });
                }
            });
        }
        private void Machine_BreakpointHit(object? sender, BreakPointEventArgs e)
        {
            try
            {
                var cmd = e.Breakpoint.Id;

                if (cmd == null)
                    return;

                switch (cmd)
                {
                    case ZXConstants.BASIC_BREAKPOINT:
                    case ZXConstants.ASSEMBLER_BREAKPOINT:
                    case ZXConstants.USER_BREAKPOINT:
                        if (Breakpoint != null)
                        {
                            e.StopExecution = true;
                            Breakpoint(this, new BreakpointEventArgs(e.Breakpoint));
                        }
                        break;
                }
            }
            catch(Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void Start() 
        { 
            try 
            {
                if (machine.Running)
                    return;

                emuScr.IsRunning = true; 
                machine.Start(true); 
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void Stop()
        {
            try
            {
                machine.Stop();
                emuScr.IsRunning = false;
                emuKeyb.CapsModifier = false;
                emuKeyb.SymbolModifier = false;
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void Pause()
        {
            try
            {
                machine.Pause();
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void Step()
        {
            try
            {
                machine.Step();
                    
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void Resume()
        {
            try
            {
                machine.Resume();
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void Reset() 
        {
            try
            {
                machine.Reset();
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void RefreshScreen()
        {
            try
            {
                if (DirectMode)
                {
                    try
                    {
                        renderer.DumpScreenMemory(machine);
                        emuScr.RenderFrame(renderer.VideoBuffer);
                    }
                    catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
                }
                else
                    Machine_FrameRendered(null, null);
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void SendKeyDown(SpectrumKeys Key)
        {
            try
            {
                machine.PressKey(Key);
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void SendKeyUp(SpectrumKeys Key)
        {
            try
            {
                machine.ReleaseKey(Key);
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public bool UpdateBreakpoints(IEnumerable<Breakpoint>? Breakpoints)
        {
            try
            {
                if (Running && !Paused)
                    return false;

                Thread.Sleep(20);

                machine.ClearBreakpoints();

                if (Breakpoints != null)
                {
                    foreach (var breakpoint in Breakpoints)
                        breakpoint.Executed = false;

                    machine.AddBreakpoints(Breakpoints);
                }
                return true;
            }
            catch (Exception ex) 
            { 
                if (ExceptionTrapped != null) 
                    ExceptionTrapped(this, new ExceptionEventArgs(ex)); 

                return false; 
            }
        }

        public void InjectProgram(ushort Address, byte[] Data, bool ImmediateJump)
        {
            ProgramImage img = new ProgramImage 
            { 
                Org = Address, 
                InitialBank = 0, 
                Chunks = new[] 
                { 
                    new ImageChunk { Address = Address, Bank = 0, Data = Data }  
                } 
            };

            machine.InjectProgram(img);
            emuScr.IsRunning = true;
        }

        private void Machine_FrameRendered(object? sender, EventArgs e)
        {
            try
            {
                if (emuScr == null)
                    return;

                emuScr.RenderFrame(renderer.VideoBuffer);
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
        }

        public void ProcessRawInput(RawInputEventArgs value)
        {
            if (mapper.Active)
                mapper.ProcessInput(value);
            else
            {
                if (value is RawKeyEventArgs)
                {
                    RawKeyEventArgs args = (RawKeyEventArgs)value;

                    try
                    {
                        switch (args.Key)
                        {
                            case Key.LeftShift:
                            case Key.RightShift:

                                if (args.Type == RawKeyEventType.KeyUp)
                                    SendKeyUp(CoreSpectrum.Enums.SpectrumKeys.Caps);
                                else
                                    SendKeyDown(CoreSpectrum.Enums.SpectrumKeys.Caps);
                                break;
                            case Key.LeftCtrl:
                            case Key.RightCtrl:
                                if (args.Type == RawKeyEventType.KeyUp)
                                    SendKeyUp(CoreSpectrum.Enums.SpectrumKeys.Sym);
                                else
                                    SendKeyDown(CoreSpectrum.Enums.SpectrumKeys.Sym);
                                break;
                            case Key.Enter:
                                if (args.Type == RawKeyEventType.KeyUp)
                                    SendKeyUp(CoreSpectrum.Enums.SpectrumKeys.Enter);
                                else
                                    SendKeyDown(CoreSpectrum.Enums.SpectrumKeys.Enter);
                                break;
                            default:
                                if (Enum.TryParse<SpectrumKeys>(args.Key.ToString(), true, out var key))
                                {
                                    if (args.Type == RawKeyEventType.KeyUp)
                                        SendKeyUp(key);
                                    else
                                        SendKeyDown(key);
                                }
                                break;
                        }
                    }
                    catch { }

                }
            }
        }
    }

    #region EventArgs
    public class BreakpointEventArgs : EventArgs
    {
        public BreakpointEventArgs(Breakpoint Breakpoint) 
        {
            this.Breakpoint = Breakpoint;
        }
        public Breakpoint Breakpoint { get; set; }
    }

    public class ExceptionEventArgs : EventArgs 
    {
        public Exception TrappedException { get; set; }
        public ExceptionEventArgs(Exception Exception) 
        {
            this.TrappedException = Exception;
        } 
    }
    #endregion

}
