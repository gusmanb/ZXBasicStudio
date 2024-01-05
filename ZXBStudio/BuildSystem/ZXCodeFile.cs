using Avalonia.Svg.Skia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic;

namespace ZXBasicStudio.BuildSystem
{
    public class ZXCodeFile
    {
        static Regex regInclude = new Regex("^\\s*?\\#include\\s+[\"']([^\"']+)[\"'][^\\n]*", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regOrg = new Regex("(^|\\s)org ([0-9]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regAsmExclude = new Regex("(^\\s*?PROC.*?$)|(^\\s*?LOCAL.*?$)|(^\\s*?ENDP.*?$)|(^\\s*?ORG.*?$)|(^\\s*?END.*?$)|(^\\s*?NAMESPACE.*?$)|(^\\s*?ALIGN.*?$)|(^\\s*?pop\\snamespace.*?$)|(^\\s*?push\\snamespace.*?$)|(^\\s*?;.*?$)|(\\sEQU\\s)|(^\\s*#)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regBasicExclude = new Regex("((^|\\s)+DIM\\s[^:]+$|(^\\s*ASM(\\s|\\r|\\n|$))|(^\\s*END\\s*ASM(\\s|\\r|\\n|$)))|(^\\s*')|(^\\s*REM)|(^\\s*#)|(^\\s*(fastcall)?(sub|function))|(^\\s*loop\\s)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regMultiline = new Regex("^.*?(\\\\|^_|[^\\w]+_)(\\s*$|\\s*REM|\\s*').*?", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regEndAsm = new Regex("^\\s*end\\s*asm", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regFindLabelNumbers = new Regex("^\\.LABEL\\.__LABEL([0-9]+)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regFindUsedLabels = new Regex("^.*?[^\\s]+.*?(\\.LABEL\\._file__[^\\s\\n:]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regClean = new Regex("^(\\.LABEL\\._)?file__[^:\\n]*?:(\\ )?", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regRemoveEmpty = new Regex("^\\s*(\\r)?\\n", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public string Name { get; set; }
        public string Directory { get; set; }
        public string AbsolutePath { get; set; }
        public string TempFileName { get; set; }
        public string FileGuid { get; private set; } = Guid.NewGuid().ToString().Replace("-", "_");
        public string Content { get; set; }
        public string BuildContent { get; set; }
        public ZXFileType FileType { get; set; }

        public ZXCodeFile(string FilePath, bool Disassembly = false)
        {
            var fName = Path.GetFileName(FilePath);

            if (fName == null)
                throw new ArgumentException("Invalid file");

            Name = fName;

            AbsolutePath = Path.GetFullPath(FilePath);

            var dName = Path.GetDirectoryName(AbsolutePath);

            if (dName == null)
                throw new ArgumentException("Invalid file");

            Directory = dName;

            var docType = ZXDocumentProvider.GetDocumentType(Name);

            if (docType is ZXBasicDocument)
                FileType = ZXFileType.Basic;
            else if (docType is ZXAssemblerDocument)
                FileType = ZXFileType.Assembler;
            else
                throw new ArgumentException("Invalid file");

            TempFileName = Path.GetFileNameWithoutExtension(FilePath) + ".buildtemp" + Path.GetExtension(FilePath);

            Content = File.ReadAllText(FilePath);

            if (Disassembly)
                ContentCleanup();
        }

        void ContentCleanup()
        {
            if (Content == null) return;

            var labelMatches = regFindLabelNumbers.Matches(Content);

            int maxLabel = 0;

            foreach (Match labelMatch in labelMatches)
            {
                int num = int.Parse(labelMatch.Groups[1].Value);
                if (num > maxLabel)
                    maxLabel = num;
            }
            maxLabel++;

            var usedLabels = regFindUsedLabels.Matches(Content);

            foreach (Match usedLabel in usedLabels)
            {
                var label = usedLabel.Groups[1].Value;
                string replaceLabel = $".LABEL.__LABEL{maxLabel++}";
                Content = Content.Replace(label, replaceLabel);
            }

            Content = regClean.Replace(Content, "");
            Content = regRemoveEmpty.Replace(Content, "");
        }

        public void CreateBuildFile(IEnumerable<ZXCodeFile> AllFiles)
        {
            string content = Content;
            string[] lines = content.Replace("\r", "").Split("\n");

            if (FileType == ZXFileType.Assembler)
            {

                StringBuilder sb = new StringBuilder();
                StringBuilder sbSource = new StringBuilder();

                int lineIndex = 0;

                for (int lineNum = 0; lineNum < lines.Length; lineNum++)
                {
                    string line = lines[lineNum];
                    string trim = line.Trim().ToLower();

                    if (string.IsNullOrWhiteSpace(trim))
                    {
                        sbSource.AppendLine(line);
                        sb.AppendLine(line);
                        continue;
                    }

                    if (trim.StartsWith("#line"))
                        continue;

                    if (regInclude.IsMatch(line))
                    {
                        var file = regInclude.Match(line).Groups[1].Value;
                        var absoluteFile = Path.GetFullPath(Path.Combine(Directory, file));

                        var codeFile = AllFiles.FirstOrDefault(f => f.AbsolutePath == absoluteFile);

                        if (codeFile != null)
                        {
                            sbSource.AppendLine(line);
                            line = line.Replace(file, Path.Combine(codeFile.Directory, codeFile.TempFileName));
                            sb.AppendLine(line);
                        }
                        else
                        {
                            sbSource.AppendLine(line);
                            sb.AppendLine(line);
                        }
                    }

                    else if (regAsmExclude.IsMatch(trim))
                    {
                        sbSource.AppendLine(line);
                        sb.AppendLine(line);
                    }
                    else
                    {
                        sbSource.AppendLine(line);
                        line = $"file__{FileGuid}__{lineIndex}: {line}";
                        sb.AppendLine(line);
                    }

                    lineIndex++;
                }

                BuildContent = sb.ToString();

                bool doTrim = true;

                while (doTrim)
                {
                    BuildContent.TrimEnd();

                    if (BuildContent.EndsWith("\r") || BuildContent.EndsWith("\n"))
                    {

                        if (BuildContent.EndsWith("\r\n"))
                            BuildContent = BuildContent.Substring(0, BuildContent.Length - 2);
                        else
                            BuildContent = BuildContent.Substring(0, BuildContent.Length - 1);

                    }
                    else
                        doTrim = false;
                }

                File.WriteAllText(Path.Combine(Directory, TempFileName), BuildContent);

                Content = sbSource.ToString();

                doTrim = true;

                while (doTrim)
                {

                    Content.TrimEnd();

                    if (Content.EndsWith("\r") || Content.EndsWith("\n"))
                    {

                        if (Content.EndsWith("\r\n"))
                            Content = Content.Substring(0, Content.Length - 2);
                        else
                            Content = Content.Substring(0, Content.Length - 1);

                    }
                    else
                        doTrim = false;
                }
            }
            else
            {
                bool prevMultiLine = false;
                bool inAsm = false;
                StringBuilder sb = new StringBuilder();

                for (int buc = 0; buc < lines.Length; buc++)
                {
                    var line = lines[buc];
                    string trim = line.Trim().ToLower();

                    if (string.IsNullOrWhiteSpace(trim))
                    {
                        sb.AppendLine(line);
                        continue;
                    }

                    if (!inAsm)
                        inAsm = line.Trim().ToLower().StartsWith("asm");
                    else
                        inAsm = !regEndAsm.IsMatch(line);

                    if (!prevMultiLine)
                    {

                        if (regInclude.IsMatch(line))
                        {
                            var file = regInclude.Match(line).Groups[1].Value;
                            var absoluteFile = Path.GetFullPath(Path.Combine(Directory, file));

                            var codeFile = AllFiles.FirstOrDefault(f => f.AbsolutePath == absoluteFile);

                            if (codeFile != null)
                                line = line.Replace(file, Path.Combine(codeFile.Directory, codeFile.TempFileName));
                        }
                        else if (inAsm && !regAsmExclude.IsMatch(line))
                        {
                            line = $"file__{FileGuid}__{buc}: {line}";
                        }
                        else if (!inAsm && !string.IsNullOrWhiteSpace(line) && !regBasicExclude.IsMatch(line))
                        {
                            line = $"file__{FileGuid}__{buc}: {line}";
                        }

                    }

                    prevMultiLine = regMultiline.IsMatch(line);

                    sb.AppendLine(line);
                }

                BuildContent = sb.ToString();

                bool dotrim = true;

                while (dotrim)
                {

                    BuildContent.TrimEnd();

                    if (BuildContent.EndsWith("\r") || BuildContent.EndsWith("\n"))
                    {

                        if (BuildContent.EndsWith("\r\n"))
                            BuildContent = BuildContent.Substring(0, BuildContent.Length - 2);
                        else
                            BuildContent = BuildContent.Substring(0, BuildContent.Length - 1);

                    }
                    else
                        dotrim = false;
                }

                File.WriteAllText(Path.Combine(Directory, TempFileName), sb.ToString());
            }
        }

        public ushort FindOrg()
        {
            if (FileType != ZXFileType.Assembler)
                throw new InvalidOperationException("Only assembler files have an ORG.");

            var orgs = regOrg.Matches(Content);
            ushort orgValue = 0xFFFF;

            foreach (Match org in orgs)
            {
                ushort currentOrg = ushort.Parse(org.Groups[2].Value);
                if (currentOrg < orgValue)
                    orgValue = currentOrg;
            }

            return orgValue;
        }

        public string CreateCompleteBuildSource(IEnumerable<ZXCodeFile> AllFiles)
        {
            StringBuilder sb = new StringBuilder();

            string[] lines = Content.Replace("\r", "").Split("\n");

            for (int buc = 0; buc < lines.Length; buc++)
            {
                var line = lines[buc];

                if (!regInclude.IsMatch(line))
                    sb.AppendLine(line);
                else
                {
                    var file = regInclude.Match(line).Groups[1].Value;
                    var absoluteFile = Path.GetFullPath(Path.Combine(Directory, file));

                    var codeFile = AllFiles.Where(f => f.AbsolutePath.ToLower() == absoluteFile.ToLower()).FirstOrDefault();

                    if (codeFile == null)
                        sb.AppendLine(line);
                    else
                        sb.AppendLine(codeFile.CreateCompleteBuildSource(AllFiles));
                }
            }

            return sb.ToString();
        }
    }
}
