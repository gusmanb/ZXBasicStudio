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
        static readonly string[] _docExtensions = { ".zxasm", ".zxa", ".z80asm", ".asm" };
        static readonly string _docName = "Assembler file";
        static readonly string _docDesc = "Z80 Assembler source code file (with ZXASM-compatible syntax)";
        static readonly string _docCat = "Code";
        static readonly string _docAspect = "/Svg/Documents/file-zxasm.svg";

        static readonly ZXAssemblerDocumentFactory _factory = new ZXAssemblerDocumentFactory();
        Bitmap? _icon;

        public string[] DocumentExtensions => _docExtensions;
        public string DocumentName => _docName;
        public string DocumentDescription => _docDesc;
        public string DocumentCategory => _docCat;
        public string? DocumentAspect => _docAspect;

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
