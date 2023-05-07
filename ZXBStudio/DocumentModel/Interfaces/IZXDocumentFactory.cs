using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentModel.Classes;

namespace ZXBasicStudio.DocumentModel.Interfaces
{
    /// <summary>
    /// Interface used to create documents and its editors
    /// </summary>
    public interface IZXDocumentFactory
    {
        /// <summary>
        /// Used to create a new document in the file system
        /// </summary>
        /// <param name="Path">Path of the new document</param>
        /// <param name="OutputLog">TextWriter used to show logs to the user</param>
        /// <returns>True if the document was created successfully, false in other case</returns>
        bool CreateDocument(string Path, TextWriter OutputLog);
        /// <summary>
        /// Used to create and editor for an existing document
        /// </summary>
        /// <param name="Path">Path to the document</param>
        /// <param name="OutputLog">TextWriter used to show logs to the user</param>
        /// <returns>The new document editor if success, null in other case</returns>
        ZXDocumentEditorBase? CreateEditor(string Path, TextWriter OutputLog);
    }
}
