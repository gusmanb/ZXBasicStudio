using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZXBasicStudio.BuildSystem
{
    public class ZXVariableMap
    {
        static Regex regGlobalFlatVars = new Regex("\\('var',\\ '([^']+)',\\ '([0-9]+)'\\)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regGlobalInitializedFlatVars = new Regex("\\('vard',\\ '(_[^'\\.]+)',", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regGlobalArrVars = new Regex("\\('varx',\\ '([^'\\.]+)',\\ 'u16',\\ \"\\['([^']+)']\\\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regLocalEndFunction = new Regex("\\('label', '([^']*)__leave'\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regLocalVar = new Regex("\\('pstore([iuf](8|16|32)|str|f)', '(-[0-9]+)'", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regLocalVar2 = new Regex("\\('pload([iuf](8|16|32)|str|f)', '[^']*', '(-[0-9]+)'\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regLocalArr = new Regex("\\('larrd',\\ '([0-9a-fA-F]+)', \"\\[('[0-9A-F]+',?\\ ?)+\\]\",\\ '([0-9]+)'", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        static string arrGlobalInfoTemplate = "\\('vard',\\ '{0}',\\ \"\\['(?<extraDims>[0-9A-Fa-f]{{4}})',\\ (?<extraDimsSize>'[0-9A-Fa-f]{{4}}',\\ )*'(?<itemSize>[0-9]+)'\\]\\\"";
        static string addrGlobalTemplate = "^([0-9A-F]+):[^\\n]*\\ {0}\\s*$";
        static string regLocalStartFunction = "\\('label', '{0}'\\)";
        static string addrLocalTemplate = "^([0-9A-F]+):[^\\n]*\\ \\.{0}\\s*$";

        List<ZXVariable> vars = new List<ZXVariable>();
        public IEnumerable<ZXVariable> Variables { get { return vars; } }

        public IEnumerable<ZXVariable> VariablesInRange(ushort ScopeAddress)
        {
            return vars.Where(v => v.Scope.InRange(ScopeAddress));
        }

        public ZXVariableMap(string ICFile, string MapFile, ZXBasicMap BasicMap)
        {
            string icContent = File.ReadAllText(ICFile);
            string mapContent = File.ReadAllText(MapFile);
            ProcessGlobalVariables(icContent, mapContent, BasicMap);
            ProcessLocalVariables(icContent, mapContent, BasicMap);
        }


        private void ProcessGlobalVariables(string icContent, string mapContent, ZXBasicMap BasicMap)
        {
            int splitIndex = icContent.IndexOf("--- end of user code ---");

            string icTop = icContent.Substring(0, splitIndex);
            string icBottom = icContent.Substring(splitIndex);

            var variableMatches = regGlobalFlatVars.Matches(icBottom);
            var initVariableMatches = regGlobalInitializedFlatVars.Matches(icBottom);

            List<Match> varMatches = new List<Match>();
            varMatches.AddRange(variableMatches.Cast<Match>());
            varMatches.AddRange(initVariableMatches.Cast<Match>());
            foreach (Match m in varMatches)
            {
                string varName = m.Groups[1].Value;
                string bVarName = varName.Substring(1);

                var basicVar = BasicMap.GlobalVariables.FirstOrDefault(v => v.Name == bVarName);

                if (basicVar == null)
                    continue;

                ZXVariableStorage storage = basicVar.Storage;

                Regex regAddr = new Regex(string.Format(addrGlobalTemplate, "\\." + varName), RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var addrMatch = regAddr.Match(mapContent);

                if (addrMatch == null || !addrMatch.Success)
                    continue;

                var addr = ushort.Parse(addrMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);

                ZXVariable newVar = new ZXVariable
                {
                    Name = bVarName,
                    Address = new ZXVariableAddress { AddressType = ZXVariableAddressType.Absolute, AddressValue = addr },
                    Scope = ZXVariableScope.GlobalScope,
                    VariableType = ZXVariableType.Flat,
                    StorageType = storage
                };

                vars.Add(newVar);

            }

            var arrayMatches = regGlobalArrVars.Matches(icBottom);

            foreach (Match m in arrayMatches)
            {
                string varName = m.Groups[1].Value;
                string infoLabelName = m.Groups[2].Value;

                string bVarName = varName.Substring(1);

                var basicVar = BasicMap.GlobalVariables.FirstOrDefault(v => v.Name == bVarName);

                if (basicVar == null)
                    continue;

                ZXVariableStorage storage = basicVar.Storage;

                Regex regInfo = new Regex(string.Format(arrGlobalInfoTemplate, infoLabelName.Replace(".", "\\.")), RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var infoMatch = regInfo.Match(icBottom);

                if (infoMatch == null || !infoMatch.Success)
                    continue;

                var elemSize = int.Parse(infoMatch.Groups["itemSize"].Value, System.Globalization.NumberStyles.HexNumber);
                List<int> dimsSize = new List<int>();

                foreach (Capture capture in infoMatch.Groups["extraDimsSize"].Captures)
                {
                    string cleanValue = capture.Value.Replace("'", "").Replace(",", "").Replace(" ", "");
                    int realValue = int.Parse(cleanValue, System.Globalization.NumberStyles.HexNumber);
                    dimsSize.Add(realValue + 1);
                }

                Regex regAddr = new Regex(string.Format(addrGlobalTemplate, "\\." + varName), RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var addrMatch = regAddr.Match(mapContent);

                if (addrMatch == null || !addrMatch.Success)
                    continue;

                var addr = ushort.Parse(addrMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);

                ZXVariable newVar = new ZXVariable
                {
                    Name = bVarName,
                    Address = new ZXVariableAddress { AddressType = ZXVariableAddressType.Absolute, AddressValue = addr },
                    Scope = ZXVariableScope.GlobalScope,
                    VariableType = ZXVariableType.Array,
                    StorageType = storage
                };

                vars.Add(newVar);
            }
        }

        private void ProcessLocalVariables(string icContent, string mapContent, ZXBasicMap BasicMap)
        {
            int splitIndex = icContent.IndexOf("--- end of user code ---");

            string icTop = icContent.Substring(0, splitIndex);

            var functionEnds = regLocalEndFunction.Matches(icContent);

            foreach (Match functionEnd in functionEnds)
            {
                string label = functionEnd.Groups[1].Value;
                var regStart = new Regex(string.Format(regLocalStartFunction, label));

                var startMatch = regStart.Match(icTop);

                if (startMatch == null || !startMatch.Success)
                    continue;

                var startAddrReg = new Regex(string.Format(addrLocalTemplate, label), RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var endAddrReg = new Regex(string.Format(addrLocalTemplate, label + "__leave"), RegexOptions.Multiline | RegexOptions.IgnoreCase);

                var mStart = startAddrReg.Match(mapContent);
                var mEnd = endAddrReg.Match(mapContent);

                if (mStart == null || mEnd == null || !mStart.Success || !mEnd.Success)
                    continue;

                ushort startAddr = ushort.Parse(mStart.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                ushort endAddr = ushort.Parse(mEnd.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);

                string locName = label.Substring(1);

                ZXVariableScope currentScope = new ZXVariableScope { ScopeName = locName, StartAddress = startAddr, EndAddress = endAddr };

                ZXBasicSub? sub = BasicMap.Subs.Where(m => m.Name == locName).FirstOrDefault();

                if (sub == null)
                    sub = BasicMap.Functions.Where(m => m.Name == locName).FirstOrDefault();

                //Function params
                if (sub != null)
                {
                    foreach (var param in sub.InputParameters)
                    {
                        ZXVariable lVar = new ZXVariable { Name = param.Name, Address = new ZXVariableAddress { AddressType = ZXVariableAddressType.Relative, AddressValue = param.Offset }, Scope = currentScope, StorageType = param.Storage, VariableType = param.IsArray ? ZXVariableType.Array : ZXVariableType.Flat, IsReference = param.IsArray | param.ByRef, IsParameter = true };
                        vars.Add(lVar);
                    }
                }

                string funcCode = icTop.Substring(startMatch.Index, functionEnd.Index - startMatch.Index);

                List<Match> localVarMatches = new List<Match>();
                List<ZXVariable> localVars = new List<ZXVariable>();

                localVarMatches.AddRange(regLocalVar.Matches(funcCode));
                localVarMatches.AddRange(regLocalVar2.Matches(funcCode));

                foreach (Match localMatch in localVarMatches)
                {
                    ZXVariableStorage storage = Enum.Parse<ZXVariableStorage>(localMatch.Groups[1].Value.ToUpper());
                    int offset = int.Parse(localMatch.Groups[3].Value);
                    ZXVariable lVar = new ZXVariable { Name = offset.ToString(), Address = new ZXVariableAddress { AddressType = ZXVariableAddressType.Relative, AddressValue = offset }, Scope = currentScope, StorageType = storage, VariableType = ZXVariableType.Flat };
                    localVars.Add(lVar);
                }

                var localArrayMatches = regLocalArr.Matches(funcCode);
                foreach (Match localMatch in localArrayMatches)
                {

                    int arrOffset = -int.Parse(localMatch.Groups[1].Value);
                    int loadOffset = arrOffset + 2;

                    int itemLen = int.Parse(localMatch.Groups[2].Captures.Last().Value.Replace("\"", "").Replace("'", "").Replace(",", "").Trim(), System.Globalization.NumberStyles.HexNumber);
                    int store = int.Parse(localMatch.Groups[3].Value);

                    ZXVariableStorage storage = ZXVariableStorage.LA8;

                    switch (itemLen)
                    {
                        case 1:
                            storage = ZXVariableStorage.LA8;
                            break;
                        case 2:
                            storage = ZXVariableStorage.LA16;
                            break;
                        case 4:
                            storage = ZXVariableStorage.LA32;
                            break;
                        case 5:
                            storage = ZXVariableStorage.LAF;
                            break;
                    }

                    ZXVariable lVar = new ZXVariable { Name = arrOffset.ToString(), Address = new ZXVariableAddress { AddressType = ZXVariableAddressType.Relative, AddressValue = arrOffset }, Scope = currentScope, StorageType = storage, VariableType = ZXVariableType.Array, StorageSize = store };

                    var pload = localVars.Where(v => v.Address.AddressValue == loadOffset).ToArray();

                    if (pload != null && pload.Length > 0)
                        foreach (var p in pload)
                            localVars.Remove(p);

                    localVars.Add(lVar);
                }

                if (localVars.Count == 0)
                    continue;

                localVars = localVars.Distinct(new localVarComparer()).ToList();

                vars.AddRange(localVars);



                if (sub != null)
                {
                    var vars = sub.LocalVariables.ToArray();

                    if (localVars.Count != vars.Length)
                        vars = vars.Where(v => !v.Unused).ToArray();

                    if (localVars.Count == vars.Length)
                    {
                        var nonArrays = localVars.Where(v => v.VariableType == ZXVariableType.Flat).OrderByDescending(v => v.Address.AddressValue).ToArray();
                        var lNonArrays = vars.Where(v => !v.IsArray).OrderBy(v => StackSizeByStorageType(v.Storage)).ToArray();

                        if (nonArrays.Length != lNonArrays.Length)
                            continue;


                        for (int buc = 0; buc < nonArrays.Length; buc++)
                        {
                            nonArrays[buc].StorageType = lNonArrays[buc].Storage;
                            nonArrays[buc].Name = lNonArrays[buc].Name;
                        }

                        var arrays = localVars.Where(v => v.VariableType == ZXVariableType.Array).OrderByDescending(v => v.Address.AddressValue).ToArray();
                        var lArrays = vars.Where(v => v.IsArray).ToArray();

                        if (arrays.Length != lArrays.Length)
                            continue;

                        for (int buc = 0; buc < arrays.Length; buc++)
                        {
                            arrays[buc].StorageType = lArrays[buc].Storage;
                            arrays[buc].Name = lArrays[buc].Name;
                        }
                    }
                    else
                        continue;
                }

            }
        }

        static int StackSizeByStorageType(ZXVariableStorage Storage)
        {
            switch (Storage)
            {
                case ZXVariableStorage.I8:
                case ZXVariableStorage.U8:
                case ZXVariableStorage.I16:
                case ZXVariableStorage.U16:
                case ZXVariableStorage.STR:

                    return 1;

                case ZXVariableStorage.I32:
                case ZXVariableStorage.U32:
                case ZXVariableStorage.F16:

                    return 2;

                default: //float

                    return 3;
            }
        }

        class localVarComparer : IEqualityComparer<ZXVariable>
        {
            public bool Equals(ZXVariable? x, ZXVariable? y)
            {
                if (x == y)
                    return true;

                if (x == null || y == null)
                    return false;

                if (x.Scope != y.Scope)
                    return false;

                if (x.Address.AddressValue != y.Address.AddressValue)
                    return false;

                return true;
            }

            public int GetHashCode([DisallowNull] ZXVariable obj)
            {
                return $"{obj.Name}-{obj.Address.AddressType}-{obj.Address.AddressValue}-{obj.Scope.ScopeName}-{obj.Scope.StartAddress}-{obj.Scope.EndAddress}".GetHashCode();
            }
        }
    }
}
