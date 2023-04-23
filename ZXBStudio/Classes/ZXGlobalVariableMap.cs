using MessageBox.Avalonia.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXGlobalVariableMap
    {
        static Regex regFlatVars = new Regex("\\('var',\\ '([^']+)',\\ '([0-9]+)'\\)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regInitializedFlatVars = new Regex("\\('vard',\\ '(_[^'\\.]+)',", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regArrVars = new Regex("\\('varx',\\ '([^'\\.]+)',\\ 'u16',\\ \"\\['([^']+)']\\\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        static string arrInfoTemplate = "\\('vard',\\ '{0}',\\ \"\\['(?<extraDims>[0-9A-Fa-f]{{4}})',\\ (?<extraDimsSize>'[0-9A-Fa-f]{{4}}',\\ )*'(?<itemSize>[0-9]+)'\\]\\\"";

        static string addrTemplate = "^([0-9A-F]+):[^\\n]*\\ {0}\\s*$";

        List<ZXVariable> vars = new List<ZXVariable>();
        public IEnumerable<ZXVariable> Variables { get { return vars; } }

        public ZXGlobalVariableMap(string ICFile, string MapFile, ZXBasicMap BasicMap)
        {
            string icContent = File.ReadAllText(ICFile);
            string mapContent = File.ReadAllText(MapFile);

            int splitIndex = icContent.IndexOf("--- end of user code ---");

            string icTop = icContent.Substring(0, splitIndex);
            string icBottom = icContent.Substring(splitIndex);

            var variableMatches = regFlatVars.Matches(icBottom);
            var initVariableMatches = regInitializedFlatVars.Matches(icBottom);

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
                
                Regex regAddr = new Regex(string.Format(addrTemplate, "\\."+varName), RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var addrMatch = regAddr.Match(mapContent);

                if(addrMatch == null || !addrMatch.Success) 
                    continue;

                var addr = ushort.Parse(addrMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);

                ZXVariable newVar = new ZXVariable(bVarName, addr, ZXVariableType.Flat, storage);
                vars.Add(newVar);

            }

            var arrayMatches = regArrVars.Matches(icBottom);

            foreach (Match m in arrayMatches)
            {
                string varName = m.Groups[1].Value;
                string infoLabelName = m.Groups[2].Value;

                string bVarName = varName.Substring(1);

                var basicVar = BasicMap.GlobalVariables.FirstOrDefault(v => v.Name == bVarName);

                if (basicVar == null)
                    continue;

                ZXVariableStorage storage = basicVar.Storage;

                Regex regInfo = new Regex(string.Format(arrInfoTemplate, infoLabelName.Replace(".", "\\.")), RegexOptions.IgnoreCase | RegexOptions.Multiline);

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

                Regex regAddr = new Regex(string.Format(addrTemplate, "\\." + varName), RegexOptions.IgnoreCase | RegexOptions.Multiline);

                var addrMatch = regAddr.Match(mapContent);

                if (addrMatch == null || !addrMatch.Success)
                    continue;

                var addr = ushort.Parse(addrMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);

                ZXVariable newVar = new ZXVariable(bVarName, addr, ZXVariableType.Array, storage);
                vars.Add(newVar);
            }
        }

    }
}
