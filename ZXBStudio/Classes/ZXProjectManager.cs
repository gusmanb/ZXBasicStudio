using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.Classes
{
    public class ZXProjectManager
    {
        public static ZXProjectManager? Current { get; private set; }

        internal static bool OpenProject(string ProjectPath)
        {
            if (Current != null)
                return false;

            string path = Path.GetFullPath(ProjectPath);

            if(!Directory.Exists(path)) 
                return false;

            Current = new ZXProjectManager(path);

            return true;
        }

        internal static bool CreateProject(string ProjectPath, ZXBuildSettings Settings, TextWriter OutputLog)
        {
            try
            {
                var path = Path.GetFullPath(ProjectPath);

                OutputLog.WriteLine($"Creating project at {path}...");

                if (!Directory.Exists(path))
                {
                    OutputLog.WriteLine($"Folder {path} not found, creating...");
                    Directory.CreateDirectory(path);
                }

                OutputLog.WriteLine($"Saving build settings...");

                var settingsFile = Path.Combine(path, ZXConstants.BUILDSETTINGS_FILE);
                var content = JsonConvert.SerializeObject(Settings);
                File.WriteAllText(settingsFile, content);

                OutputLog.WriteLine($"Project created successfully.");

                return true;
            }
            catch(Exception ex) 
            {
                OutputLog.WriteLine($"Unexpected error creating project: {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        internal static void CloseProject()
        {
            Current = null;
        }

        public string ProjectPath { get; private set; }
        
        private ZXProjectManager(string projectPath)
        {
            ProjectPath = Path.GetFullPath(projectPath.Trim());
        }

        public ZXBuildSettings GetProjectSettings() 
        {
            string buildFile = Path.Combine(ProjectPath, ZXConstants.BUILDSETTINGS_FILE);

            if (File.Exists(buildFile))
            {
                var settings = JsonConvert.DeserializeObject<ZXBuildSettings>(File.ReadAllText(buildFile));
                if (settings != null)
                    return settings;
            }
            else if (ZXOptions.Current.DefaultBuildSettings != null)
                return ZXOptions.Current.DefaultBuildSettings;

            return new ZXBuildSettings();
        }

        public bool SaveProjectSettings(ZXBuildSettings Settings)
        {
            try
            {
                string buildFile = Path.Combine(ProjectPath, ZXConstants.BUILDSETTINGS_FILE);
                File.WriteAllText(buildFile, JsonConvert.SerializeObject(Settings));
                return true;
            }
            catch { return false; }
        }

        public ZXExportOptions? GetExportOptions() 
        {
            ZXExportOptions? opts = null;
            string optsFile = Path.Combine(ProjectPath, ZXConstants.EXPORTSETTINGS_FILE);
            if (File.Exists(optsFile))
            {
                try
                {
                    opts = JsonConvert.DeserializeObject<ZXExportOptions>(File.ReadAllText(optsFile));
                }
                catch { }
            }

            return opts;
        }

        public bool SaveExportOptions(ZXExportOptions Options)
        {
            string optsFile = Path.Combine(ProjectPath, ZXConstants.EXPORTSETTINGS_FILE);

            try 
            {
                File.WriteAllText(optsFile, JsonConvert.SerializeObject(Options));
                return true;
            } 
            catch { return false; }
        }

        public string[] FindFiles(IEnumerable<string> SearchPatterns)
        {
            List<string> files = new List<string>();

            foreach(var pattern in SearchPatterns)
                files.AddRange(Directory.EnumerateFiles(ProjectPath, pattern));

            return files.ToArray();
        }

        public string? FindFile(string FileName)
        {
            return Directory.EnumerateFiles(ProjectPath, FileName, SearchOption.AllDirectories).FirstOrDefault();
        }

        public string? GetMainFile()
        {
            var settings = GetProjectSettings();
            string mainFile = null;

            if (string.IsNullOrWhiteSpace(settings.MainFile))
            {
                List<string> mainFiles = new List<string>();

                foreach (var ext in ZXExtensions.ZXBasicFiles)
                    mainFiles.AddRange(Directory.GetFiles(ProjectPath, $"main{ext}"));

                if (mainFiles.Count > 1)
                    return null;
                else if (mainFiles.Count < 1)
                    return null;

                mainFile = mainFiles[0];
            }
            else
            {
                mainFile = Path.Combine(ProjectPath, settings.MainFile);
                if (!File.Exists(mainFile))
                    return null;
            }

            return mainFile;
        }
    }
}
