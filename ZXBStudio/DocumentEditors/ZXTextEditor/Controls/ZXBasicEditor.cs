using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.Folding;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls
{
    public class ZXBasicEditor : ZXTextEditor
    {
        static ZXBasicDefinition def = new ZXBasicDefinition();
        static ZXBasicFoldingStrategy strategy = new ZXBasicFoldingStrategy();
        static Regex regCancel = new Regex("^(\\s*'|((\\s*|_)REM\\s)|^\\s*$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        protected override LanguageDefinitionBase? langDef => def;
        protected override IBrush? searchMarkerBrush => Brushes.Red;
        protected override AbstractFoldingStrategy? foldingStrategy => strategy;
        protected override char? commentChar => '\'';
        protected override Regex? regCancelBreakpoint => regCancel;
        protected override bool allowsBreakpoints => true;

        public ZXBasicEditor() : base() { }
        public ZXBasicEditor(string FileName) : base(FileName) { }
    }
}
