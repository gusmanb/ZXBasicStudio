using AvaloniaEdit.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXMemoryMap
    {
        static Regex regLine = new Regex("^([0-9A-F]+):[^\\n]*file__([0-9a-zA-Z_]+)__([0-9]*)", RegexOptions.Multiline);
        List<ZXCodeLine> lines = new List<ZXCodeLine>();

        public IEnumerable<ZXCodeLine> Lines { get { return lines; } }

        public ZXMemoryMap(string MapFile, IEnumerable<ZXCodeFile> Files) 
        {
            string mapContent = File.ReadAllText(MapFile);
            var matches = regLine.Matches(mapContent);

            Dictionary<string, ZXCodeFile> files = new Dictionary<string, ZXCodeFile>();

            foreach(var file in Files)
                files[file.FileGuid] = file;

            foreach(Match m in matches) 
            {
                var address = m.Groups[1].Value;
                var fileId = m.Groups[2].Value;
                var lineNumber = m.Groups[3].Value;

                if (!files.ContainsKey(fileId))
                    continue;

                var file = files[fileId];

                var line = new ZXCodeLine(file.FileType, file.AbsolutePath, int.Parse(lineNumber), ushort.Parse(address, System.Globalization.NumberStyles.HexNumber));
                lines.Add(line);
            }

            var dupes = lines.GroupBy(l => l.Address).Where(g => g.Count() > 1).ToArray();

            foreach(var dupe in dupes)
            {
                var dLines = dupe.OrderBy(d => d.LineNumber).Take(dupe.Count() - 1);
                foreach (var dline in dLines)
                    lines.Remove(dline);
            }
        }
    }
}
