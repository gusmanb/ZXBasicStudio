using Avalonia;
using Avalonia.Media;
using CoreSpectrum.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Controls;

namespace ZXBasicStudio.Emulator.Binding
{
    public class ZXKey : AvaloniaObject
    {
        public static StyledProperty<string> UpperCommandProperty = AvaloniaProperty.Register<ZXKey, string>("UpperCommand");
        public static StyledProperty<string> LowerCommandProperty = AvaloniaProperty.Register<ZXKey, string>("LowerCommand");
        public static StyledProperty<string> CommandProperty = AvaloniaProperty.Register<ZXKey, string>("Command");
        public static StyledProperty<string> SymbolProperty = AvaloniaProperty.Register<ZXKey, string>("Symbol");
        public static StyledProperty<string> CharProperty = AvaloniaProperty.Register<ZXKey, string>("Char");
        public static StyledProperty<SolidColorBrush> UpperColorProperty = AvaloniaProperty.Register<ZXKey, SolidColorBrush>("UpperColor");
        public static StyledProperty<SolidColorBrush> LowerColorProperty = AvaloniaProperty.Register<ZXKey, SolidColorBrush>("LowerColor");
        public static StyledProperty<SolidColorBrush> SymbolColorProperty = AvaloniaProperty.Register<ZXKey, SolidColorBrush>("SymbolColor");
        public static StyledProperty<SolidColorBrush> CommandColorProperty = AvaloniaProperty.Register<ZXKey, SolidColorBrush>("CommandColor");
        public static StyledProperty<bool> DoubleLayoutProperty = AvaloniaProperty.Register<ZXKey, bool>("DoubleLayout");
        public static StyledProperty<SolidColorBrush> KeyBackgroundProperty = AvaloniaProperty.Register<ZXKey, SolidColorBrush>("KeyBackground");
        public string UpperCommand
        {
            get { return GetValue(UpperCommandProperty); }
            set { SetValue(UpperCommandProperty, value); }
        }
        public string LowerCommand
        {
            get { return GetValue(LowerCommandProperty); }
            set { SetValue(LowerCommandProperty, value); }
        }
        public string Command
        {
            get { return GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public string Symbol
        {
            get { return GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }
        public string Char
        {
            get { return GetValue(CharProperty); }
            set { SetValue(CharProperty, value); }
        }
        public SolidColorBrush UpperColor
        {
            get { return GetValue(UpperColorProperty); }
            set { SetValue(UpperColorProperty, value); }
        }
        public SolidColorBrush LowerColor
        {
            get { return GetValue(LowerColorProperty); }
            set { SetValue(LowerColorProperty, value); }
        }
        public SolidColorBrush SymbolColor
        {
            get { return GetValue(SymbolColorProperty); }
            set { SetValue(SymbolColorProperty, value); }
        }
        public SolidColorBrush CommandColor
        {
            get { return GetValue(CommandColorProperty); }
            set { SetValue(CommandColorProperty, value); }
        }
        public bool DoubleLayout
        {
            get { return GetValue(DoubleLayoutProperty); }
            set { SetValue(DoubleLayoutProperty, value); }
        }
        public SolidColorBrush KeyBackground
        {
            get { return GetValue(KeyBackgroundProperty); }
            set { SetValue(KeyBackgroundProperty, value); }
        }
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
