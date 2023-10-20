using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
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
        static ZXBasicCompletionData[] keywords = new ZXBasicCompletionData[] 
        {
            /*
             * List of keywords
             * Original source:
             * # Keywords
                "abs": "ABS",
                "acs": "ACS",
                "and": "AND",
                "as": "AS",
                "at": "AT",
                "asm": "ASM",
                "asn": "ASN",
                "atn": "ATN",
                "band": "BAND",
                "beep": "BEEP",
                "bin": "BIN",
                "bnot": "BNOT",
                "bold": "BOLD",
                "bor": "BOR",
                "border": "BORDER",
                "bright": "BRIGHT",
                "bxor": "BXOR",
                "byref": "BYREF",
                "byval": "BYVAL",
                "cast": "CAST",
                "chr": "CHR",
                "chr$": "CHR",
                "circle": "CIRCLE",
                "cls": "CLS",
                "code": "CODE",
                "const": "CONST",
                "continue": "CONTINUE",
                "cos": "COS",
                "data": "DATA",
                "declare": "DECLARE",
                "dim": "DIM",
                "do": "DO",
                "draw": "DRAW",
                "else": "ELSE",
                "elseif": "ELSEIF",
                "end": "END",
                "endif": "ENDIF",
                "error": "ERROR",
                "exit": "EXIT",
                "exp": "EXP",
                "fastcall": "FASTCALL",
                "flash": "FLASH",
                "for": "FOR",
                "function": "FUNCTION",
                "go": "GO",
                "goto": "GOTO",
                "gosub": "GOSUB",
                "if": "IF",
                "in": "IN",
                "ink": "INK",
                "inkey": "INKEY",
                "inkey$": "INKEY",
                "int": "INT",
                "inverse": "INVERSE",
                "italic": "ITALIC",
                "lbound": "LBOUND",
                "let": "LET",
                "len": "LEN",
                "ln": "LN",
                "load": "LOAD",
                "loop": "LOOP",
                "mod": "MOD",
                "next": "NEXT",
                "not": "NOT",
                "on": "ON",
                "or": "OR",
                "out": "OUT",
                "over": "OVER",
                "paper": "PAPER",
                "pause": "PAUSE",
                "peek": "PEEK",
                "pi": "PI",
                "plot": "PLOT",
                "poke": "POKE",
                "print": "PRINT",
                "randomize": "RANDOMIZE",
                "read": "READ",
                "restore": "RESTORE",
                "return": "RETURN",
                "rnd": "RND",
                "save": "SAVE",
                "sgn": "SGN",
                "shl": "SHL",
                "shr": "SHR",
                "sin": "SIN",
                "sizeof": "SIZEOF",
                "sqr": "SQR",
                "stdcall": "STDCALL",
                "step": "STEP",
                "stop": "STOP",
                "str": "STR",
                "str$": "STR",
                "sub": "SUB",
                "tab": "TAB",
                "tan": "TAN",
                "then": "THEN",
                "to": "TO",
                "ubound": "UBOUND",
                "until": "UNTIL",
                "usr": "USR",
                "val": "VAL",
                "verify": "VERIFY",
                "wend": "WEND",
                "while": "WHILE",
                "xor": "XOR",
             */
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ABS", "Returns the absolute value of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ACS", "Returns the arccosine of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "AND", "Performs a bitwise AND operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "AS", "Declares a variable as a specific type."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "AT", "Sets the cursor position."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ASM", "Declares a block of assembly code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ASN", "Returns the arcsine of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ATN", "Returns the arctangent of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BAND", "Performs a bitwise AND operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BEEP", "Plays a beep sound."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BIN", "Returns a string representation of a number in binary format."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BNOT", "Performs a bitwise NOT operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BOLD", "Sets the text style to bold."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BOR", "Performs a bitwise OR operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BORDER", "Sets the border color."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BRIGHT", "Sets the text color to bright."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BXOR", "Performs a bitwise XOR operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BYREF", "Declares a parameter as passed by reference."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BYVAL", "Declares a parameter as passed by value."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "CAST", "Casts a variable to a different type."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "CHR", "Returns a string containing the character represented by the specified ASCII code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "CIRCLE", "Draws a circle."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "CLS", "Clears the screen."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "CODE", "Declares a block of assembly code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "CONST", "Declares a constant."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "CONTINUE", "Continues execution at the next iteration of a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "COS", "Returns the cosine of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "DATA", "Declares a block of data."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "DECLARE", "Declares a function or subroutine."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "DIM", "Declares a variable."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "DO", "Starts a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "DRAW", "Draws a line."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ELSE", "Starts an alternative block of code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ELSEIF", "Starts an alternative block of code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "END", "Ends a block of code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ENDIF", "Ends an alternative block of code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ERROR", "Returns the last error number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "EXIT", "Exits a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "EXP", "Returns e raised to the specified power."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "FASTCALL", "Declares a function or subroutine."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "FLASH", "Sets the text style to flash."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "FOR", "Starts a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "FUNCTION", "Declares a function."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "GO", "Jumps to a line number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "GOTO", "Jumps to a line number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "GOSUB", "Jumps to a subroutine."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "IF", "Starts an alternative block of code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "IN", "Returns the value of a port."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "INK", "Sets the ink color."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "INKEY", "Returns the ASCII code of the last key pressed."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "INT", "Returns the integer part of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "INVERSE", "Sets the text style to inverse."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ITALIC", "Sets the text style to italic."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "LBOUND", "Returns the lower bound of an array."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "LET", "Assigns a value to a variable."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "LEN", "Returns the length of a string."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "LN", "Returns the natural logarithm of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "LOAD", "Loads a file."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "LOOP", "Ends a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "MOD", "Returns the remainder of a division."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "NEXT", "Ends a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "NOT", "Performs a bitwise NOT operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "ON", "Starts a block of code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "OR", "Performs a bitwise OR operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "OUT", "Sets the value of a port."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "OVER", "Sets the text style to over."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "PAPER", "Sets the paper color."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "PAUSE", "Pauses execution."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "PEEK", "Returns the value of a memory address."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "PI", "Returns the value of pi."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "PLOT", "Draws a pixel."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "POKE", "Sets the value of a memory address."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "PRINT", "Prints a string."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "RANDOMIZE", "Initializes the random number generator."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "READ", "Reads a value from a data block."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "RESTORE", "Sets the data block pointer."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "RETURN", "Returns from a subroutine."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "RND", "Returns a random number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "SAVE", "Saves a file."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "SGN", "Returns the sign of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "SHL", "Performs a bitwise left shift operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "SHR", "Performs a bitwise right shift operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "SIN", "Returns the sine of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "SIZEOF", "Returns the size of a variable."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "SQR", "Returns the square root of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "STDCALL", "Declares a function or subroutine."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "STEP", "Sets the step value for a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "STOP", "Stops execution."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "STR", "Returns a string representation of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "SUB", "Declares a subroutine."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "TAB", "Sets the cursor position."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "TAN", "Returns the tangent of a number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "THEN", "Starts an alternative block of code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "TO", "Sets the end value for a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "UBOUND", "Returns the upper bound of an array."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "UNTIL", "Ends a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "USR", "Calls a machine code routine."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "VAL", "Returns the numeric value of a string."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "VERIFY", "Verifies a file."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "WEND", "Ends a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "WHILE", "Starts a loop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "XOR", "Performs a bitwise XOR operation."),


        };
        static ZXBasicCompletionData[] types = new ZXBasicCompletionData[]
        {
            /*
             * List of types
             * Original source:
             * # Data types
                "byte": "BYTE",
                "ubyte": "UBYTE",
                "integer": "INTEGER",
                "uinteger": "UINTEGER",
                "long": "LONG",
                "ulong": "ULONG",
                "fixed": "FIXED",
                "float": "FLOAT",
                "string": "STRING"
             */
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "BYTE", "8-bit signed integer."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "UBYTE", "8-bit unsigned integer."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "INTEGER", "16-bit signed integer."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "UINTEGER", "16-bit unsigned integer."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "LONG", "32-bit signed integer."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "ULONG", "32-bit unsigned integer."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "FIXED", "16-bit fixed-point number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "FLOAT", "40-bit floating-point number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Type, "STRING", "String."),

        };
        static ZXBasicCompletionData[] directives = new ZXBasicCompletionData[]
        {
            /*
             * List of directives
             * Original source:
             * INCLUDE = "INCLUDE"
                ONCE = "ONCE"
                DEFINE = "DEFINE"
                UNDEF = "UNDEF"
                IF = "IF"
                IFDEF = "IFDEF"
                IFNDEF = "IFNDEF"
                ELSE = "ELSE"
                ELIF = "ELIF"
                ENDIF = "ENDIF"
                INIT = "INIT"
                LINE = "LINE"
                REQUIRE = "REQUIRE"
                PRAGMA = "PRAGMA"
                ERROR = "ERROR"
                WARNING = "WARNING"
             */
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "INCLUDE", "Includes a file."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "ONCE", "Includes a file only once."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "DEFINE", "Defines a macro."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "UNDEF", "Undefines a macro."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "IF", "Starts a conditional block."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "IFDEF", "Starts a conditional block."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "IFNDEF", "Starts a conditional block."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "ELSE", "Starts an alternative block."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "ELIF", "Starts an alternative block."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "ENDIF", "Ends a conditional block."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "INIT", "Declares a block of assembly code."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "LINE", "Sets the current line number."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "REQUIRE", "Includes a file only once."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "PRAGMA", "Sets a compiler option."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "ERROR", "Raises an error."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Directive, "WARNING", "Raises a warning."),

        };

        static Regex matchType = new Regex("\\ as$", RegexOptions.IgnoreCase);

        protected override LanguageDefinitionBase? langDef => def;
        protected override IBrush? searchMarkerBrush => Brushes.Red;
        protected override AbstractFoldingStrategy? foldingStrategy => strategy;
        protected override char? commentChar => '\'';
        protected override Regex? regCancelBreakpoint => regCancel;
        protected override bool allowsBreakpoints => true;

        public ZXBasicEditor() : base() { }
        public ZXBasicEditor(string DocumentPath) : base(DocumentPath, ZXBasicDocument.Id) { }

        protected override IEnumerable<ICompletionData> ShouldComplete(IDocument Document, int Line, int Column, char? RequestedChar, bool ByRequest)
        {
            if (ByRequest)
            {
                List<ICompletionData> allData = new List<ICompletionData>();
                allData.AddRange(keywords);
                allData.AddRange(types);
                allData.AddRange(directives);
                return allData;
            }

            var line = Document.GetLineByNumber(Line);
            var text = Document.GetText(line.Offset, line.Length);

            string preText = text.Substring(0, Column);
            string postText = text.Substring(Column);

            if(string.IsNullOrWhiteSpace(preText))
            {
                if (RequestedChar == '#')
                    return directives;

                return keywords;
            }

            string trimmed = preText.Trim();

            if (matchType.IsMatch(trimmed))
            {
                return types;
            }

            return null;
        }
    }
}
