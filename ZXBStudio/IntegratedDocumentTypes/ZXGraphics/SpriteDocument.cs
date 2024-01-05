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
    public class SpriteDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".spr" };
        static readonly string _docName = "Sprites file";
        static readonly string _docDesc = "Sprite files allow you to create and modify graphics of different sizes and formats. Once created, they can be built in multiple formats.";
        static readonly string _docCat = "Graphics";
        static readonly string _docAspect = "/Svg/Documents/file-sprite.svg";
        static readonly Guid _docId = Guid.Parse("E5D4D440-B156-42F1-8FBB-E78D655E754E");
        public static Guid Id => _docId;

        static readonly SpriteDocumentFactory _factory = new SpriteDocumentFactory();
        Bitmap? _icon;

        public string[] DocumentExtensions => _docExtensions;
        public string DocumentName => _docName;
        public string DocumentDescription => _docDesc;
        public string DocumentCategory => _docCat;
        public string? DocumentAspect => _docAspect;

        private static readonly ExportManager _exportManager = new ExportManager();

        static readonly ZXKeybCommand[] _editCommands = new ZXKeybCommand[]
        {
            SpriteEditor.keyboardCommands["Save"],
            SpriteEditor.keyboardCommands["Cut"],
            SpriteEditor.keyboardCommands["Copy"],
            SpriteEditor.keyboardCommands["Paste"],
            SpriteEditor.keyboardCommands["Clear"],
            SpriteEditor.keyboardCommands["Undo"],
            SpriteEditor.keyboardCommands["Redo"],
            SpriteEditor.keyboardCommands["Rotate Right"],
            SpriteEditor.keyboardCommands["Rotate Left"],
            SpriteEditor.keyboardCommands["Horizontal Mirror"],
            SpriteEditor.keyboardCommands["Vertical Mirror"],
            SpriteEditor.keyboardCommands["Shift Up"],
            SpriteEditor.keyboardCommands["Shift Right"],
            SpriteEditor.keyboardCommands["Shift Down"],
            SpriteEditor.keyboardCommands["Shift Left"],
            SpriteEditor.keyboardCommands["Move Up"],
            SpriteEditor.keyboardCommands["Move Right"],
            SpriteEditor.keyboardCommands["Move Down"],
            SpriteEditor.keyboardCommands["Move Left"],
            SpriteEditor.keyboardCommands["Invert"],
            SpriteEditor.keyboardCommands["Mask"],
            SpriteEditor.keyboardCommands["Export"],
            SpriteEditor.keyboardCommands["Zoom In"],
            SpriteEditor.keyboardCommands["Zoom Out"],
        };


        public SpriteDocument()
        {
            _exportManager.Initialize(DocumentEditors.ZXGraphics.neg.FileTypes.Sprite);
        }


        public Bitmap DocumentIcon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/zxGraphics_spr.png")));
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
