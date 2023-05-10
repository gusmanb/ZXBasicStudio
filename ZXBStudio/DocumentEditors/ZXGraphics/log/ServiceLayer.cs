using ZXBasicStudio.DocumentEditors.ZXGraphics.dat;
using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZXBasicStudio.Classes;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.log
{

    /// <summary>
    /// Main service an logic layer
    /// </summary>
    public static class ServiceLayer
    {
        public static string LastError = "";

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
                    ftc.FileType = FileTypes.GDU;
                    ftc.FirstIndex = 64;    // CHAR A
                    ftc.NumerOfPatterns = 21;
                    break;
                case ZXExtensions.ZX_GRAPHICS_FNT:
                    ftc.FileType = FileTypes.Font;
                    ftc.FirstIndex = 32;    // SPACE
                    ftc.NumerOfPatterns = 96;
                    break;
                case ZXExtensions.ZX_GRAPHICS_SPR:
                    ftc.FileType = FileTypes.Sprite;
                    ftc.FirstIndex = 0;
                    ftc.NumerOfPatterns = 256;
                    break;
                case ZXExtensions.ZX_GRAPHICS_TIL:
                    ftc.FileType = FileTypes.Tile;
                    ftc.FirstIndex = 0;
                    ftc.NumerOfPatterns = 256;
                    break;
                case ZXExtensions.ZX_GRAPHICS_MAP:
                    ftc.FileType = FileTypes.Map;
                    ftc.FirstIndex = 0;
                    ftc.NumerOfPatterns = 0;
                    break;
                case ZXExtensions.ZX_GRAPHICS_GCFG:
                    ftc.FileType = FileTypes.Config;
                    ftc.FirstIndex = 0;
                    ftc.NumerOfPatterns = 256;
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
                return dataLayer.WriteFileData(fileType.FileName, data);
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
        /// <returns>Arrfay oof byte with the data ready to save oon disk</returns>
        public static byte[] Files_CreateBinData_GDUorFont(FileTypeConfig fileType, IEnumerable<Pattern> patterns)
        {
            try
            {
                if (fileType.FileType != FileTypes.GDU && fileType.FileType != FileTypes.Font)
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
                return dataLayer.WriteFileData(fileName, data);
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
    }
}