using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.Folding;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions;
using ZXBasicStudio.IntegratedDocumentTypes.CodeDocuments.Basic;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls
{
    public class ZXBasicEditor : ZXTextEditor
    {
        static ZXBasicDefinition def = new ZXBasicDefinition();
        static ZXBasicFoldingStrategy strategy = new ZXBasicFoldingStrategy();
        static Regex regCancel = new Regex("^(\\s*'|((\\s*|_)REM\\s)|^\\s*$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static ZXBasicCompletionData[] completion = new ZXBasicCompletionData[] 
        {
            new ZXBasicCompletionData{ Text = "for" },
            new ZXBasicCompletionData{ Text = "next" }
        };
        protected override LanguageDefinitionBase? langDef => def;
        protected override IBrush? searchMarkerBrush => Brushes.Red;
        protected override AbstractFoldingStrategy? foldingStrategy => strategy;
        protected override char? commentChar => '\'';
        protected override Regex? regCancelBreakpoint => regCancel;
        protected override bool allowsBreakpoints => true;

        protected override ICompletionData[] CompletionData
        {
            get
            {
                return completion;
            }
        }
        public ZXBasicEditor() : base() { }
        public ZXBasicEditor(string DocumentPath) : base(DocumentPath, ZXBasicDocument.Id) { }
    }
}
