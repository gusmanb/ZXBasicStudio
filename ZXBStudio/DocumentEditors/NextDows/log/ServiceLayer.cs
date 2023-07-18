using ZXBasicStudio.DocumentEditors.NextDows.dat;
using ZXBasicStudio.DocumentEditors.NextDows.neg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Common;
using System.Runtime;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Media;

namespace ZXBasicStudio.DocumentEditors.NextDows.log
{

    /// <summary>
    /// Main service an logic layer
    /// </summary>
    public static class ServiceLayer
    {
        public static string LastError = "";
        public static bool Initialized = false;

        private static DataLayer dataLayer = null;


        /// <summary>
        /// Initializes the dataLayer
        /// </summary>
        /// <returns>True if ok, false if error</returns>
        public static bool Initialize()
        {
            if (dataLayer == null)
            {
                dataLayer = new DataLayer();
                if (!dataLayer.Initialize())
                {
                    return false;
                }
            }
            Initialized = true;
            return true;
        }

        /// <summary>
        /// Get all files oof a type, in a directory and his subdirectories
        /// </summary>
        /// <param name="path">Root path</param>
        /// <param name="filetyle">FileType</param>
        /// <returns>Array of strings with the fullpath filenames</returns>
        public static string[] Files_GetAllConfigFiles(string path, string extension)
        {
            var lst = new List<string>();
            dataLayer.Files_GetAllFileNames(path, extension, ref lst);
            return lst.ToArray();
        }


        /// <summary>
        /// Generates de Next default palette
        /// </summary>
        /// <returns>Array of 256 elements with the default palette used by Next</returns>
        public static PaletteColor[] CrateNextDefaultPalette()
        {
            int iRed = 0;
            int iGreen = 0;
            int iBlue = 0;
            int index = 0;

            PaletteColor[] palette = new PaletteColor[256];

            for(index=0; index<256; index++)
            {
                var p = new PaletteColor();
                p.Index = index;
                iRed = (byte)((index & 0b11100000) >> 5);
                p.Red = (byte)((255 * iRed) / 7);
                iGreen = (byte)((index & 0b00011100) >> 2);
                p.Green = (byte)((255 * iGreen) / 7);
                iBlue = (byte)((index & 0b00000011) << 1);
                if(iBlue != 0){
                    iBlue++;
                }
                p.Blue = (byte)((255 * iBlue) / 7);
                p.Brush = new SolidColorBrush(new Color(255, p.Red, p.Green, p.Blue));
                palette[index] = p;                
            }
            return palette;
        }
    }
}