﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXGraphics.log;
using ZXBasicStudio.DocumentModel.Enums;
using ZXBasicStudio.DocumentModel.Interfaces;

namespace ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.ZXGraphics
{
    public class UDGDocument : IZXDocumentType
    {
        static readonly string[] _docExtensions = { ".gdu", ".udg" };
        static readonly string _docName = "UDG file";
        static readonly string _docDesc = "User Defined Graphics data (21 chars). Array of 168 bytes to use classical ZX Spectrum UDGs/GDUs.\nThe GDU editor allows you to create and modify graphics with the mouse and export in multiple formats.";
        static readonly string _docCat = "Graphics";
        static readonly string _docAspect = "/Svg/Documents/file-udg.svg";

        static readonly UDGDocumentFactory _factory = new UDGDocumentFactory();
        Bitmap? _icon;

        public string[] DocumentExtensions => _docExtensions;
        public string DocumentName => _docName;
        public string DocumentDescription => _docDesc;
        public string DocumentCategory => _docCat;
        public string? DocumentAspect => _docAspect;

        private static readonly ExportManager _exportManager = new ExportManager();

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
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

                    if (assets == null)
                        throw new AvaloniaInternalException("Cannot create assets loader");

                    _icon = new Bitmap(assets.Open(new Uri("avares://ZXBasicStudio/Assets/zxGraphics_udg.png")));
                }

                return _icon;
            }
        }

        public bool CanCreate => true;

        public bool CanEdit => true;

        public IZXDocumentFactory DocumentFactory => _factory;

        public IZXDocumentBuilder? DocumentBuilder => _exportManager;

        public ZXBuildStage? DocumentBuildStage => ZXBuildStage.PreBuild;
    }
}