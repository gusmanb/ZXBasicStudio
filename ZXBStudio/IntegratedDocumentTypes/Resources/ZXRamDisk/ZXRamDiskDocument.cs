using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.Resources.ZXRamDisk
{
    public class ZXRamDiskDocument : IZXDocumentType
    {
        static string[] _docExtensions = new string[] { ".zxr" };
        static string _docName = "ZX RAM disk";
        static string _docDesc = "Creates a RAM disk file that allows to store resources in the extender memory of the 128k models.";
        static string _docCategory = "Resources";
        static string _docAspect = "/Svg/Documents/file-zxramdisk.svg";
        static readonly Guid _docId = Guid.Parse("9de74910-7f8e-4751-aaf0-b310a16986f0");
        static ZXRamDiskFactory _docFactory = new ZXRamDiskFactory();
        static ZXRamDiskBuilder _docBuilder = new ZXRamDiskBuilder();
        Bitmap? _icon;

        public Guid DocumentTypeId => _docId;

        public string[] DocumentExtensions => _docExtensions;

        public string DocumentName => _docName;

        public string DocumentDescription => _docDesc;

        public string DocumentCategory => _docCategory;

        public string? DocumentAspect => _docAspect;

        public Bitmap DocumentIcon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/ramdiskFile.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => true;

        public bool CanEdit => true;

        public IZXDocumentFactory DocumentFactory => _docFactory;

        public IZXDocumentBuilder? DocumentBuilder => _docBuilder;

        public ZXBuildStage? DocumentBuildStage => ZXBuildStage.PreBuild | ZXBuildStage.PostBuild;

        public ZXKeybCommand[]? EditorCommands => null;
    }
}
