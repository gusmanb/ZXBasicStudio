using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using static System.Environment;

namespace ZXBasicStudio.Classes
{
    public static class ZXApplicationFileProvider
    {
        static string filePath = Path.Combine(Environment.GetFolderPath(SpecialFolder.ApplicationData), "ZXBasicStudio");

        static ZXApplicationFileProvider() 
        {
            Directory.CreateDirectory(filePath);
        }

        public static bool Exists(string FileName) => File.Exists(Path.Combine(filePath, FileName));

        public static string ReadAllText(string FileName) => File.ReadAllText(Path.Combine(filePath, FileName));

        public static byte[] ReadAllBytes(string FileName) => File.ReadAllBytes(Path.Combine(filePath, FileName));

        public static void WriteAllText(string FileName, string Data) => File.WriteAllText(Path.Combine(filePath, FileName), Data);

        public static void WriteAllBytes(string FileName, byte[] Data) => File.WriteAllBytes(Path.Combine(filePath, FileName), Data);

        public static void Delete(string FileName) => File.Delete(Path.Combine(filePath, FileName));
    }
}
