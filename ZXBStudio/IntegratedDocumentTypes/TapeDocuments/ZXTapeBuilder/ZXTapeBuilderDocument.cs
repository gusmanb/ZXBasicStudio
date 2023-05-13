using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.TapeDocuments.ZXTapeBuilder
{
    public class ZXTapeBuilderDocument : IZXDocumentType
    {
        static string[] _docExtensions = new string[] { ".zxt" };
        static string _docName = "ZX Tape builder";
        static string _docDesc = "Creates a tape builder file which allows to define a tape structure to be composed when the project is built in release mode.";
        static string _docCategory = "Tools";
        static string _docAspect = "/Svg/Documents/file-zxtape.svg";
        static ZXTapeBuilderFactory _docFactory = new ZXTapeBuilderFactory();
        static ZXTapeBuilderBuilder _docBuilder = new ZXTapeBuilderBuilder();
        Bitmap? _icon;

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
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

                    if (assets == null)
                        throw new AvaloniaInternalException("Cannot create asstes loader");

                    _icon = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/zxtFile.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => true;

        public bool CanEdit => true;

        public IZXDocumentFactory DocumentFactory => _docFactory;

        public IZXDocumentBuilder? DocumentBuilder => _docBuilder;

        public ZXBuildStage? DocumentBuildStage => ZXBuildStage.PostBuild;
    }
}
