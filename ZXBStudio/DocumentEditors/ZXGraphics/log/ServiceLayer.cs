using ZXBasicStudio.DocumentEditors.ZXGraphics.dat;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZXBasicStudio.Classes;
using ZXBasicStudio.Common;
using System.Runtime;
using Newtonsoft.Json;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.log
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
        /// Returns the FileTypes of a file based in its fileName
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <returns>FileTypes, 5 (config) for other files</returns>
        public static FileTypeConfig GetFileType(string filename)
        {
            var ext = Path.GetExtension(filename).ToLower();
            var ftc = new FileTypeConfig();
            ftc.FileName = filename;

            switch (ext)
            {
                case ZXExtensions.ZX_GRAPHICS_UDG:
                case ZXExtensions.ZX_GRAPHICS_GDU:
                    ftc.FileType = FileTypes.UDG;
                    ftc.FirstIndex = 64;    // CHAR A
                    ftc.NumerOfPatterns = 21;
                    break;
                case ZXExtensions.ZX_GRAPHICS_FNT:
                    ftc.FileType = FileTypes.Font;
                    ftc.FirstIndex = 32;    // SPACE
                    ftc.NumerOfPatterns = 96;
                    break;
            }
            return ftc;
        }


        /// <summary>
        /// Reads the binary content of a file
        /// </summary>
        /// <param name="fileName">Filename with path</param>
        /// <returns>Array of byte with data or null if error</returns>
        public static byte[] GetFileData(string fileName)
        {
            return dataLayer.ReadFileData(fileName);
        }


        /// <summary>
        /// Extract PointData from binary data in classic mode (1 color)
        /// </summary>
        /// <param name="id">Id of the char/font/sprite/tile</param>
        /// <param name="fileData">Complete file binary data</param>
        /// <param name="startX">Offset x, only for label</param>
        /// <param name="startY">Offset y, only for label</param>
        /// <returns>Array of PointData</returns>
        public static PointData[] Binary2PointData(int id, byte[] fileData, int startX, int startY)
        {
            try
            {
                var pds = new List<PointData>();
                var binData = new byte[8];
                Array.Copy(fileData, id * 8, binData, 0, 8);

                for (int y = 0; y < 8; y++)
                {
                    for (var b = 0; b < 8; b++)
                    {
                        var pd = new PointData();
                        pd.X = startX + (7 - b);
                        pd.Y = startY + y;
                        int bit = binData[y] & 1;
                        pd.ColorIndex = bit;
                        pds.Add(pd);
                        binData[y] = (byte)(binData[y] >> 1);
                    }
                }
                return pds.ToArray();
            }
            catch (Exception ex)
            {
                LastError = "ERROR extracting in pattern data: " + ex.Message + ex.StackTrace;
                return null;
            }
        }


        /// <summary>
        /// Converts binary data to tap format binary data
        /// </summary>
        /// <param name="fileName">ZX Spectrum file name</param>
        /// <param name="address">address of the binary data on ZX Spectrum</param>
        /// <param name="data">Data to convert</param>
        /// <returns>Array of bytes or null if error</returns>
        public static byte[] Bin2Tap(string fileName, int address, byte[] data)
        {
            try
            {
                var tapGen = new Common.cTapGenerator();
                var fileTap = new Common.cTapGenerator.tTapFile()
                {
                    blockName = fileName,
                    blockSize = data.Length,
                    data = data,
                    startAddress = address
                };
                return tapGen.createTap(fileTap);
            }
            catch (Exception ex)
            {
                LastError = "ERROR generating tap file: " + ex.Message + ex.StackTrace;
                return null;
            }
        }


        /// <summary>
        /// Save a file of type GDU or Font to disk
        /// </summary>
        /// <param name="fileType">File information</param>
        /// <param name="patterns">Pattens to save</param>
        /// <returns>True if OK or False if error</returns>
        public static bool Files_Save_GDUorFont(FileTypeConfig fileType, IEnumerable<Pattern> patterns)
        {
            try
            {
                var data = Files_CreateBinData_GDUorFont(fileType, patterns);
                return dataLayer.Files_WriteFileData(fileType.FileName, data);
            }
            catch (Exception ex)
            {
                LastError = "ERROR saving file to disk: " + ex.Message + ex.StackTrace;
                return false;
            }
        }


        /// <summary>
        /// Creates the binary data for a file of type GDU or Font to disk
        /// </summary>
        /// <param name="fileType">File information</param>
        /// <param name="patterns">Pattens to use</param>
        /// <returns>Arrfay of byte with the data ready to save on disk</returns>
        public static byte[] Files_CreateBinData_GDUorFont(FileTypeConfig fileType, IEnumerable<Pattern> patterns)
        {
            try
            {
                if (fileType.FileType != FileTypes.UDG && fileType.FileType != FileTypes.Font)
                {
                    return null;
                }

                var data = new byte[fileType.NumerOfPatterns * 8];
                int index = 0;
                for (int idPattern = 0; idPattern < fileType.NumerOfPatterns; idPattern++)
                {
                    var pattern = patterns.FirstOrDefault(d => d.Id == idPattern);
                    if (pattern == null)
                    {
                        for (int n = 0; n < 8; n++)
                        {
                            data[index] = 0;
                            index++;
                        }
                    }
                    else
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            int b = 0;
                            for (int x = 0; x < 8; x++)
                            {
                                var p = pattern.Data.FirstOrDefault(d => d.Y == y && d.X == x);
                                if (p == null)
                                {
                                    continue;
                                }
                                if (p.ColorIndex == 1)
                                {
                                    b = b | (int)Math.Pow(2, (7 - x));
                                }
                            }
                            data[index] = (byte)b;
                            index++;
                        }
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                LastError = "ERROR generating binary data: " + ex.Message + ex.StackTrace;
                return null;
            }
        }


        /// <summary>
        /// Save binary data to a file
        /// </summary>
        /// <param name="fileName">File name with full path</param>
        /// <param name="data">Data to save</param>
        /// <returns>True if ok or false if error</returns>
        public static bool Files_SaveFileData(string fileName, byte[] data)
        {
            try
            {
                return dataLayer.Files_WriteFileData(fileName, data);
            }
            catch (Exception ex)
            {
                LastError = "ERROR saving data: " + ex.Message + ex.StackTrace;
                return false;
            }
        }


        /// <summary>
        /// Save string data to a file
        /// </summary>
        /// <param name="fileName">File name with full path</param>
        /// <param name="data">Data to save</param>
        /// <returns>True if ok or false if error</returns>
        public static bool Files_SaveFileString(string fileName, string data)
        {
            try
            {
                return dataLayer.Files_SetString(fileName, data);
            }
            catch (Exception ex)
            {
                LastError = "ERROR saving data: " + ex.Message + ex.StackTrace;
                return false;
            }
        }


        /// <summary>
        /// Create defalt data for .gdu or .fnt files
        /// </summary>
        /// <param name="fileType">File type</param>
        /// <returns>Array of bytes with the data or null if the type is unsuported</returns>
        public static byte[] Files_CreateData(FileTypeConfig fileType)
        {
            return dataLayer.Files_CreateData(fileType);
        }


        // <summary>
        /// Rename a file
        /// </summary>
        /// <param name="oldName">Old filename</param>
        /// <param name="newName">New filename</param>
        /// <returns>True if OK or False if error</returns>
        public static bool Files_Rename(string oldName, string newName)
        {
            if (!dataLayer.Files_Rename(oldName, newName))
            {
                LastError = dataLayer.LastError;
                return false;
            }
            return true;
        }


        /// <summary>
        /// Get all files oof a type, in a directory and his subdirectories
        /// </summary>
        /// <param name="path">Root path</param>
        /// <param name="filetyle">FileType</param>
        /// <returns>Array of strings with the fullpath filenames</returns>
        public static string[] Files_GetAllConfigFiles(string path, FileTypes filetyle)
        {
            var lst = new List<string>();
            switch (filetyle)
            {
                case FileTypes.UDG:
                    dataLayer.Files_GetAllFileNames(path, ".udg.zbs", ref lst);
                    dataLayer.Files_GetAllFileNames(path, ".gdu.zbs", ref lst);
                    return lst.ToArray();
                case FileTypes.Font:
                    dataLayer.Files_GetAllFileNames(path, ".fnt.zbs", ref lst);
                    return lst.ToArray();
            }
            return null;
        }


        /// <summary>
        /// Returns the ExportConfig (.zbs) of a file
        /// </summary>
        /// <param name="fileName">Full path and name of the config file</param>
        /// <returns>ExportConfig object or null if file not exists</returns>
        public static ExportConfig Export_GetConfigFile(string fileName)
        {
            try
            {
                var jsonData = dataLayer.Files_GetString(fileName);
                if (string.IsNullOrEmpty(jsonData))
                {
                    return null;
                }

                ExportConfig exportConfig = jsonData.Deserializar<ExportConfig>();
                return exportConfig;
            }
            catch (Exception ex)
            {
                LastError = "Error deserializing \"" + fileName + "\" to ExportConfig";
                return null;
            }
        }


        public static bool Export_SetConfigFile(string fileName, ExportConfig exportConfig)
        {
            try
            {
                if (exportConfig == null)
                {
                    return false;
                }

                var jsonData = exportConfig.Serializar();
                return dataLayer.Files_SetString(fileName, jsonData);
            }
            catch (Exception ex)
            {
                LastError = "Error deserializing \"" + fileName + "\" to ExportConfig";
                return false;
            }
        }


        public static ZXBuildSettings GetProjectSettings()
        {
            //ZXProjectManager
            try
            {
                var settingsPath = MainWindow.GetProjectRootPath();
                var settingsFile = Path.Combine(settingsPath, ZXConstants.BUILDSETTINGS_FILE);

                if (!string.IsNullOrEmpty(settingsPath))
                {
                    var jsonData = dataLayer.Files_GetString(settingsFile);
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        var settings = JsonConvert.DeserializeObject<ZXBuildSettings>(jsonData);
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = "Error getting project settings";
            }
            return null;
        }


        #region Palettes

        private static PaletteColor[] DefaultColors = new PaletteColor[]
        {
            new PaletteColor() { Red=0x00, Green=0x00, Blue=0x00 },     // Black
            new PaletteColor() { Red=0x00, Green=0x00, Blue=0xa0 },     // Blue
            new PaletteColor() { Red=0xdc, Green=0x00, Blue=0x00 },     // Red
            new PaletteColor() { Red=0xe4, Green=0x00, Blue=0xb4 },     // Magenta
            new PaletteColor() { Red=0x00, Green=0xd4, Blue=0x00 },     // Green
            new PaletteColor() { Red=0x00, Green=0xd4, Blue=0xd4 },     // Cyan
            new PaletteColor() { Red=0xd0, Green=0xd0, Blue=0x00 },     // Yellow
            new PaletteColor() { Red=0xc8, Green=0xc8, Blue=0xc8 },     // White
            // Bright 1
            new PaletteColor() { Red=0x00, Green=0x00, Blue=0x00 },     // Black
            new PaletteColor() { Red=0x00, Green=0x00, Blue=0xac },     // Blue
            new PaletteColor() { Red=0xf0, Green=0x00, Blue=0x00 },     // Red
            new PaletteColor() { Red=0xfc, Green=0x00, Blue=0xdc },     // Magenta
            new PaletteColor() { Red=0x00, Green=0xf0, Blue=0x00 },     // Green
            new PaletteColor() { Red=0x00, Green=0xfc, Blue=0xfc },     // Cyan
            new PaletteColor() { Red=0xfc, Green=0xfc, Blue=0x00 },     // Yellow
            new PaletteColor() { Red=0xfc, Green=0xfc, Blue=0xfc }      // White
        };


        public static PaletteColor[] GetPalette(GraphicsModes mode)
        {
            switch (mode)
            {
                case GraphicsModes.Monochrome:
                    return new PaletteColor[]
                    {
                        DefaultColors[7], DefaultColors[0]
                    };

                case GraphicsModes.ZXSpectrum:
                    return DefaultColors;

                default:
                    // TODO: Next Palete
                    return DefaultColors;
            }
        }

        #endregion


        #region Sprites

        /// <summary>
        /// Resizes the point data of the sprite to the new size
        /// </summary>
        /// <param name="sprite">Old sprite with new Width and Height properties set to target. Patterns will be updated.</param>
        /// <param name="oldWidth">Old Width of the sprite, the new must set in sprite parameter</param>
        /// <param name="oldHeight">Old Height of the spritye, the new must set in sprite parameter</param>
        /// <returns>True if OK or False if error</returns>
        public static bool SpriteData_Resize(ref Sprite sprite, int oldWidth, int oldHeight)
        {
            for (int p = 0; p < sprite.Patterns.Count; p++)
            {
                var pattern = sprite.Patterns[p];
                var patList = pattern.Data.ToList();
                for (int y = 0; y < sprite.Height; y++)
                {
                    for (int x = 0; x < sprite.Width; x++)
                    {
                        var point = patList.FirstOrDefault(d => d.X == x && d.Y == y);
                        if (point == null)
                        {
                            patList.Add(new PointData()
                            {
                                ColorIndex = sprite.DefaultColor,
                                X = x,
                                Y = y
                            });
                        }
                    }
                }

                var w = sprite.Width;
                var h = sprite.Height;
                patList.RemoveAll(d => d.X >= w || d.Y >= h);
                sprite.Patterns[p].Data = patList.ToArray();
            }
            return true;
        }


        /// <summary>
        /// Change the sprite mode
        /// </summary>
        /// <param name="sprite">Old sprite with new graphic mode property set to target. Patterns will be updated.</param>
        /// <param name="oldMode">Old graphic mode, the new must set in sprite parameter</param>
        /// <returns>True if OK or False if error</returns>
        public static bool SpriteData_ChangeMode(ref Sprite sprite, GraphicsModes oldMode)
        {
            // TODO: Do it!!!
            return true;
        }


        /// <summary>
        /// Change the sprite mask parameter
        /// </summary>
        /// <param name="sprite">Old sprite with new mask status property set to target. Patterns will be updated.</param>
        /// <param name="oldMasked">Old mask value, the new must set in sprite parameter</param>
        /// <returns>True if OK or False if error</returns>
        public static bool SpriteData_ChangeMasked(ref Sprite sprite, bool oldMasked)
        {
            // TODO: Do it!!!
            return true;
        }


        /// <summary>
        /// Change the sprite frames parameter
        /// </summary>
        /// <param name="sprite">Old sprite with new Frames property set to target. Patterns will be updated.</param>
        /// <param name="oldFrames">Old Frames value, the new must set in sprite parameter</param>
        /// <returns>True if OK or False if error</returns>
        public static bool SpriteData_ChangeFrames(ref Sprite sprite, byte oldFrames)
        {
            // TODO: Do it!!!
            return true;
        }

        #endregion
    }
}