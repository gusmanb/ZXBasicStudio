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
        public const string ZX_GRAPHICS_GDU = ".gdu";
        public const string ZX_GRAPHICS_UDG = ".gdu";
        public const string ZX_GRAPHICS_FNT = ".gdu";
        public const string ZX_GRAPHICS_SPR = ".gdu";
        public const string ZX_GRAPHICS_TIL = ".gdu";
        public const string ZX_GRAPHICS_MAP = ".gdu";
        public const string ZX_GRAPHICS_GFCG = ".gdu";

        static string[] basicFiles = new string[] { ".bas", ".zxbas", ".zxb" };
        static string[] asmFiles = new string[] { ".asm", ".zxasm", ".zxa", ".z80asm" };
        static string[] configFiles = new string[] { ".zbs" };
        static string[] graphicFiles = new string[] 
        { 
            ZX_GRAPHICS_GDU, 
            ZX_GRAPHICS_UDG, 
            ZX_GRAPHICS_FNT, 
            ZX_GRAPHICS_SPR, 
            ZX_GRAPHICS_TIL,
            ZX_GRAPHICS_MAP,
            ZX_GRAPHICS_GFCG
        };

        static string[] tapeFiles = new string[] { ".tap", ".tzx" };
        public static string[] ZXBasicFiles { get { return basicFiles; } }
        public static string[] ZXAssemblerFiles { get { return asmFiles; } }
        public static string[] ZXConfigFiles { get { return configFiles; } }
        public static string[] ZXGraphicFiles { get { return graphicFiles; } }

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

        /// <summary>
        /// Check if the file is a ZXGraphics file type
        /// .gdu -> GDUs (21 x 8)
        /// .fnt -> Fonts (96 x 8)
        /// .spr -> Sprites (256 x 8)
        /// .til -> Tiles (same as sprites 256 x 8)
        /// .map -> Map data (variable size)
        /// </summary>
        /// <param name="fileName">File name to check</param>
        /// <returns>True if filename is a ZXGraphics file</returns>
        public static bool IsZXGraphics(this string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ZXGraphicFiles.Contains(ext);
        }


        /// <summary>
        /// Returns the type index of a ZXGraphics file. Used to select file icon
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static int GetZXGraphicsSubType(this string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            for (int n=0; n< ZXGraphicFiles.Length; n++)
            {
                if (ZXGraphicFiles[n] == ext)
                {
                    return n;
                }
            }
            return 5;
        }
    }
}
