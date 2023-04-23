using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.Controls
{
    public partial class ZXKeyView : UserControl
    {
        public static StyledProperty<ZXKey> KeyProperty = StyledProperty<ZXKey>.Register<ZXKeyView, ZXKey>("Key");

        public event EventHandler? UpperCommandClicked;
        public event EventHandler? LowerCommandClicked;
        public event EventHandler? SymbolClicked;
        public event EventHandler? KeyClicked;
        public ZXKey Key 
        {
            get { return GetValue<ZXKey>(KeyProperty); }
            set { SetValue<ZXKey>(KeyProperty, value); }
        }

        

        bool _held;
        public bool Held 
        { 
            get { return _held; } 
            set 
            { 
                _held = value;
                if (_held)
                    Key.KeyBackground = new SolidColorBrush(Color.Parse("#3E5D73"));
                else
                    Key.KeyBackground = new SolidColorBrush(Color.Parse("#507793"));
            }
        }

        public ZXKeyView()
        {
            Key = new ZXKey();
            Key.SetData(new ZXKeyDescriptor { Char = "Q", UpperCommand = "SQRT", LowerCommand = "ASN", Symbol = "@", Command = "PLOT", KeyType = ZXKeyType.Normal });
            DataContext = this;
            InitializeComponent();
            tbUpperCommand.PointerReleased += UpperCommandClick;
            tbLowerCommand.PointerReleased += LowerCommandClick;
            tbSymbol.PointerReleased += SymbolClick;
            brdKey.PointerReleased += KeyClick;
            tbLetter.PointerReleased += KeyClick;
            tbCommand.PointerReleased += KeyClick;
        }
        void UpperCommandClick(object? sender, PointerReleasedEventArgs e)
        {
            if(UpperCommandClicked != null)
                UpperCommandClicked(this, EventArgs.Empty);
        }
        void LowerCommandClick(object? sender, PointerReleasedEventArgs e)
        {
            if (LowerCommandClicked != null)
                LowerCommandClicked(this, EventArgs.Empty);
        }
        void SymbolClick(object? sender, PointerReleasedEventArgs e)
        {
            if (SymbolClicked != null)
                SymbolClicked(this, EventArgs.Empty);
        }
        void KeyClick(object? sender, PointerReleasedEventArgs e)
        {
            if (KeyClicked != null)
                KeyClicked(this, EventArgs.Empty);
        }
    }
}
