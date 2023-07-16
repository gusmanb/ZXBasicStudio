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
                    OutputLogWritter.WriteLine("No open projcet, aborting...");
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

                OutputLogWritter.WriteLine("Building program...");

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

                OutputLogWritter.WriteLine("Program built successfully.");

                return program;
            }
            catch (Exception ex)
            {
                OutputLogWritter.WriteLine($"Exception: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }
        public static ZXProgram? BuildDebug(TextWriter OutputLogWritter)
        {
            try
            {
                if (ZXProjectManager.Current == null)
                {
                    OutputLogWritter.WriteLine("No open projcet, aborting...");
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
                    file.CreateBuildFile(files);

                OutputLogWritter.WriteLine("Building program map...");

                // TODO: DUEFECTU 2023.05.17: Bug for long path
                var codeFile = files.FirstOrDefault(f => f.AbsolutePath == Path.GetFullPath(mainFile));
                if (codeFile == null)
                {
                    Cleanup(project.ProjectPath);
                    OutputLogWritter.WriteLine("Main file path not found. More than 256 chars?");
                    return null;
                }

                var proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{Path.Combine(codeFile.Directory, codeFile.TempFileName)}\" -M MEMORY_MAP " + args) { WorkingDirectory = project.ProjectPath, RedirectStandardError = true, CreateNoWindow = true });

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

                proc = Process.Start(new ProcessStartInfo(Path.GetFullPath(ZXOptions.Current.ZxbcPath), $"\"{Path.Combine(codeFile.Directory, codeFile.TempFileName)}\" -E " + args) { WorkingDirectory = project.ProjectPath, RedirectStandardError = true, CreateNoWindow = true });

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

                return program;
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
                    OutputLogWritter.WriteLine("No open projcet, aborting...");
                    return false;
                }

                var project = ZXProjectManager.Current;

                Cleanup(project.ProjectPath);
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