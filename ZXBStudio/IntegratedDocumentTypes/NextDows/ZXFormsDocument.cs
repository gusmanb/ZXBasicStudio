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
using ZXBasicStudio.DocumentEditors.NextDows.log;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.NextDows
{
    public class ZXFormsDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".zxf" };
        static readonly string _docName = "ZXForms file";
        static readonly string _docDesc = "ZXForms files allow you to create and modify NextDows Forms definitios, known as ZXForms. NextDows is a brand new operating system based on windows environment for ZX Spectrum Next/N-GO devices and compatibles.";
        static readonly string _docCat = "NextDows";
        static readonly string _docAspect = "/Svg/Documents/file-zxforms.svg";
        static readonly Guid _docId = Guid.Parse("03FD2BD3-F067-4123-B6DB-F5EFF91C87B4");

        static readonly ZXFormsDocumentFactory _factory = new ZXFormsDocumentFactory();
        Bitmap? _icon;

        public string[] DocumentExtensions => _docExtensions;
        public string DocumentName => _docName;
        public string DocumentDescription => _docDesc;
        public string DocumentCategory => _docCat;
        public string? DocumentAspect => _docAspect;

        private static readonly ExportManager _exportManager = new ExportManager();


        public ZXFormsDocument()
        {
            _exportManager.Initialize(DocumentEditors.ZXGraphics.neg.FileTypes.NextDows_ZXForms);
        }


        public Bitmap DocumentIcon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/zxForms.png")));
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

        ZXKeybCommand[]? IZXDocumentType.EditorCommands => new ZXKeybCommand[0];
    }
}
