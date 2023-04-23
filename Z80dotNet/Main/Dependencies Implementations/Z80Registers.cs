using System;
namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Represents a full set of Z80 registers. This is the default implementation of
    /// <see cref="IZ80Registers"/>.
    /// </summary>
    public class Z80Registers : MainZ80Registers, IZ80Registers
    {
        public Z80Registers()
        {
            Alternate = new MainZ80Registers();
        }

        private IMainZ80Registers _Alternate;
        public IMainZ80Registers Alternate
        {
            get
            {
                return _Alternate;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("Alternate");

                _Alternate = value;
            }
        }

        public short IX { get; set; }

        public short IY { get; set; }

        public ushort PC { get; set; }

        public short SP { get; set; }

        public short IR { get; set; }

        public Bit IFF1 { get; set; }

        public Bit IFF2 { get; set; }

        public byte IXH
        {
            get
            {
                return IX.GetHighByte();
            }
            set
            {
                IX = IX.SetHighByte(value);
            }
        }

        public byte IXL
        {
            get
            {
                return IX.GetLowByte();
            }
            set
            {
                IX = IX.SetLowByte(value);
            }
        }

        public byte IYH
        {
            get
            {
                return IY.GetHighByte();
            }
            set
            {
                IY = IY.SetHighByte(value);
            }
        }

        public byte IYL
        {
            get
            {
                return IY.GetLowByte();
            }
            set
            {
                IY = IY.SetLowByte(value);
            }
        }

        public byte I
        {
            get
            {
                return IR.GetHighByte();
            }
            set
            {
                IR = IR.SetHighByte(value);
            }
        }

        public byte R
        {
            get
            {
                return IR.GetLowByte();
            }
            set
            {
                IR = IR.SetLowByte(value);
            }
        }
    }
}