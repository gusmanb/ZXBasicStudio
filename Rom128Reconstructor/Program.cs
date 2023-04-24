//ROM assembly sources taken from http://www.fruitcake.plus.com/Sinclair/Spectrum128/ROMDisassembly/Spectrum128ROMDisassembly.htm
//PASMO: https://pasmo.speccy.org/

using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

Regex regSkip = new Regex("(^\\s*?;.*$)|(^\\s*[a-zA-Z0-9_]*\\s+EQU\\s+)|(^$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
Regex regLabel = new Regex("^([a-zA-Z0-9_]+)\\:", RegexOptions.IgnoreCase | RegexOptions.Multiline);
Regex regSymbol = new Regex("^([a-zA-Z0-9_]+)\\s+EQU\\s+([0-9a-fA-FH]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

Regex regSymbolLine = new Regex("^LINE_([0-9]+)_LABEL\\s+EQU\\s+([0-9a-fA-FH]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

string[] romLines = File.ReadAllLines("Spectrum128_ROM0.asm");
var reconstructedRom0128 = ProcessROMListing(romLines);
romLines = File.ReadAllLines("Spectrum128_ROM1.asm");
var reconstructedRom1128 = ProcessROMListing(romLines);
romLines = File.ReadAllLines("SpectrumPlus2_ROM0.asm");
var reconstructedRom0plus2 = ProcessROMListing(romLines);
romLines = File.ReadAllLines("SpectrumPlus2_ROM1.asm");
var reconstructedRom1plus2 = ProcessROMListing(romLines);

Directory.CreateDirectory("Recreated");

File.Copy("Spectrum128_ROM0.asm", "Recreated/128k_0.asm");
File.WriteAllBytes("Recreated/128k_0.bin", reconstructedRom0128.Binary);
File.WriteAllText("Recreated/128k_0.map", JsonConvert.SerializeObject(reconstructedRom0128.Map));

File.Copy("Spectrum128_ROM1.asm", "Recreated/128k_1.asm");
File.WriteAllBytes("Recreated/128k_1.bin", reconstructedRom1128.Binary);
File.WriteAllText("Recreated/128k_1.map", JsonConvert.SerializeObject(reconstructedRom1128.Map));

File.Copy("SpectrumPlus2_ROM0.asm", "Recreated/Plus2_0.asm");
File.WriteAllBytes("Recreated/Plus2_0.bin", reconstructedRom0plus2.Binary);
File.WriteAllText("Recreated/Plus2_0.map", JsonConvert.SerializeObject(reconstructedRom0plus2.Map));

File.Copy("SpectrumPlus2_ROM1.asm", "Recreated/Plus2_1.asm");
File.WriteAllBytes("Recreated/Plus2_1.bin", reconstructedRom1plus2.Binary);
File.WriteAllText("Recreated/Plus2_1.map", JsonConvert.SerializeObject(reconstructedRom1plus2.Map));

ReconstructedROM ProcessROMListing(string[] ROMLines)
{
    StringBuilder labeledSource = new StringBuilder();
    List<(string label, int number)> nativeLabels = new List<(string label, int number)>();

    for (int buc = 0; buc < ROMLines.Length; buc++)
    {
        string line = ROMLines[buc];

        if (regSkip.IsMatch(line))
        {
            labeledSource.AppendLine(line);
            continue;
        }

        var matchLabel = regLabel.Match(line);

        if(matchLabel != null && matchLabel.Success) 
        {
            nativeLabels.Add((matchLabel.Groups[1].Value, buc));
            labeledSource.AppendLine(line);
            continue;
        }

        line = $"LINE_{buc}_LABEL: " + line;
        labeledSource.AppendLine(line);
    }

    File.WriteAllText("composite.asm", labeledSource.ToString());

    Process p = new Process();
    p.StartInfo = new ProcessStartInfo("pasmo.exe") { Arguments = "composite.asm composite.bin composite.symbol" };
    p.Start();
    p.WaitForExit();

    string[] symbolLines = File.ReadAllLines("composite.symbol");

    List<LineAddress> addresses = new List<LineAddress>();

    foreach (var sLine in symbolLines) 
    {
        var match = regSymbolLine.Match(sLine);

        if (match != null && match.Success)
        {
            int line = int.Parse(match.Groups[1].Value);
            int address = int.Parse(match.Groups[2].Value.Replace("H", ""), System.Globalization.NumberStyles.HexNumber);

            addresses.Add(new LineAddress { Line = line, Address = (ushort)address });
        }
        else
        {
            match = regSymbol.Match(sLine);

            if (match != null && match.Success)
            { 
                var symbol = match.Groups[1].Value;
                int address = int.Parse(match.Groups[2].Value.Replace("H", ""), System.Globalization.NumberStyles.HexNumber);
                var tuple = nativeLabels.Where(n => n.label == symbol).FirstOrDefault();

                if(tuple != default)
                    addresses.Add(new LineAddress { Line = tuple.number, Address = (ushort)address });
            }
        }
    }

    byte[] binary = File.ReadAllBytes("composite.bin");

    return new ReconstructedROM { Binary = binary, Map = addresses.ToArray() };

}

class ReconstructedROM
{
    public required byte[] Binary { get; set; }
    public required LineAddress[] Map { get; set; }
}

class LineAddress
{
    public int Line { get; set; }
    public ushort Address { get; set; }
}