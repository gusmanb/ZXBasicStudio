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

namespace ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Configuration
{
    public class ZXConfigurationDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".zbs" };
        static readonly string _docName = "Configuration file";
        static readonly string _docDesc = "ZX Basic Studio configuration file.";
        static readonly string _docCat = "General";
        static readonly ZXConfigurationFactory _factory = new ZXConfigurationFactory();
        static readonly Guid _docId = Guid.Parse("b13aae8d-6fcc-4bff-9f2a-f2d3bce373ac");

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
        public string? DocumentAspect => null;

        public Bitmap DocumentIcon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/cfgFile.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => false;

        public bool CanEdit => true;

        public IZXDocumentFactory DocumentFactory => _factory;

        public IZXDocumentBuilder? DocumentBuilder => null;

        public ZXBuildStage? DocumentBuildStage => null;

        public ZXKeybCommand[]? EditorCommands => _editCommands;
    }
}
