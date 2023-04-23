using NUnit.Framework;
using System;

namespace Konamiman.Z80dotNet.Tests
{
    public class NumberUtilsTests
    {
        [Test]
        public void GetHighByte_works_for_ushort_values_over_8000h()
        {
            byte expected = 0xDE;
            var actual = ((ushort)0xDE12).GetHighByte();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetHighByte_works_for_short_values_under_8000h()
        {
            byte expected = 0x12;
            var actual = ((short)0x12DE).GetHighByte();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SetHighByte_works_for_ushort_values_over_8000h()
        {
            var expected = (ushort)0xDE12;
            var actual = ((ushort)0xFF12).SetHighByte(0xDE);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SetHighByte_works_for_short_values_under_8000()
        {
            short expected = 0x12DE;
            var actual = ((short)0x34DE).SetHighByte(0x12);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SetHighByte_works_for_short_values_over_8000()
        {
            short expected = 0xDE12 - 65536;
            var actual = ((short)0x3412).SetHighByte(0xDE);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetLowByte_works_for_shorts()
        {
            byte expected = 0xDE;
            var actual = ((short)0x12DE).GetLowByte();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetLowByte_works_for_ushorts()
        {
            byte expected = 0xDE;
            var actual = ((ushort)0xFFDE).GetLowByte();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SetLowByte_works_for_ushorts()
        {
            var expected = (ushort)0xDE12;
            var actual = ((ushort)0xDEFF).SetLowByte(0x12);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SetLowByte_works_for_shorts()
        {
            var expected = (short)0x12DE;
            var actual = ((short)0x12FF).SetLowByte(0xDE);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CreateShort_works_for_high_bytes_under_80h()
        {
            short expected = 0x12DE;
            var actual = NumberUtils.CreateShort(0xDE, 0x12);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CreateShort_works_for_high_bytes_over_80h()
        {
            short expected = 0xDE12 - 65536;
            var actual = NumberUtils.CreateShort(0x12, 0xDE);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetBit_works_for_lsb()
        {
            byte lsb0 = 0xFE;
            Assert.AreEqual(0, lsb0.GetBit(0));

            byte lsb1 = 0x01;
            Assert.AreEqual(1, lsb1.GetBit(0));
        }

        [Test]
        public void GetBit_works_for_msb()
        {
            byte msb0 = 0x7F;
            Assert.AreEqual(0, msb0.GetBit(7));

            byte msb1 = 0x80;
            Assert.AreEqual(1, msb1.GetBit(7));
        }

        [Test]
        public void GetBit_works_for_middle_bit()
        {
            byte bit4reset = 0x10 ^ 0xFF ;
            Assert.AreEqual(0, bit4reset.GetBit(4));

            byte bit4set = 0x10;
            Assert.AreEqual(1, bit4set.GetBit(4));
        }

        [Test]
        public void GetBit_fails_for_negative_bit_number()
        {
            Assert.Throws<InvalidOperationException>(() => ((byte)0).GetBit(-1));
        }

        [Test]
        public void GetBit_fails_for_bit_number_over_7()
        {
            Assert.Throws<InvalidOperationException>(() => ((byte)0).GetBit(8));
        }

        [Test]
        public void SetBit_works_for_lsb()
        {
            byte lsb0 = 0xFF;
            Assert.AreEqual(0xFE, lsb0.WithBit(0, 0));

            byte lsb1 = 0x00;
            Assert.AreEqual(0x01, lsb1.WithBit(0, 1));
        }

        [Test]
        public void SetBit_works_for_msb()
        {
            byte msb0 = 0xFF;
            Assert.AreEqual(0x7F, msb0.WithBit(7, 0));

            byte msb1 = 0x00;
            Assert.AreEqual(0x80, msb1.WithBit(7, 1));
        }

        [Test]
        public void SetBit_works_for_middle_bit()
        {
            byte bit4reset = 0xFF;
            Assert.AreEqual(0xEF, bit4reset.WithBit(4, 0));

            byte bit4set = 0x00;
            Assert.AreEqual(0x10, bit4set.WithBit(4, 1));
        }

        [Test]
        public void SetBit_fails_for_negative_bit_number()
        {
            Assert.Throws<InvalidOperationException>(() => ((byte)0).WithBit(-1, 0));
        }

        [Test]
        public void SetBit_fails_for_bit_number_over_7()
        {
            Assert.Throws<InvalidOperationException>(() => ((byte)0).WithBit(8, 0));
        }

        [Test]
        public void ToShort_works_for_numbers_below_8000h()
        {
            Assert.AreEqual((short)0x1234, 0x1234.ToShort());
        }

        [Test]
        public void ToShort_works_for_numbers_above_8000h()
        {
            Assert.AreEqual((short)-1, 0xFFFF.ToShort());
        }

        [Test]
        public void ToShort_works_for_8000h()
        {
            Assert.AreEqual((short)-32768, 0x8000.ToShort());
        }

        [Test]
        public void ToUshort_works_for_number_below_zero()
        {
            Assert.AreEqual((ushort)0xFFFF, (-1).ToUShort());
        }

        [Test]
        public void ToUshort_works_for_number_above_zero()
        {
            Assert.AreEqual((ushort)1, 1.ToUShort());
        }

        [Test]
        public void ToUshort_works_for_zero()
        {
            Assert.AreEqual((ushort)0, 0.ToUShort());
        }

        [Test]
        public void ToSignedByte_works_for_numbers_below_80h()
        {
            Assert.AreEqual((SByte)0x12, 0x12.ToSignedByte());
        }

        [Test]
        public void ToSignedByte_works_for_80h()
        {
            Assert.AreEqual((SByte)(-128), 0x80.ToSignedByte());
        }

        [Test]
        public void ToSignedByte_works_for_numbers_above_80h()
        {
            Assert.AreEqual((SByte)(-1), 0xFF.ToSignedByte());
        }

        [Test]
        public void Inc_short_works_for_non_boundary_values()
        {
            Assert.AreEqual(2, ((short)1).Inc());
        }

        [Test]
        public void Inc_short_works_for_boundary_values()
        {
            Assert.AreEqual(0, 0xFFFF.ToShort().Inc());
        }

        [Test]
        public void Inc_ushort_works_for_non_boundary_values()
        {
            Assert.AreEqual(2, ((ushort)1).Inc());
        }

        [Test]
        public void Inc_ushort_works_for_boundary_values()
        {
            Assert.AreEqual(0, 0xFFFF.ToUShort().Inc());
        }

        [Test]
        public void Dec_short_works_for_non_boundary_values()
        {
            Assert.AreEqual(1, ((short)2).Dec());
        }

        [Test]
        public void Dec_short_works_for_boundary_values()
        {
            Assert.AreEqual(0xFFFF.ToShort(), ((short)0).Dec());
        }

        [Test]
        public void Add_short_works_for_non_boundary_values()
        {
            Assert.AreEqual(5, ((short)2).Add(3));
        }

        [Test]
        public void Add_ushort_works_for_boundary_values()
        {
            Assert.AreEqual(1, 0xFFFE.ToUShort().Add(3));
        }

        [Test]
        public void Add_ushort_works_for_non_boundary_values()
        {
            Assert.AreEqual(5, ((ushort)2).Add(3));
        }

        [Test]
        public void Add_short_works_for_boundary_values()
        {
            Assert.AreEqual(1, 0xFFFE.ToShort().Add(3));
        }

        [Test]
        public void Sub_short_works_for_non_boundary_values()
        {
            Assert.AreEqual(2, ((short)5).Sub(3));
        }

        [Test]
        public void Sub_short_works_for_boundary_values()
        {
            Assert.AreEqual(0xFFFE.ToShort(), ((short)1).Sub(3));
        }

        [Test]
        public void Sub_ushort_works_for_non_boundary_values()
        {
            Assert.AreEqual(2, ((ushort)5).Sub(3));
        }

        [Test]
        public void Sub_ushort_works_for_boundary_values()
        {
            Assert.AreEqual(0xFFFE.ToUShort(), ((ushort)1).Sub(3));
        }

        [Test]
        public void Inc_byte_works_for_non_boundary_values()
        {
            Assert.AreEqual(2, ((byte)1).Inc());
        }

        [Test]
        public void Inc_byte_works_for_boundary_values()
        {
            Assert.AreEqual(0, ((byte)0xFF).Inc());
        }

        [Test]
        public void Dec_byte_works_for_non_boundary_values()
        {
            Assert.AreEqual(1, ((byte)2).Dec());
        }

        [Test]
        public void Dec_byte_works_for_boundary_values()
        {
            Assert.AreEqual(0xFF.ToShort(), ((byte)0).Dec());
        }

        [Test]
        public void Add_byte_works_for_non_boundary_values()
        {
            Assert.AreEqual(5, ((byte)2).Add(3));
        }

        [Test]
        public void Add_byte_works_for_boundary_values()
        {
            Assert.AreEqual(1, ((byte)0xFE).Add(3));
        }

        [Test]
        public void Sub_byte_works_for_non_boundary_values()
        {
            Assert.AreEqual(2, ((byte)5).Sub(3));
        }

        [Test]
        public void Sub_byte_works_for_boundary_values()
        {
            Assert.AreEqual(0xFE.ToShort(), ((byte)1).Sub(3));
        }

        [Test]
        public void Test7Bits_works_as_expected()
        {
            Assert.AreEqual(1, ((byte)0).Inc7Bits());
            Assert.AreEqual(0, ((byte)0x7F).Inc7Bits());
            Assert.AreEqual(0x81, ((byte)0x80).Inc7Bits());
            Assert.AreEqual(0x80, ((byte)0xFF).Inc7Bits());
        }

        [Test]
        public void Between_works_as_expected()
        {
            byte value = 0x80;

            Assert.False(value.Between(0, 0x7F));
            Assert.True(value.Between(0x7F, 0xFF));
            Assert.True(value.Between(0x80, 0xFF));
            Assert.False(value.Between(0x81, 0xFF));
        }

        [Test]
        public void AddSignedByte_works_for_positive_values()
        {
            Assert.AreEqual(0x8010, ((ushort)0x8000).AddSignedByte(0x10));
            Assert.AreEqual(0x807F, ((ushort)0x8000).AddSignedByte(0x7F));
        }

        [Test]
        public void AddSignedByte_works_for_negative_values()
        {
            Assert.AreEqual(0x7FF0, ((ushort)0x8000).AddSignedByte(0xF0));
            Assert.AreEqual(0x7F80, ((ushort)0x8000).AddSignedByte(0x80));
        }

        [Test]
        public void ToByteArray_works_for_shorts()
        {
            short value = 0x1234;

            var actual = value.ToByteArray();

            Assert.AreEqual(new byte[] {0x34, 0x12}, actual);
        }

        [Test]
        public void ToByteArray_works_for_ushorts()
        {
            ushort value = 0x1234;

            var actual = value.ToByteArray();

            Assert.AreEqual(new byte[] {0x34, 0x12}, actual);
        }
    }
}
