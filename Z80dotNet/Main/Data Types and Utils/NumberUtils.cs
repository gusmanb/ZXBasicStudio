using System;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Class with utility static and extension methods for manipulating numbers.
    /// </summary>
    public static class NumberUtils
    {
        /// <summary>
        /// Gets the high byte of a short value.
        /// </summary>
        /// <param name="value">Number to get the high byte from</param>
        /// <returns>High byte of the number</returns>
        public static byte GetHighByte(this short value)
        {
            return (byte)(value >> 8);
        }

        /// <summary>
        /// Gets the high byte of an ushort value.
        /// </summary>
        /// <param name="value">Number to get the high byte from</param>
        /// <returns>High byte of the number</returns>
        public static byte GetHighByte(this ushort value)
        {
            return (byte)(value >> 8);
        }

        /// <summary>
        /// Returns a modified version of an ushort number that has
        /// the specified value in the high byte.
        /// </summary>
        /// <param name="value">Original number</param>
        /// <param name="highByte">New high byte</param>
        /// <returns>Number with the original low byte and the new high byte</returns>
        public static ushort SetHighByte(this ushort value, byte highByte)
        {
            return (ushort)((value & 0x00FF) | (highByte << 8));
        }

        /// <summary>
        /// Returns a modified version of a short number that has
        /// the specified value in the high byte.
        /// </summary>
        /// <param name="value">Original number</param>
        /// <param name="highByte">New high byte</param>
        /// <returns>Number with the original low byte and the new high byte</returns>
        public static short SetHighByte(this short value, byte highByte)
        {
            return ((value & 0x00FF) | (highByte << 8)).ToShort();
        }

        /// <summary>
        /// Gets the low byte of a short value.
        /// </summary>
        /// <param name="value">Number to get the low byte from</param>
        /// <returns>Loq byte of the number</returns>
        public static byte GetLowByte(this short value)
        {
            return (byte)(value & 0xFF);
        }

        /// <summary>
        /// Gets the low byte of an ushort value.
        /// </summary>
        /// <param name="value">Number to get the low byte from</param>
        /// <returns>Loq byte of the number</returns>
        public static byte GetLowByte(this ushort value)
        {
            return (byte)(value & 0xFF);
        }

        /// <summary>
        /// Returns a modified version of an ushort number that has
        /// the specified value in the low byte.
        /// </summary>
        /// <param name="value">Original number</param>
        /// <param name="lowByte">New low byte</param>
        /// <returns>Number with the original high byte and the new low byte</returns>
        public static ushort SetLowByte(this ushort value, byte lowByte)
        {
            return (ushort)((value & 0xFF00) | lowByte);
        }

        /// <summary>
        /// Returns a modified version of a short number that has
        /// the specified value in the low byte.
        /// </summary>
        /// <param name="value">Original number</param>
        /// <param name="lowByte">New low byte</param>
        /// <returns>Number with the original high byte and the new low byte</returns>
        public static short SetLowByte(this short value, byte lowByte)
        {
            return (short)((value & 0xFF00) | lowByte);
        }

        /// <summary>
        /// Generates a short number from two bytes.
        /// </summary>
        /// <param name="lowByte">Low byte of the new number</param>
        /// <param name="highByte">High byte of the new number</param>
        /// <returns>Generated number</returns>
        public static short CreateShort(byte lowByte, byte highByte)
        {
            return (short)((highByte << 8) | lowByte);
        }

        /// <summary>
        /// Generates a ushort number from two bytes.
        /// </summary>
        /// <param name="lowByte">Low byte of the new number</param>
        /// <param name="highByte">High byte of the new number</param>
        /// <returns>Generated number</returns>
        public static ushort CreateUshort(byte lowByte, byte highByte)
        {
            return (ushort)((highByte << 8) | lowByte);
        }

        /// <summary>
        /// Gets the value of a certain bit in a byte.
        /// The rightmost bit has position 0, the leftmost bit has position 7.
        /// </summary>
        /// <param name="value">Number to get the bit from</param>
        /// <param name="bitPosition">Bit position to retrieve</param>
        /// <returns>Retrieved bit value</returns>
        public static Bit GetBit(this byte value, int bitPosition)
        {
            if(bitPosition < 0 || bitPosition > 7)
                throw new InvalidOperationException("bit position must be between 0 and 7");

            return (value & (1 << bitPosition));
        }

        /// <summary>
        /// Retuens a copy of the value that has a certain bit set or reset.
        /// The rightmost bit has position 0, the leftmost bit has position 7.
        /// </summary>
        /// <param name="number">The original number</param>
        /// <param name="bitPosition">The bit position to modify</param>
        /// <param name="value">The bit value</param>
        /// <returns>The original number with the bit appropriately modified</returns>
        public static byte WithBit(this byte number, int bitPosition, Bit value)
        {
            if(bitPosition < 0 || bitPosition > 7)
                throw new InvalidOperationException("bit position must be between 0 and 7");

            if(value) 
            {
                return (byte)(number | (1 << bitPosition));
            }
            else 
            {
                return (byte)(number & ~(1 << bitPosition));
            }
        }

        /// <summary>
        /// Converts a number to short by substracting 65536 when the number is 32768 or higher.
        /// </summary>
        /// <param name="value">Number to convert</param>
        /// <returns>Converted number</returns>
        public static short ToShort(this int value)
        {
            return (short)(ushort)value;
        }

        /// <summary>
        /// Converts a number to short by substracting 65536 when the number is 32768 or higher.
        /// </summary>
        /// <param name="value">Number to convert</param>
        /// <returns>Converted number</returns>
        public static short ToShort(this ushort value)
        {
            return (short)value;
        }

        /// <summary>
        /// Converts a number to ushort by adding 65536 when the number is negative.
        /// </summary>
        /// <param name="value">Number to convert</param>
        /// <returns>Converted number</returns>
        public static ushort ToUShort(this int value)
        {
            return (ushort)value;
        }

        /// <summary>
        /// Converts a number to ushort by adding 65536 when the number is negative.
        /// </summary>
        /// <param name="value">Number to convert</param>
        /// <returns>Converted number</returns>
        public static ushort ToUShort(this short value)
        {
            return (ushort)value;
        }

        /// <summary>
        /// Converts a number to signed byte by substracting 256 when the number is 128 or higher.
        /// </summary>
        /// <param name="value">Number to convert</param>
        /// <returns>Converted number</returns>
        public static SByte ToSignedByte(this int value)
        {
            return (SByte)value;
        }

        /// <summary>
        /// Converts a number to signed byte by substracting 256 when the number is 128 or higher.
        /// </summary>
        /// <param name="value">Number to convert</param>
        /// <returns>Converted number</returns>
        public static SByte ToSignedByte(this byte value)
        {
            return (SByte)value;
        }

        /// <summary>
        /// Increases a number and turns it into min value if it has its maximum value.
        /// </summary>
        /// <param name="value">Number to increase</param>
        /// <returns>Increased number, or zero</returns>
        public static short Inc(this short value)
        {
            return (short)(value + 1);
        }

        /// <summary>
        /// Increases a number and turns it into zero if it has its maximum value.
        /// </summary>
        /// <param name="value">Number to increase</param>
        /// <returns>Increased number, or zero</returns>
        public static ushort Inc(this ushort value)
        {
            return (ushort)(value + 1);
        }

        /// <summary>
        /// Decreases a number and turns it into max value if it passes under its minimum value.
        /// </summary>
        /// <param name="value">Number to increase</param>
        /// <returns>Increased number, or zero</returns>
        public static ushort Dec(this ushort value)
        {
            return (ushort)(value - 1);
        }

        /// <summary>
        /// Decreases a number and turns it into max value if it passes under its minimum value.
        /// </summary>
        /// <param name="value">Number to decrease</param>
        /// <returns>Increased number, or zero</returns>
        public static short Dec(this short value)
        {
            return (short)(value - 1);
        }

        /// <summary>
        /// Adds a value to a number and overlaps it from zero if it passes its maximum value.
        /// </summary>
        /// <param name="value">Number to be added to</param>
        /// <param name="amount">Number to add</param>
        /// <returns>Increased number, or zero</returns>
        public static short Add(this short value, short amount)
        {
            return (short)(value + amount);
        }

        /// <summary>
        /// Adds a value to a number and overlaps it from zero if it passes its maximum value.
        /// </summary>
        /// <param name="value">Number to increase</param>
        /// <returns>Increased number, or zero</returns>
        public static ushort Add(this ushort value, ushort amount)
        {
            return (ushort)(value + amount);
        }

        /// <summary>
        /// Adds a value to a number and overlaps it from zero if it passes its maximum value.
        /// </summary>
        /// <param name="value">Number to increase</param>
        /// <returns>Increased number, or zero</returns>
        public static ushort Add(this ushort value, SByte amount)
        {
            return (ushort)(value + amount);
        }

        /// <summary>
        /// Substract a value to a number and overlaps it from its max value if it passes below its minimum value.
        /// </summary>
        /// <param name="value">Number to decrease</param>
        /// <returns>Increased number, or zero</returns>
        public static short Sub(this short value, short amount)
        {
            return (short)(value - amount);
        }

        /// <summary>
        /// Substract a value to a number and overlaps it from its max value if it passes below its minimum value.
        /// </summary>
        /// <param name="value">Number to decrease</param>
        /// <returns>Increased number, or zero</returns>
        public static ushort Sub(this ushort value, ushort amount)
        {
            return (ushort)(value - amount);
        }

        /// <summary>
        /// Increases a number and turns it into zero if it has its maximum value.
        /// </summary>
        /// <param name="value">Number to increase</param>
        /// <returns>Increased number, or zero</returns>
        public static byte Inc(this byte value)
        {
            return (byte)(value + 1);
        }

        /// <summary>
        /// Decreases a number and turns it into max value if it passes under its minimum value.
        /// </summary>
        /// <param name="value">Number to decrease</param>
        /// <returns>Increased number, or zero</returns>
        public static byte Dec(this byte value)
        {
            return (byte)(value - 1);

        }

        /// <summary>
        /// Adds a value to the number and overlaps it from zero if it passes its maximum value.
        /// </summary>
        /// <param name="value">Number to increase</param>
        /// <param name="amount">Amount to add to the number</param>
        /// <returns>Increased number, or zero</returns>
        public static short Add(this byte value, byte amount)
        {
            return (byte)(value + amount);
        }

        /// <summary>
        /// Adds a value to the number and overlaps it from zero if it passes its maximum value.
        /// </summary>
        /// <param name="value">Number to increase</param>
        /// <param name="amount">Amount to add to the number</param>
        /// <returns>Increased number, or zero</returns>
        public static short Add(this byte value, int amount)
        {
            return (byte)(value + (byte)amount);
        }

        /// <summary>
        /// Substract a value to the number and overlaps it from its max value if it passes below its minimum value.
        /// </summary>
        /// <param name="value">Number to decrease</param>
        /// <param name="amount">Amount to substract to the number</param>
        /// <returns>Increased number, or zero</returns>
        public static short Sub(this byte value, byte amount)
        {
            return (short)(value - amount);
        }

        /// <summary>
        /// Substract a value to the number and overlaps it from its max value if it passes below its minimum value.
        /// </summary>
        /// <param name="value">Number to decrease</param>
        /// <param name="amount">Amount to substract to the number</param>
        /// <returns>Increased number, or zero</returns>
        public static short Sub(this byte value, int amount)
        {
            return (byte)(value - (byte)amount);
        }

        /// <summary>
        /// Increments the value using only the lowest seven bits (the most significant bit is unchanged).
        /// </summary>
        /// <param name="value">Number to increment</param>
        /// <returns>Incremented number</returns>
        public static byte Inc7Bits(this byte value)
        {
            return (byte)((value & 0x80)==0 ? (value+1) & 0x7F : (value+1) & 0x7F | 0x80);	
        }

        /// <summary>
        /// Checks if the value lies in a specified range.
        /// </summary>
        /// <param name="value">The number to check</param>
        /// <param name="fromInclusive">The lower end of the range</param>
        /// <param name="toInclusive">The higher end of the range</param>
        /// <returns>True if the value lies within the range, false otherwise</returns>
        public static bool Between(this byte value, byte fromInclusive, byte toInclusive)
        {
            return value >= fromInclusive && value <= toInclusive;
        }

        /// <summary>
        /// Adds a byte interpreted as a signed value.
        /// </summary>
        /// <param name="value">Number to increase or decrease</param>
        /// <param name="amount">Amount to be added or substracted</param>
        /// <returns>Updated value</returns>
        public static ushort AddSignedByte(this ushort value, byte amount)
        {
            return amount < 0x80 ? value.Add(amount) : value.Sub((ushort)(256 - amount));
        }

        /// <summary>
        /// Generates a byte array from the low and high byte of the value.
        /// </summary>
        /// <param name="value">Original value</param>
        /// <returns>Generated byte array</returns>
        public static byte[] ToByteArray(this short value)
        {
            return new[] { value.GetLowByte(), value.GetHighByte() };
        }

        /// <summary>
        /// Generates a byte array from the low and high byte of the value.
        /// </summary>
        /// <param name="value">Original value</param>
        /// <returns>Generated byte array</returns>
        public static byte[] ToByteArray(this ushort value)
        {
            return new[] { value.GetLowByte(), value.GetHighByte() };
        }
    }
}
