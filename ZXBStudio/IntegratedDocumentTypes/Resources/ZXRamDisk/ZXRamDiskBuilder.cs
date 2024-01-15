using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.BuildSystem;
using ZXBasicStudio.DocumentEditors.ZXRamDisk.Classes;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;
using ZXBasicStudio.IntegratedDocumentTypes.TapeDocuments.ZXTapeBuilder;

namespace ZXBasicStudio.IntegratedDocumentTypes.Resources.ZXRamDisk
{
    public class ZXRamDiskBuilder : IZXDocumentBuilder
    {
        public bool Build(string BuildPath, ZXBuildStage Stage, ZXBuildType BuildType, ZXProgram? CompiledProgram, TextWriter OutputLog)
        {
            switch (Stage)
            {
                case ZXBuildStage.PreBuild:

                    return BuildDiskAndCode(BuildPath, BuildType, OutputLog);

                case ZXBuildStage.PostBuild:

                    if (CompiledProgram == null)
                        return false;

                    return InjectDisk(BuildPath, CompiledProgram, OutputLog);
            }

            return false;
        }

        private bool InjectDisk(string buildPath, ZXProgram compiledProgram, TextWriter outputLog)
        {
            string[] diskBuilds = Directory.GetFiles(buildPath, "*" + ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXRamDiskDocument)).DocumentExtensions[0], SearchOption.AllDirectories);

            if (diskBuilds == null || diskBuilds.Length == 0)
                return true;

            foreach(var diskPath in diskBuilds) 
            {
                string fileName = Path.GetFileName(diskPath);
                outputLog.WriteLine($"Injecting RAM disk {fileName}...");

                try
                {
                    ZXRamDiskFile diskFile = JsonConvert.DeserializeObject<ZXRamDiskFile>(File.ReadAllText(diskPath));
                    byte[] binFile = File.ReadAllBytes(diskPath.Substring(0, diskPath.Length - 4) + ".zxrbin");

                    compiledProgram.RamDisks.Add(new DocumentEditors.ZXRamDisk.Classes.ZXRamDisk { Bank = diskFile.Bank, Data = binFile });
                }
                catch (Exception ex)
                {
                    outputLog.WriteLine($"Error injecting RAM disk {diskPath}: \r\n" + ex.ToString());
                    return false;
                }
            }

            return true;
        }

        private bool BuildDiskAndCode(string buildPath, ZXBuildType BuildType, TextWriter outputLog)
        {
            string[] diskBuilds = Directory.GetFiles(buildPath, "*" + ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXRamDiskDocument)).DocumentExtensions[0], SearchOption.AllDirectories);

            if (diskBuilds == null || diskBuilds.Length == 0)
                return true;

            foreach (var diskPath in diskBuilds)
            {
                string fileName = Path.GetFileName(diskPath);
                outputLog.WriteLine($"Generating RAM disk {fileName}...");

                try
                {
                    ZXRamDiskFile diskFile = JsonConvert.DeserializeObject<ZXRamDiskFile>(File.ReadAllText(diskPath));

                    if (diskFile == null)
                    {
                        outputLog.WriteLine($"Error reading RAM disk {diskPath}, aborted.");
                        return false;
                    }

                    StringBuilder sb = new StringBuilder();

                    List<byte> data = new List<byte>();

                    sb.AppendLine($"#define {diskFile.DiskName} {(int)diskFile.Bank}");

                    foreach(var file in diskFile.Files) 
                    {
                        sb.AppendLine($"#define {file.Name} {data.Count}");
                        byte[] fileData;

                        if (File.Exists(file.SourcePath))
                            fileData = File.ReadAllBytes(file.SourcePath);
                        else
                        {
                            outputLog.WriteLine($"File {file.SourcePath} not found, using original content...");
                            fileData = file.Content;
                        }

                        data.AddRange(fileData);
                        sb.AppendLine($"#define {file.Name}Size {fileData.Length}");

                        sb.AppendLine($"#define Load{file.Name}From{diskFile.DiskName}(Dest) LoadRamData({diskFile.DiskName}, {file.Name} + $C000, Dest, {file.Name}Size)");

                        sb.AppendLine($"#define LoadPartial{file.Name}From{diskFile.DiskName}(Dest, Size) LoadRamData({diskFile.DiskName}, {file.Name} + $C000, Dest, Size)");
                    }

                    if(BuildType == ZXBuildType.Release)
                        sb.AppendLine($"\r\nLoadRamDisk({diskFile.DiskName})");

                    outputLog.WriteLine("Writting binary disk...");
                    File.WriteAllBytes(diskPath.Substring(0, diskPath.Length - 4) + ".zxrbin", data.ToArray());
                    outputLog.WriteLine("Writting include file...");
                    File.WriteAllText(diskPath.Substring(0, diskPath.Length - 4) + ".zxbas", sb.ToString());
                    outputLog.WriteLine($"RAM disk {fileName} successfully built.");
                } 
                catch (Exception ex)
                {
                    outputLog.WriteLine($"Error generating RAM disk {diskPath}: \r\n" + ex.ToString());
                    return false;
                }
            }

            return true;
        }
    }
}
