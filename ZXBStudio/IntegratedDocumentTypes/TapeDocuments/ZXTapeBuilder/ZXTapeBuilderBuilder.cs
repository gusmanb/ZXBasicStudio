using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.BuildSystem;
using ZXBasicStudio.Common.TAPTools;
using ZXBasicStudio.Common.ZXSinclairBasic;
using ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Classes;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;
using I = ZXBasicStudio.Common.ZXSinclairBasic.ZXSinclairBasicInstruction;

namespace ZXBasicStudio.IntegratedDocumentTypes.TapeDocuments.ZXTapeBuilder
{
    public class ZXTapeBuilderBuilder : IZXDocumentBuilder
    {
        public bool Build(string BuildPath, ZXBuildStage Stage, ZXBuildType BuildType, ZXProgram? CompiledProgram, TextWriter OutputLog)
        {
            if (Stage != ZXBuildStage.PostBuild || BuildType != ZXBuildType.Release || CompiledProgram == null)
                return true;

            string[] tapeBuilds = Directory.GetFiles(BuildPath, "*" + ZXDocumentProvider.GetDocumentTypeInstance(typeof(ZXTapeBuilderDocument)).DocumentExtensions[0], SearchOption.AllDirectories);

            if (tapeBuilds == null || tapeBuilds.Length == 0)
                return true;

            OutputLog.WriteLine("Starting tape building...");

            foreach (var tapeBuild in tapeBuilds)
            {
                ZXTapeBuilderFile? buildFile;

                try
                {
                    string fileContent = File.ReadAllText(tapeBuild);
                    buildFile = JsonConvert.DeserializeObject<ZXTapeBuilderFile>(fileContent);
                }
                catch (Exception ex)
                {
                    OutputLog.WriteLine($"Error loading file {tapeBuild}: {ex.Message}");
                    return false;
                }

                if (buildFile == null)
                {
                    OutputLog.WriteLine($"Malformed tape builder file {tapeBuild}");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(buildFile.ProgramName))
                {
                    OutputLog.WriteLine($"Malformed tape builder file {tapeBuild}, missing required program name.");
                    return false;
                }

                string folder = Path.GetDirectoryName(tapeBuild) ?? "";
                string tapeFile = Path.GetFileNameWithoutExtension(tapeBuild) + ".tap";

                string fullTapeFilePath = Path.Combine(folder, tapeFile);

                OutputLog.WriteLine($"Building tape {tapeFile} at {folder}...");

                TAPFile tFile = new TAPFile();

                OutputLog.WriteLine($"Building basic loader...");

                ZXSinclairBasicLine line = new ZXSinclairBasicLine(10);

                if (buildFile.UseBorder)
                    line.AddTokens(I.BORDER, buildFile.Border);

                if (buildFile.UsePaper)
                    line.AddTokens(I.PAPER, buildFile.Paper);

                if (buildFile.UseInk)
                    line.AddTokens(I.INK, buildFile.Ink);

                line.AddTokens(I.CLEAR, CompiledProgram.Org);

                if (buildFile.HideHeaders)
                    line.AddTokens(I.POKE, 23739, ",", 111);

                if (buildFile.PokesBeforeLoad != null && buildFile.PokesBeforeLoad.Length > 0)
                {
                    foreach (var poke in buildFile.PokesBeforeLoad)
                        line.AddTokens(I.POKE, poke.Address, ",", poke.Value);
                }

                if (!string.IsNullOrWhiteSpace(buildFile.ScreenFile) && !string.IsNullOrWhiteSpace(buildFile.ScreenName))
                    line.AddTokens(I.LOAD, "\"\"", I.SCREENS, ":");

                line.AddTokens(I.LOAD, "\"\"", I.CODE, CompiledProgram.Org);

                if (buildFile.DataBlocks != null && buildFile.DataBlocks.Length > 0 && buildFile.DataBlocks.Any(b => b.BasicLoad))
                {
                    foreach(var block in buildFile.DataBlocks.Where(b => b.BasicLoad))
                        line.AddTokens(I.LOAD, "\"\"", I.CODE, block.BlockAddress);
                }

                if (buildFile.HideHeaders)
                    line.AddTokens(I.POKE, 23739, ",", 244);

                if (buildFile.PokesAfterLoad != null && buildFile.PokesAfterLoad.Length > 0)
                {
                    foreach (var poke in buildFile.PokesAfterLoad)
                        line.AddTokens(I.POKE, poke.Address, ",", poke.Value);
                }

                line.AddTokens(I.RANDOMIZE, I.USR, CompiledProgram.Org);

                ZXSinclairBasicProgram loader = new ZXSinclairBasicProgram();
                loader.Lines.Add(line);

                tFile.Blocks.Add(TAPBlock.CreateBasicBlock(buildFile.ProgramName, loader.Serialize(), 10));

                if (!string.IsNullOrWhiteSpace(buildFile.ScreenFile) && !string.IsNullOrWhiteSpace(buildFile.ScreenName))
                {
                    if (!File.Exists(buildFile.ScreenFile))
                    {
                        OutputLog.WriteLine($"Missing screen file {buildFile.ScreenFile}, aborting.");
                        return false;
                    }

                    OutputLog.WriteLine($"Adding screen file {buildFile.ScreenFile}...");

                    try
                    {

                        if (Path.GetExtension(buildFile.ScreenFile).ToLower() == ".tap")
                            tFile.Blocks.Add(new TAPRawBlock(File.ReadAllBytes(buildFile.ScreenFile)));
                        else
                            tFile.Blocks.Add(TAPBlock.CreateScreensBlock(buildFile.ScreenName, File.ReadAllBytes(buildFile.ScreenFile)));
                    }
                    catch(Exception ex) 
                    {
                        OutputLog.WriteLine($"Error adding screen file: {ex.Message}");
                        return false;
                    }
                }

                OutputLog.WriteLine($"Adding main program...");

                tFile.Blocks.Add(TAPBlock.CreateDataBlock(buildFile.ProgramName, CompiledProgram.Binary, CompiledProgram.Org));

                if (buildFile.DataBlocks != null && buildFile.DataBlocks.Length > 0)
                {
                    OutputLog.WriteLine("Adding data blocks...");

                    foreach(var block in buildFile.DataBlocks) 
                    {
                        if (!File.Exists(block.BlockFile))
                        {
                            OutputLog.WriteLine($"Missing data block file {block.BlockFile}, aborting.");
                            return false;
                        }

                        OutputLog.WriteLine($"Adding data file {block.BlockFile}...");

                        try
                        {

                            if (Path.GetExtension(block.BlockFile).ToLower() == ".tap")
                                tFile.Blocks.Add(new TAPRawBlock(File.ReadAllBytes(block.BlockFile)));
                            else
                                tFile.Blocks.Add(TAPBlock.CreateDataBlock(block.BlockName, File.ReadAllBytes(block.BlockFile), block.BlockAddress));
                        }
                        catch (Exception ex)
                        {
                            OutputLog.WriteLine($"Error adding data file: {ex.Message}");
                            return false;
                        }
                    }
                }

                OutputLog.WriteLine("Saving file to disk...");

                try 
                {
                    byte[] fileData = tFile.Serialize();
                    File.WriteAllBytes(fullTapeFilePath, fileData);
                    OutputLog.WriteLine("Tape file created successfully.");

                } catch(Exception ex) 
                {
                    OutputLog.WriteLine($"Error creating tape file: {ex.Message}");
                    return false;
                }
            }

            OutputLog.WriteLine("Tape files built successfully.");
            return true;
        }
    }
}
