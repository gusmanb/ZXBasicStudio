using ZXBasicStudio.DocumentEditors.ZXGraphics.neg;
using System;
using System.IO;
using System.Collections.Generic;

namespace ZXBasicStudio.DocumentEditors.NextDows.dat
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
        internal bool Files_WriteFileData(string fileName, byte[] data)
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
        /// Rename a file
        /// </summary>
        /// <param name="oldName">Old filename</param>
        /// <param name="newName">New filename</param>
        /// <returns>True if OK or False if error</returns>
        public bool Files_Rename(string oldName, string newName)
        {
            try
            {
                if (File.Exists(newName))
                {
                    LastError = "A file with this name already exists.";
                    return false;
                }
                File.Move(oldName, newName);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }


        /// <summary>
        /// Get (load) the file data in string format
        /// </summary>
        /// <param name="fileName">Full path of the file</param>
        /// <returns>String with all file contents or null is error or not exist</returns>
        public string Files_GetString(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    var data = File.ReadAllText(fileName);
                    return data;
                }
                else
                {
                    LastError = "File " + fileName + " not found";
                    return null;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return null;
            }
        }


        /// <summary>
        /// Set (save) the file data in string format
        /// </summary>
        /// <param name="fileName">Full path pf the file</param>
        /// <param name="data">Data to save</param>
        /// <returns>True if correct or false if error</returns>
        public bool Files_SetString(string fileName, string data)
        {
            try
            {
                File.WriteAllText(fileName, data);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }


        /// <summary>
        /// Get all files oof a type, in a directory and his subdirectories
        /// </summary>
        /// <param name="path">Root path</param>
        /// <param name="extension">file extension (.gdu)</param>
        /// <returns>Array of strings with the fullpath filenames</returns>
        public void Files_GetAllFileNames(string path, string extension, ref List<string> lst)
        {
            var files = Directory.GetFiles(path, "*" + extension);
            lst.AddRange(files);

            var directories = Directory.GetDirectories(path);
            foreach(var dir in directories)
            {
                Files_GetAllFileNames(dir, extension, ref lst);
            }
        }
    }
}