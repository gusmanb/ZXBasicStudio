namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Default implementation of <see cref="IMainZ80Registers"/>.
    /// </summary>
    public class MainZ80Registers : IMainZ80Registers
    {
        public short AF { get; set; }

        public short BC { get; set; }

        public short DE { get; set; }

        public short HL { get; set; }

        public byte A
        {
            get
            {
                return AF.GetHighByte();
            }
            set
            {
                AF = AF.SetHighByte(value);
            }
        }

        public byte F
        {
            get
            {
                return AF.GetLowByte();
            }
            set
            {
                AF = AF.SetLowByte(value);
            }
        }

        public byte B
        {
            get
            {
                return BC.GetHighByte();
            }
            set
            {
                BC = BC.SetHighByte(value);
            }
        }

        public byte C
        {
            get
            {
                return BC.GetLowByte();
            }
            set
            {
                BC = BC.SetLowByte(value);
            }
        }

        public byte D
        {
            get
            {
                return DE.GetHighByte();
            }
            set
            {
                DE = DE.SetHighByte(value);
            }
        }

        public byte E
        {
            get
            {
                return DE.GetLowByte();
            }
            set
            {
                DE = DE.SetLowByte(value);
            }
        }

        public byte H
        {
            get
            {
                return HL.GetHighByte();
            }
            set
            {
                HL = HL.SetHighByte(value);
            }
        }

        public byte L
        {
            get
            {
                return HL.GetLowByte();
            }
            set
            {
                HL = HL.SetLowByte(value);
            }
        }

        public Bit CF
        {
            get
            {
                return F.GetBit(0);
            }
            set
            {
                F = F.WithBit(0, value);
            }
        }

        public Bit NF
        {
            get
            {
                return F.GetBit(1);
            }
            set
            {
                F = F.WithBit(1, value);
            }
        }

        public Bit PF
        {
            get
            {
                return F.GetBit(2);
            }
            set
            {
                F = F.WithBit(2, value);
            }
        }

        public Bit Flag3
        {
            get
            {
                return F.GetBit(3);
            }
            set
            {
                F = F.WithBit(3, value);
            }
        }

        public Bit HF
        {
            get
            {
                return F.GetBit(4);
            }
            set
            {
                F = F.WithBit(4, value);
            }
        }

        public Bit Flag5
        {
            get
            {
                return F.GetBit(5);
            }
            set
            {
                F = F.WithBit(5, value);
            }
        }

        public Bit ZF
        {
            get
            {
                return F.GetBit(6);
            }
            set
            {
                F = F.WithBit(6, value);
            }
        }

        public Bit SF
        {
            get
            {
                return F.GetBit(7);
            }
            set
            {
                F = F.WithBit(7, value);
            }
        }
    }
}