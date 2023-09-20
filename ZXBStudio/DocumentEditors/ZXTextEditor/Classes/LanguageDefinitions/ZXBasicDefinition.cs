using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions
{
    public class ZXBasicDefinition : LanguageDefinitionBase
    {
        const string zxbDef = @"<SyntaxDefinition name=""ZXBasic""
        xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Keywords"" foreground=""#569cd6"" />
    <Color name=""Types"" foreground=""#4dc9b0"" />
    <Color name=""Operators"" foreground=""#9adbfe"" />
    <Color name=""Number"" foreground=""#dcdcaa"" />
    <Color name=""Separators"" foreground=""#ff6347"" />
    <Color name=""Comment"" foreground=""#00ef00"" />
    <Color name=""String"" foreground=""#d69d85"" />
    <Color name=""Directives"" foreground=""#c586c0"" />
    <Color name=""Grouping"" foreground=""#ffd68f"" />
    <Color name=""Assembler"" foreground=""#ffc484"" />
    <Color name=""Label"" foreground=""#fdc4f9"" />
    <RuleSet ignoreCase=""true"">
        <Keywords color=""Keywords"">
            <!-- keywords -->
            <Word>AS</Word>
            <Word>BEEP</Word>
            <Word>BORDER</Word>
            <Word>BRIGHT</Word>
            <Word>CIRCLE</Word>
            <Word>CLS</Word>
            <Word>CONTINUE</Word>
            <Word>DIM</Word>
            <Word>DO</Word>
            <Word>DATA</Word>
            <Word>DRAW</Word>
            <Word>EXIT</Word>
            <Word>FLASH</Word>
            <Word>FOR</Word>
            <Word>GO TO</Word>
            <Word>GOTO</Word>
            <Word>GO SUB</Word>
            <Word>GOSUB</Word>
            <Word>IF</Word>
            <Word>INK</Word>
            <Word>INVERSE</Word>
            <Word>LET</Word>
            <Word>LOAD</Word>
            <Word>LOOP</Word>
            <Word>NEXT</Word>
            <Word>OVER</Word>
            <Word>OUT</Word>
            <Word>PAPER</Word>
            <Word>PAUSE</Word>
            <Word>PLOT</Word>
            <Word>POKE</Word>
            <Word>PRINT</Word>
            <Word>RANDOMIZE</Word>
            <Word>READ</Word>
            <Word>RESTORE</Word>
            <Word>RETURN</Word>
            <Word>SAVE</Word>
            <Word>UNTIL</Word>
            <Word>VERIFY</Word>
            <Word>WEND</Word>
            <Word>WHILE</Word>
            <Word>AT</Word>
            <Word>BOLD</Word>
            <Word>BYREF</Word>
            <Word>BYVAL</Word>
            <Word>CONST</Word>
            <Word>DECLARE</Word>
            <Word>ELSE</Word>
            <Word>ELSEIF</Word>
            <Word>END</Word>
            <Word>FASTCALL</Word>
            <Word>FUNCTION</Word>
            <Word>ITALIC</Word>
            <Word>LBOUND</Word>
            <Word>PI</Word>
            <Word>STDCALL</Word>
            <Word>STEP</Word>
            <Word>STOP</Word>
            <Word>SUB</Word>
            <Word>THEN</Word>
            <Word>TO</Word>
            <Word>TRUE</Word>
            <Word>FALSE</Word>
            <!-- Functions -->
            <Word>ABS</Word>
            <Word>ACS</Word>
            <Word>ASN</Word>
            <Word>ATN</Word>
            <Word>CAST</Word>
            <Word>CODE</Word>
            <Word>COS</Word>
            <Word>EXP</Word>
            <Word>IN</Word>
            <Word>INT</Word>
            <Word>LEN</Word>
            <Word>LN</Word>
            <Word>PEEK</Word>
            <Word>RND</Word>
            <Word>SGN</Word>
            <Word>SIN</Word>
            <Word>SQR</Word>
            <Word>TAN</Word>
            <Word>UBOUND</Word>
            <Word>VAL</Word>
            <Word>CHR</Word>
            <Word>INKEY</Word>
            <Word>INPUT</Word>
            <Word>STR</Word>
        </Keywords>
        <Keywords color=""Types"">
            <Word>BYTE</Word>
            <Word>UBYTE</Word>
            <Word>INTEGER</Word>
            <Word>UINTEGER</Word>
            <Word>LONG</Word>
            <Word>ULONG</Word>
            <Word>FIXED</Word>
            <Word>FLOAT</Word>
            <Word>STRING</Word>
        </Keywords>
        <Keywords color=""Operators"">
            <Word>!</Word>
            <Word>=</Word>
            <Word>&gt;</Word>
            <Word>&lt;</Word>
            <Word>*</Word>
            <Word>/</Word>
            <Word>+</Word>
            <Word>-</Word>
            <Word>AND</Word>
            <Word>BAND</Word>
            <Word>BNOT</Word>
            <Word>BOR</Word>
            <Word>BXOR</Word>
            <Word>MOD</Word>
            <Word>NOT</Word>
            <Word>OR</Word>
            <Word>SHL</Word>
            <Word>SHR</Word>
            <Word>XOR</Word>
        </Keywords>
        <Keywords color=""Grouping"">
            <Word>(</Word>
            <Word>)</Word>
            <Word>{</Word>
            <Word>}</Word>
            <Word>,</Word>
        </Keywords>
        <Span color=""Comment"">
			<Begin>(^|\s|:)REM(\s|\r|\n|$)</Begin>
		</Span>
        <Span color=""Comment"">
			<Begin>(^|\s|:)'</Begin>
		</Span>
		<Span color=""Comment"" multiline=""true"">
			<Begin>/'</Begin>
			<End>'/</End>
		</Span>
        <Span color=""Assembler"" multiline=""true"">
            <Begin>(^|\s|:)asm(\s|\r|\n|$)</Begin>
			<End>(^|\s|:)end\sasm(\s|\r|\n|$)</End>
        </Span>
        <Span color=""String"">
			<Begin>""</Begin>
			<End>""</End>
            <RuleSet>
				<Span begin=""\\"" end="".""/>
			</RuleSet>
		</Span>
        <Rule color=""Number"">
            (^|\s)\$[0-9a-fA-F]+
        </Rule>
        <Rule color=""Number"">
            (^|\s)[0-9][0-9a-fA-F]+h
        </Rule>
        <Rule color=""Number"">
            ([^\w]|^)[0-9]+
        </Rule>
        <Span color=""Directives"">
            <Begin>^\s*?\#[a-zA-Z]+</Begin>
        </Span>
        <Rule color=""Label"">
            ^\s*?[a-zA-Z0-9_]+:
        </Rule>
    </RuleSet>
</SyntaxDefinition>";

        public override string XshdDefinition
        {
            get
            {
                return zxbDef;
            }
        }
    }
}
