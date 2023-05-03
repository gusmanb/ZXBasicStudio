using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXBasicStudio.DocumentEditors.ZXTextEditor.Classes.LanguageDefinitions
{
    public class ZXAssemblerDefinition : LanguageDefinitionBase
    {
        const string zxAsmDef = @"<SyntaxDefinition name=""ZXAssembler""
        xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Keywords"" foreground=""#569cd6"" />
    <Color name=""Pseudo"" foreground=""#4dc9b0"" />
    <Color name=""Registers"" foreground=""#ffe56c"" />
    <Color name=""LongRegisters"" foreground=""#ffd929"" />
    <Color name=""Flags"" foreground=""#ffabd3"" />
    <Color name=""Comment"" foreground=""#00ef00"" />
    <Color name=""String"" foreground=""#d69d85"" />
    <Color name=""Number"" foreground=""#ff871d"" />
    <Color name=""HexNumber"" foreground=""#ffa555"" />
    <Color name=""BinNumber"" foreground=""#ffc592"" />
    <Color name=""Directives"" foreground=""#fdc4f9"" />
    <Color name=""Label"" foreground=""#d7ff5e"" />
    <RuleSet ignoreCase=""true"">
        <Keywords color=""Keywords"">
            <Word>ADC</Word>
            <Word>ADD</Word>
            <Word>AND</Word>
            <Word>BIT</Word>
            <Word>CALL</Word>
            <Word>CCF</Word>
            <Word>CP</Word>
            <Word>CPD</Word>
            <Word>CPDR</Word>
            <Word>CPI</Word>
            <Word>CPIR</Word>
            <Word>CPL</Word>
            <Word>DAA</Word>
            <Word>DEC</Word>
            <Word>DI</Word>
            <Word>DJNZ</Word>
            <Word>EI</Word>
            <Word>EX</Word>
            <Word>EXX</Word>
            <Word>HALT</Word>
            <Word>IM</Word>
            <Word>IN</Word>
            <Word>INC</Word>
            <Word>IND</Word>
            <Word>INDR</Word>
            <Word>INI</Word>
            <Word>INIR</Word>
            <Word>JP</Word>
            <Word>JR</Word>
            <Word>LD</Word>
            <Word>LDD</Word>
            <Word>LDDR</Word>
            <Word>LDI</Word>
            <Word>LDIR</Word>
            <Word>NEG</Word>
            <Word>NOP</Word>
            <Word>OR</Word>
            <Word>OTDR</Word>
            <Word>OTIR</Word>
            <Word>OUT</Word>
            <Word>OUTD</Word>
            <Word>OUTI</Word>
            <Word>POP</Word>
            <Word>PUSH</Word>
            <Word>RES</Word>
            <Word>RET</Word>
            <Word>RETI</Word>
            <Word>RETN</Word>
            <Word>RL</Word>
            <Word>RLA</Word>
            <Word>RLC</Word>
            <Word>RLCA</Word>
            <Word>RLD</Word>
            <Word>RR</Word>
            <Word>RRA</Word>
            <Word>RRC</Word>
            <Word>RRCA</Word>
            <Word>RRD</Word>
            <Word>RST</Word>
            <Word>SBC</Word>
            <Word>SCF</Word>
            <Word>SET</Word>
            <Word>SLA</Word>
            <Word>SLL</Word>
            <Word>SRA</Word>
            <Word>SRL</Word>
            <Word>SUB</Word>
            <Word>XOR</Word>
        </Keywords>
        <Keywords color=""Pseudo"">
            <Word>ALIGN</Word>
            <Word>ORG</Word>
            <Word>DEFB</Word>
            <Word>DEFB</Word>
            <Word>DEFB</Word>
            <Word>DEFS</Word>
            <Word>DEFW</Word>
            <Word>DEFS</Word>
            <Word>DEFW</Word>
            <!--<Word>EQU</Word>-->
            <Word>PROC</Word>
            <Word>ENDP</Word>
            <Word>LOCAL</Word>
            <Word>END</Word>
            <Word>INCBIN</Word>
            <Word>NAMESPACE</Word>
        </Keywords>
        <Rule color=""Pseudo"">
            ^.*?\sEQU\s.*$
        </Rule>
        <Keywords color=""Registers"">
            <Word>A</Word>
            <Word>B</Word>
            <Word>C</Word>
            <Word>D</Word>
            <Word>E</Word>
            <Word>H</Word>
            <Word>L</Word>
            <Word>I</Word>
            <Word>R</Word>
            <Word>IXH</Word>
            <Word>IXL</Word>
            <Word>IYH</Word>
            <Word>IYL</Word>
        </Keywords>
        <Keywords color=""LongRegisters"">
            <Word>AF</Word>
            <Word>BC</Word>
            <Word>DE</Word>
            <Word>HL</Word>
            <Word>IX</Word>
            <Word>IY</Word>
            <Word>SP</Word>
        </Keywords>
        <Keywords color=""Flags"">
            <Word>Z</Word>
            <Word>NZ</Word>
            <Word>NC</Word>
            <Word>PO</Word>
            <Word>PE</Word>
            <Word>P</Word>
            <Word>M</Word>
        </Keywords>
        <Span color=""Comment"">
			<Begin>;</Begin>
		</Span>
        <Rule color=""String"">
			""(""""|[^""])*""
		</Rule>
        <Rule color=""HexNumber"">
            ([0-9][0-9a-fA-F]*H)|(0x[0-9a-fA-F]+)
        </Rule>
        <Rule color=""BinNumber"">
            (%[01](_?[01])*)|(0[bB](_?[01])+)
        </Rule>
        <Rule color=""Number"">
            (^|\s)[0-9]+
        </Rule>
        <Rule color=""Directives"">
            \#[a-zA-Z]+
        </Rule>
        <Rule color=""Label"">
            ^\s*?[a-zA-Z0-9_\.]+:
        </Rule>
    </RuleSet>
</SyntaxDefinition>";

        public override string XshdDefinition
        {
            get
            {
                return zxAsmDef;
            }
        }
    }
}
