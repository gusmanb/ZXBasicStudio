using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DocumentEditors.ZXRamDisk.Classes;

namespace ZXBasicStudio.BuildSystem
{
    public class ZXProgram
    {
        public IEnumerable<ZXCodeFile>? Files { get; set; }
        public ZXCodeFile? Disassembly { get; set; }
        public ZXMemoryMap? ProgramMap { get; set; }
        public ZXMemoryMap? DisassemblyMap { get; set; }
        public ZXVariableMap? Variables { get; set; }
        public List<ZXRamDisk> RamDisks { get; } = new List<ZXRamDisk>();
        public byte[] Binary { get; set; }
        public ushort Org { get; set; }
        public bool Debug { get; set; }
        private ZXProgram(IEnumerable<ZXCodeFile>? Files, ZXCodeFile? Disassembly, ZXMemoryMap? ProgramMap, ZXMemoryMap? DisassemblyMap, ZXVariableMap? Vars, byte[] Binary, ushort Org, bool Debug)
        {
            this.Files = Files;
            this.Disassembly = Disassembly;
            this.ProgramMap = ProgramMap;
            this.DisassemblyMap = DisassemblyMap;
            Variables = Vars;
            this.Binary = Binary;
            this.Org = Org;
            this.Debug = Debug;

            if (DisassemblyMap != null)
                foreach (var line in DisassemblyMap.Lines)
                    line.File = ZXConstants.DISASSEMBLY_DOC;

        }
        public static ZXProgram CreateDebugProgram(IEnumerable<ZXCodeFile> Files, ZXCodeFile Disassembly, ZXMemoryMap ProgramMap, ZXMemoryMap DisassemblyMap, ZXVariableMap Vars, byte[] Binary, ushort Org)
        {
            return new ZXProgram(Files, Disassembly, ProgramMap, DisassemblyMap, Vars, Binary, Org, true);
        }
        public static ZXProgram CreateReleaseProgram(byte[] Binary, ushort Org)
        {
            return new ZXProgram(null, null, null, null, null, Binary, Org, false);
        }
    }
}
