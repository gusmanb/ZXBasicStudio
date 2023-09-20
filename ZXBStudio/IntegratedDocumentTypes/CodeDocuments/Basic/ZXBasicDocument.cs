using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic
{
    public class ZXBasicDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".zxbas", ".zxb", ".bas" };
        static readonly string _docName = "ZX Basic file";
        static readonly string _docDesc = "ZX Basic source code file.";
        static readonly string _docCat = "Code";
        static readonly string _docAspect = "/Svg/Documents/file-zxbasic.svg";
        static readonly Guid _docId = Guid.Parse("b7f0d8e8-7fc6-4cb1-92a9-e574547f43e9");

        public static Guid Id => _docId;

        static readonly ZXBasicDocumentFactory _factory = new ZXBasicDocumentFactory();
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
            ZXTextEditor.keyboardCommands["Replace"],
            ZXTextEditor.keyboardCommands["Collapse"],
            ZXTextEditor.keyboardCommands["Expand"],
            ZXTextEditor.keyboardCommands["Comment"],
            ZXTextEditor.keyboardCommands["Uncomment"]
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
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/zxbFile.png")));
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
