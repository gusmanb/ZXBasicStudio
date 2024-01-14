using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXRamDisk.Classes;
using ZXBasicStudio.DocumentEditors.ZXRamDisk.Controls;
using ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Classes;
using ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Controls;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.Resources.ZXRamDisk
{
    public class ZXRamDiskFactory : IZXDocumentFactory
    {
        public bool CreateDocument(string Path, TextWriter OutputLog)
        {
            try
            {
                ZXRamDiskFile doc = new ZXRamDiskFile { DiskName = "Unnamed", Bank = RamDiskBank.Bank4 };
                string content = JsonConvert.SerializeObject(doc);
                File.WriteAllText(Path, content);
                return true;
            }
            catch (Exception ex)
            {
                OutputLog.WriteLine($"Error creating document: {ex.Message}");
                return false;
            }
        }

        public ZXDocumentEditorBase? CreateEditor(string Path, TextWriter OutputLog)
        {
            try
            {
                return new ZXRamDiskEditor(Path);
            }
            catch (Exception ex)
            {
                OutputLog.WriteLine("Error opening the document. Aborting.");
                return null;
            }
        }
    }
}
