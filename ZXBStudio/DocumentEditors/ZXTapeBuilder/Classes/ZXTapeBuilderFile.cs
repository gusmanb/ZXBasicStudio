using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Classes
{
    public class ZXTapeBuilderFile
    {
        public required string ProgramName { get; set; }
        public bool UseInk { get; set; }
        public int Ink { get; set; }
        public bool UsePaper { get; set; }
        public int Paper { get; set; }
        public bool UseBorder { get; set; }
        public int Border { get; set; }
        public bool HideHeaders { get; set; }
        public ZXTapeBuilderPoke[]? PokesBeforeLoad { get; set; }
        public ZXTapeBuilderPoke[]? PokesAfterLoad { get; set; }
        public string? ScreenName { get; set; }
        public string? ScreenFile { get; set; }
        public ZXTapeBuilderDataBlock[]? DataBlocks { get; set; }
        public bool IncludeRAMDisk { get; set; }
        public ZXRAMDiskOrder RAMDiskOrder { get; set; }
    }

    public enum ZXRAMDiskOrder
    {
        Before,
        After
    }

    public class ZXTapeBuilderPoke
    {
        public ushort Address { get; set; }
        public byte Value { get; set; }
    }
    public class ZXTapeBuilderDataBlock
    {
        public required string BlockFile { get; set; }
        public required string BlockName { get; set; }
        public ushort BlockAddress { get; set; }
        public bool BasicLoad { get; set; }
    }
}
