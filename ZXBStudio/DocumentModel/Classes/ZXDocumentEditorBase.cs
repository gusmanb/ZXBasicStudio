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
    public abstract class ZXDocumentEditorBase : UserControl, IDisposable
    {
        /// <summary>
        /// Event rised when the document is modified
        /// </summary>
        public abstract event EventHandler? DocumentModified;
        /// <summary>
        /// Event rised when the document is restored to its initial state (changes have been undone)
        /// </summary>
        public abstract event EventHandler? DocumentRestored;
        /// <summary>
        /// Event rised when the document is saved
        /// </summary>
        public abstract event EventHandler? DocumentSaved;
        /// <summary>
        /// Event rised when the editor wants to save the document (for example if the user uses a shortcut that is handled by the editor)
        /// </summary>
        public abstract event EventHandler? RequestSaveDocument;
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
        /// Requests the editor to rename the document, only for internal purposes, the file itself will be renamed by the IDE
        /// </summary>
        /// <param name="OutputLog">TextWriter used to show logs to the user</param>
        /// <param name="NewName">New name of the document, this will always be the full path of the document. DocumentName and DocumentPath MUST be synchronized with it.</param>
        /// <returns>True if the document was renamed successfully, false in other case</returns>
        public abstract bool RenameDocument(string NewName, TextWriter OutputLog);
        /// <summary>
        /// Request the document editor to close the document.
        /// </summary>
        /// <param name="OutputLog">TextWriter used to show logs to the user</param>
        /// <param name="ForceClose">If there is any error and this parameter is true the document will be closed forcibly</param>
        /// <returns>True if the document was closed successfully, false in other case</returns>
        public abstract bool CloseDocument(TextWriter OutputLog, bool ForceClose);
        /// <summary>
        /// IDisposable implementation. All derived classes need to implement it in order to at least clear the event handlers
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Called when the document is activated (shown to the user)
        /// </summary>
        public virtual void Activated()
        {
            
        }

        /// <summary>
        /// Called when the document is deactivated (hidden from the user)
        /// </summary>
        public virtual void Deactivated()
        {

        }
    }
}
