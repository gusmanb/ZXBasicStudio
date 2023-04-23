using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXRegister : AvaloniaObject
    {
        public static StyledProperty<string> NameProperty = StyledProperty<string>.Register<ZXRegister, string>("Name", "");
        public static StyledProperty<string> HexProperty = StyledProperty<string>.Register<ZXRegister, string>("Hex", "--");
        public static StyledProperty<short> SignedProperty = StyledProperty<short>.Register<ZXRegister, short>("Signed", 0);
        public static StyledProperty<ushort> UnsignedProperty = StyledProperty<ushort>.Register<ZXRegister, ushort>("Unsigned", 0);
        
        public string Name {
            get { return GetValue<string>(NameProperty); }
            set { SetValue<string>(NameProperty, value); }
        }
        public string Hex {
            get { return GetValue<string>(HexProperty); }
            set { SetValue<string>(HexProperty, value); }
        }
        public short Signed {
            get { return GetValue<short>(SignedProperty); }
            set { SetValue<short>(SignedProperty, value); }
        }
        public ushort Unsigned {
            get { return GetValue<ushort>(UnsignedProperty); }
            set { SetValue<ushort>(UnsignedProperty, value); }
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
