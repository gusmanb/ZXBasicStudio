using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Emulator.Classes;

namespace ZXBasicStudio.DocumentEditors.ZXRamDisk.Classes
{
    public class ZXRamDiskFile
    {
        public bool EnableIndirect { get; set; } = false;
        public int IndirectBufferSize { get; set; } = 64;
        public bool RelocateStack { get; set; } = false;
        public bool PreserveBin { get; set; } = false;
        public ZXRamDiskLogicBank[] Banks { get; set; } = new ZXRamDiskLogicBank[]
        {
            new ZXRamDiskLogicBank{ Bank = ZXMemoryBank.Bank4 },
            new ZXRamDiskLogicBank{ Bank = ZXMemoryBank.Bank6 },
            new ZXRamDiskLogicBank{ Bank = ZXMemoryBank.Bank1 },
            new ZXRamDiskLogicBank{ Bank = ZXMemoryBank.Bank3 },
            new ZXRamDiskLogicBank{ Bank = ZXMemoryBank.Bank7 },
        };
    }

    public class ZXRamDiskLogicBank
    {
        public ZXMemoryBank Bank { get; set; }
        public List<ZXRamDiskContainedFile> Files { get; set; } = new List<ZXRamDiskContainedFile>();
    }

    public class ZXRamDiskContainedFile
    {
        public required string Name { get; set; }
        public required string SourcePath { get; set; }
        public byte[] Content { get { return File.ReadAllBytes(Path.Combine(ZXProjectManager.Current.ProjectPath, SourcePath)); } }
        public int Size => Content.Length;
    }

    
}
