using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentModel.Interfaces;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Configuration;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Text;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.ZXGraphics;

namespace ZXBasicStudio.DocumentModel.Classes
{
    /// <summary>
    /// Static provider for document types
    /// </summary>
    public static class ZXDocumentProvider
    {
        static List<IZXDocumentType> _docTypes = new List<IZXDocumentType>();
        static ZXDocumentProvider()
        {
            //Add internal document types
            _docTypes.Add(new ZXBasicDocument());
            _docTypes.Add(new ZXAssemblerDocument());
            _docTypes.Add(new ZXTextDocument());
            _docTypes.Add(new ZXConfigurationDocument());
            // ZXGraphics
            _docTypes.Add(new UDGDocument());
            _docTypes.Add(new FontDocument());
            //TODO: Load external document types
        }

        /// <summary>
        /// List of all supported documents
        /// </summary>
        public static IEnumerable<IZXDocumentType> DocumentTypes => _docTypes;

        /// <summary>
        /// Gets the document type for a given document file name
        /// </summary>
        /// <param name="Document">Document file name</param>
        /// <returns>The document type instance if it is a supported document, null in other case</returns>
        public static IZXDocumentType? GetDocumentType(string Document)
        { 
            string docExtension = Path.GetExtension(Document).ToLower();
            return _docTypes.FirstOrDefault(d => d.DocumentExtensions.Any(de => de.ToLower() == docExtension));
        }

        /// <summary>
        /// Gets the document factory for a given document file name
        /// </summary>
        /// <param name="Document">Document file name</param>
        /// <returns>The document factory instance if it is a supported document, null in other case</returns>
        public static IZXDocumentFactory? GetDocumentFactory(string Document)
        {
            return GetDocumentType(Document)?.DocumentFactory;
        }

        /// <summary>
        /// Gets the document builder for a given document file name
        /// </summary>
        /// <param name="Document">Document file name</param>
        /// <returns>The document factory instance if it is a supported document, null in other case</returns>
        public static IZXDocumentBuilder? GetDocumentBuilder(string Document) 
        {
            return GetDocumentType(Document)?.DocumentBuilder;
        }

        /// <summary>
        /// Get docuemnt extension filters
        /// </summary>
        /// <returns></returns>
        public static FilePickerFileType[] GetDocumentFilters()
        {
            List<FilePickerFileType> filters = new List<FilePickerFileType>();

            foreach (var doc in _docTypes)
                filters.Add(new FilePickerFileType(doc.DocumentDescription) { Patterns = doc.DocumentExtensions });

            return filters.ToArray();
        }

        public static IEnumerable<IZXDocumentType> GetDocumentsInCategory(string Category)
        {
            return _docTypes.Where(d => d.DocumentCategory== Category).OrderBy(d => d.DocumentName);
        }

        public static IEnumerable<string> GetDocumentCategories()
        {
            return _docTypes.Select(d => d.DocumentCategory).Distinct().Order();
        }

        public static IEnumerable<IZXDocumentBuilder> GetPrecompilationDocumentBuilders()
        {
            return _docTypes.Where(d => d.DocumentBuilder != null && d.DocumentBuildStage == Enums.ZXBuildStage.PreBuild).Select(d => d.DocumentBuilder);
        }

        public static IEnumerable<IZXDocumentBuilder> GetPostcompilationDocumentBuilders()
        {
            return _docTypes.Where(d => d.DocumentBuilder != null && d.DocumentBuildStage == Enums.ZXBuildStage.PostBuild).Select(d => d.DocumentBuilder);
        }
    }
}
