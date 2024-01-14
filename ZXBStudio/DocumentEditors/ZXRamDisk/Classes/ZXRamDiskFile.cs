using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXRamDisk.Classes
{
    public class ZXRamDiskFile
    {
        public required string DiskName { get; set; }
        public RamDiskBank Bank { get; set; }
        public List<ZXRamDiskContainedFile> Files { get; set; } = new List<ZXRamDiskContainedFile>();
    }

    public class ZXRamDisk
    {
        public RamDiskBank Bank { get; set; }
        public required byte[] Data { get; set; }
    }

    public class ZXRamDiskContainedFile
    {
        public required string Name { get; set; }
        public required string SourcePath { get; set; }
        public required byte[] Content { get; set; }
        public int Size => Content?.Length ?? 0;
    }

    public enum RamDiskBank
    {
        Bank1 = 1,
        Bank3 = 3,
        Bank4 = 4,
        Bank6 = 6,
        Bank7 = 7
    }
}
