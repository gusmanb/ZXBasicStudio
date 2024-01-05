using System;
using System.Linq;
using Avalonia.Controls;

namespace ZXBasicStudio.DebuggingTools.Flags.Controls
{
    public partial class ZXFlagsView : UserControl
    {
        
        public ZXFlagsView()
        {
            InitializeComponent();
        }
        
        public void Update(byte value)
        {
            var bits = GetBinaryFromByte(value);
            Bit7.Text = bits[7].ToString();
            Bit6.Text = bits[6].ToString();
            Bit5.Text = bits[5].ToString();
            Bit4.Text = bits[4].ToString();
            Bit3.Text = bits[3].ToString();
            Bit2.Text = bits[2].ToString();
            Bit1.Text = bits[1].ToString();
            Bit0.Text = bits[0].ToString();
        }
        
        private static int[] GetBinaryFromByte(byte value)
        {
            string s = Convert.ToString(value, 2);
            int[] bits = s.PadLeft(8, '0')
                .Select(c => int.Parse(c.ToString()))
                .ToArray();

            Array.Reverse(bits);
            return bits;
        }
        
        public void Clear()
        {
            Bit7.Text = "-";
            Bit6.Text = "-";
            Bit5.Text = "-";
            Bit4.Text = "-";
            Bit3.Text = "-";
            Bit2.Text = "-";
            Bit1.Text = "-";
            Bit0.Text = "-";
            
        }
    }
}
