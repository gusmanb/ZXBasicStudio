using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ZXBasicStudio.BuildSystem
{
    public class ZXBasicMap
    {
        static Regex regSub = new Regex("^\\s*([^\\s,;:]*:\\ *?)?(fastcall)?sub\\s+(fastcall\\s+)?([^\\(]+)\\(((?:[^()]+|\\([^()]*\\))*)\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regFunc = new Regex("^\\s*([^\\s,;:]*:\\ *?)?(fastcall)?function\\s+(fastcall\\s+)?([^\\(]+)\\(((?:[^()]+|\\([^()]*\\))*)\\)\\s*(as\\s+([a-zA-Z]+))?", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        static Regex regEndSub = new Regex("^\\s*end\\s*sub(\\s|$|')", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regEndFunc = new Regex("^\\s*end\\s*function(\\s|$|')", RegexOptions.Multiline | RegexOptions.IgnoreCase);

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
            cleanContent = regRemoveStrings.Replace(cleanContent, "\"\"");
            cleanContent = regRemoveSingleComments.Replace(cleanContent, string.Empty);
            

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

            for (int buc = 0; buc < splitLines.Count; buc++)
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
                if (!inAsm && line.Trim().ToLower() == "asm")
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
                    currentSub.EndLine = buc;
                    subs.Add(currentSub);
                    currentSub = null;
                    continue;
                }

                //Check for function end
                if (currentFunction != null && regEndFunc.IsMatch(line))
                {
                    currentFunction.EndLine = buc;
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
                        currentSub = new ZXBasicSub { FastCall = subMatch.Groups[2].Success || subMatch.Groups[3].Success, Name = subMatch.Groups[4].Value.Trim(), StartLine = buc };

                        if (subMatch.Groups[5].Success && !string.IsNullOrWhiteSpace(subMatch.Groups[5].Value))
                            ParseInputParameters(subMatch.Groups[5].Value, currentSub.InputParameters);

                        continue;
                    }

                    //Check function begin
                    var funcMatch = regFunc.Match(line);

                    if (funcMatch != null && funcMatch.Success)
                    {
                        currentFunction = new ZXBasicFunction { FastCall = funcMatch.Groups[2].Success || funcMatch.Groups[3].Success, Name = funcMatch.Groups[4].Value.Trim(), StartLine = buc };

                        if (funcMatch.Groups[5].Success && !string.IsNullOrWhiteSpace(funcMatch.Groups[5].Value))
                            ParseInputParameters(funcMatch.Groups[5].Value, currentFunction.InputParameters);

                        if (funcMatch.Groups[7].Success)
                            currentFunction.ReturnType = StorageFromString(funcMatch.Groups[5].Value, currentFunction.Name);
                        else
                            currentFunction.ReturnType = ZXVariableStorage.F;

                        continue;
                    }

                }

                //Check for global variables
                if (currentSub == null && currentFunction == null)
                {
                    //Check for dim
                    var dimMatch = regDim.Match(line);

                    if (dimMatch != null && dimMatch.Success)
                    {

                        string varNameDef = dimMatch.Groups[2].Value;

                        if (varNameDef.Contains("(")) //array
                        {
                            string varName = varNameDef.Substring(0, varNameDef.IndexOf("(")).Trim();

                            if (!jointLines.Skip(buc + 1).Any(l => Regex.IsMatch(l, $"(^|[^a-zA-Z0-9_]){varName}($|[^a-zA-Z0-9_])", RegexOptions.Multiline)))
                                continue;

                            string[] dims = varNameDef.Substring(varNameDef.IndexOf("(") + 1).Replace(")", "").Split(",", StringSplitOptions.RemoveEmptyEntries);

                            ZXBasicVariable varArr = new ZXBasicVariable { Name = varName, IsArray = true, Dimensions = dims.Select(d => GetDimensionSize(d)).ToArray(), Storage = StorageFromString(dimMatch.Groups[5].Value, varName) };

                            globalVars.Add(varArr);
                        }
                        else
                        {
                            string[] varNames = dimMatch.Groups[2].Value.Split(",", StringSplitOptions.RemoveEmptyEntries);

                            foreach (var vName in varNames)
                            {
                                string varName = vName.Trim();

                                if (!jointLines.Skip(buc + 1).Any(l => Regex.IsMatch(l, $"(^|[^a-zA-Z0-9_]){varName}($|[^a-zA-Z0-9_])", RegexOptions.Multiline)))
                                    continue;

                                var storage = StorageFromString(dimMatch.Groups[5].Value, varName);
                                ZXBasicVariable var = new ZXBasicVariable { Name = varName, IsArray = false, Storage = storage };
                                
                                globalVars.Add(var);
                            }
                        }
                    }
                }
            }

            List<ZXBasicLocation> locations = new List<ZXBasicLocation>();

            foreach (var file in AllFiles)
                locations.AddRange(GetBuildLocations(file));

            GlobalVariables = globalVars.ToArray();
            Subs = subs.ToArray();
            Functions = functions.ToArray();

            foreach(var sub in Subs)
                GetSubVars(sub, jointLines.Skip(sub.StartLine).Take(sub.EndLine - sub.StartLine + 1).ToArray());

            foreach (var sub in Functions)
                GetSubVars(sub, jointLines.Skip(sub.StartLine).Take(sub.EndLine - sub.StartLine + 1).ToArray());

            //Build locations may contain commented functions/subs, not a problem for now.
            BuildLocations = locations.ToArray();

            var unusedMatches = regUnused.Matches(BuildLog);

            foreach (Match unused in unusedMatches)
            {
                string file = unused.Groups[1].Value;
                string line = unused.Groups[2].Value;
                string varName = unused.Groups[3].Value.Trim();

                int lineNum = int.Parse(line) - 1;

                var unusedVar = FindUnusedVar(AllFiles, varName, lineNum, locations, file, subs, functions);

                //Var can be already purged by the basic map, the compiler has an unconsistent behavior
                //informing about unused variables so the map purges vars that finds that are totally
                //unused, it keeps the ones that have been assigned in different statements than its dim
                //but those are informed by the compiler consistently

                if (unusedVar != null)
                    unusedVar.Unused = true;
            }

        }

        ZXBasicVariable? FindUnusedVar(IEnumerable<ZXCodeFile> AllFiles, string varName, int lineNum, List<ZXBasicLocation> locations, string file, List<ZXBasicSub> subs, List<ZXBasicFunction> functions)
        {

            ZXBasicVariable? foundVar = null;

            //First, search by file name

            string bPath = Path.GetFullPath(file).ToLower();

            //Find a location in the file which contains the line number of the var declaration
            var location = locations.Where(l => l.File.ToLower() == bPath && l.FirstLine <= lineNum && l.LastLine >= lineNum).FirstOrDefault();

            if (location != null)
            {
                //Search for the var in the sub/function that the location points to
                if (location.LocationType == ZXBasicLocationType.Sub)
                {
                    var sub = subs.Where(s => s.Name == location.Name).FirstOrDefault();
                    if(sub != null)
                        foundVar = sub.LocalVariables.Where(v => v.Name == varName).FirstOrDefault();
                }
                else
                {
                    var func = functions.Where(f => f.Name == location.Name).FirstOrDefault();
                    if (func != null)
                        foundVar = func.LocalVariables.Where(v => v.Name == varName).FirstOrDefault();
                }
            }

            //In this case we do not check if it is flagged as unused as the file and location matched
            if (foundVar != null)
                return foundVar;

            //If not found, search by declaration. Unsafe but used to avoid bug in compiler.
            //Must be removed once bug is corrected in the compiler.

            //Find all files that contain a Dim for the specified var name and line number
            var files = AllFiles.Where(f => ContainsBuildDim(f, varName, lineNum)).Select(f => Path.GetFullPath(Path.Combine(f.Directory, f.TempFileName)).ToLower());

            //Find in those files a location that contains the Dim line
            var possibleLocations = locations.Where(l => l.FirstLine <= lineNum && l.LastLine >= lineNum && files.Contains(l.File.ToLower()));

            foreach (var possibleLocation in possibleLocations)
            {
                //Search for a var with the specified name and that is not flagged as unused
                //(to avoid the very unprobable case where the same var is defined in different files in locations that match the same range)
                if (possibleLocation.LocationType == ZXBasicLocationType.Sub)
                {
                    var sub = subs.Where(s => s.Name == possibleLocation.Name).FirstOrDefault();
                    if (sub != null)
                        foundVar = sub.LocalVariables.Where(v => v.Name == varName && !v.Unused).FirstOrDefault();
                }
                else
                {
                    var func = functions.Where(f => f.Name == possibleLocation.Name).FirstOrDefault();
                    if (func != null)
                        foundVar = func.LocalVariables.Where(v => v.Name == varName && !v.Unused).FirstOrDefault();
                }
                
                //If the criteria finds a var, return it
                if (foundVar != null)
                    return foundVar;
            }

            //return null, not found
            return null;
        }

        void GetSubVars(ZXBasicSub Sub, string[] Lines)
        {
            for (int buc = 0; buc < Lines.Length; buc++) 
            {
                string line = Lines[buc];
                
                //Check for dim
                var dimMatch = regDim.Match(line);

                if (dimMatch != null && dimMatch.Success)
                {

                    string varNameDef = dimMatch.Groups[2].Value;

                    if (varNameDef.Contains("(")) //array
                    {
                        string varName = varNameDef.Substring(0, varNameDef.IndexOf("(")).Trim();

                        //Ignore unused vars (vars that are found only on its dim line, there may be the improbable
                        //case where a var is defined and used in the same line using a colon and not used
                        //anywhere else, but that would be an awful code :) )
                        if (!Lines.Skip(buc+1).Any(l => Regex.IsMatch(l, $"(^|[^a-zA-Z0-9_]){varName}($|[^a-zA-Z0-9_])", RegexOptions.Multiline)))
                            continue;

                        string[] dims = varNameDef.Substring(varNameDef.IndexOf("(") + 1).Replace(")", "").Split(",", StringSplitOptions.RemoveEmptyEntries);

                        ZXBasicVariable varArr = new ZXBasicVariable { Name = varName, IsArray = true, Dimensions = dims.Select(d => GetDimensionSize(d)).ToArray(), Storage = StorageFromString(dimMatch.Groups[5].Value, varName) };

                        Sub.LocalVariables.Add(varArr);
                    }
                    else
                    {
                        string[] varNames = dimMatch.Groups[2].Value.Split(",", StringSplitOptions.RemoveEmptyEntries);

                        foreach (var vName in varNames)
                        {
                            string varName = vName.Trim();

                            //Ignore unused vars
                            if (!Lines.Skip(buc+1).Any(l => Regex.IsMatch(l, $"(^|[^a-zA-Z0-9_]){varName}($|[^a-zA-Z0-9_])", RegexOptions.Multiline)))
                                continue;

                            var storage = StorageFromString(dimMatch.Groups[5].Value, varName);
                            ZXBasicVariable var = new ZXBasicVariable { Name = varName, IsArray = false, Storage = storage };

                            Sub.LocalVariables.Add(var);
                        }
                    }
                }
            }
        }

        public List<ZXBasicLocation> GetBuildLocations(ZXCodeFile CodeFile)
        {
            List<ZXBasicLocation> locations = new List<ZXBasicLocation>();

            if (CodeFile.FileType != ZXFileType.Basic)
                return locations;

            string[] lines = CodeFile.Content.Replace("\r", "").Split("\n");

            ZXBasicLocation? loc = null;

            for (int buc = 0; buc < lines.Length; buc++)
            {
                var line = lines[buc];

                if (loc == null)
                {
                    var subMatch = regSub.Match(line);

                    if (subMatch != null && subMatch.Success)
                    {
                        loc = new ZXBasicLocation { Name = subMatch.Groups[2].Value.Trim(), LocationType = ZXBasicLocationType.Sub, FirstLine = buc, File = Path.Combine(CodeFile.Directory, CodeFile.TempFileName) };
                        continue;
                    }

                    var funcMatch = regFunc.Match(line);

                    if (funcMatch != null && funcMatch.Success)
                    {
                        loc = new ZXBasicLocation { Name = funcMatch.Groups[2].Value.Trim(), LocationType = ZXBasicLocationType.Function, FirstLine = buc, File = Path.Combine(CodeFile.Directory, CodeFile.TempFileName) };
                        continue;
                    }
                }
                else
                {
                    if (loc.LocationType == ZXBasicLocationType.Sub)
                    {
                        if (regEndSub.IsMatch(line))
                        {
                            loc.LastLine = buc;
                            locations.Add(loc);
                            loc = null;
                            continue;
                        }
                    }
                    else
                    {
                        if (regEndFunc.IsMatch(line))
                        {
                            loc.LastLine = buc;
                            locations.Add(loc);
                            loc = null;
                            continue;
                        }
                    }
                }
            }

            return locations;
        }

        public bool ContainsBuildDim(ZXCodeFile CodeFile, string VarName, int LineNumber)
        {
            if (CodeFile.FileType != ZXFileType.Basic)
                return false;

            string[] lines = CodeFile.Content.Replace("\r", "").Split("\n");

            if (LineNumber >= lines.Length)
                return false;

            return Regex.IsMatch(lines[LineNumber], $"(\\s|,){VarName}(\\s|,|\\(|$)", RegexOptions.Multiline);
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

                bParam.Offset = offset;

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
                        return int.Parse(parts[1]) - int.Parse(parts[0]) + 1;
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

        public int StartLine { get; set; }
        public int EndLine { get; set; }
    }

    public class ZXBasicFunction : ZXBasicSub
    {
        public ZXVariableStorage ReturnType { get; set; }
    }
}
