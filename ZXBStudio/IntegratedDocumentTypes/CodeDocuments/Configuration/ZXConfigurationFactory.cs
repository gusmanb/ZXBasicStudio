using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Configuration
{
    public class ZXConfigurationFactory : IZXDocumentFactory
    {
        public bool CreateDocument(string Path, TextWriter OutputLog)
        {
            return false;
        }

        public ZXDocumentEditorBase? CreateEditor(string Path, TextWriter OutputLog)
        {
            var type = ZXDocumentProvider.GetDocumentType(Path);

            if (type is not ZXConfigurationDocument)
            {
                OutputLog.WriteLine($"Document {Path} is not a configuration file, internal document handling error, operation aborted.");
                return null;
            }
            ZXJsonEditor editor = new ZXJsonEditor(Path);
            return editor;
        }
    }
}
