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
        static Regex regCancel = new Regex("^(\\s*;|\\s*[a-zA-Z0-9_]+:(\\s*|\\s*;.*?)$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        protected override LanguageDefinitionBase? langDef
        {
            get
            {
                return def;
            }
        }

        protected override char? CommentChar
        {
            get
            {
                return ';';
            }
        }

        protected override Regex? RegCancelBreakpoint
        {
            get
            {
                return regCancel;
            }
        }

        public ZXAssemblerEditor() : base() { }
        public ZXAssemblerEditor(string FileName) : base(FileName) { }
    }
}
