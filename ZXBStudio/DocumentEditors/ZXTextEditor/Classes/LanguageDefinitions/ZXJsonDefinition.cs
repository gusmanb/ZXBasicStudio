using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions
{
    public class ZXJsonDefinition : LanguageDefinitionBase
    {
        const string jsonDefinition = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<SyntaxDefinition name=""Json"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
  <Color name=""Digits"" foreground=""#d670fb"" exampleText=""3.14"" />
  <Color name=""Value"" foreground=""#569fff"" exampleText=""var text = &quot;Hello, World!&quot;;"" />
  <Color name=""ParamName"" foreground=""#2ad623""  exampleText=""var text = &quot;Hello, World!&quot;;"" />
  <RuleSet ignoreCase=""false"">
    <Keywords color=""Digits"" >
      <Word>true</Word>
      <Word>false</Word>
    </Keywords>
    <Span color=""ParamName"">
      <Begin>""</Begin>
      <End>(?=:)</End>
    </Span>
    <Span color=""Value"" multiline=""true"">
      <Begin>
        (?&lt;=:)\040""[^""]*
      </Begin>
      <End>""</End>
    </Span>
    <Rule color=""Digits"">\b0[xX][0-9a-fA-F]+|(\b\d+(\.[0-9]+)?|\.[0-9]+)([eE][+-]?[0-9]+)?</Rule>
  </RuleSet>
</SyntaxDefinition>";

        public override string XshdDefinition
        {
            get
            {
                return jsonDefinition;
            }
        }
    }
}
