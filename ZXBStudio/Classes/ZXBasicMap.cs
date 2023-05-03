using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXBasicMap
    {
        static Regex regSub =  new Regex("^\\s*(fastcall)?\\s*sub\\s+([^\\(]+)\\(((?:[^()]+|\\([^()]*\\))*)\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regFunc = new Regex("^\\s*(fastcall)?\\s*function\\s+([^\\(]+)\\(((?:[^()]+|\\([^()]*\\))*)\\)\\s*(as\\s+([a-zA-Z]+))?", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        static Regex regEndSub = new Regex("^\\s*end\\s*sub(\\s|$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regEndFunc = new Regex("^\\s*end\\s*function(\\s|$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        static Regex regMulti = new Regex("(\\s|^|[^a-zA-Z0-9])(_)(\\s|$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        static Regex regDim = new Regex("(^|\\s+)dim\\s+(.*?)((\\s+as\\s+([a-zA-Z\\$]+))|\\n|$|:)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        static Regex regParam = new Regex("(byval|byref)?\\s*?([^\\ \\(]+)(\\s*?\\(\\))?(\\s*as\\s*([a-zA-Z]+))?", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        static Regex regRemoveStrings = new Regex("\"([^\"\\\\]|\\\\.)*\"", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regRemoveMultiComments = new Regex("/'.*?'/", RegexOptions.Singleline);
        static Regex regRemoveSingleComments = new Regex("('|((?<=(^|\\s|_|:))REM)).*", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        static Regex regUnused = new Regex("(^.*?):([0-9]+): warning: \\[W150\\] Variable '([^']*)'", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        public string PlainContent { get; set; }
        public ZXBasicVariable[] GlobalVariables { get; set; }
        public ZXBasicSub[] Subs { get; set; }
        public ZXBasicFunction[] Functions { get; set; }

        public ZXBasicLocation[] BuildLocations { get; set; }

        public ZXBasicMap(ZXCodeFile MainFile, IEnumerable<ZXCodeFile> AllFiles, string BuildLog) 
        {
            //vars = byval, arrs = byref
            PlainContent = MainFile.CreateCompleteBuildSource(AllFiles);

            //Remove strings and comments to ease tasks
            string cleanContent = regRemoveMultiComments.Replace(PlainContent, string.Empty);
            cleanContent = regRemoveSingleComments.Replace(cleanContent, string.Empty);
            cleanContent = regRemoveStrings.Replace(cleanContent, string.Empty);

            string[] allLines = cleanContent.Replace("\r", "").Split("\n");

            List<string> splitLines = new List<string>();

            //Split by semicolon, we don't care about labels as our only target is to find subs/functions/variables

            for (int buc = 0; buc < allLines.Length; buc++)
            {
                var line = allLines[buc];
                
                List<string> lines = new List<string>();

                string[] splitLins = line.Split(":", StringSplitOptions.RemoveEmptyEntries);

                splitLines.AddRange(splitLins);
            }


            //Join multiline sentences
            List<string> jointLines = new List<string>();

            bool inMulti = false;
            string currentMulti = "";

            for(int buc = 0; buc < splitLines.Count; buc++) 
            {
                var line = splitLines[buc];

                var multiMatch = regMulti.Match(line);

                if (multiMatch.Success)
                {
                    var pos = multiMatch.Groups[2].Index;
                    line = line.Substring(0, pos);
                    if (inMulti)
                        currentMulti += " " + line;
                    else
                    {
                        currentMulti = line;
                        inMulti = true;
                    }
                }
                else if (inMulti)
                {
                    currentMulti += " " + line;
                    jointLines.Add(currentMulti);
                    inMulti = false;
                }
                else
                    jointLines.Add(line);
            }

            if (inMulti)
                jointLines.Add(currentMulti);


            List<ZXBasicVariable> globalVars = new List<ZXBasicVariable>();
            List<ZXBasicSub> subs = new List<ZXBasicSub>();
            List<ZXBasicFunction> functions = new List<ZXBasicFunction>();

            ZXBasicSub? currentSub = null;
            ZXBasicFunction? currentFunction = null;

            bool inAsm = false;

            //Start searching

            for (int buc = 0; buc < jointLines.Count; buc++)
            {
                var line = jointLines[buc];

                //Skip inline asm blocks
                if(!inAsm && line.Trim().ToLower() == "asm")
                {
                    inAsm = true;
                    continue;
                }

                if (inAsm)
                {
                    if (line.Trim().ToLower() == "end asm")
                        inAsm = false;

                    continue;
                }

                //Check for sub end
                if (currentSub != null && regEndSub.IsMatch(line))
                {
                    subs.Add(currentSub);
                    currentSub = null;
                    continue;
                }

                //Check for function end
                if (currentFunction != null && regEndFunc.IsMatch(line))
                {
                    functions.Add(currentFunction);
                    currentFunction = null;
                    continue;
                }

                //If there is no open sub/function...
                if (currentFunction == null && currentSub == null)
                {

                    //Check sub begin
                    var subMatch = regSub.Match(line);

                    if (subMatch != null && subMatch.Success)
                    {
                        currentSub = new ZXBasicSub { FastCall = subMatch.Groups[1].Success, Name = subMatch.Groups[2].Value.Trim() };

                        if (subMatch.Groups[3].Success && !string.IsNullOrWhiteSpace(subMatch.Groups[3].Value))
                            ParseInputParameters(subMatch.Groups[3].Value, currentSub.InputParameters);

                        continue;
                    }

                    //Check function begin
                    var funcMatch = regFunc.Match(line);

                    if (funcMatch != null && funcMatch.Success)
                    {
                        currentFunction = new ZXBasicFunction { FastCall = funcMatch.Groups[1].Success, Name = funcMatch.Groups[2].Value.Trim() };

                        if (funcMatch.Groups[3].Success && !string.IsNullOrWhiteSpace(funcMatch.Groups[3].Value))
                            ParseInputParameters(funcMatch.Groups[3].Value, currentFunction.InputParameters);

                        if (funcMatch.Groups[5].Success)
                            currentFunction.ReturnType = StorageFromString(funcMatch.Groups[5].Value, currentFunction.Name);
                        else
                            currentFunction.ReturnType = ZXVariableStorage.F;

                        continue;
                    }

                }

                //Check for dim
                var dimMatch = regDim.Match(line);

                if (dimMatch != null && dimMatch.Success)
                {

                    string varNameDef = dimMatch.Groups[2].Value;

                    if (varNameDef.Contains("(")) //array
                    {
                        string varName = varNameDef.Substring(0, varNameDef.IndexOf("(")).Trim();
                        string[] dims = varNameDef.Substring(varNameDef.IndexOf("(") + 1).Replace(")", "").Split(",", StringSplitOptions.RemoveEmptyEntries);

                        ZXBasicVariable varArr = new ZXBasicVariable { Name = varName, IsArray = true, Dimensions = dims.Select(d => GetDimensionSize(d)).ToArray(), Storage = StorageFromString(dimMatch.Groups[5].Value, varName) };

                        if (currentSub != null)
                            currentSub.LocalVariables.Add(varArr);
                        else if (currentFunction != null)
                            currentFunction.LocalVariables.Add(varArr);
                        else
                            globalVars.Add(varArr);
                    }
                    else
                    {
                        string[] varNames = dimMatch.Groups[2].Value.Split(",", StringSplitOptions.RemoveEmptyEntries);

                        foreach (var vName in varNames)
                        {
                            var storage = StorageFromString(dimMatch.Groups[5].Value, vName);
                            ZXBasicVariable var = new ZXBasicVariable { Name = vName.Trim(), IsArray = false, Storage = storage };
                            if (currentSub != null)
                                currentSub.LocalVariables.Add(var);
                            else if (currentFunction != null)
                                currentFunction.LocalVariables.Add(var);
                            else
                                globalVars.Add(var);
                        }
                    }
                }
            }

            List<ZXBasicLocation> locations = new List<ZXBasicLocation>();

            foreach(var file in AllFiles)
                locations.AddRange(file.GetBuildLocations());

            GlobalVariables = globalVars.ToArray();
            Subs = subs.ToArray();
            Functions = functions.ToArray();

            //Build locations may contain commented functions/subs, not a problem for now.
            BuildLocations = locations.ToArray();

            var unusedMatches = regUnused.Matches(BuildLog);

            foreach(Match unused in unusedMatches) 
            {
                string file = unused.Groups[1].Value;
                string line = unused.Groups[2].Value;
                string varName = unused.Groups[3].Value.Trim();

                int lineNum = int.Parse(line) - 1;

                var cFile = AllFiles.FirstOrDefault(f => f.ContainsBuildDim(varName, lineNum));

                if (cFile == null)
                    continue;

                string bPath = Path.GetFullPath(Path.Combine(cFile.Directory, cFile.TempFileName)).ToLower();

                var location = locations.Where(l => l.FirstLine <= lineNum && l.LastLine >= lineNum && l.File.ToLower() == bPath).FirstOrDefault();

                if (location != null)
                {
                    if (location.LocationType == ZXBasicLocationType.Sub)
                    {
                        var sub = subs.Where(s => s.Name == location.Name).First();
                        var zVar = sub.LocalVariables.Where(v => v.Name == varName).First();
                        zVar.Unused = true;
                    }
                    else
                    {
                        var func = functions.Where(f => f.Name == location.Name).First();
                        var zVar = func.LocalVariables.Where(v => v.Name == varName).First();
                        zVar.Unused = true;
                    }
                }
                else
                {
                    var zVar = globalVars.Where(g => g.Name == varName).FirstOrDefault();
                    if (zVar != null)
                        zVar.Unused = true;
                }
            }

        }

        private static void ParseInputParameters(string ParameterString, List<ZXBasicParameter> Storage)
        {
            string[] paramsStrings = ParameterString.Split(",");

            int offset = 4;

            foreach (var param in paramsStrings)
            {
                var mParam = regParam.Match(param.Trim());

                if (mParam == null || !mParam.Success)
                {
                    Storage.Clear();
                    return;
                }

                ZXBasicParameter bParam = new ZXBasicParameter { Name = mParam.Groups[2].Value.Trim() };
                bParam.IsArray = mParam.Groups[3].Success;

                if (mParam.Groups[1].Success)
                    bParam.ByRef = mParam.Groups[1].Value.Trim().ToLower() == "byref";
                else
                    bParam.ByRef = false;

                if (mParam.Groups[5].Success)
                    bParam.Storage = StorageFromString(mParam.Groups[5].Value, bParam.Name);
                else
                    bParam.Storage = ZXVariableStorage.F;

                bParam.Offset= offset;

                if (bParam.ByRef || bParam.IsArray)
                    offset += 2;
                else
                {

                    switch (bParam.Storage)
                    {
                        case ZXVariableStorage.U8:
                        case ZXVariableStorage.I8:
                            bParam.Offset += 1;
                            offset += 2;
                            break;

                        case ZXVariableStorage.I16:
                        case ZXVariableStorage.U16:
                        case ZXVariableStorage.STR:
                            offset += 2;
                            break;
                        case ZXVariableStorage.I32:
                        case ZXVariableStorage.U32:
                        case ZXVariableStorage.F16:
                            offset += 4;
                            break;
                        case ZXVariableStorage.F:
                            bParam.Offset += 1;
                            offset += 6;
                            break;
                    }
                }

                Storage.Add(bParam);
            }
        }

        private static ZXVariableStorage StorageFromString(string? Value, string VariableName)
        {
            string? strStorage = Value?.Trim().ToLower();

            switch (strStorage)
            {
                case "byte":
                    return ZXVariableStorage.I8;
                case "ubyte":
                    return ZXVariableStorage.U8;
                case "integer":
                    return ZXVariableStorage.I16;
                case "uinteger":
                    return ZXVariableStorage.U16;
                case "long":
                    return ZXVariableStorage.I32;
                case "ulong":
                    return ZXVariableStorage.U32;
                case "fixed":
                    return ZXVariableStorage.F16;
                case "string":
                    return ZXVariableStorage.STR;
                default:
                    return VariableName.EndsWith("$") ? ZXVariableStorage.STR : ZXVariableStorage.F;
            }
        }

        private static int GetDimensionSize(string DimensionString)
        {
            int size = 0;

            if (!int.TryParse(DimensionString, out size))
            {
                if (DimensionString.ToLower().Contains("to"))
                {
                    string[] parts = DimensionString.ToLower().Split("to");
                    if (parts.Length == 1)
                        return int.Parse(parts[0]);
                    else
                        return (int.Parse(parts[1]) - int.Parse(parts[0])) + 1;
                }
                else
                    return 0;
            }

            return size;
        }
    }

    public class ZXBasicVariable
    {
        public required string Name { get; set; }
        public ZXVariableStorage Storage { get; set; }
        public bool IsArray { get; set; }
        public int[]? Dimensions { get; set; }
        public bool Unused { get; set; }
    }

    public class ZXBasicParameter : ZXBasicVariable
    {
        public bool ByRef { get; set; }
        public int Offset { get; set; }
    }

    public class ZXBasicSub
    {
        public required string Name { get; set; }
        public bool FastCall { get; set; }
        public List<ZXBasicParameter> InputParameters { get; set; } = new List<ZXBasicParameter>();
        public List<ZXBasicVariable> LocalVariables { get; set; } = new List<ZXBasicVariable>();
    }

    public class ZXBasicFunction : ZXBasicSub
    {
        public ZXVariableStorage ReturnType { get; set; }
    }
}
