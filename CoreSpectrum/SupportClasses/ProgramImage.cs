using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.SupportClasses
{
    public class ProgramImage
    {
        public required ushort Org { get; set; }
        public required byte InitialBank { get; set; }
        public required ImageChunk[] Chunks { get; set; }
    }

    public class ImageChunk
    {
        public ushort Address { get; set; }
        public byte Bank { get; set; }
        public required byte[] Data { get; set; }
    }
}
