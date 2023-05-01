using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public static class ZXExtensions
    {
        static string[] basicFiles = new string[] { ".bas", ".zxbas", ".zxb" };
        static string[] asmFiles = new string[] { ".asm", ".zxasm", ".zxa", ".z80asm" };
        static string[] configFiles = new string[] { ".zbs" };
        static string[] tapeFiles = new string[] { ".tap", ".tzx" };
        public static string[] ZXBasicFiles { get { return basicFiles; } }
        public static string[] ZXAssemblerFiles { get { return asmFiles; } }
        public static string[] ZXConfigFiles { get { return configFiles; } }
        public static string[] ZXTapeFiles { get { return tapeFiles; } }
        public static bool IsZXBasic(this string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ZXBasicFiles.Contains(ext);
        }

        public static bool IsZXAssembler(this string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ZXAssemblerFiles.Contains(ext);
        }

        public static bool IsZXConfig(this string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ZXConfigFiles.Contains(ext);
        }
    }
}
