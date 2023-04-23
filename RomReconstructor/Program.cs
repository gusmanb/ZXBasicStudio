using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

Regex regAddress = new Regex("^.\\$([0-9A-F]{4})");


string[] skoolSkips = new string[] { "; #LIST", "; LIST#", "@keep", "@nowarn", "@ignoreua", "@ssub", "@bfix", "@refs" };

string[] skoolLines = File.ReadAllLines("rom.skool");
string[] asmLines = File.ReadAllLines("rom.asm");

Dictionary<string, string[]> skoolGroups= new Dictionary<string, string[]>();
Dictionary<string, string[]> asmGroups= new Dictionary<string, string[]>();

Dictionary<string, SkoolLine[]> skoolClassGroups = new Dictionary<string, SkoolLine[]>();
Dictionary<string, AsmLine[]> asmClassGroups = new Dictionary<string, AsmLine[]>();

List<string> currentList = new List<string>();

List<SkoolLine> currentSkoolClassList = new List<SkoolLine>();
List<AsmLine> currentAsmClassList = new List<AsmLine>();

List<CompositeLine> finalLines = new List<CompositeLine>();

string currentGroup = "";
string lastLabel = "";
bool groupFound = false;
bool prevComment = false;
for (int buc = 0; buc < skoolLines.Length; buc++)
{
    if (!groupFound)
    {
        if (skoolLines[buc].StartsWith("@label="))
        {
            groupFound = true;
            currentGroup = skoolLines[buc].Substring(7) + ":";
            lastLabel = skoolLines[buc];
        }
    }
    else
    {
        if (string.IsNullOrWhiteSpace(skoolLines[buc]))
        {
            groupFound = false;
            skoolGroups[currentGroup] = currentList.ToArray();
            currentList.Clear();

            skoolClassGroups[currentGroup] = currentSkoolClassList.ToArray();
            currentSkoolClassList.Clear();
        }
        else
        {

            if (skoolLines[buc].Trim().StartsWith(";"))
                continue;

            if (skoolSkips.Any(l => skoolLines[buc].Trim().StartsWith(l)))
                continue;

            if (skoolLines[buc] == lastLabel)
                continue;

            if (skoolLines[buc].StartsWith("@label="))
                lastLabel = skoolLines[buc];

            currentList.Add(skoolLines[buc]);

            SkoolLine linw = new SkoolLine { Line = skoolLines[buc] };

            var match = regAddress.Match(linw.Line);

            if(match != null && match.Success)
                linw.Address= match.Groups[1].Value;

            currentSkoolClassList.Add(linw);
        }
    }
}

if (groupFound)
{
    groupFound = false;
    skoolGroups[currentGroup] = currentList.ToArray();
    currentList.Clear();
}

currentList.Clear();

for (int buc = 0; buc < asmLines.Length; buc++)
{
    if (!groupFound)
    {
        if (!asmLines[buc].StartsWith(";") && asmLines[buc].EndsWith(":"))
        {
            groupFound = true;
            currentGroup = asmLines[buc];
        }
    }
    else
    {
        if (string.IsNullOrWhiteSpace(asmLines[buc]))
        {
            groupFound = false;
            asmGroups[currentGroup] = currentList.ToArray();
            currentList.Clear();

            asmClassGroups[currentGroup] = currentAsmClassList.ToArray();
            currentAsmClassList.Clear();
        }
        else
        {
            if (asmLines[buc].Trim().StartsWith(";"))
                continue;

            currentList.Add(asmLines[buc]);
            AsmLine linw = new AsmLine { Line = asmLines[buc], LineNumber = buc };
            currentAsmClassList.Add(linw);
        }
    }
}

if (groupFound)
{
    groupFound = false;
    asmGroups[currentGroup] = currentList.ToArray();
    currentList.Clear();
}
Console.WriteLine("--Missing groups in ASM---");
foreach (var group in skoolGroups)
{
    if (!asmGroups.ContainsKey(group.Key))
    {
        Console.WriteLine($"Group missing: {group.Key}");
        foreach (var str in group.Value)
            Console.WriteLine($"    {str}");
    }
}
Console.WriteLine("--Missing groups in SKOOL---");
foreach (var group in asmGroups)
{
    if (!skoolGroups.ContainsKey(group.Key))
    {
        Console.WriteLine($"Group missing: {group.Key}");
        foreach (var str in group.Value)
            Console.WriteLine($"    {str}");
    }
}

Console.WriteLine("--Lines per group---");
foreach (var group in asmGroups)
{
    if (!skoolGroups.ContainsKey(group.Key))
        continue;
    var skoolGroup = skoolGroups[group.Key];
    if (skoolGroup.Length != group.Value.Length)
    {
        Console.WriteLine($"Mismatched lines: group={group.Key}, asm = {group.Value.Length}, skool={skoolGroup.Length}");
        Console.WriteLine("    ASM lines");
        foreach (var str in group.Value)
            Console.WriteLine($"        {str}");
        Console.WriteLine("    SKOOL lines");
        foreach (var str in skoolGroup)
            Console.WriteLine($"        {str}");
    }
}

foreach (var group in asmClassGroups)
{
    var skGroup = skoolClassGroups[group.Key];

    for (int buc = 0; buc < skGroup.Length; buc++)
    {
        CompositeLine line = new CompositeLine { LineNumber = group.Value[buc].LineNumber, ASMLine = group.Value[buc].Line };
        if (skGroup[buc].Address != null)
            line.Address = skGroup[buc].Address;

        finalLines.Add(line);
    }
}

StringBuilder sb = new StringBuilder();

for (int buc = 0; buc < asmLines.Length; buc++)
{
    var cLine = finalLines.FirstOrDefault(l => l.LineNumber== buc);

    if (cLine != null && cLine.Address != null)
        sb.AppendLine($"{cLine.Address}: {asmLines[buc]}");
    else
        sb.AppendLine(asmLines[buc]);
}

File.WriteAllText("romaddr.asm", sb.ToString());

List<LineAddress> map = new List<LineAddress>();

foreach (var line in finalLines)
{
    if (line.Address != null)
    {
        LineAddress ln = new LineAddress { Line = line.LineNumber, Address = ushort.Parse(line.Address, System.Globalization.NumberStyles.HexNumber) };
        map.Add(ln);
    }
}

string mapStr = JsonConvert.SerializeObject(map, Formatting.Indented);
File.WriteAllText("rom.map", mapStr);
Console.WriteLine();


class SkoolLine
{
    public string Address { get; set; }
    public string Line { get; set; }
}

class AsmLine
{
    public int LineNumber { get; set; }
    public string Line { get; set; }
}

class CompositeLine
{
    public int LineNumber { get; set; }
    public string Address { get; set; }
    public string ASMLine { get; set; }
}

class LineAddress
{
    public int Line { get; set; }
    public ushort Address { get; set; }
}