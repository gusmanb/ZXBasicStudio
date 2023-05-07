using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentModel.Interfaces;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic;

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
    }
}
