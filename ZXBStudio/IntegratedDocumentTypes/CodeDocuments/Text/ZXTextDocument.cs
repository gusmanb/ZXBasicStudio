using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Text
{
    public class ZXTextDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".txt", ".md" };
        static readonly string _docName = "Text document";
        static readonly string _docDesc = "Plain text document.";
        static readonly string _docCat = "General";
        static readonly string _docAspect = "/Svg/Documents/file-text.svg";
        static readonly ZXTextDocumentFactory _factory = new ZXTextDocumentFactory();
        static readonly Guid _docId = Guid.Parse("1202e397-504e-4666-9369-4ea0e888fbcb");

        public static Guid Id => _docId;

        static readonly ZXKeybCommand[] _editCommands = new ZXKeybCommand[]
        {
            ZXTextEditor.keyboardCommands["Save"],
            ZXTextEditor.keyboardCommands["Copy"],
            ZXTextEditor.keyboardCommands["Cut"],
            ZXTextEditor.keyboardCommands["Paste"],
            ZXTextEditor.keyboardCommands["Select"],
            ZXTextEditor.keyboardCommands["Undo"],
            ZXTextEditor.keyboardCommands["Redo"],
            ZXTextEditor.keyboardCommands["Find"],
            ZXTextEditor.keyboardCommands["Replace"]
        };

        Bitmap? _icon;

        public Guid DocumentTypeId => _docId;
        public string[] DocumentExtensions => _docExtensions;
        public string DocumentName => _docName;
        public string DocumentDescription => _docDesc;
        public string DocumentCategory => _docCat;
        public string? DocumentAspect => _docAspect;

        public Bitmap DocumentIcon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/txtFile.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => true;

        public bool CanEdit => true;

        public IZXDocumentFactory DocumentFactory => _factory;

        public IZXDocumentBuilder? DocumentBuilder => null;

        public ZXBuildStage? DocumentBuildStage => null;

        public ZXKeybCommand[]? EditorCommands => _editCommands;
    }
}
