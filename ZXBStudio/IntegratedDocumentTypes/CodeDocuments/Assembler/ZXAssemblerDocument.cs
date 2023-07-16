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
    public class ZXAssemblerDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".zxasm", ".zxa", ".z80asm", ".asm" };
        static readonly string _docName = "Assembler file";
        static readonly string _docDesc = "Z80 Assembler source code file (with ZXASM-compatible syntax)";
        static readonly string _docCat = "Code";
        static readonly string _docAspect = "/Svg/Documents/file-zxasm.svg";
        static readonly Guid _docId = Guid.Parse("bb4e7fc3-9454-4583-b121-38427b5fecf8");

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
            ZXTextEditor.keyboardCommands["Replace"],
            ZXTextEditor.keyboardCommands["Comment"],
            ZXTextEditor.keyboardCommands["Uncomment"]
        };


        static readonly ZXAssemblerDocumentFactory _factory = new ZXAssemblerDocumentFactory();
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
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/asmFile.png")));
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
