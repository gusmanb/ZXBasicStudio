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
using ZXBasicStudio.DocumentEditors.ZXGraphics;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.ZXGraphics
{
    public class UDGDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".udg", ".gdu" };
        static readonly string _docName = "UDG file";
        static readonly string _docDesc = "User Defined Graphics data (21 chars). Array of 168 bytes to use classical ZX Spectrum UDGs/GDUs.\nThe GDU editor allows you to create and modify graphics with the mouse and build in multiple formats.";
        static readonly string _docCat = "Graphics";
        static readonly string _docAspect = "/Svg/Documents/file-udg.svg";
        static readonly Guid _docId = Guid.Parse("468D8D2B-8461-4950-A4C3-8B20454B851A");

        public static Guid Id => _docId;

        static readonly UDGDocumentFactory _factory = new UDGDocumentFactory();
        Bitmap? _icon;

        public string[] DocumentExtensions => _docExtensions;
        public string DocumentName => _docName;
        public string DocumentDescription => _docDesc;
        public string DocumentCategory => _docCat;
        public string? DocumentAspect => _docAspect;

        private static readonly ExportManager _exportManager = new ExportManager();

        static readonly ZXKeybCommand[] _editCommands = new ZXKeybCommand[]
        {
            FontGDUEditor.keyboardCommands["Save"],
            FontGDUEditor.keyboardCommands["Cut"],
            FontGDUEditor.keyboardCommands["Copy"],
            FontGDUEditor.keyboardCommands["Paste"],
            FontGDUEditor.keyboardCommands["Clear"],
            FontGDUEditor.keyboardCommands["Undo"],
            FontGDUEditor.keyboardCommands["Redo"],
            FontGDUEditor.keyboardCommands["Rotate Right"],
            FontGDUEditor.keyboardCommands["Rotate Left"],
            FontGDUEditor.keyboardCommands["Horizontal Mirror"],
            FontGDUEditor.keyboardCommands["Vertical Mirror"],
            FontGDUEditor.keyboardCommands["Shift Up"],
            FontGDUEditor.keyboardCommands["Shift Right"],
            FontGDUEditor.keyboardCommands["Shift Down"],
            FontGDUEditor.keyboardCommands["Shift Left"],
            FontGDUEditor.keyboardCommands["Move Up"],
            FontGDUEditor.keyboardCommands["Move Right"],
            FontGDUEditor.keyboardCommands["Move Down"],
            FontGDUEditor.keyboardCommands["Move Left"],
            FontGDUEditor.keyboardCommands["Invert"],
            FontGDUEditor.keyboardCommands["Mask"],
            FontGDUEditor.keyboardCommands["Export"],
            FontGDUEditor.keyboardCommands["Zoom In"],
            FontGDUEditor.keyboardCommands["Zoom Out"],
        };

        public UDGDocument()
        {
            _exportManager.Initialize(DocumentEditors.ZXGraphics.neg.FileTypes.UDG);
        }


        public Bitmap DocumentIcon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/zxGraphics_udg.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => true;

        public bool CanEdit => true;

        public IZXDocumentFactory DocumentFactory => _factory;

        public IZXDocumentBuilder? DocumentBuilder => _exportManager;

        public ZXBuildStage? DocumentBuildStage => ZXBuildStage.PreBuild;

        Guid IZXDocumentType.DocumentTypeId => _docId;

        ZXKeybCommand[]? IZXDocumentType.EditorCommands => _editCommands;
    }
}
