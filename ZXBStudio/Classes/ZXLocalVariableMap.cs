using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXLocalVariableMap
    {
        static Regex regEndFunction = new Regex("\\('label', '([^']*)__leave'\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regLocalVar = new Regex("\\('pstore([iuf](8|16|32)|str|f)', '(-[0-9]+)'", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static Regex regLocalVar2 = new Regex("\\('pload([iuf](8|16|32)|str|f)', '[^']*', '(-[0-9]+)'\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        static string regStartFunction = "\\('label', '{0}'\\)";
        static string addrTemplate = "^([0-9A-F]+):[^\\n]*\\ \\.{0}\\s*$";
        static Regex regLocalArr = new Regex("\\('larrd',\\ '([0-9a-fA-F]+)', \"\\[('[0-9A-F]+',?\\ ?)+\\]\",\\ '([0-9]+)'", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        List<ZXVariable> variables = new List<ZXVariable>();
        public IEnumerable<ZXVariable> Variables { get { return variables; } }
        public ZXLocalVariableMap(string ICFile, string MapFile, ZXBasicMap BasicMap)
        {
            string icContent = File.ReadAllText(ICFile);
            string mapContent = File.ReadAllText(MapFile);

            int splitIndex = icContent.IndexOf("--- end of user code ---");

            string icTop = icContent.Substring(0, splitIndex);

            var functionEnds = regEndFunction.Matches(icContent);

            foreach(Match functionEnd in functionEnds)
            {
                string label = functionEnd.Groups[1].Value;
                var regStart = new Regex(string.Format(regStartFunction, label));

                var startMatch = regStart.Match(icTop);

                if (startMatch == null || !startMatch.Success)
                    continue;

                var startAddrReg = new Regex(string.Format(addrTemplate, label), RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var endAddrReg = new Regex(string.Format(addrTemplate, label + "__leave"), RegexOptions.Multiline | RegexOptions.IgnoreCase);

                var mStart = startAddrReg.Match(mapContent);
                var mEnd = endAddrReg.Match(mapContent);

                if (mStart == null || mEnd == null || !mStart.Success || !mEnd.Success)
                    continue;

                ushort startAddr = ushort.Parse(mStart.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                ushort endAddr = ushort.Parse(mEnd.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);

                string locName = label.Substring(1);

                ZXVariableScope currentScope = new ZXVariableScope { ScopeName = locName, StartAddress = startAddr, EndAddress = endAddr };


                string funcCode = icTop.Substring(startMatch.Index, functionEnd.Index - startMatch.Index);

                List<Match> localVarMatches = new List<Match>();

                localVarMatches.AddRange(regLocalVar.Matches(funcCode));
                localVarMatches.AddRange(regLocalVar2.Matches(funcCode));

                List<ZXVariable> localVars = new List<ZXVariable>();
                foreach(Match localMatch in localVarMatches) 
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
                    int loadOffset = arrOffset - 2;

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
                        foreach(var p in pload)
                            localVars.Remove(p);
                    
                    localVars.Add(lVar);
                }

                if (localVars.Count == 0)
                    continue;

                variables.AddRange(localVars);

                ZXBasicSub? sub = BasicMap.Subs.Where(m => m.Name == locName).FirstOrDefault();

                if(sub == null)
                    sub = BasicMap.Functions.Where(m => m.Name == locName).FirstOrDefault();

                if (sub != null)
                {

                    if (sub.Name.ToLower() == "updatescreen")
                    {
                        sub.Name = sub.Name;
                    }

                    var vars = sub.LocalVariables.ToArray();//.Where(v => !v.Unused).ToArray();

                    if(localVars.Count != vars.Length)
                        vars = vars.Where(v => !v.Unused).ToArray();

                    if (localVars.Count == vars.Length)
                    {
                        var nonArrays = localVars.Where(v => v.VariableType == ZXVariableType.Flat).ToArray();
                        var lNonArrays = vars.Where(v => !v.IsArray).ToArray();

                        if (nonArrays.Length != lNonArrays.Length)
                            continue;


                        for (int buc = 0; buc < nonArrays.Length; buc++)
                        {
                            nonArrays[buc].StorageType = lNonArrays[buc].Storage;
                            nonArrays[buc].Name = lNonArrays[buc].Name;
                        }

                        var arrays = localVars.Where(v => v.VariableType == ZXVariableType.Array).ToArray();
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
    }
    public class ZXLocalVariablesRange
    {
        public ushort StartAddress { get; set; }
        public ushort EndAddress { get; set; }
        public string FunctionName { get; set; }

        List<ZXLocalVariable> variables = new List<ZXLocalVariable>();
        public List<ZXLocalVariable> Variables { get { return variables; } }
    }
    public class ZXLocalVariable
    {
        public string? Name { get; set; }
        public ZXVariableType VariableType { get; set; }
        public int StackOffset { get; set; }
        public ZXVariableStorage StorageType { get; set; }
        public int StorageSize { get; set; }
        public ZXArrayDescriptor GetArrayDescriptor(IMemory Memory, IZ80Registers Registers)
        {
            var descAddress = (ushort)(((ushort)Registers.IX) + StackOffset - 2);
            var storeAddress = (ushort)(((ushort)Registers.IX) + StackOffset);
            return ZXVariableHelper.GetArrayDescriptor(Memory, descAddress, storeAddress, StorageSize);
        }

        public object GetValue(IMemory Memory, IZ80Registers Registers)
        {
            var address = (ushort)(((ushort)Registers.IX) + StackOffset);

            if (VariableType == ZXVariableType.Array)
                return ZXVariableHelper.GetValue(Memory, address, ZXVariableType.Array, ZXVariableStorage.U16);

            return ZXVariableHelper.GetValue(Memory, address, ZXVariableType.Flat, StorageType);
        }
    }

    public class ZXLocalVariableComparator : IEqualityComparer<ZXLocalVariable>
    {
        public bool Equals(ZXLocalVariable? x, ZXLocalVariable? y)
        {
            if (x == null && y == null) return true;
            if (x != null && y == null) return false;
            if (x == null && y != null) return false;

            return x.StorageType == y.StorageType && x.StackOffset == y.StackOffset;
        }

        public int GetHashCode([DisallowNull] ZXLocalVariable obj)
        {
            long hash = obj.StorageType.GetHashCode() + obj.StackOffset.GetHashCode();
            return hash.GetHashCode();
        }
    }
}
