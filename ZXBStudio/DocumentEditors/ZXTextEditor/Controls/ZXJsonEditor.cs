using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.Folding;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Text;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls
{
    public class ZXJsonEditor : ZXTextEditor
    {
        static ZXJsonDefinition def = new ZXJsonDefinition();

        protected override LanguageDefinitionBase? langDef => def;
        protected override IBrush? searchMarkerBrush => Brushes.Red;
        protected override bool allowsBreakpoints => false;

        public ZXJsonEditor() : base() { }
        public ZXJsonEditor(string DocumentPath) : base(DocumentPath, ZXTextDocument.Id) { }
    }
}
