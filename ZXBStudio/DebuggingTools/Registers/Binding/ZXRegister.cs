using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DebuggingTools.Registers.Binding
{
    public class ZXRegister : AvaloniaObject
    {
        public static StyledProperty<string> NameProperty = AvaloniaProperty.Register<ZXRegister, string>("Name", "");
        public static StyledProperty<string> HexProperty = AvaloniaProperty.Register<ZXRegister, string>("Hex", "--");
        public static StyledProperty<short> SignedProperty = AvaloniaProperty.Register<ZXRegister, short>("Signed", 0);
        public static StyledProperty<ushort> UnsignedProperty = AvaloniaProperty.Register<ZXRegister, ushort>("Unsigned", 0);

        public string Name
        {
            get { return GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public string Hex
        {
            get { return GetValue(HexProperty); }
            set { SetValue(HexProperty, value); }
        }
        public short Signed
        {
            get { return GetValue(SignedProperty); }
            set { SetValue(SignedProperty, value); }
        }
        public ushort Unsigned
        {
            get { return GetValue(UnsignedProperty); }
            set { SetValue(UnsignedProperty, value); }
        }
        public bool IsByte { get; set; }

        public void SetValue(string Value)
        {
            if (IsByte)
            {
                byte value = byte.Parse(Value, System.Globalization.NumberStyles.HexNumber);
                SetValue(value);
            }
            else
            {
                ushort value = ushort.Parse(Value, System.Globalization.NumberStyles.HexNumber);
                SetValue(value);
            }
        }
        public void SetValue(short Value)
        {
            if (IsByte)
                throw new InvalidOperationException("Register is byte");
            Hex = Value.ToString("X4");
            Signed = Value;
            Unsigned = unchecked((ushort)Value);
        }
        public void SetValue(ushort Value)
        {
            if (IsByte)
                throw new InvalidOperationException("Register is byte");

            Hex = Value.ToString("X4");
            Signed = unchecked((short)Value);
            Unsigned = Value;
        }
        public void SetValue(byte Value)
        {
            if (!IsByte)
                throw new InvalidOperationException("Register is not byte");

            Hex = Value.ToString("X2");
            Signed = (sbyte)Value;
            Unsigned = Value;
        }
    }
}
