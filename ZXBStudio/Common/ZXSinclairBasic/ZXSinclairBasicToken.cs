using Avalonia.Controls;
using AvaloniaEdit.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.BuildSystem;

namespace ZXBasicStudio.Common.ZXSinclairBasic
{
    public class ZXSinclairBasicToken
    {
        public ZXSinclairBasicTokenType TokenType { get; private set; }
        public string? StringContent { get; set; }
        public double? FloatContent { get; set; }
        public int? IntegerContent { get; set; }
        public ZXSinclairBasicInstruction? Instruction { get; set; }
        public ZXSinclairBasicToken(ZXSinclairBasicInstruction Instruction) 
        {
            this.Instruction = Instruction;
            TokenType =  ZXSinclairBasicTokenType.Instruction;
        }
        public ZXSinclairBasicToken(string StringContent)
        {
            this.TokenType = ZXSinclairBasicTokenType.String;
            this.StringContent = StringContent;

            if (string.IsNullOrEmpty(StringContent))
                throw new ArgumentException("String content cannot be empty");
        }

        public ZXSinclairBasicToken(int IntegerContent)
        {
            this.TokenType = ZXSinclairBasicTokenType.IntegerNumber;
            this.IntegerContent = IntegerContent;
        }

        public ZXSinclairBasicToken(double FloatContent)
        {
            this.TokenType = ZXSinclairBasicTokenType.FloatNumber;
            this.FloatContent = FloatContent;
        }

        public override string ToString()
        {
            switch(TokenType) 
            {
                case ZXSinclairBasicTokenType.String:
                    return (StringContent ?? "") + " ";
                case ZXSinclairBasicTokenType.FloatNumber:
                    return (FloatContent?.ToString() ?? "") + " ";
                case ZXSinclairBasicTokenType.IntegerNumber:
                    return (IntegerContent?.ToString() ?? "") + " ";
                default:
                    return (Instruction?.ToString() ?? "") + " ";
            }
        }

        public byte[] ToBinary()
        {

            switch (TokenType)
            {
                case ZXSinclairBasicTokenType.String:
                    if (StringContent == null)
                        throw new InvalidOperationException("StringContent was null!");

                    return Encoding.ASCII.GetBytes(StringContent);

                case ZXSinclairBasicTokenType.FloatNumber:
                    {
                        if (FloatContent == null)
                            throw new InvalidOperationException("FloatContent was null!");

                        byte[]? data = ZXVariableHelper.GetBytesFloat40(FloatContent.Value);

                        if (data == null)
                            throw new InvalidOperationException("Cannot cast numeric value to Sinclair float!");

                        List<byte> finalData = new List<byte>();
                        finalData.AddRange(Encoding.ASCII.GetBytes(ToString().Trim()));
                        finalData.Add(0x0E);
                        finalData.AddRange(data.Reverse());

                        return finalData.ToArray();
                    }
                case ZXSinclairBasicTokenType.IntegerNumber:
                    {
                        if (IntegerContent == null)
                            throw new InvalidOperationException("IntegerContent was null!");

                        List<byte> intData = new List<byte>();
                        intData.Add(0);
                        intData.AddRange(BitConverter.GetBytes((ushort)(Math.Abs(IntegerContent.Value))).Reverse());
                        intData.Add(0);
                        intData.Add(0);
                        intData.Reverse();

                        List<byte> finalData = new List<byte>();
                        finalData.AddRange(Encoding.ASCII.GetBytes(ToString().Trim()));
                        finalData.Add(0x0E);
                        finalData.AddRange(intData);

                        return finalData.ToArray();
                    }
                default:
                    if (Instruction == null)
                        throw new InvalidOperationException("Instruction was null!");

                    return new byte[] { (byte)Instruction };
            }

        }

        public static implicit operator ZXSinclairBasicToken(string value) => new ZXSinclairBasicToken(value);
        public static implicit operator ZXSinclairBasicToken(int value) => new ZXSinclairBasicToken(value);
        public static implicit operator ZXSinclairBasicToken(float value) => new ZXSinclairBasicToken(value);
        public static implicit operator ZXSinclairBasicToken(double value) => new ZXSinclairBasicToken(value);
        public static implicit operator ZXSinclairBasicToken(ZXSinclairBasicInstruction value) => new ZXSinclairBasicToken(value);
    }

    public enum ZXSinclairBasicTokenType
    {
        String,
        IntegerNumber,
        FloatNumber,
        Instruction
    }
}
