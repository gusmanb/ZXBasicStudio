using Avalonia.Controls;
using CoreSpectrum.Enums;
using System;
using System.Linq.Expressions;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXKeyboardView : UserControl
    {
        static ZXKeyDescriptor[] keyb = new ZXKeyDescriptor[]
        {
            new ZXKeyDescriptor{Char="1", UpperCommand="EDIT", LowerCommand="DEF FN", Symbol = "▝", Command="!", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D1},
            new ZXKeyDescriptor{Char="2", UpperCommand="CAPS LOCK", LowerCommand="FN", Symbol="▘", Command="@", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D2 } ,
            new ZXKeyDescriptor{Char="3", UpperCommand="TRUE VIDEO", LowerCommand="LINE", Symbol="▀", Command="#", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D3 },
            new ZXKeyDescriptor{Char="4", UpperCommand="INV. VIDEO", LowerCommand="OPEN#", Symbol="▗", Command="$", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D4},
            new ZXKeyDescriptor{Char="5", UpperCommand="⇐", LowerCommand="CLOSE#", Symbol="▐", Command="%", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D5},
            new ZXKeyDescriptor{Char="6", UpperCommand="⇓", LowerCommand="MOVE", Symbol="▚", Command="&", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D6},
            new ZXKeyDescriptor{Char="7", UpperCommand="⇑", LowerCommand="ERASE", Symbol="▜", Command="'", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D7},
            new ZXKeyDescriptor{Char="8", UpperCommand="⇒", LowerCommand = "POINT", Symbol = "█", Command="(", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D8},
            new ZXKeyDescriptor{Char="9", UpperCommand="GRAPHICS", LowerCommand="CAT", Symbol="", Command=")", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D9},
            new ZXKeyDescriptor{Char="0", UpperCommand="DELETE", LowerCommand="FORMAT", Symbol="", Command="_", KeyType = ZXKeyType.Number, SpectrumKey = SpectrumKeys.D0},
            new ZXKeyDescriptor{Char="Q", UpperCommand="SIN", LowerCommand="ASN", Symbol="<=", Command="PLOT", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.Q},
            new ZXKeyDescriptor{Char="W", UpperCommand="COS", LowerCommand="ACS", Symbol="<>", Command="DRAW", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.W},
            new ZXKeyDescriptor{Char="E", UpperCommand="TAN", LowerCommand="ATN", Symbol=">=", Command="REM", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.E},
            new ZXKeyDescriptor{Char="R", UpperCommand="INT", LowerCommand="VERIFY", Symbol="<", Command="RUN", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.R},
            new ZXKeyDescriptor{Char="T", UpperCommand="RND", LowerCommand="MERGE", Symbol=">", Command="RAND", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.T},
            new ZXKeyDescriptor{Char="Y", UpperCommand="STR$", LowerCommand="[", Symbol="AND", Command="RETURN", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.Y},
            new ZXKeyDescriptor{Char="U", UpperCommand="CHR$", LowerCommand="]", Symbol="OR", Command="IF", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.U},
            new ZXKeyDescriptor{Char="I", UpperCommand="CODE", LowerCommand="IN", Symbol="AT", Command="INPUT", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.I},
            new ZXKeyDescriptor{Char="O", UpperCommand="PEEK", LowerCommand="OUT", Symbol=";", Command="POKE", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.O},
            new ZXKeyDescriptor{Char="P", UpperCommand="TAB", LowerCommand="©", Symbol="\"", Command="PRINT", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.P},
            new ZXKeyDescriptor{Char="A", UpperCommand="READ", LowerCommand="~", Symbol="STOP", Command="NEW", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.A},
            new ZXKeyDescriptor{Char="S", UpperCommand="RESTORE", LowerCommand="|", Symbol="NOT", Command="SAVE", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.S},
            new ZXKeyDescriptor{Char="D", UpperCommand="DATA", LowerCommand="\\", Symbol="STEP", Command="DIM", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.D},
            new ZXKeyDescriptor{Char="F", UpperCommand="SGN", LowerCommand="{", Symbol="TO", Command="FOR", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.F},
            new ZXKeyDescriptor{Char="G", UpperCommand="ABS", LowerCommand="}", Symbol="THEN", Command="GOTO", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.G},
            new ZXKeyDescriptor{Char="H", UpperCommand="SQR", LowerCommand="CIRCLE", Symbol="↑", Command="GOSUB", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.H},
            new ZXKeyDescriptor{Char="J", UpperCommand="VAL", LowerCommand="VAL$", Symbol="-", Command="LOAD", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.J},
            new ZXKeyDescriptor{Char="K", UpperCommand="LEN", LowerCommand="SCREEN$", Symbol="+", Command="LIST", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.K},
            new ZXKeyDescriptor{Char="L", UpperCommand="USR", LowerCommand="ATTR", Symbol="=", Command="LET", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.L},
            new ZXKeyDescriptor{Char="Enter", UpperCommand="ENTER", LowerCommand="", Symbol="", Command="", KeyType = ZXKeyType.Control, SpectrumKey = SpectrumKeys.Enter},
            new ZXKeyDescriptor{Char="Shift", UpperCommand="CAPS", LowerCommand="SHIFT", Symbol="", Command="", KeyType = ZXKeyType.Control, SpectrumKey = SpectrumKeys.Caps},
            new ZXKeyDescriptor{Char="Z", UpperCommand="LN", LowerCommand="BEEP", Symbol=":", Command="COPY", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.Z},
            new ZXKeyDescriptor{Char="X", UpperCommand="EXP", LowerCommand="INK", Symbol="£", Command="CLEAR", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.X},
            new ZXKeyDescriptor{Char="C", UpperCommand="LPRINT", LowerCommand="PAPER", Symbol="?", Command="CONT", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.C},
            new ZXKeyDescriptor{Char="V", UpperCommand="LLIST", LowerCommand="FLASH", Symbol="/", Command="CLS", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.V},
            new ZXKeyDescriptor{Char="B", UpperCommand="BIN", LowerCommand="BRIGHT", Symbol="*", Command="BORDER", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.B},
            new ZXKeyDescriptor{Char="N", UpperCommand="INKEY$", LowerCommand="OVER", Symbol=",", Command="NEXT", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.N},
            new ZXKeyDescriptor{Char="M", UpperCommand="PI", LowerCommand="INVERSE", Symbol="·", Command="PAUSE", KeyType = ZXKeyType.Normal, SpectrumKey = SpectrumKeys.M},
            new ZXKeyDescriptor{Char="Symbol", UpperCommand="SYMBOL", LowerCommand="SHIFT", Symbol="", Command="", KeyType = ZXKeyType.Symbol, SpectrumKey = SpectrumKeys.Sym},
            new ZXKeyDescriptor{Char="Space", UpperCommand="BREAK", LowerCommand="SPACE", Symbol="", Command="", KeyType = ZXKeyType.Control, SpectrumKey = SpectrumKeys.Space},

        };

        public event EventHandler<ZXKeyboardEventArgs>? KeyPressed;

        bool _caps;
        public bool CapsModifier { get { return _caps; } set { _caps = value; if (_caps) keyShift.Held = true; else keyShift.Held = false; } }
        bool _symbol;
        public bool SymbolModifier { get { return _symbol; } set { _symbol = value; if (_symbol) keySymbol.Held = true; else keySymbol.Held = false; } }

        public ZXKeyboardView()
        {
            InitializeComponent();
            foreach (var key in keyb)
            {
                var control = this.FindControl<ZXKeyView>("key" + key.Char);

                if (control != null)
                {
                    control.Key.SetData(key);
                    control.KeyClicked += Key_KeyClicked;
                }
            }
        }

        private void Key_KeyClicked(object? sender, System.EventArgs e)
        {
            var key = sender as ZXKeyView;

            if (key == null || KeyPressed == null)
                return;

            KeyPressed(this, new ZXKeyboardEventArgs { Key = key.Key, Modifier = ZXKeyModifier.None });
        }
    }

    public class ZXKeyboardEventArgs : EventArgs
    {
        public required ZXKey Key { get; set; }
        public ZXKeyModifier Modifier { get; set; }
    }

    public enum ZXKeyModifier
    {
        None,
        UpperCommand,
        LowerCommand,
        Symbol
    }
}
