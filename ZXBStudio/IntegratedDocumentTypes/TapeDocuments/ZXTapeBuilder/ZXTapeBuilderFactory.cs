using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Controls;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.TapeDocuments.ZXTapeBuilder
{
    public class ZXTapeBuilderFactory : IZXDocumentFactory
    {
        public bool CreateDocument(string Path, TextWriter OutputLog)
        {
            try
            {
                ZXTapeBuilderDocument doc = new ZXTapeBuilderDocument();
                string content = JsonConvert.SerializeObject(doc);
                File.WriteAllText(Path, content);
                return true;
            }
            catch(Exception ex) 
            {
                OutputLog.WriteLine($"Error creating document: {ex.Message}");
                return false;
            }
        }

        public ZXDocumentEditorBase? CreateEditor(string Path, TextWriter OutputLog)
        {
            try
            {
                return new ZXTapeBuilderEditor(Path);
            }
            catch (Exception ex) 
            {
                OutputLog.WriteLine("Error opening the document. Aborting.");
                return null;
            }
        }
    }
}
