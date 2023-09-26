using Avalonia.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXGraphics;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls;
using ZXBasicStudio.DocumentModel.Classes;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.ZXGraphics
{
    public class UDGDocumentFactory : IZXDocumentFactory
    {
        public bool CreateDocument(string Path, TextWriter OutputLog)
        {
            var type = ZXDocumentProvider.GetDocumentType(Path);

            if (type is not UDGDocument)
            {
                OutputLog.WriteLine($"Document {Path} is not a UDG file, internal document handling error, operation aborted.");
                return false;
            }

            if (File.Exists(Path))
            {
                OutputLog.WriteLine($"File {Path} already exists, cannot create.");
                return false;
            }

            try
            {
                ServiceLayer.Initialize();

                File.Create(Path).Dispose();

                var config = ServiceLayer.Export_FontGDU_GetDefaultConfig(Path);
                if (!ServiceLayer.Files_SaveFileString(Path + ".zbs", JsonConvert.SerializeObject(config)))
                {
                    OutputLog.WriteLine(ServiceLayer.LastError);
                    return false;
                }

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

            if (type is not UDGDocument)
            {
                OutputLog.WriteLine($"Document {Path} is not a UDG file, internal document handling error, operation aborted.");
                return null;
            }
            FontGDUEditor editor = new FontGDUEditor(Path,UDGDocument.Id);
            return editor;
        }
    }
}
