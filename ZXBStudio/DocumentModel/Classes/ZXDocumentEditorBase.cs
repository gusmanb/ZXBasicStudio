using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentModel.Classes
{
    /// <summary>
    /// Base class for all the document editors.
    /// </summary>
    public abstract class ZXDocumentEditorBase : Control
    {
        /// <summary>
        /// Event rised when the document is modified
        /// </summary>
        public abstract event EventHandler? DocumentModified;
        /// <summary>
        /// Event rised when the document is saved internally
        /// </summary>
        public abstract event EventHandler? DocumentSaved;
        /// <summary>
        /// Visible name of the document, used to show it in the document tabs
        /// </summary>
        public abstract string DocumentName { get; }
        /// <summary>
        /// Path of the document, must be preserved as-is when the system sends it to the factory or rename.
        /// </summary>
        public abstract string DocumentPath { get; }
        /// <summary>
        /// Indicates if the document has been modified and not saved
        /// </summary>
        public abstract bool Modified { get; }
        /// <summary>
        /// Requests the editor to save the document
        /// </summary>
        /// <param name="OutputLog">TextWriter used to show logs to the user</param>
        /// <returns>True if the document was saved successfully, false in other case</returns>
        public abstract bool SaveDocument(TextWriter OutputLog);
        /// <summary>
        /// Requests the editor to rename the document
        /// </summary>
        /// <param name="OutputLog">TextWriter used to show logs to the user</param>
        /// <param name="NewName">New name of the document, this may be a full path or just a file name, if the name is a full path the file is being moved, else only its name is changed. DocumentName and DocumentPath MUST be synchronized with it.</param>
        /// <returns>True if the document was renamed successfully, false in other case</returns>
        public abstract bool RenameDocument(string NewName, TextWriter OutputLog);
        /// <summary>
        /// Request the document editor to close the document.
        /// </summary>
        /// <param name="OutputLog">TextWriter used to show logs to the user</param>
        /// <param name="ForceClose">If there is any error and this parameter is true the document will be closed forcibly</param>
        /// <returns>True if the document was closed successfully, false in other case</returns>
        public abstract bool CloseDocument(TextWriter OutputLog, bool ForceClose);

    }
}
