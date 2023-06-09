﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DocumentModel.Classes;

namespace ZXBasicStudio.BuildSystem
{
    public class ZXProjectBuilder
    {
        public static ZXProgram? Build(string Folder, TextWriter OutputLogWritter)
        {
            try
            {
                Cleanup(Folder);
                string settingsFile = Path.Combine(Folder, ZXConstants.BUILDSETTINGS_FILE);
                ZXBuildSettings? settings = null;
                string? mainFile = null;

                if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
                {
                    OutputLogWritter.WriteLine("Paths for ZXBASM and ZXBC not configured, aborting...");
                    return null;
                }

                if (!File.Exists(ZXOptions.Current.ZxbcPath) || !File.Exists(ZXOptions.Current.ZxbasmPath))
                {
                    OutputLogWritter.WriteLine("ZXBASM/ZXBC not found, aborting...");
                    return null;
                }

                if (File.Exists(settingsFile))
                {
                    settings = JsonConvert.DeserializeObject<ZXBuildSettings>(File.ReadAllText(settingsFile));

                    if (settings == null)
                    {
                        OutputLogWritter.WriteLine($"Error deserializing settings file \"{Path.GetFileName(settingsFile)}\", aborting...");
                        return null;
                    }

                }
                else
                {
                    OutputLogWritter.WriteLine("No settings file found, using default settings.");

                    if (ZXOptions.Current.DefaultBuildSettings != null)
                        settings = ZXOptions.Current.DefaultBuildSettings.Clone();
                    else
                        settings = new ZXBuildSettings();
                }

                if (string.IsNullOrWhiteSpace(settings.MainFile))
                {
                    OutputLogWritter.WriteLine("Main file not configured, scanning for default files...");

                    List<string> mainFiles = new List<string>();

                    foreach (var ext in ZXExtensions.ZXBasicFiles)
                        mainFiles.AddRange(Directory.GetFiles(Folder, $"main{ext}"));

                    if (mainFiles.Count > 1)
                    {
                        OutputLogWritter.WriteLine("Found multiple main files, aborting...");
                        return null;
                    }
                    else if (mainFiles.Count < 1)
                    {
                        OutputLogWritter.WriteLine("No main file found, aborting...");
                        return null;
                    }

                    mainFile = mainFiles[0];

                    OutputLogWritter.WriteLine($"Using main file \"{Path.GetFileName(mainFile)}\".");
                }
                else
                {
                    mainFile = Path.Combine(Folder, settings.MainFile);
                    if (!File.Exists(mainFile))
                    {
                        OutputLogWritter.WriteLine($"Main file {settings.MainFile} not found, aborting...");
                        return null;
                    }
                    OutputLogWritter.WriteLine($"Using main file \"{settings.MainFile}\".");
                }

                if (!PreBuild(false, Folder, OutputLogWritter))
                    return null;

                var args = settings.GetSettings();

                OutputLogWritter.WriteLine("Building program...");

                var proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{mainFile}\" " + args) { WorkingDirectory = Folder, RedirectStandardError = true, CreateNoWindow = true });

                string logOutput;

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                var ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(Folder);
                    OutputLogWritter.WriteLine("Error building program, aborting...");
                    return null;
                }

                string binFile = Path.Combine(Folder, Path.GetFileNameWithoutExtension(mainFile) + ".bin");

                byte[] binary = File.ReadAllBytes(binFile);

                Cleanup(Folder, binFile);

                ZXProgram program = ZXProgram.CreateReleaseProgram(binary, settings.Origin ?? 32768);

                if (binary.Length + program.Org > 0xFFFF)
                {
                    OutputLogWritter.WriteLine("Program too long, change base address or reduce code size.");
                    return null;
                }

                if (!PostBuild(false, Folder, program, OutputLogWritter))
                    return null;

                OutputLogWritter.WriteLine($"Program size: {binary.Length} bytes");

                OutputLogWritter.WriteLine("Program built successfully.");

