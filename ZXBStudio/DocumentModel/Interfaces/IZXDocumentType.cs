using Avalonia.Controls;
using Avalonia.Svg.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentModel.Enums;

namespace ZXBasicStudio.DocumentModel.Interfaces
{
    /// <summary>
    /// Interface for document description
    /// </summary>
    public interface IZXDocumentType
    {
        /// <summary>
        /// List of possible extensions for the document type.
        /// </summary>
        string[] DocumentExtensions { get; }
        /// <summary>
        /// Human-readable description of the document.
        /// Used in the document creation dialog.
        /// </summary>
        string DocumentDescription { get; }
        /// <summary>
        /// Appearance of the document.
        /// Used in the document creation dialog.
        /// </summary>
        Avalonia.Svg.Skia.Svg DocumentAspect { get; }
        /// <summary>
        /// Icon of the document.
        /// Used in the project explorer.
        /// </summary>
        Image DocumentIcon { get; }
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
    }
}
