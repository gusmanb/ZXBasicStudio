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
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic
{
    public class ZXAssemblerDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".asm", ".zxasm", ".zxa", ".z80asm" };
        static readonly string _docDesc = "ZX Assembler document";
        static readonly string _docCat = "Code";
        static readonly ZXAssemblerDocumentFactory _factory = new ZXAssemblerDocumentFactory();
        Avalonia.Svg.Skia.Svg? _aspect;
        Bitmap? _icon;

        public string[] DocumentExtensions => _docExtensions;
        public string DocumentDescription => _docDesc;
        public string DocumentCategory => _docCat;
        public Avalonia.Svg.Skia.Svg? DocumentAspect
        {
            get
            {
                if (_aspect == null)
                    _aspect = new Avalonia.Svg.Skia.Svg(new Uri("/Svg/Documents/file-zxasm.svg"));

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
                        throw new AvaloniaInternalException("Cannot create assets loader");

                    _icon = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/asmFile.png")));
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
