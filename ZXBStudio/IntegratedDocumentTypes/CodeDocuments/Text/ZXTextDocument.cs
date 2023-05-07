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

namespace ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Text
{
    public class ZXTextDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".txt" };
        static readonly string _docDesc = "Text document";
        static readonly ZXTextDocumentFactory _factory = new ZXTextDocumentFactory();
        Avalonia.Svg.Skia.Svg? _aspect;
        Bitmap? _icon;

        public string[] DocumentExtensions => _docExtensions;

        public string DocumentDescription => _docDesc;

        public Avalonia.Svg.Skia.Svg? DocumentAspect
        {
            get
            {
                if (_aspect == null)
                    _aspect = new Avalonia.Svg.Skia.Svg(new Uri("/Svg/Documents/file-text.svg"));

                return _aspect;
            }
        }

        public Bitmap DocumentIcon
        {
            get
            {
                if (_icon == null)
                {
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

                    if (assets == null)
                        throw new AvaloniaInternalException("Cannot create asstes loader");

                    _icon = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/txtFile.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => true;

        public bool CanEdit => true;

        public IZXDocumentFactory DocumentFactory => _factory;

        public IZXDocumentBuilder? DocumentBuilder => null;

        public ZXBuildStage? DocumentBuildStage => null;
    }
}
