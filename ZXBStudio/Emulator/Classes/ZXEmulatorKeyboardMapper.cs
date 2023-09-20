using Avalonia.Input;
using Avalonia.Input.Raw;
using CoreSpectrum.Enums;
using CoreSpectrum.Hardware;
using CoreSpectrum.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ZXBasicStudio.Emulator.Classes
{
    /// <summary>
    /// Keyboard mapper.
    /// Allows to redefine the keyboard and even map text inputs to key strokes
    /// </summary>
    public class ZXEmulatorKeyboardMapper : ISynchronizedExecution
    {
        /// <summary>
        /// Base char map, based on spanish keyboards
        /// </summary>
        public static readonly ZXMappedChar[] baseCharMap = new ZXMappedChar[] 
        {
            //Lowercase
            new("a", SpectrumKeys.A, false, false),
            new("b", SpectrumKeys.B, false, false),
            new("c", SpectrumKeys.C, false, false),
            new("d", SpectrumKeys.D, false, false),
            new("e", SpectrumKeys.E, false, false),
            new("f", SpectrumKeys.F, false, false),
            new("g", SpectrumKeys.G, false, false),
            new("h", SpectrumKeys.H, false, false),
            new("i", SpectrumKeys.I, false, false),
            new("j", SpectrumKeys.J, false, false),
            new("k", SpectrumKeys.K, false, false),
            new("l", SpectrumKeys.L, false, false),
            new("m", SpectrumKeys.M, false, false),
            new("n", SpectrumKeys.N, false, false),
            new("o", SpectrumKeys.O, false, false),
            new("p", SpectrumKeys.P, false, false),
            new("q", SpectrumKeys.Q, false, false),
            new("r", SpectrumKeys.R, false, false),
            new("s", SpectrumKeys.S, false, false),
            new("t", SpectrumKeys.T, false, false),
            new("u", SpectrumKeys.U, false, false),
            new("v", SpectrumKeys.V, false, false),
            new("w", SpectrumKeys.W, false, false),
            new("x", SpectrumKeys.X, false, false),
            new("y", SpectrumKeys.Y, false, false),
            new("z", SpectrumKeys.Z, false, false),
            //Uppercase
            new("A", SpectrumKeys.A, true, false),
            new("B", SpectrumKeys.B, true, false),
            new("C", SpectrumKeys.C, true, false),
            new("D", SpectrumKeys.D, true, false),
            new("E", SpectrumKeys.E, true, false),
            new("F", SpectrumKeys.F, true, false),
            new("G", SpectrumKeys.G, true, false),
            new("H", SpectrumKeys.H, true, false),
            new("I", SpectrumKeys.I, true, false),
            new("J", SpectrumKeys.J, true, false),
            new("K", SpectrumKeys.K, true, false),
            new("L", SpectrumKeys.L, true, false),
            new("M", SpectrumKeys.M, true, false),
            new("N", SpectrumKeys.N, true, false),
            new("O", SpectrumKeys.O, true, false),
            new("P", SpectrumKeys.P, true, false),
            new("Q", SpectrumKeys.Q, true, false),
            new("R", SpectrumKeys.R, true, false),
            new("S", SpectrumKeys.S, true, false),
            new("T", SpectrumKeys.T, true, false),
            new("U", SpectrumKeys.U, true, false),
            new("V", SpectrumKeys.V, true, false),
            new("W", SpectrumKeys.W, true, false),
            new("X", SpectrumKeys.X, true, false),
            new("Y", SpectrumKeys.Y, true, false),
            new("Z", SpectrumKeys.Z, true, false),
            //numbers
            new("0", SpectrumKeys.D0, false, false),
            new("1", SpectrumKeys.D1, false, false),
            new("2", SpectrumKeys.D2, false, false),
            new("3", SpectrumKeys.D3, false, false),
            new("4", SpectrumKeys.D4, false, false),
            new("5", SpectrumKeys.D5, false, false),
            new("6", SpectrumKeys.D6, false, false),
            new("7", SpectrumKeys.D7, false, false),
            new("8", SpectrumKeys.D8, false, false),
            new("9", SpectrumKeys.D9, false, false),
            //Special chars
            new(" ", SpectrumKeys.Space, false, false),
            //Simbols
            new("_", SpectrumKeys.D0, false, true),
            new("!", SpectrumKeys.D1, false, true),
            new("@", SpectrumKeys.D2, false, true),
            new("#", SpectrumKeys.D3, false, true),
            new("$", SpectrumKeys.D4, false, true),
            new("%", SpectrumKeys.D5, false, true),
            new("&", SpectrumKeys.D6, false, true),
            new("'", SpectrumKeys.D7, false, true),
            new("(", SpectrumKeys.D8, false, true),
            new(")", SpectrumKeys.D9, false, true),
            new("<", SpectrumKeys.R, false, true),
            new(">", SpectrumKeys.T, false, true),
            new(";", SpectrumKeys.O, false, true),
            new("\"", SpectrumKeys.P, false, true),
            new("^", SpectrumKeys.H, false, true),
            new("-", SpectrumKeys.J, false, true),
            new("+", SpectrumKeys.K, false, true),
            new("=", SpectrumKeys.L, false, true),
            new(":", SpectrumKeys.Z, false, true),
            new("?", SpectrumKeys.C, false, true),
            new("/", SpectrumKeys.V, false, true),
            new("*", SpectrumKeys.B, false, true),
            new(",", SpectrumKeys.N, false, true),
            new(".", SpectrumKeys.M, false, true),
            //Extra
            //new("[", SpectrumKeys.Y, true, true),
            //new("]", SpectrumKeys.U, true, true),
            //new("~", SpectrumKeys.A, true, true),
            //new("|", SpectrumKeys.S, true, true),
            //new("\\", SpectrumKeys.D, true, true),
            //new("{", SpectrumKeys.F, true, true),
            //new("}", SpectrumKeys.G, true, true),

        };

        /// <summary>
        /// Base key map, based on spanish keyboards but it should be universal
        /// </summary>
        public static readonly ZXMappedKey[] baseKeyMap = new ZXMappedKey[] 
        {
            new ZXMappedKey(Key.Return, SpectrumKeys.Enter, false, false),
            new ZXMappedKey(Key.Back, SpectrumKeys.D0, true, false),
            new ZXMappedKey(Key.RightShift, SpectrumKeys.None, true, false),
            new ZXMappedKey(Key.RightCtrl, SpectrumKeys.None, true, false),
            new ZXMappedKey(Key.Up, SpectrumKeys.D7, true, false),
            new ZXMappedKey(Key.Down, SpectrumKeys.D6, true, false),
            new ZXMappedKey(Key.Left, SpectrumKeys.D5, true, false),
            new ZXMappedKey(Key.Right, SpectrumKeys.D8, true, false),

        };

        SpectrumBase? _emulator = null;

        /// <summary>
        /// Emulator controlled by the key mapper
        /// </summary>
        public SpectrumBase? Emulator 
        { 
            get 
            {
                return _emulator; 
            } 
            set 
            {
                Stop();
                _emulator = value;
            } 
        }
        public bool Active { get { return _active; } set { _active = value; CheckActiveStatus(); } }

        bool _symbolActive = false;
        bool _shiftActive = false;
        bool _running = false;
        bool _active = false;

        object _locker = new object();

        Dictionary<Key, IZXKeystroke> _keyMap = new Dictionary<Key, IZXKeystroke>();
        Dictionary<string, IZXKeystroke> _charMap = new Dictionary<string, IZXKeystroke>();

        Queue<ZXKeyboardAction> _actionQueue = new Queue<ZXKeyboardAction>();
        Timer? _tmrKeyb;

        IZXKeystroke? _pressedKey;

        /// <summary>
        /// Basic constructor, default key mapping applied
        /// </summary>
        public ZXEmulatorKeyboardMapper()
        {
            foreach (var mapped in baseCharMap)
                _charMap.Add(mapped.Character, mapped);

            foreach(var mapped in baseKeyMap)
                _keyMap.Add(mapped.ComputerKey, mapped);
        }

        /// <summary>
        /// Extended constructor with custom key mapping
        /// </summary>
        /// <param name="MappedStrokes">Custom key maps</param>
        public ZXEmulatorKeyboardMapper(IEnumerable<IZXKeystroke> MappedStrokes)
        {
            foreach (var mapped in MappedStrokes)
            {
                if (mapped is ZXMappedKey)
                {
                    var key = (ZXMappedKey) mapped;
                    _keyMap.Add(key.ComputerKey, key);
                }
                else if(mapped is ZXMappedChar) 
                {
                    var chr = (ZXMappedChar) mapped;
                    _charMap.Add(chr.Character, chr);
                }
            }
        }
        public void ProcessKey(Key InputKey, bool Up)
        {
            if (!_running || !Active)
                return;

            if (_keyMap.ContainsKey(InputKey))
                ProcessKeyStroke(_keyMap[InputKey], Up);
            else if (Up)
                ProcessKeyStroke(new ZXMappedChar("", SpectrumKeys.None, false, false), true);
        }
        public void ProcessTextInput(string Text)
        {
            if (!_running || !Active)
                return;

            if (_charMap.ContainsKey(Text))
                ProcessKeyStroke(_charMap[Text], false);
        }
        /*
        /// <summary>
        /// Processes input events from Avalonia
        /// </summary>
        /// <param name="e">EventArgs of the event</param>
        public void ProcessInput(RawInputEventArgs e)
        {
            if (!_running || !Active)
                return;

            if (e is RawTextInputEventArgs)
            {
                System.Diagnostics.Debug.WriteLine($"Received raw text");

                var evt = (RawTextInputEventArgs) e;

                if (_charMap.ContainsKey(evt.Text))
                    ProcessKeyStroke(_charMap[evt.Text], false);
            }
            else if(e is RawKeyEventArgs) 
            {
                var evt = (RawKeyEventArgs) e;

                System.Diagnostics.Debug.WriteLine($"Received raw key, key: {evt.Key}, type: {evt.Type}");

                if (_keyMap.ContainsKey(evt.Key))
                    ProcessKeyStroke(_keyMap[evt.Key], evt.Type == RawKeyEventType.KeyUp);
                else if(evt.Type == RawKeyEventType.KeyUp)
                    ProcessKeyStroke(new ZXMappedChar("", SpectrumKeys.None, false, false), true);
            }
        }
        */

        /// <summary>
        /// Process a key stroke
        /// </summary>
        /// <param name="Stroke">The stroke to process</param>
        /// <param name="Release">True if it is a release event, false in other case</param>
        void ProcessKeyStroke(IZXKeystroke Stroke, bool Release)
        {
            lock (_locker)
            {
                switch (Stroke.Key)
                {
                    case SpectrumKeys.Caps:
                        if (_shiftActive == Release)
                        {
                            _shiftActive = !Release;
                            _actionQueue.Enqueue(new ZXKeyboardAction(Stroke, _shiftActive, _symbolActive, Release));
                        }
                        break;
                    case SpectrumKeys.Sym:
                        if (_symbolActive == Release)
                        {
                            _symbolActive = !Release;
                            _actionQueue.Enqueue(new ZXKeyboardAction(Stroke, _shiftActive, _symbolActive, Release));
                        }
                        break;
                    default:

                        if (Release)
                        {
                            if (_pressedKey == null)
                                break;

                            _actionQueue.Enqueue(new ZXKeyboardAction(_pressedKey, _shiftActive, _symbolActive, true));
                            _pressedKey = null;
                        }
                        else
                        {
                            if (_pressedKey == Stroke)
                                break;

                            if (_pressedKey != null)
                            {
                                _actionQueue.Enqueue(new ZXKeyboardAction(_pressedKey, _shiftActive, _symbolActive, true));
                            }
                            _actionQueue.Enqueue(new ZXKeyboardAction(Stroke, _shiftActive, _symbolActive, false));
                            _pressedKey = Stroke;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Process the key action queue, executed in timed intervals, each interval is longer than 50ms to ensure that one frame happens and
        /// Basic has time to scan the keyboard in the interrupt
        /// </summary>
        /// <param name="State"></param>
        void ProcessQueue(object? State)
        {
            lock(_locker) 
            { 
                if(_actionQueue.Count > 0) 
                {
                    var action = _actionQueue.Dequeue();
                    if (action.Release)
                    {
                        System.Diagnostics.Debug.WriteLine($"Releasing key {action.KeyStroke.Key}");
                        if (action.KeyStroke.Key != SpectrumKeys.None)
                            _emulator?.ReleaseKey(action.KeyStroke.Key);

                        if (!action.SymbolActive && action.KeyStroke.Symbol)
                            _emulator?.ReleaseKey(SpectrumKeys.Sym);

                        if (!action.ShiftActive && action.KeyStroke.Shift)
                            _emulator?.ReleaseKey(SpectrumKeys.Caps);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Pressing key {action.KeyStroke.Key}");
                        //TODO: Check with Extra symbols
                        if (!action.SymbolActive && action.KeyStroke.Symbol)
                            _emulator?.PressKey(SpectrumKeys.Sym);

                        if (!action.ShiftActive && action.KeyStroke.Shift)
                            _emulator?.PressKey(SpectrumKeys.Caps);

                        if(action.KeyStroke.Key != SpectrumKeys.None)
                            _emulator?.PressKey(action.KeyStroke.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if needs to start/stop the timer when the active status is changed
        /// </summary>
        void CheckActiveStatus()
        {
            lock(_locker) 
            {
                if (!_active)
                    Stop();
                else
                {
                    if ((_emulator?.Running ?? false) && !(_emulator?.Paused ?? false))
                        Start();
                }
            }
        }

        #region ISynchronizedExecution implementation

        public void Pause()
        {
            lock (_locker)
            {
                if (!_active)
                    return;

                _tmrKeyb?.Change(Timeout.Infinite, Timeout.Infinite);
                _tmrKeyb?.Dispose();
                _tmrKeyb = null;
                if(_pressedKey != null) 
                {
                    ProcessKeyStroke(_pressedKey, true);
                    _pressedKey = null;
                }
                _symbolActive = false;
                _shiftActive = false;
                _running = false;
            }
        }

        public void Reset()
        {
            lock (_locker)
            {
                if (!_active)
                    return;

                _tmrKeyb?.Change(Timeout.Infinite, Timeout.Infinite);
                _tmrKeyb?.Dispose();
                _tmrKeyb = null;
                _pressedKey = null;
                _symbolActive = false;
                _shiftActive = false;
                _actionQueue.Clear();
            }
        }

        public void Resume()
        {
            lock (_locker)
            {
                if (!_active)
                    return;

                _tmrKeyb = new Timer(ProcessQueue, null, 51, 51);
                _running = true;
            }
        }

        public void Start()
        {
            lock (_locker)
            {
                if (!_active)
                    return;

                _tmrKeyb = new Timer(ProcessQueue, null, 51, 51);
                _running = true;
            }
        }

        public void Step()
        {
            
        }

        public void Stop()
        {
            lock (_locker)
            {
                _tmrKeyb?.Change(Timeout.Infinite, Timeout.Infinite);
                _tmrKeyb?.Dispose();
                _tmrKeyb = null;
                _pressedKey = null;
                _symbolActive = false;
                _shiftActive = false;
                _actionQueue.Clear();
                _running = false;
            }
        }

        public void Turbo(bool Enable)
        {
            
        }

        #endregion

        class ZXKeyboardAction
        {
            public IZXKeystroke KeyStroke { get; set; }
            public bool ShiftActive { get; set; }
            public bool SymbolActive { get; set; }
            public bool Release { get; set; }

            public ZXKeyboardAction(IZXKeystroke KeyStroke, bool ShiftActive, bool SymbolActive, bool Release)
            {
                this.KeyStroke = KeyStroke;
                this.Release = Release;
            }
        }

    }

    /// <summary>
    /// Maped char, for text input mapping
    /// </summary>
    public class ZXMappedChar : IZXKeystroke
    {
        public string Character { get; set; }
        public SpectrumKeys Key { get; set; }
        public bool Shift { get; set; }
        public bool Symbol { get; set; }

        public ZXMappedChar(string character, SpectrumKeys key, bool shift, bool symbol)
        {
            Character = character;
            Key = key;
            Shift = shift;
            Symbol = symbol;
        }
    }

    /// <summary>
    /// Mapped key, for raw key input
    /// </summary>
    public class ZXMappedKey : IZXKeystroke
    {
        public Key ComputerKey { get; set; }
        public SpectrumKeys Key { get; set; }
        public bool Shift { get; set; }
        public bool Symbol { get; set; }

        public ZXMappedKey(Key computerKey, SpectrumKeys key, bool shift, bool symbol)
        {
            ComputerKey = computerKey;
            Key = key;
            Shift = shift;
            Symbol = symbol;
        }
    }

    /// <summary>
    /// General interface for keystrokes
    /// </summary>
    public interface IZXKeystroke
    {
        SpectrumKeys Key { get; set; }
        bool Shift { get; set; }
        bool Symbol { get; set; }
    }
}
