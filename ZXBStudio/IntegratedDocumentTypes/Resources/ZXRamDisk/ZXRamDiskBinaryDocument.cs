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
    public class ZXRamDiskBinaryDocument : IZXDocumentType
    {
        static string[] _docExtensions = new string[] { ".zxrbin" };
        static string _docName = "ZX RAM disk binary";
        static string _docDesc = "Compiled RAM disk binary.";
        static string _docCategory = "Resources";
        static string _docAspect = "/Svg/Documents/file-zxramdisk.svg";
        static readonly Guid _docId = Guid.Parse("e13fdac1-ce07-4ffc-b96d-df3657993689");

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
                    _icon = new Bitmap(AssetLoader.Open(new Uri("avares://ZXBasicStudio/Assets/ramdiskBinFile.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => false;

        public bool CanEdit => false;

        public IZXDocumentFactory DocumentFactory => null;

        public IZXDocumentBuilder? DocumentBuilder => null;

        public ZXBuildStage? DocumentBuildStage => ZXBuildStage.PreBuild;

        public ZXKeybCommand[]? EditorCommands => null;
    }
}
