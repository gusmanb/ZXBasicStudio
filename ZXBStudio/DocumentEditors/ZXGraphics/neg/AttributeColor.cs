using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.neg
{
    public class AttributeColor
    {
        public int Ink { get; set; }
        public int Paper { get; set; }
        public bool Bright { get; set; }
        public bool Flash { get; set; }
        public byte Attribute
        {
            get
            {
                byte atr = (byte)((Ink & 0b00000111) | ((Paper & 0b00000111) << 3));
                if (Ink > 7 || Paper > 7)
                {
                    Bright = true;
                }
                else
                {
                    Bright = false;
                }
                if (Bright)
                {
                    atr += 0b01000000;
                }
                if (Flash)
                {
                    atr += 0b10000000;
                }
                return atr; ;
            }
            set
            {
                Flash = (value & 0b10000000) > 0;
                Bright = (value & 0b01000000) > 0;
                Paper = (value >> 3) & 0b00000111;
                Ink = value & 0b00000111;
            }
        }
    }
}
