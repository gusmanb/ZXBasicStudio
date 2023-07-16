using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreSpectrum.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Controls;

namespace ZXBasicStudio.Emulator.Binding
{
    public partial class ZXKey : ObservableObject
    {
        [ObservableProperty]
        string upperCommand = "";
        [ObservableProperty]
        string lowerCommand = "";
        [ObservableProperty]
        string command = "";
        [ObservableProperty]
        string symbol = "";
        [ObservableProperty]
        string @char = "";
        [ObservableProperty]
        SolidColorBrush? upperColor;
        [ObservableProperty]
        SolidColorBrush? lowerColor;
        [ObservableProperty]
        SolidColorBrush? symbolColor;
        [ObservableProperty]
        SolidColorBrush? commandColor;
        [ObservableProperty]
        bool doubleLayout;
        [ObservableProperty]
        SolidColorBrush? keyBackground;

        public SpectrumKeys SpectrumKey { get; set; }

        ZXKeyType _keyType;
        public ZXKeyType KeyType
        {
            get { return _keyType; }
            set
            {
                _keyType = value;
                switch (_keyType)
                {
                    case ZXKeyType.Normal:
                        DoubleLayout = false;
                        UpperColor = new SolidColorBrush(Color.Parse("#00ff00"));
                        LowerColor = new SolidColorBrush(Color.Parse("#ff0000"));
                        SymbolColor = new SolidColorBrush(Color.Parse("#ff0000"));
                        CommandColor = new SolidColorBrush(Color.Parse("#ffffff"));
                        break;
                    case ZXKeyType.Number:
                        DoubleLayout = false;
                        UpperColor = new SolidColorBrush(Color.Parse("#ffffff"));
                        LowerColor = new SolidColorBrush(Color.Parse("#ff0000"));
                        SymbolColor = new SolidColorBrush(Color.Parse("#ffffff"));
                        CommandColor = new SolidColorBrush(Color.Parse("#ff0000"));
                        break;
                    case ZXKeyType.Symbol:
                        DoubleLayout = true;
                        UpperColor = new SolidColorBrush(Color.Parse("#ff0000"));
                        LowerColor = new SolidColorBrush(Color.Parse("#ff0000"));
                        break;
                    case ZXKeyType.Control:
                        DoubleLayout = true;
                        UpperColor = new SolidColorBrush(Color.Parse("#ffffff"));
                        LowerColor = new SolidColorBrush(Color.Parse("#ffffff"));
                        break;
                }
            }
        }

        public void SetData(ZXKeyDescriptor Descriptor)
        {
            UpperCommand = Descriptor.UpperCommand;
            LowerCommand = Descriptor.LowerCommand;
            Command = Descriptor.Command;
            Symbol = Descriptor.Symbol;
            Char = Descriptor.Char;
            KeyType = Descriptor.KeyType;
            SpectrumKey = Descriptor.SpectrumKey;
            KeyBackground = new SolidColorBrush(Color.Parse("#507793"));
        }
    }

    public class ZXKeyDescriptor
    {
        public required string UpperCommand { get; set; }
        public required string LowerCommand { get; set; }
        public required string Command { get; set; }
        public required string Symbol { get; set; }
        public required string Char { get; set; }
        public SpectrumKeys SpectrumKey { get; set; }
        public ZXKeyType KeyType { get; set; }
    }

    public enum ZXKeyType
    {
        Normal,
        Number,
        Symbol,
        Control
    }
}
