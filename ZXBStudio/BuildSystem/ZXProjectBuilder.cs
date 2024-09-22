using Newtonsoft.Json;
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
using ZXBasicStudio.Dialogs;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic;

namespace ZXBasicStudio.BuildSystem
{
    public class ZXProjectBuilder
    {
        public static ZXProgram? Build(TextWriter OutputLogWritter)
        {
            try
            {

                if (ZXProjectManager.Current == null)
                {
                    OutputLogWritter.WriteLine("No open project, aborting...");
                    return null;
                }

                var project = ZXProjectManager.Current;

                Cleanup(project.ProjectPath);
                ZXBuildSettings? settings = null;
                settings = null;
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

                settings = project.GetProjectSettings();
                mainFile = project.GetMainFile();

                if (mainFile == null)
                {
                    OutputLogWritter.WriteLine("Cannot find main file, check that it exists and if there are more than one that is specified in the build settings.");
                    return null;
                }

                if (!PreBuild(false, project.ProjectPath, OutputLogWritter))
                    return null;

                var args = settings.GetSettings();

                var startTime = DateTime.Now;
                OutputLogWritter.WriteLine("Project path: " + project.ProjectPath);
                OutputLogWritter.WriteLine("Building program " + mainFile);
                OutputLogWritter.WriteLine("Building starts at " + startTime);

                var proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{mainFile}\" " + args) { WorkingDirectory = project.ProjectPath, RedirectStandardError = true, CreateNoWindow = true });

                string logOutput;

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                var ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(project.ProjectPath);
                    OutputLogWritter.WriteLine("Error building program, aborting...");
                    return null;
                }

                string binFile = Path.Combine(project.ProjectPath, Path.GetFileNameWithoutExtension(mainFile) + ".bin");

                byte[] binary = File.ReadAllBytes(binFile);

                Cleanup(project.ProjectPath, binFile);

                ZXProgram program = ZXProgram.CreateReleaseProgram(binary, settings.Origin ?? 32768);

                if (binary.Length + program.Org > 0xFFFF)
                {
                    OutputLogWritter.WriteLine("Program too long, change base address or reduce code size.");
                    return null;
                }

                if (!PostBuild(false, project.ProjectPath, program, OutputLogWritter))
                    return null;

                OutputLogWritter.WriteLine($"Program size: {binary.Length} bytes");

                OutputLogWritter.WriteLine("Program built successfully (Elapsed time: " + (startTime - DateTime.Now).Duration().ToString(@"hh\:mm\:ss") + " seconds)");

                if (settings.NextMode)
                {
                    if (!BuildNexFile(binary, settings, project, OutputLogWritter))
                    {
                        return null;
                    }
                }

