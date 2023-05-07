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

namespace ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Configuration
{
    public class ZXConfigurationDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".zbs" };
        static readonly string _docDesc = "ZX configuration document";
        static readonly string _docCat = "General";
        static readonly ZXConfigurationFactory _factory = new ZXConfigurationFactory();
        Bitmap? _icon;

        public string[] DocumentExtensions => _docExtensions;
        public string DocumentDescription => _docDesc;
        public string DocumentCategory => _docCat;
        public Avalonia.Svg.Skia.Svg? DocumentAspect => null;

        public Bitmap DocumentIcon
        {
            get
            {
                if (_icon == null)
                {
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

                    if (assets == null)
                        throw new AvaloniaInternalException("Cannot create asstes loader");

                    _icon = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/cfgFile.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => false;

        public bool CanEdit => true;

        public IZXDocumentFactory DocumentFactory => _factory;

        public IZXDocumentBuilder? DocumentBuilder => null;

        public ZXBuildStage? DocumentBuildStage => null;
    }
}
