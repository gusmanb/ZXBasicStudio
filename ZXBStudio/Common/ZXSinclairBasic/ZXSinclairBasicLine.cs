using AvaloniaEdit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.ZXSinclairBasic
{
    public class ZXSinclairBasicLine
    {
        const byte LINE_END = 0x0D;

        public List<ZXSinclairBasicToken> Tokens { get; private set; } = new List<ZXSinclairBasicToken>();
        public int LineNumber { get; private set; }
        public ZXSinclairBasicLine(int LineNumber, params ZXSinclairBasicToken[] Tokens) 
        {
            this.LineNumber = LineNumber;
            this.Tokens.AddRange(Tokens);
        }

        public override string ToString()
        {
            return $"{LineNumber} {String.Concat(Tokens.Select(t => t.ToString()))}";
        }

        public byte[] ToBinary()
        {
            List<byte> tokenBuffer = new List<byte>();

            foreach (var token in Tokens)
            {
                byte[] data = token.ToBinary();
                tokenBuffer.AddRange(data);
            }

            tokenBuffer.Add(LINE_END);

            List<byte> finalBuffer = new List<byte>();

            finalBuffer.AddRange(BitConverter.GetBytes((short)LineNumber).Reverse());
            finalBuffer.AddRange(BitConverter.GetBytes((short)tokenBuffer.Count));
            finalBuffer.AddRange(tokenBuffer);

            return finalBuffer.ToArray();
        }
    }
}
