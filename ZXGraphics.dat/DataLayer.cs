namespace ZXGraphics.dat
{
    /// <summary>
    /// Data layer, access to disk files, databases and services
    /// </summary>
    public class DataLayer
    {
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
        public byte[] GetFileData(string fileName)
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
    }
}