using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.Folding
{
    public class ZXBasicFoldingStrategy : AbstractFoldingStrategy
    {
        Regex regStartSubFold = new Regex("(^|:)[^\\S\\r\\n^:]*(fastcall)?[^\\S\\r\\n^:]*?(sub|function)\\s+(fastcall\\s+)?([^\\s\\(]+)\\([^\\n]*?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regEndSubFold = new Regex("(^|[^\\S\\r\\n^:]+|:)[^\\S\\r\\n^:]*?end\\s+(sub|function)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        Regex regStartCommentFold = new Regex("/'", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regEndCommentFold = new Regex("'/", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        Regex regStartDimMulti = new Regex("dim\\s+([^\\s\\(\\)\\:]+)[^\\n]*_\\s*('[^\\n]*|rem\\s+)?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regEndDimMulti = new Regex("^[^_]*?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        Regex regStartRegion = new Regex("^[^\\S$\\n]*'region\\s([^$\\n]*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regEndRegion = new Regex("^[^\\S$\\n]*'end region[^\\S$\\n]*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        Regex regStartMultiDefine = new Regex("^[^\\S$\\n]*?#define\\s+(\\w+(\\([^\\)]*\\)?))[^\\n]*?\\\\[^\\S$\\n]*?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        Regex regEndMultiDefine = new Regex("^[^\\\\]*?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            int subError = -1;
            var subFoldings = CreateSubFunctionFoldings(document, out subError);
            int commentError = -1;
            var commentFoldings = CreateCommentFoldings(document, out commentError);
            int dimError = -1;
            var dimFoldings = CreateMultiDimFoldings(document, out dimError);
            int regionError = -1;
            var regionFoldings = CreateRegionFoldings(document, out regionError);
            int defineError = -1;
            var defineFoldings = CreateMultiDefineFoldings(document, out defineError);

            List<NewFolding> allFoldings = new List<NewFolding>();
            allFoldings.AddRange(subFoldings);
            allFoldings.AddRange(commentFoldings);
            allFoldings.AddRange(dimFoldings);
            allFoldings.AddRange(regionFoldings);
            allFoldings.AddRange(defineFoldings);

            int[] errs = new int[] { subError, commentError, dimError, regionError, defineError };

            firstErrorOffset = !errs.Any(e => e != -1) ? -1 : errs.Where(e => e != -1).Min();

            return allFoldings.OrderBy(f => f.StartOffset);
        }
        private IEnumerable<NewFolding> CreateSubFunctionFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            List<NewFolding> foldings = new List<NewFolding>();
            var startFoldings = regStartSubFold.Matches(document.Text).ToArray();
            var endFoldings = regEndSubFold.Matches(document.Text).ToArray();

            if (startFoldings.Length != endFoldings.Length) //Unclosed sub/function?
            {
                firstErrorOffset = startFoldings.FirstOrDefault()?.Index ?? 0;
                return foldings;
            }

            for (int buc = 0; buc < startFoldings.Length; buc++)
            {
                var start = startFoldings[buc];
                var end = endFoldings[buc];
                var next = buc < startFoldings.Length - 1 ? startFoldings[buc + 1] : null;

                if (next != null)
                {
                    if (next.Index <= end.Index)
                    {
                        firstErrorOffset = buc;
                        return foldings;
                    }
                }

                var name = "...";

                int subOffset = start.Value.EndsWith("\r") ? -1 : 0;

                foldings.Add(new NewFolding { StartOffset = start.Index + start.Length + subOffset, EndOffset = end.Index + end.Length, DefaultClosed = false, IsDefinition = false, Name = name });
            }

            return foldings;
        }
        private IEnumerable<NewFolding> CreateCommentFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            List<NewFolding> foldings = new List<NewFolding>();
            var startFoldings = regStartCommentFold.Matches(document.Text).ToArray();
            var endFoldings = regEndCommentFold.Matches(document.Text).ToArray();

            if (startFoldings.Length != endFoldings.Length) //Unclosed comment?
            {
                firstErrorOffset = startFoldings.FirstOrDefault()?.Index ?? 0;
                return foldings;
            }

            for (int buc = 0; buc < startFoldings.Length; buc++)
            {
                var start = startFoldings[buc];
                var end = endFoldings[buc];
                var next = buc < startFoldings.Length - 1 ? startFoldings[buc + 1] : null;

                if (next != null)
                {
                    if (next.Index <= end.Index)
                    {
                        firstErrorOffset = buc;
                        return foldings;
                    }
                }

                string name = "/'...'/";

                foldings.Add(new NewFolding { StartOffset = start.Index, EndOffset = end.Index + end.Length, DefaultClosed = false, IsDefinition = false, Name = name });
            }

            return foldings;
        }
        private IEnumerable<NewFolding> CreateMultiDimFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            List<NewFolding> foldings = new List<NewFolding>();
            var startFoldings = regStartDimMulti.Matches(document.Text).ToArray();

            for (int buc = 0; buc < startFoldings.Length; buc++)
            {
                var start = startFoldings[buc];

                var end = regEndDimMulti.Match(document.Text, start.Index + start.Length);

                if (end == null || !end.Success)
                    continue;

                //string name = $"Dim {start.Groups[1].Value}";
                string name = "...";

                int disp = end.Value.EndsWith('\n') || end.Value.EndsWith('\r') ? 1 : 0;

                foldings.Add(new NewFolding { StartOffset = start.Groups[1].Index + start.Groups[1].Length, EndOffset = end.Index + end.Length - disp, DefaultClosed = false, IsDefinition = true, Name = name });
            }

            return foldings;
        }
        private IEnumerable<NewFolding> CreateRegionFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            List<NewFolding> foldings = new List<NewFolding>();
            var startFoldings = regStartRegion.Matches(document.Text).ToArray();

            for (int buc = 0; buc < startFoldings.Length; buc++)
            {
                var start = startFoldings[buc];

                var end = regEndRegion.Match(document.Text, start.Index + start.Length);

                if (end == null || !end.Success)
                    continue;

                string name = start.Groups[1].Value.Trim();

                int disp = end.Value.EndsWith('\n') || end.Value.EndsWith('\r') ? 1 : 0;

                foldings.Add(new NewFolding { StartOffset = start.Index, EndOffset = end.Index + end.Length - disp, DefaultClosed = true, IsDefinition = true, Name = name });
            }

            return foldings;
        }
        private IEnumerable<NewFolding> CreateMultiDefineFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            List<NewFolding> foldings = new List<NewFolding>();
            var startFoldings = regStartMultiDefine.Matches(document.Text).ToArray();

            for (int buc = 0; buc < startFoldings.Length; buc++)
            {
                var start = startFoldings[buc];

                var end = regEndMultiDefine.Match(document.Text, start.Index + start.Length);

                if (end == null || !end.Success)
                    continue;

                string name = start.Groups[1].Value.Trim();

                int disp = end.Value.EndsWith('\n') || end.Value.EndsWith('\r') ? 1 : 0;

                foldings.Add(new NewFolding { StartOffset = start.Index, EndOffset = end.Index + end.Length - disp, DefaultClosed = true, IsDefinition = true, Name = name });
            }

            return foldings;
        }
    }
}
