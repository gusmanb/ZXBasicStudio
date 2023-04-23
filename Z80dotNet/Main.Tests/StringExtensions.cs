using System;

namespace Konamiman.Z80dotNet.Tests
{
    public static class StringExtensions
    {
        public static byte AsBinaryByte(this string binaryString)
        {
            return (byte)Convert.ToInt32(binaryString.Replace(" ", ""), 2);
        }
    }
}
