﻿using Avalonia.Platform.Storage;
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
using ZXBasicStudio.IntegratedDocumentTypes.ZXGraphics;
using ZXBasicStudio.IntegratedDocumentTypes.NextDows;
using ZXBasicStudio.IntegratedDocumentTypes.TapeDocuments.ZXTapeBuilder;
using ZXBasicStudio.IntegratedDocumentTypes.Resources.ZXRamDisk;

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
            _docTypes.Add(new ZXTapeBuilderDocument());
            _docTypes.Add(new ZXRamDiskDocument());
            _docTypes.Add(new ZXRamDiskBinaryDocument());
            // ZXGraphics
            _docTypes.Add(new UDGDocument());
            _docTypes.Add(new FontDocument());
            _docTypes.Add(new SpriteDocument());
            // NextDows
            //_docTypes.Add(new ZXFormsDocument());
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
            var builders = _docTypes.Where(d => d.DocumentBuilder != null && (d.DocumentBuildStage?.HasFlag(Enums.ZXBuildStage.PreBuild) ?? false)).Select(d => d.DocumentBuilder);

            var sortedBuilders = SortByDependency(builders, builder => builders.Where(b => b != null && builder != null && builder.DependsOn != null && builder.DependsOn.Contains(b.Id)));

            return sortedBuilders;
        }

        public static IEnumerable<IZXDocumentBuilder> GetPostcompilationDocumentBuilders()
        {
            var builders = _docTypes.Where(d => d.DocumentBuilder != null && (d.DocumentBuildStage?.HasFlag(Enums.ZXBuildStage.PostBuild) ?? false)).Select(d => d.DocumentBuilder);

            var sortedBuilders = SortByDependency(builders, builder => builders.Where(b => b != null && builder != null && builder.DependsOn != null && builder.DependsOn.Contains(b.Id)));

            return sortedBuilders;
        }

        private static IEnumerable<T> SortByDependency<T>(this IEnumerable<T> nodes,
                                                Func<T, IEnumerable<T>> connected)
        {
            var elems = nodes.ToDictionary(node => node,
                                           node => new HashSet<T>(connected(node)));
            while (elems.Count > 0)
            {
                var elem = elems.FirstOrDefault(x => x.Value.Count == 0);
                if (elem.Key == null)
                {
                    throw new ArgumentException("Cyclic connections are not allowed");
                }
                elems.Remove(elem.Key);
                foreach (var selem in elems)
                {
                    selem.Value.Remove(elem.Key);
                }
                yield return elem.Key;
            }
        }

        public static IZXDocumentType GetDocumentTypeInstance(Type DocumentType)
        {
            return _docTypes.First(d => d.GetType() == DocumentType);
        }
    }
}
