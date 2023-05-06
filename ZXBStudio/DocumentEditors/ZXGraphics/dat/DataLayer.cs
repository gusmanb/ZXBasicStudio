using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.IO;

namespace ZXBasicStudio.DocumentEditors.ZXGraphics.dat
{
    /// <summary>
    /// Data layer, access to disk files, databases and services
    /// </summary>
    public class DataLayer
    {
        /// <summary>
        /// Description of last error
        /// Ignore it if the last function hasn't error
        /// </summary>
        public string LastError = "";

        /// <summary>
        /// Default GDU data (21 chars = 168 bytes)
        /// </summary>
        private byte[] GDUData =
        {
            0,60,66,66,126,66,66,0,0,124,66,124,66,66,124,0,0,60,66,64,64,66,60,0,0,120,68,66,66,68,120,0,0,126,64,124,64,64,126,0,0,126,64,124,64,64,64,0,0,60,66,64,78,66,60,0,0,66,66,126,66,66,66,0,0,62,8,8,8,8,62,0,0,2,2,2,66,66,60,0,0,68,72,112,72,68,66,0,0,64,64,64,64,64,126,0,0,66,102,90,66,66,66,0,0,66,98,82,74,70,66,0,0,60,66,66,66,66,60,0,0,124,66,66,124,64,64,0,0,60,66,66,82,74,60,0,0,124,66,66,124,68,66,0,0,60,64,60,2,66,60,0,0,254,16,16,16,16,16,0,0,66,66,66,66,66,60,0
        };

        /// <summary>
        /// Default font data (96 chars = 768 bytes)
        /// </summary>
        private byte[] FontData =
        {
          0,0,0,0,0,0,0,0,0,16,16,16,16,0,16,0,0,36,36,0,0,0,0,0,0,36,126,36,36,126,36,0,0,8,62,40,62,10,62,8,0,98,100,8,16,38,70,0,0,16,40,16,42,68,58,0,0,8,16,0,0,0,0,0,0,4,8,8,8,8,4,0,0,32,16,16,16,16,32,0,0,0,20,8,62,8,20,0,0,0,8,8,62,8,8,0,0,0,0,0,0,8,8,16,0,0,0,0,62,0,0,0,0,0,0,0,0,24,24,0,0,0,2,4,8,16,32,0,0,60,70,74,82,98,60,0,0,24,40,8,8,8,62,0,0,60,66,2,60,64,126,0,0,60,66,12,2,66,60,0,0,8,24,40,72,126,8,0,0,126,64,124,2,66,60,0,0,60,64,124,66,66,60,0,0,126,2,4,8,16,16,0,0,60,66,60,66,66,60,0,0,60,66,66,62,2,60,0,0,0,0,16,0,0,16,0,0,0,16,0,0,16,16,32,0,0,4,8,16,8,4,0,0,0,0,62,0,62,0,0,0,0,16,8,4,8,16,0,0,60,66,4,8,0,8,0,0,60,74,86,94,64,60,0,0,60,66,66,126,66,66,0,0,124,66,124,66,66,124,0,0,60,66,64,64,66,60,0,0,120,68,66,66,68,120,0,0,126,64,124,64,64,126,0,0,126,64,124,64,64,64,0,0,60,66,64,78,66,60,0,0,66,66,126,66,66,66,0,0,62,8,8,8,8,62,0,0,2,2,2,66,66,60,0,0,68,72,112,72,68,66,0,0,64,64,64,64,64,126,0,0,66,102,90,66,66,66,0,0,66,98,82,74,70,66,0,0,60,66,66,66,66,60,0,0,124,66,66,124,64,64,0,0,60,66,66,82,74,60,0,0,124,66,66,124,68,66,0,0,60,64,60,2,66,60,0,0,254,16,16,16,16,16,0,0,66,66,66,66,66,60,0,0,66,66,66,66,36,24,0,0,66,66,66,66,90,36,0,0,66,36,24,24,36,66,0,0,130,68,40,16,16,16,0,0,126,4,8,16,32,126,0,0,14,8,8,8,8,14,0,0,0,64,32,16,8,4,0,0,112,16,16,16,16,112,0,0,16,56,84,16,16,16,0,0,0,0,0,0,0,0,255,0,28,34,120,32,32,126,0,0,0,56,4,60,68,60,0,0,32,32,60,34,34,60,0,0,0,28,32,32,32,28,0,0,4,4,60,68,68,60,0,0,0,56,68,120,64,60,0,0,12,16,24,16,16,16,0,0,0,60,68,68,60,4,56,0,64,64,120,68,68,68,0,0,16,0,48,16,16,56,0,0,4,0,4,4,4,36,24,0,32,40,48,48,40,36,0,0,16,16,16,16,16,12,0,0,0,104,84,84,84,84,0,0,0,120,68,68,68,68,0,0,0,56,68,68,68,56,0,0,0,120,68,68,120,64,64,0,0,60,68,68,60,4,6,0,0,28,32,32,32,32,0,0,0,56,64,56,4,120,0,0,16,56,16,16,16,12,0,0,0,68,68,68,68,56,0,0,0,68,68,40,40,16,0,0,0,68,84,84,84,40,0,0,0,68,40,16,40,68,0,0,0,68,68,68,60,4,56,0,0,124,8,16,32,124,0,0,14,8,48,8,8,14,0,0,8,8,8,8,8,8,0,0,112,16,12,16,16,112,0,0,20,40,0,0,0,0,0,60,66,153,161,161,153,66,60
        };


        /// <summary>
        /// Initializes the dataLayer
        /// </summary>
        /// <returns>True if ok, false if error</returns>
        public bool Initialize()
        {
            return true;
        }


        /// <summary>
        /// Reads the binary content of a file
        /// </summary>
        /// <param name="fileName">Filename with path</param>
        /// <returns>Array of byte with data or null if error</returns>
        public byte[] ReadFileData(string fileName)
        {
            try
            {
                var data = File.ReadAllBytes(fileName);
                return data;
            }
            catch (Exception ex)
            {
                LastError = "ERROR reading file: " + ex.Message + ex.StackTrace;
                return null;
            }
        }


        /// <summary>
        /// Write file to disk, if the file exist, it is overwritten
        /// </summary>
        /// <param name="fileName">Filename width path</param>
        /// <param name="data">Data to write</param>
        /// <returns>True if correct or False if error</returns>
        internal bool WriteFileData(string fileName, byte[] data)
        {
            try
            {
                File.WriteAllBytes(fileName, data);
                return true;
            }
            catch (Exception ex)
            {
                LastError = "ERROR reading file: " + ex.Message + ex.StackTrace;
                return false;
            }
        }


        /// <summary>
        /// Create defalt data for .gdu or .fnt files
        /// </summary>
        /// <param name="fileType">File type</param>
        /// <returns>Array of bytes with the data or null if the type is unsuported</returns>
        public byte[] Files_CreateData(FileTypeConfig fileType)
        {
            switch (fileType.FileType)
            {
                case FileTypes.GDU:
                    return GDUData;
                case FileTypes.Font:
                    return FontData;
            }
            return null;
        }
    }
}