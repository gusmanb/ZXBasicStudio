using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Controls
{
    public class ZXAssemblerEditor : ZXTextEditor
    {
        static ZXAssemblerDefinition def = new ZXAssemblerDefinition();
        static Regex regCancel = new Regex("^(\\s*;|\\s*[a-zA-Z0-9_]+:(\\s*|\\s*;.*?)$)|^\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        protected override LanguageDefinitionBase? langDef => def;
        protected override char? commentChar => ';';
        protected override Regex? regCancelBreakpoint => regCancel;
        protected override IBrush? searchMarkerBrush => Brushes.Red;
        protected override bool allowsBreakpoints => true;

        public ZXAssemblerEditor() : base() { }
        public ZXAssemblerEditor(string DocumentPath) : base(DocumentPath) { }
    }
}