                return program;
            }
            catch (Exception ex)
            {
                OutputLogWritter.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }


        private static bool BuildNexFile(byte[] binary, ZXBuildSettings settings, ZXProjectManager project, TextWriter outputLogWritter)
        {
            try
            {
                outputLogWritter.WriteLine("Building .nex file...");
                string binFile = "";
                string cfgFile = "";

                // Create .bin file
                {
                    outputLogWritter.WriteLine("Creating .bin file...");
                    binFile = Path.Combine(project.ProjectPath, Path.GetFileNameWithoutExtension(settings.MainFile) + ".bin");
                    if (File.Exists(binFile))
                    {
                        File.Delete(binFile);
                    }
                    File.WriteAllBytes(binFile, binary);
                }

                // Create nex.cfg file
                {
                    outputLogWritter.WriteLine("Creating nex.cfg configuration file...");
                    var sb = new StringBuilder();
                    sb.AppendLine("; Minimum core version");
                    sb.AppendLine("!COR3,0,0");
                    // sysvars.inc
                    {
                        var sysVarsPath = Path.Combine(Environment.CurrentDirectory, "Resources", "sysvars.inc");
                        var sysVarsDest = Path.Combine(project.ProjectPath, "sysvars.inc");
                        if (!File.Exists(sysVarsDest))
                        {
                            File.Copy(sysVarsPath, sysVarsDest);
                        }
                        sb.AppendLine("!MMU./sysvars.inc,10,$1C00");
                    }
                    // Origin
                    int org = settings.Origin == null ? 32768 : settings.Origin.Value;
                    sb.AppendLine(string.Format("!PCSP${0:X2},${1:X2}", org, org - 2));
                    // Main file
                    {
                        int[] nextBank16K = { 255, 5, 2, 0 };
                        int bank = org / 16384;
                        int offset = org - (bank * 16384);
                        if (bank < 0 || bank > 3)
                        {
                            outputLogWritter.WriteLine("Error: Invalid ORG direction, must be >0 and <65535");
                            return false;
                        }
                        bank = nextBank16K[bank];
                        sb.AppendLine(string.Format(".\\{0},{1},${2:X2}",
                            Path.Combine(Path.GetFileNameWithoutExtension(settings.MainFile) + ".bin"),
                            bank,
                            offset));
                    }
                    // Save nex.cfg file
                    {
                        cfgFile = Path.Combine(project.ProjectPath, "nex.cfg");
                        if (File.Exists(cfgFile))
                        {
                            File.Delete(cfgFile);
                        }
                        File.WriteAllText(cfgFile, sb.ToString());
                    }
                }
                // Build nex file
                {
                    string nextDriveFolder = "nextdrive";
                    // Delete old .nex file
                    string nexFile = Path.Combine(project.ProjectPath, Path.GetFileNameWithoutExtension(settings.MainFile) + ".nex");
                    if (File.Exists(nexFile))
                    {
                        File.Delete(nexFile);
                    }
                    // Check if nextdata folder exists
                    string dataFolder = Path.Combine(project.ProjectPath, nextDriveFolder);
                    if (!Directory.Exists(dataFolder))
                    {
                        Directory.CreateDirectory(dataFolder);
                    }
                    // Delete old .next file in data folder
                    string nexDataFile = Path.Combine(project.ProjectPath, nextDriveFolder, Path.GetFileNameWithoutExtension(settings.MainFile) + ".nex");
                    if (File.Exists(nexDataFile))
                    {
                        File.Delete(nexDataFile);
                    }

                    outputLogWritter.WriteLine("Building .nex file...");
                    Process process = new Process();
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        CheckNextCreator();
                        process.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(ZXOptions.Current.ZxbcPath), "python", "python.exe");
                        process.StartInfo.Arguments = string.Format("{0} nex.cfg {1}",
                            Path.Combine(Path.GetDirectoryName(ZXOptions.Current.ZxbcPath), "tools", "nextcreator.py"),
                            Path.GetFileNameWithoutExtension(settings.MainFile) + ".nex");
                        process.StartInfo.WorkingDirectory = project.ProjectPath;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardOutput = true;
                    }
                    else
                    {
                        process.StartInfo.FileName = Path.Combine(Path.GetDirectoryName(ZXOptions.Current.ZxbcPath), "tools", "nextcreator.py");
                        process.StartInfo.Arguments = "nex.cfg " + Path.GetFileNameWithoutExtension(settings.MainFile) + ".nex";
                        process.StartInfo.WorkingDirectory = project.ProjectPath;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardOutput = true;
                    }
                    outputLogWritter.WriteLine(string.Format("{0} {1}",
                        process.StartInfo.FileName,
                        process.StartInfo.Arguments));
                    process.Start();
                    process.WaitForExit();

                    if (!File.Exists(nexFile))
                    {
                        outputLogWritter.WriteLine("Error building .nex file");
                        outputLogWritter.WriteLine(process.StartInfo.WorkingDirectory);

                        using (StreamReader reader = process.StandardOutput)
                        {
                            string output = reader.ReadToEnd();
                            outputLogWritter.WriteLine(output);
                        }
                        return false;
                    }

                    // Copy .nex file to data folder
                    File.Copy(nexFile, nexDataFile);
                }
                return true;
            }
            catch (Exception ex)
            {
                outputLogWritter.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
                return false;
            }

        }


        private static void CheckNextCreator()
        {
            var fNCexe = Path.Combine(Path.GetDirectoryName(ZXOptions.Current.ZxbcPath), "tools", "nextcreator.exe");
            if (File.Exists(fNCexe))
            {
                return;
            }

            File.Copy(ZXOptions.Current.ZxbcPath, fNCexe);
            return;
        }


        public static ZXProgram? BuildDebug(TextWriter OutputLogWritter)
        {
            try
            {
                if (ZXProjectManager.Current == null)
                {
                    OutputLogWritter.WriteLine("No open project, aborting...");
                    return null;
                }

                var project = ZXProjectManager.Current;

                Cleanup(project.ProjectPath);
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

                settings = project.GetProjectSettings();
                mainFile = project.GetMainFile();

                if (mainFile == null)
                {
                    OutputLogWritter.WriteLine("Cannot find main file, check that it exists and if there are more than one that is specified in the build settings.");
                    return null;
                }

                if (!PreBuild(true, project.ProjectPath, OutputLogWritter))
                    return null;

                var files = ScanFolder(project.ProjectPath);

                if (files.Count() == 0)
                {
                    OutputLogWritter.WriteLine("No file to build, aborting...");
                    return null;
                }

                string logOutput;

                var args = settings.GetDebugSettings();

                OutputLogWritter.WriteLine("Building map files...");

                foreach (var file in files)
                {
                    file.CreateBuildFile(files);
                }

                OutputLogWritter.WriteLine("Building program map...");

                // TODO: DUEFECTU 2023.05.17: Bug for long path
                var codeFile = files.FirstOrDefault(f => f.AbsolutePath == Path.GetFullPath(mainFile));
                if (codeFile == null)
                {
                    Cleanup(project.ProjectPath);
                    OutputLogWritter.WriteLine("Main file path not found. More than 256 chars?");
                    return null;
                }

                var proc = Process.Start(
                    new ProcessStartInfo(
                        Path.GetFullPath(ZXOptions.Current.ZxbcPath), 
                        $"\"{Path.Combine(codeFile.Directory, codeFile.TempFileName)}\" -M MEMORY_MAP " + args) { WorkingDirectory = project.ProjectPath, RedirectStandardError = true, CreateNoWindow = true });

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                var ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(project.ProjectPath);
                    OutputLogWritter.WriteLine("Error building map, aborting...");
                    return null;
                }

                var progMap = new ZXMemoryMap(Path.Combine(project.ProjectPath, "MEMORY_MAP"), files);
                string binFile = Path.Combine(project.ProjectPath, Path.GetFileNameWithoutExtension(codeFile.TempFileName) + ".bin");

                byte[] binary = File.ReadAllBytes(binFile);

                OutputLogWritter.WriteLine("Building variable map...");

                // DUEFECTU: 2024.09.11 -> Force .ic extension for debug
                //proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{Path.Combine(codeFile.Directory, codeFile.TempFileName)}\" -E " + args) { WorkingDirectory = project.ProjectPath, RedirectStandardError = true, CreateNoWindow = true });
                var pi = new ProcessStartInfo();
                pi.WorkingDirectory = project.ProjectPath;
                pi.RedirectStandardError = true;
                pi.CreateNoWindow = true;
                // Compile command
                var tempFileName = Path.Combine(codeFile.Directory, codeFile.TempFileName);
                var debugFile = Path.GetFileNameWithoutExtension(tempFileName) + ".ic"; // force .ic extension
                pi.FileName = Path.GetFullPath(ZXOptions.Current.ZxbcPath);      // ZXBC.exe
                pi.Arguments = string.Format("\"{0}\" -E -o {1} {2}",
                    tempFileName,                                               // Main project file
                    debugFile,                                                  // Debug file
                    args);                                                      // user arguments
                // Go for it
                proc = Process.Start(pi);

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(project.ProjectPath, binFile);
                    OutputLogWritter.WriteLine("Error building variable map, aborting...");
                    return null;
                }

                /// DUEFECTU: 2023.06.04 -> Bug
                //var mainCodeFile = files.Where(f => Path.GetFullPath(mainFile.ToLower()) == Path.GetFullPath(f.AbsolutePath.ToLower())).First();
                var mainCodeFile = files.Where(f => Path.GetFullPath(mainFile.ToLower()) == Path.GetFullPath(f.AbsolutePath.ToLower())).FirstOrDefault();
                if (mainCodeFile == null)
                {
                    return null;
                }

                ZXBasicMap bMap = new ZXBasicMap(mainCodeFile, files, logOutput);

                string varFile = Path.Combine(project.ProjectPath, Path.GetFileNameWithoutExtension(codeFile.TempFileName) + ".ic");
                string mapFile = Path.Combine(project.ProjectPath, "MEMORY_MAP");
                var varMap = new ZXVariableMap(varFile, mapFile, bMap);

                OutputLogWritter.WriteLine("Building disassembly...");

                proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{Path.Combine(codeFile.Directory, codeFile.TempFileName)}\" -A " + args) { WorkingDirectory = project.ProjectPath, RedirectStandardError = true, CreateNoWindow = true });

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(project.ProjectPath, binFile);
                    OutputLogWritter.WriteLine("Error building disassembly, aborting...");
                    return null;
                }

                OutputLogWritter.WriteLine("Building disassembly map...");

                string disFile = Path.Combine(project.ProjectPath, Path.GetFileNameWithoutExtension(Path.Combine(codeFile.Directory, codeFile.TempFileName)) + ".asm");
                var disasFile = new ZXCodeFile(disFile, true);

                disasFile.CreateBuildFile(files);

                proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbasmPath), $"\"{Path.Combine(disasFile.Directory, disasFile.TempFileName)}\" -M MEMORY_MAP") { WorkingDirectory = project.ProjectPath, RedirectStandardError = true, CreateNoWindow = true });

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(project.ProjectPath, binFile, disFile);
                    OutputLogWritter.WriteLine("Error building disassembly map, aborting...");
                    return null;
                }

                var asmMap = new ZXMemoryMap(Path.Combine(project.ProjectPath, "MEMORY_MAP"), new ZXCodeFile[] { disasFile });

                Cleanup(project.ProjectPath, binFile, disFile);

                ushort org = disasFile.FindOrg();

                ZXProgram program = ZXProgram.CreateDebugProgram(files, disasFile, progMap, asmMap, varMap, binary, org);

                if (binary.Length + program.Org > 0xFFFF)
                {
                    OutputLogWritter.WriteLine("Program too long, change base address or reduce code size.");
                    return null;
                }

                if (!PostBuild(true, project.ProjectPath, program, OutputLogWritter))
                    return null;

                OutputLogWritter.WriteLine($"Program size: {binary.Length} bytes");

                OutputLogWritter.WriteLine("Program built successfully.");

                if (settings.NextMode)
                {
                    OutputLogWritter.WriteLine("Debugging in Next not supported.");
                    return null;
                }

                return program;
            }
            catch (LineOutOfRangeException ex)
            {
                OutputLogWritter.WriteLine($"Found invalid program: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                OutputLogWritter.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }


        public static bool Export(ZXExportOptions Export, TextWriter OutputLogWritter)
        {
            try
            {
                if (ZXProjectManager.Current == null)
                {
                    OutputLogWritter.WriteLine("No open project, aborting...");
                    return false;
                }

                var project = ZXProjectManager.Current;

                Cleanup(project.ProjectPath);
                ZXBuildSettings? settings;
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

                settings = project.GetProjectSettings();
                mainFile = project.GetMainFile();

                if (mainFile == null)
                {
                    OutputLogWritter.WriteLine("Cannot find main file, check that it exists and if there are more than one that is specified in the build settings.");
                    return false;
                }

                var args = settings.GetSettings();

                OutputLogWritter.WriteLine("Exporting program...");

                var proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{mainFile}\" " + args + " " + Export.GetExportOptions()) { WorkingDirectory = project.ProjectPath, RedirectStandardError = true, CreateNoWindow = true });

                string logOutput;

                OutputProcessLog(OutputLogWritter, proc, out logOutput);

                var ecode = proc.ExitCode;

                if (ecode != 0)
                {
                    Cleanup(project.ProjectPath);
                    OutputLogWritter.WriteLine("Error building program, aborting...");
                    return false;
                }

                OutputLogWritter.WriteLine("Program exported successfully.");

                return true;
            }
            catch (LineOutOfRangeException ex)
            {
                OutputLogWritter.WriteLine($"Found invalid program: {ex.Message}");
                return false;
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
                var docType = ZXDocumentProvider.GetDocumentType(fFile);

                if (docType is ZXBasicDocument || docType is ZXAssemblerDocument)
                    files.Add(new ZXCodeFile(fFile));
            }

            var fDirs = Directory.GetDirectories(folder);

            foreach (var fDir in fDirs)
                files.AddRange(ScanFolder(fDir));

            return files;
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


        private static bool PreBuild(bool debug, string path, TextWriter outLog)
        {
            outLog.WriteLine("Building precompilation documents...");
            var builders = ZXDocumentProvider.GetPrecompilationDocumentBuilders();

            foreach (var builder in builders)
            {
                if (!builder.Build(path, DocumentModel.Enums.ZXBuildStage.PreBuild, debug ? DocumentModel.Enums.ZXBuildType.Debug : DocumentModel.Enums.ZXBuildType.Release, null, outLog))
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
                if (!builder.Build(path, DocumentModel.Enums.ZXBuildStage.PostBuild, debug ? DocumentModel.Enums.ZXBuildType.Debug : DocumentModel.Enums.ZXBuildType.Release, CompiledProgram, outLog))
                {
                    outLog.WriteLine("Error on post-build stage, aborting...");
                    return false;
                }
            }

            return true;
        }
    }
}