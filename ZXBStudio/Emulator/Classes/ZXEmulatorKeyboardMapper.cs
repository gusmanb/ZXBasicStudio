using Avalonia.Input;
using Avalonia.Input.Raw;
using CoreSpectrum.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Emulator.Classes
{
    public class ZXEmulatorKeyboardMapper
    {

        static readonly ZXMappedChar[] charMap = new ZXMappedChar[] 
        {
            //lowercase
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
            //uppercase
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
            //Simbols
            new("!", SpectrumKeys.D1, false, true),
            new("\"", SpectrumKeys.P, false, true),
            new("#", SpectrumKeys.D3, false, true),
            new("$", SpectrumKeys.D4, false, true),
            new("%", SpectrumKeys.D5, false, true),
            new("&", SpectrumKeys.D6, false, true),
            new("/", SpectrumKeys.V, false, true),
            new("(", SpectrumKeys.D8, false, true),
            new(")", SpectrumKeys.D9, false, true),
            new("=", SpectrumKeys.L, false, true),
            new("?", SpectrumKeys.C, false, true),
            new("'", SpectrumKeys.D7, false, true),
            new("*", SpectrumKeys.B, false, true),
            new("+", SpectrumKeys.K, false, true),
            new("-", SpectrumKeys.J, false, true),
            new(",", SpectrumKeys.N, false, true),
            new(";", SpectrumKeys.D9, false, false),
            new(".", SpectrumKeys.D9, false, false),
            new(":", SpectrumKeys.D9, false, false),
            new("_", SpectrumKeys.D9, false, false),
            new("{", SpectrumKeys.D9, false, false),
            new("}", SpectrumKeys.D9, false, false),
            new("[", SpectrumKeys.D9, false, false),
            new("]", SpectrumKeys.D9, false, false),
            new("|", SpectrumKeys.D9, false, false),
            new("~", SpectrumKeys.D9, false, false),
            new("@", SpectrumKeys.D9, false, false),
            new("<", SpectrumKeys.D9, false, false),
            new(">", SpectrumKeys.D9, false, false),
        };
        static readonly ZXMappedKey[] keyMap = new ZXMappedKey[] { };
        ZXKeystroke? pressedKey;

        public void ProcessInput(RawInputEventArgs e)
        {
            //if()
        }
    }

    public class ZXMappedChar : ZXKeystroke
    {
        public string TextInput { get; set; }
        public SpectrumKeys Key { get; set; }
        public bool Shift { get; set; }
        public bool Symbol { get; set; }

        public ZXMappedChar(string textInput, SpectrumKeys key, bool shift, bool symbol)
        {
            TextInput = textInput;
            Key = key;
            Shift = shift;
            Symbol = symbol;
        }
    }
    public class ZXMappedKey : ZXKeystroke
    {
        public Key ComputerKey { get; set; }
        public SpectrumKeys Key { get; set; }
        public bool Shift { get; set; }
        public bool Symbol { get; set; }
    }

    public interface ZXKeystroke
    {
        SpectrumKeys Key { get; set; }
        bool Shift { get; set; }
        bool Symbol { get; set; }
    }
}
