using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.NextDows;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.NextDows
{
    public class ZXFormsDocumentFactory : IZXDocumentFactory
    {
        public bool CreateDocument(string Path, TextWriter OutputLog)
        {
            var type = ZXDocumentProvider.GetDocumentType(Path);

            if (type is not ZXFormsDocument)
            {
                OutputLog.WriteLine($"Document {Path} is not an ZXForms file, internal document handling error, operation aborted.");
                return false;
            }

            if (File.Exists(Path))
            {
                OutputLog.WriteLine($"File {Path} already exists, cannot create.");
                return false;
            }

            try
            {
                File.Create(Path).Dispose();
                return true;
            }
            catch (Exception ex)
            {
                OutputLog.WriteLine($"Error creating file {Path}: {ex.Message}");
                return false;
            }
        }

        public ZXDocumentEditorBase? CreateEditor(string Path, TextWriter OutputLog)
        {
            var type = ZXDocumentProvider.GetDocumentType(Path);

            if (type is not ZXFormsDocument)
            {
                OutputLog.WriteLine($"Document {Path} is not an ZXForms file, internal document handling error, operation aborted.");
                return null;
            }
            ZXFormsEditor editor = new ZXFormsEditor(Path);
            return editor;
        }
    }
}
