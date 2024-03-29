﻿using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DocumentModel.Enums;

namespace ZXBasicStudio.DocumentModel.Interfaces
{
    /// <summary>
    /// Interface for document description
    /// </summary>
    public interface IZXDocumentType
    {
        /// <summary>
        /// Unique identifier of the document type
        /// </summary>
        Guid DocumentTypeId { get; }
        /// <summary>
        /// List of possible extensions for the document type.
        /// </summary>
        string[] DocumentExtensions { get; }
        /// <summary>
        /// Name of the document type
        /// </summary>
        string DocumentName { get; }
        /// <summary>
        /// Human-readable description of the document.
        /// Used in the document creation dialog.
        /// </summary>
        string DocumentDescription { get; }
        /// <summary>
        /// Category of the document, used in the "Create document" dialog
        /// </summary>
        string DocumentCategory { get; }
        /// <summary>
        /// Appearance of the document.
        /// Used in the document creation dialog.
        /// This must be the path to a SVG resource
        /// 
        /// If the document cannot be created this is not required.
        /// </summary>
        string? DocumentAspect { get; }
        /// <summary>
        /// Icon of the document.
        /// Used in the project explorer.
        /// </summary>
        Bitmap DocumentIcon { get; }
        /// <summary>
        /// If true the document can be created and the IZXDocumentFactory must support this functionality.
        /// Also, if supported, the DocumentAspect must be provided.
        /// </summary>
        bool CanCreate { get; }
        /// <summary>
        /// If true the document can be edited and an implementation of the IZXDocumentEditor must be provided by the factory
        /// </summary>
        bool CanEdit { get; }
        /// <summary>
        /// Factory for the document.
        /// </summary>
        IZXDocumentFactory DocumentFactory { get; }
        /// <summary>
        /// If the document exposes an IZXDocumentBuilder then it means it will need to be built when the project is compiled.
        /// When it will be compiled is determined by DocumentBuildStage
        /// </summary>
        IZXDocumentBuilder? DocumentBuilder { get; }
        /// <summary>
        /// Building stage, null if the document type does not need to be built
        /// </summary>
        ZXBuildStage? DocumentBuildStage { get; }
        /// <summary>
        /// List of editor command shortcuts, used to remap them in the keyboard mapper
        /// </summary>
        ZXKeybCommand[]? EditorCommands { get; }
    }
}
