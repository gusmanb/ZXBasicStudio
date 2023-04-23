using Avalonia;
using Avalonia.Media;
using CoreSpectrum.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Controls;

namespace ZXBasicStudio.Classes
{
    public class ZXKey : AvaloniaObject
    {
        public static StyledProperty<string> UpperCommandProperty = StyledProperty<string>.Register<ZXKey, string>("UpperCommand");
        public static StyledProperty<string> LowerCommandProperty = StyledProperty<string>.Register<ZXKey, string>("LowerCommand");
        public static StyledProperty<string> CommandProperty = StyledProperty<string>.Register<ZXKey, string>("Command");
        public static StyledProperty<string> SymbolProperty = StyledProperty<string>.Register<ZXKey, string>("Symbol");
        public static StyledProperty<string> CharProperty = StyledProperty<string>.Register<ZXKey, string>("Char");
        public static StyledProperty<SolidColorBrush> UpperColorProperty = StyledProperty<SolidColorBrush>.Register<ZXKey, SolidColorBrush>("UpperColor");
        public static StyledProperty<SolidColorBrush> LowerColorProperty = StyledProperty<SolidColorBrush>.Register<ZXKey, SolidColorBrush>("LowerColor");
        public static StyledProperty<SolidColorBrush> SymbolColorProperty = StyledProperty<SolidColorBrush>.Register<ZXKey, SolidColorBrush>("SymbolColor");
        public static StyledProperty<SolidColorBrush> CommandColorProperty = StyledProperty<SolidColorBrush>.Register<ZXKey, SolidColorBrush>("CommandColor");
        public static StyledProperty<bool> DoubleLayoutProperty = StyledProperty<bool>.Register<ZXKey, bool>("DoubleLayout");
        public static StyledProperty<SolidColorBrush> KeyBackgroundProperty = StyledProperty<SolidColorBrush>.Register<ZXKey, SolidColorBrush>("KeyBackground");
        public string UpperCommand
        {
            get { return GetValue<string>(UpperCommandProperty); }
            set { SetValue<string>(UpperCommandProperty, value); }
        }
        public string LowerCommand
        {
            get { return GetValue<string>(LowerCommandProperty); }
            set { SetValue<string>(LowerCommandProperty, value); }
        }
        public string Command
        {
            get { return GetValue<string>(CommandProperty); }
            set { SetValue<string>(CommandProperty, value); }
        }
        public string Symbol
        {
            get { return GetValue<string>(SymbolProperty); }
            set { SetValue<string>(SymbolProperty, value); }
        }
        public string Char
        {
            get { return GetValue<string>(CharProperty); }
            set { SetValue<string>(CharProperty, value); }
        }
        public SolidColorBrush UpperColor
        {
            get { return GetValue<SolidColorBrush>(UpperColorProperty); }
            set { SetValue<SolidColorBrush>(UpperColorProperty, value); }
        }
        public SolidColorBrush LowerColor
        {
            get { return GetValue<SolidColorBrush>(LowerColorProperty); }
            set { SetValue<SolidColorBrush>(LowerColorProperty, value); }
        }
        public SolidColorBrush SymbolColor
        {
            get { return GetValue<SolidColorBrush>(SymbolColorProperty); }
            set { SetValue<SolidColorBrush>(SymbolColorProperty, value); }
        }
        public SolidColorBrush CommandColor
        {
            get { return GetValue<SolidColorBrush>(CommandColorProperty); }
            set { SetValue<SolidColorBrush>(CommandColorProperty, value); }
        }
        public bool DoubleLayout
        {
            get { return GetValue<bool>(DoubleLayoutProperty); }
            set { SetValue<bool>(DoubleLayoutProperty, value); }
        }
        public SolidColorBrush KeyBackground
        {
            get { return GetValue<SolidColorBrush>(KeyBackgroundProperty); }
            set { SetValue<SolidColorBrush>(KeyBackgroundProperty, value); }
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
                        UpperColor= new SolidColorBrush(Color.Parse("#ff0000"));
                        LowerColor = new SolidColorBrush(Color.Parse("#ff0000"));
                        break;
                    case ZXKeyType.Control:
                        DoubleLayout = true;
                        UpperColor= new SolidColorBrush(Color.Parse("#ffffff"));
                        LowerColor= new SolidColorBrush(Color.Parse("#ffffff"));
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
            KeyType= Descriptor.KeyType;
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
