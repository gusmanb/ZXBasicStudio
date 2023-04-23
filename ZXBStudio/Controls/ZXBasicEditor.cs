using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXBasicStudio.Classes;
using ZXBasicStudio.LanguageDefinitions;

namespace ZXBasicStudio.Controls
{
    public class ZXBasicEditor : ZXTextEditor
    {
        static ZXBasicDefinition def = new ZXBasicDefinition();
        static ZXBasicFoldingStrategy strategy = new ZXBasicFoldingStrategy();
        static Regex regCancel = new Regex("^(\\s*'|((\\s*|_)REM\\s))", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        protected override LanguageDefinitionBase? langDef
        {
            get
            {
                return def;
            }
        }

        protected override IBrush? SearchMarkerBrush
        {
            get
            {
                return Brushes.Red;
            }
        }

        protected override AbstractFoldingStrategy? FoldingStrategy
        {
            get
            {
                return strategy;
            }
        }

        protected override char? CommentChar
        {
            get
            {
                return '\'';
            }
        }

        protected override Regex? RegCancelBreakpoint
        {
            get
            {
                return regCancel;
            }
        }

        public ZXBasicEditor() : base() { }
        public ZXBasicEditor(string FileName) : base(FileName) { }
    }
}