                return program;
            }
            catch (Exception ex)
            {
                OutputLogWritter.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }

        private static void OutputProcessLog(TextWriter OutputLogWritter, Process proc, out string Log)
        {
            StringBuilder sbLog = new StringBuilder();
            while (!proc.HasExited)
            {
                if (!proc.StandardError.EndOfStream)
                {
                    try
                    {
                        string? line = proc.StandardError.ReadLine();
                        OutputLogWritter.WriteLine(line);
                        if (line != null)
                            sbLog.AppendLine(line);
                    }
                    catch { }


                }
            }

            if (!proc.StandardError.EndOfStream)
            {
                string? line = proc.StandardError.ReadToEnd();
                OutputLogWritter.WriteLine(line);
                if (line != null)
                    sbLog.AppendLine(line);
            }

            Log = sbLog.ToString();
        }

        public static ZXProgram? BuildDebug(string Folder, TextWriter OutputLogWritter)
        {
            try
            {
                Cleanup(Folder);

                string settingsFile = Path.Combine(Folder, ZXConstants.BUILDSETTINGS_FILE);
                ZXBuildSettings? settings = null;
                string? mainFile = null;

                if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
                {
                    OutputLogWritter.WriteLine("Paths for ZXBASM and ZXBC not configured, aborting...");
                    return null;
                }

                if (!File.Exists(ZXOptions.Current.ZxbcPath) || !File.Exists(ZXOptions.Current.ZxbasmPath))
                {
                    OutputLogWritter.WriteLine("ZXBASM/ZXBC not found, aborting...");
                    return null;
                }

                if (File.Exists(settingsFile))
                {
                    settings = JsonConvert.DeserializeObject<ZXBuildSettings>(File.ReadAllText(settingsFile));

                    if (settings == null)
                    {
                        OutputLogWritter.WriteLine($"Error deserializing settings file \"{Path.GetFileName(settingsFile)}\", aborting...");
                        return null;
                    }
                }
                else
                {
                    OutputLogWritter.WriteLine("No settings file found, using default settings.");

                    if (ZXOptions.Current.DefaultBuildSettings != null)
                        settings = ZXOptions.Current.DefaultBuildSettings.Clone();
                    else
                        settings = new ZXBuildSettings();
                }

                if (string.IsNullOrWhiteSpace(settings.MainFile))
                {
                    OutputLogWritter.WriteLine("Main file not configured, scanning for default files...");

                    List<string> mainFiles = new List<string>();

                    foreach (var ext in ZXExtensions.ZXBasicFiles)
                        mainFiles.AddRange(Directory.GetFiles(Folder, $"main{ext}"));

                    if (mainFiles.Count > 1)
                    {
                        OutputLogWritter.WriteLine("Found multiple main files, aborting...");
                        return null;
                    }
                    else if (mainFiles.Count < 1)
                    {
                        OutputLogWritter.WriteLine("No main file found, aborting...");
                        return null;
                    }

                    mainFile = mainFiles[0];

                    OutputLogWritter.WriteLine($"Using main file \"{Path.GetFileName(mainFile)}\".");
                }
                else
                {
                    mainFile = Path.Combine(Folder, settings.MainFile);
                    if (!File.Exists(mainFile))
                    {
                        OutputLogWritter.WriteLine($"Main file {settings.MainFile} not found, aborting...");
                        return null;
                    }
                    OutputLogWritter.WriteLine($"Using main file \"{settings.MainFile}\".");
                }

                if (!PreBuild(true, Folder, OutputLogWritter))
                    return null;

                var files = ScanFolder(Folder);

                if (files.Count() == 0)
                {
                    OutputLogWritter.WriteLine("No file found to build, aborting...");
                    return null;
                }

                string logOutput;

                var args = settings.GetDebugSettings();

                OutputLogWritter.WriteLine("Building map files...");

                foreach (var file in files)
                    file.CreateBuildFile(files);

                OutputLogWritter.WriteLine("Building program map...");

                var codeFile = files.First(f => f.AbsolutePath == Path.GetFullPath(mainFile));

                var proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{Path.Combine(codeFile.Directory, codeFile.TempFileName)}\" -M MEMORY_MAP " + args) { WorkingDirectory = Folder, RedirectStandardError = true, CreateNoWindow = true });

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                var ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(Folder);
                    OutputLogWritter.WriteLine("Error building map, aborting...");
                    return null;
                }

                var progMap = new ZXMemoryMap(Path.Combine(Folder, "MEMORY_MAP"), files);
                string binFile = Path.Combine(Folder, Path.GetFileNameWithoutExtension(codeFile.TempFileName) + ".bin");

                byte[] binary = File.ReadAllBytes(binFile);

                OutputLogWritter.WriteLine("Building variable map...");

                proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{Path.Combine(codeFile.Directory, codeFile.TempFileName)}\" -E " + args) { WorkingDirectory = Folder, RedirectStandardError = true, CreateNoWindow = true });

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(Folder, binFile);
                    OutputLogWritter.WriteLine("Error building variable map, aborting...");
                    return null;
                }

                var mainCodeFile = files.Where(f => Path.GetFullPath(mainFile.ToLower()) == Path.GetFullPath(f.AbsolutePath.ToLower())).First();

                ZXBasicMap bMap = new ZXBasicMap(mainCodeFile, files, logOutput);

                string varFile = Path.Combine(Folder, Path.GetFileNameWithoutExtension(codeFile.TempFileName) + ".ic");
                string mapFile = Path.Combine(Folder, "MEMORY_MAP");
                var varMap = new ZXVariableMap(varFile, mapFile, bMap);

                OutputLogWritter.WriteLine("Building disassembly...");

                proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{Path.Combine(codeFile.Directory, codeFile.TempFileName)}\" -A " + args) { WorkingDirectory = Folder, RedirectStandardError = true, CreateNoWindow = true });

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(Folder, binFile);
                    OutputLogWritter.WriteLine("Error building disassembly, aborting...");
                    return null;
                }

                OutputLogWritter.WriteLine("Building disassembly map...");

                string disFile = Path.Combine(Folder, Path.GetFileNameWithoutExtension(Path.Combine(codeFile.Directory, codeFile.TempFileName)) + ".asm");
                var disasFile = new ZXCodeFile(disFile, true);

                disasFile.CreateBuildFile(files);

                proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbasmPath), $"\"{Path.Combine(disasFile.Directory, disasFile.TempFileName)}\" -M MEMORY_MAP") { WorkingDirectory = Folder, RedirectStandardError = true, CreateNoWindow = true });

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(Folder, binFile, disFile);
                    OutputLogWritter.WriteLine("Error building disassembly map, aborting...");
                    return null;
                }

                var asmMap = new ZXMemoryMap(Path.Combine(Folder, "MEMORY_MAP"), new ZXCodeFile[] { disasFile });

                Cleanup(Folder, binFile, disFile);

                ushort org = disasFile.FindOrg();

                ZXProgram program = ZXProgram.CreateDebugProgram(files, disasFile, progMap, asmMap, varMap, binary, org);

                if (binary.Length + program.Org > 0xFFFF)
                {
                    OutputLogWritter.WriteLine("Program too long, change base address or reduce code size.");
                    return null;
                }

                if (!PostBuild(true, Folder, program, OutputLogWritter))
                    return null;

                OutputLogWritter.WriteLine($"Program size: {binary.Length} bytes");

                OutputLogWritter.WriteLine("Program built successfully.");

                return program;
            }
            catch (Exception ex)
            {
                OutputLogWritter.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }
        public static bool Export(string Folder, ZXExportOptions Export, TextWriter OutputLogWritter)
        {
            try
            {
                Cleanup(Folder);
                string settingsFile = Path.Combine(Folder, ZXConstants.BUILDSETTINGS_FILE);
                ZXBuildSettings? settings = null;
                string? mainFile = null;

                if (string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbcPath) || string.IsNullOrWhiteSpace(ZXOptions.Current.ZxbasmPath))
                {
                    OutputLogWritter.WriteLine("Paths for ZXBASM and ZXBC not configured, aborting...");
                    return false;
                }

                if (!File.Exists(ZXOptions.Current.ZxbcPath) || !File.Exists(ZXOptions.Current.ZxbasmPath))
                {
                    OutputLogWritter.WriteLine("ZXBASM/ZXBC not found, aborting...");
                    return false;
                }

                if (File.Exists(settingsFile))
                {
                    settings = JsonConvert.DeserializeObject<ZXBuildSettings>(File.ReadAllText(settingsFile));

                    if (settings == null)
                    {
                        OutputLogWritter.WriteLine($"Error deserializing settings file \"{Path.GetFileName(settingsFile)}\", aborting...");
                        return false;
                    }

                }
                else
                {
                    OutputLogWritter.WriteLine("No settings file found, using default settings.");

                    if (ZXOptions.Current.DefaultBuildSettings != null)
                        settings = ZXOptions.Current.DefaultBuildSettings.Clone();
                    else
                        settings = new ZXBuildSettings();
                }

                if (string.IsNullOrWhiteSpace(settings.MainFile))
                {
                    OutputLogWritter.WriteLine("Main file not configured, scanning for default files...");

                    List<string> mainFiles = new List<string>();

                    foreach (var ext in ZXExtensions.ZXBasicFiles)
                        mainFiles.AddRange(Directory.GetFiles(Folder, $"main{ext}"));

                    if (mainFiles.Count > 1)
                    {
                        OutputLogWritter.WriteLine("Found multiple main files, aborting...");
                        return false;
                    }
                    else if (mainFiles.Count < 1)
                    {
                        OutputLogWritter.WriteLine("No main file found, aborting...");
                        return false;
                    }

                    mainFile = mainFiles[0];

                    OutputLogWritter.WriteLine($"Using main file \"{Path.GetFileName(mainFile)}\".");
                }
                else
                {
                    mainFile = Path.Combine(Folder, settings.MainFile);
                    if (!File.Exists(mainFile))
                    {
                        OutputLogWritter.WriteLine($"Main file {settings.MainFile} not found, aborting...");
                        return false;
                    }
                    OutputLogWritter.WriteLine($"Using main file \"{settings.MainFile}\".");
                }

                var args = settings.GetSettings();

                OutputLogWritter.WriteLine("Exporting program...");

                var proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{mainFile}\" " + args + " " + Export.GetExportOptions()) { WorkingDirectory = Folder, RedirectStandardError = true, CreateNoWindow = true });

                string logOutput;

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                var ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(Folder);
                    OutputLogWritter.WriteLine("Error building program, aborting...");
                    return false;
                }

                OutputLogWritter.WriteLine("Program exported successfully.");

                return true;
            }
            catch (Exception ex)
            {
                OutputLogWritter.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
                return false;
            }
        }
        private static void Cleanup(string Folder, string? BinFile = null, string? DisassemblyFile = null)
        {

            if (BinFile != null && File.Exists(BinFile))
                File.Delete(BinFile);

            if (DisassemblyFile != null && File.Exists(DisassemblyFile))
                File.Delete(BinFile);

            var toDelete = Directory.GetFiles(Folder, "*.buildtemp.*");
            foreach (var file in toDelete)
                File.Delete(file);

            if (File.Exists(Path.Combine(Folder, "MEMORY_MAP")))
                File.Delete(Path.Combine(Folder, "MEMORY_MAP"));

            var dirs = Directory.GetDirectories(Folder);
            foreach (var dir in dirs)
                Cleanup(dir);
        }
        private static IEnumerable<ZXCodeFile> ScanFolder(string folder)
        {
            List<ZXCodeFile> files = new List<ZXCodeFile>();
            var fFiles = Directory.GetFiles(folder);

            foreach (var fFile in fFiles)
            {
                if (fFile.IsZXBasic() || fFile.IsZXAssembler())
                    files.Add(new ZXCodeFile(fFile));
            }

            var fDirs = Directory.GetDirectories(folder);

            foreach (var fDir in fDirs)
                files.AddRange(ScanFolder(fDir));

            return files;
        }

        private static bool PreBuild(bool debug, string path, TextWriter outLog)
        {
            outLog.WriteLine("Building precompilation documents...");
            var builders = ZXDocumentProvider.GetPrecompilationDocumentBuilders();

            foreach (var builder in builders)
            {
                if (!builder.Build(path, debug ? DocumentModel.Enums.ZXBuildType.Debug : DocumentModel.Enums.ZXBuildType.Release, null, outLog))
                {
                    outLog.WriteLine("Error on pre-build stage, aborting...");
                    return false;
                }
            }

            return true;
        }
        private static bool PostBuild(bool debug, string path, ZXProgram CompiledProgram, TextWriter outLog)
        {
            outLog.WriteLine("Building postcompilation documents...");
            var builders = ZXDocumentProvider.GetPostcompilationDocumentBuilders();

            foreach (var builder in builders)
            {
                if (!builder.Build(path, debug ? DocumentModel.Enums.ZXBuildType.Debug : DocumentModel.Enums.ZXBuildType.Release, CompiledProgram, outLog))
                {
                    outLog.WriteLine("Error on post-build stage, aborting...");
                    return false;
                }
            }

            return true;
        }
    }
}