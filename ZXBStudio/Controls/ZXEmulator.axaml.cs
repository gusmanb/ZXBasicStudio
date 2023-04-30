using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
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

namespace ZXBasicStudio.Controls
{
    public partial class ZXEmulator : UserControl
    {
        private static readonly byte BRIGHT = 0xff, NORM = 0xd7;

        ZXVideoRenderer renderer;

        ZXEmulatorAudio audio;

        MachineBase machine;

        byte[]? programToInject = null;
        ushort address = 0;
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
        public IZ80Registers Registers { get { return machine.Z80.Registers; } }
        public ISpectrumMemory Memory { get { return machine.Memory; } }

        public TapePlayer Datacorder { get { return machine.DataCorder; } }

        public ulong TStates { get { return machine.Z80.TStatesElapsedSinceReset; } }

        #region Events

        public event EventHandler<BreakpointEventArgs>? Breakpoint;
        public event EventHandler? ProgramReady 
        {
            add { machine.ProgramReady += value; }
            remove { machine.ProgramReady -= value; }
        }
        public event EventHandler<ExceptionEventArgs>? ExceptionTrapped;

        #endregion

        public ZXEmulator()
        {

            /*
            System.Resources.ResourceManager resources = new System.Resources. ResourceManager("ZXBasicStudio.Resources.ZXSpectrum", typeof(ZXEmulator).Assembly);

            var rom = resources.GetObject("48k_rom") as byte[];

            if (rom == null)
                throw new InvalidProgramException("Missing ROM resource!");

            renderer = new ZXVideoRenderer();
            machine = new Spectrum48k(new byte[][] { rom }, renderer);
            */


            System.Resources.ResourceManager resources = new System.Resources.ResourceManager("ZXBasicStudio.Resources.ZXSpectrum", typeof(ZXEmulator).Assembly);

            var rom0 = resources.GetObject("Plus2_0_rom") as byte[];

            if (rom0 == null)
                throw new InvalidProgramException("Missing ROM resource!");

            var rom1 = resources.GetObject("Plus2_1_rom") as byte[];

            if (rom1 == null)
                throw new InvalidProgramException("Missing ROM resource!");

            renderer = new ZXVideoRenderer();
            machine = new Spectrum128k(new byte[][] { rom0, rom1 }, renderer);
            

            audio = new ZXEmulatorAudio(machine.ULA);
            machine.RegisterSynchronized(audio);
            machine.FrameRendered += Machine_FrameRendered;
            machine.BreakpointHit += Machine_BreakpointHit;
            InitializeComponent();
            emuKeyb.KeyPressed += EmuKeyb_KeyPressed;
            btnKeyb.Click += BtnKeyb_Click;

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

        private void Machine_FrameRendered(object? sender, SpectrumFrameArgs e)
        {
            try
            {
                if (emuScr == null)
                    return;

                emuScr.RenderFrame(renderer.VideoBuffer);
            }
            catch (Exception ex) { if (ExceptionTrapped != null) ExceptionTrapped(this, new ExceptionEventArgs(ex)); }
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
