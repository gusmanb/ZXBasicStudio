using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Common.ZXSinclairBasic
{
    public class ZXSinclairBasicProgram
    {
        public int? AutostartLine { get; set; }
        public string? ProgramName { get; set; }
        public List<ZXSinclairBasicLine> Lines { get; private set; } = new List<ZXSinclairBasicLine>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var line in Lines)
                sb.AppendLine(line.ToString());

            return sb.ToString();
        }

        public byte[] ToBinary()
        {
            List<byte> buffer = new List<byte>();

            foreach (var line in Lines)
                buffer.AddRange(line.ToBinary());

            return buffer.ToArray();
        }

        public byte[] ToTAP()
        {
            List<byte> tapContent = new List<byte>();
            tapContent.Add(19);
            tapContent.Add(0);
            tapContent.Add(0);

            

            tapContent.Add(0);

            string name = (ProgramName ?? "").PadRight(10, ' ').Substring(0, 10);

            tapContent.AddRange(Encoding.ASCII.GetBytes(name));

            byte[] programData = ToBinary();

            tapContent.AddRange(BitConverter.GetBytes((ushort)programData.Length));
            tapContent.AddRange(BitConverter.GetBytes((ushort)(AutostartLine ?? 65535)));
            tapContent.AddRange(BitConverter.GetBytes((ushort)programData.Length));

            byte xSum = 0;
            foreach (var bte in tapContent.Skip(2))
                xSum ^= bte;

            tapContent.Add(xSum);

            tapContent.AddRange(BitConverter.GetBytes((ushort)(programData.Length + 2)));

            int toSkip = tapContent.Count;

            tapContent.Add(0xFF);
            tapContent.AddRange(programData);

            xSum = 0;

            foreach (var bte in tapContent.Skip(toSkip))
                xSum ^= bte;

            tapContent.Add(xSum);

            return tapContent.ToArray();
        }
    }
}
