using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.IO;
using ZXBasicStudio.DocumentModel.Classes;

namespace ZXBasicStudio.DocumentEditors.ZXTapeBuilder.Controls
{
    public partial class ZXTapeBuilderEditor : ZXDocumentEditorBase
    {
        static readonly ZXColorItem[] _colors = new ZXColorItem[] 
        {
            new ZXColorItem{ ColorName = "Black", ColorBrush = SolidColorBrush.Parse("#000000"), ColorValue = 0 },
            new ZXColorItem{ ColorName = "Blue", ColorBrush = SolidColorBrush.Parse("#0000D7"), ColorValue = 1 },
            new ZXColorItem{ ColorName = "Red", ColorBrush = SolidColorBrush.Parse("#D70000"), ColorValue = 2 },
            new ZXColorItem{ ColorName = "Purple", ColorBrush = SolidColorBrush.Parse("#D700D7"), ColorValue = 3 },
            new ZXColorItem{ ColorName = "Green", ColorBrush = SolidColorBrush.Parse("#00D700"), ColorValue = 4 },
            new ZXColorItem{ ColorName = "Cyan", ColorBrush = SolidColorBrush.Parse("#00D7D7"), ColorValue = 5 },
            new ZXColorItem{ ColorName = "Yellow", ColorBrush = SolidColorBrush.Parse("#D7D700"), ColorValue = 6 },
            new ZXColorItem{ ColorName = "White", ColorBrush = SolidColorBrush.Parse("#D7D7D7"), ColorValue = 7 },
        };

        public override event EventHandler? DocumentModified;
        public override event EventHandler? DocumentRestored;
        public override event EventHandler? DocumentSaved;
        public override event EventHandler? RequestSaveDocument;

        public ZXColorItem[] Colors => _colors;

        public override string DocumentName
        {
            get
            {
                return "";
            }
        }

        public override string DocumentPath
        {
            get
            {
                return "";
            }
        }

        public override bool Modified
        {
            get
            {
                return false;
            }
        }

        public ZXTapeBuilderEditor()
        {
            DataContext = this;
            InitializeComponent();
        }

        public class ZXColorItem
        {
            public required string ColorName { get; set; }
            public required IBrush ColorBrush { get; set; } 
            public required int ColorValue { get; set; }
        }

        public override bool SaveDocument(TextWriter OutputLog)
        {
            throw new NotImplementedException();
        }

        public override bool RenameDocument(string NewName, TextWriter OutputLog)
        {
            throw new NotImplementedException();
        }

        public override bool CloseDocument(TextWriter OutputLog, bool ForceClose)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
