using ZXGraphics.dat;
using ZXGraphics.neg;

namespace ZXGraphics.log
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
                case ".gdu":
                    ftc.FileType = FileTypes.GDU;
                    ftc.FirstIndex = 64;    // CHAR A
                    ftc.NumerOfPatterns = 21;
                    break;
                case ".fnt":
                    ftc.FileType = FileTypes.Font;
                    ftc.FirstIndex = 32;    // SPACE
                    ftc.NumerOfPatterns = 96;
                    break;
                case ".spr":
                    ftc.FileType = FileTypes.Sprite;
                    ftc.FirstIndex = 0;
                    ftc.NumerOfPatterns = 256;
                    break;
                case ".til":
                    ftc.FileType = FileTypes.Tile;
                    ftc.FirstIndex = 0;
                    ftc.NumerOfPatterns = 256;
                    break;
                case ".map":
                    ftc.FileType = FileTypes.Map;
                    ftc.FirstIndex = 0;
                    ftc.NumerOfPatterns = 0;
                    break;
                case ".gcfg":
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
            return dataLayer.GetFileData(fileName);
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
        /// Save a file of type GDU or Font to disk
        /// </summary>
        /// <param name="fileType">File information</param>
        /// <param name="patterns">Pattens to save</param>
        /// <returns>True if OK or FGalse if error</returns>
        public static bool Files_Save_GDUorFont(FileTypeConfig fileType, IEnumerable<Pattern> patterns)
        {
            try
            {
                if (fileType.FileType != FileTypes.GDU && fileType.FileType != FileTypes.Font)
                {
                    return false;
                }

                var data = new byte[fileType.NumerOfPatterns * 8];
                int index = 0;
                for(int idPattern=0; idPattern < fileType.NumerOfPatterns; idPattern++)
                {
                    var pattern=patterns.FirstOrDefault(d=>d.Id==idPattern);
                    if (pattern == null)
                    {
                        for(int n=0; n<8; n++)
                        {
                            data[index] = 0;
                            index++;
                        }
                    }
                    else
                    {
                        for(int y=0; y<8; y++)
                        {
                            int b = 0;
                            for(int x=0; x<8; x++)
                            {
                                var p = pattern.Data.FirstOrDefault(d => d.Y == y && d.X==x);
                                if (p == null)
                                {
                                    continue;
                                }
                                if (p.ColorIndex == 1)
                                {
                                    b = b | (int)Math.Pow(2,(7-x));
                                }
                            }
                            data[index] = (byte)b;
                            index++;
                        }
                    }
                }

                File.WriteAllBytes(fileType.FileName, data);
                return true;
            }
            catch (Exception ex)
            {
                LastError = "ERROR saving file to disk: " + ex.Message + ex.StackTrace;
                return false;
            }
        }
    }
}