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
        static ZXBasicCompletionData[] basicKeywords = new ZXBasicCompletionData[] 
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
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "END", "Terminates the program."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "END ASM", "Ends a block of assembler."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "END FUNCTION", "Ends a function."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "END SUB", "Ends a subfunction."),
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
        static ZXBasicCompletionData[] basicModifiers = new ZXBasicCompletionData[] 
        {
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BYREF", "Declares a parameter as passed by reference."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Keyword, "BYVAL", "Declares a parameter as passed by value.")
        };
        static ZXBasicCompletionData[] keywords;

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

        static ZXBasicCompletionData[] assemblerKeywords = new ZXBasicCompletionData[] 
        {
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "ADC", "Add with carry."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "ADD", "Add."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "AND", "Logical AND."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "BIT", "Test bit."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "CALL", "Call routine."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "CCF", "Complement carry flag."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "CP", "Compare."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "CPD", "Compare with decrement."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "CPDR", "Compare with decrement and repeat."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "CPI", "Compare with increment."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "CPIR", "Compare with increment and repeat."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "CPL", "Complement A."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "DAA", "Decimal adjust A."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "DEC", "Decrement."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "DI", "Disable interrupts."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "DJNZ", "Decrement B and jump if not zero."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "EI", "Enable interrupts."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "EX", "Exchange registers."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "EXX", "Exchange registers multiple."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "HALT", "Halt."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IM", "Interrupt mode."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IN", "Input from port."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "INC", "Increment."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IND", "Input from port with decrement."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "INDR", "Input from port with decrement and repeat."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "INI", "Input from port with increment."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "INIR", "Input from port with increment and repeat."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "JP", "Jump."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "JR", "Jump relative."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "LD", "Load."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "LDD", "Load with decrement."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "LDDR", "Load with decrement and repeat."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "LDI", "Load with increment."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "LDIR", "Load with increment and repeat."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "NEG", "Negate."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "NOP", "No operation."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "OR", "Logical OR."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "OTDR", "Output to port with decrement and repeat."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "OTIR", "Output to port with increment and repeat."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "OUT", "Output to port."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "OUTD", "Output to port with decrement."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "OUTI", "Output to port with increment."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "POP", "Pop."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "PUSH", "Push."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RES", "Reset bit."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RET", "Return."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RETI", "Return from interrupt."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RETN", "Return from non-maskable interrupt."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RL", "Rotate left."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RLA", "Rotate left through carry."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RLC", "Rotate left with carry."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RLCA", "Rotate left with carry."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RLD", "Rotate left digit."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RR", "Rotate right."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RRA", "Rotate right through carry."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RRC", "Rotate right with carry."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RRCA", "Rotate right with carry."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RRD", "Rotate right digit."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "RST", "Restart."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SBC", "Subtract with carry."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SCF", "Set carry flag."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SET", "Set bit."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SLA", "Shift left arithmetic."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SLL", "Shift left logical."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SRA", "Shift right arithmetic."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SRL", "Shift right logical."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SUB", "Subtract."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "XOR", "Logical XOR."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "DB", "Define byte."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "DW", "Define word."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "DS", "Define space."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "EQU", "Define symbol."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "END", "End assembly."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "ORG", "Set origin."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "ASM", "End assembly."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "ALIGN", "Align section."),
        };
        static ZXBasicCompletionData[] assemblerRegisters = new ZXBasicCompletionData[]
        {
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "AF", "Register pair."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "BC", "Register pair."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "DE", "Register pair."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "HL", "Register pair."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IX", "Index register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IY", "Index register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "SP", "Stack pointer."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "PC", "Program counter."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "A", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "B", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "C", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "D", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "E", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "H", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "L", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "I", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "R", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "F", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IXH", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IXL", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IYH", "Register."),
            new ZXBasicCompletionData(ZXBasicCompletionType.Assembler, "IYL", "Register."),
        };
        static ZXBasicCompletionData[] assembler;

        static Regex regType = new Regex("\\ as$", RegexOptions.IgnoreCase);
        static Regex regDots = new Regex(":\\s*$", RegexOptions.IgnoreCase);
        static Regex regPar = new Regex("((^|\\ )sub|(^|\\ )function).*?\\(\\s*([^\\)]*,)?$", RegexOptions.IgnoreCase);
        static Regex regStartAsm = new Regex("^\\s*asm\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regEndAsm = new Regex("^\\s*end\\s+asm\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regStartMultiComment = new Regex("/'", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regEndMultiComment = new Regex("'/", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        static Regex regComment = new Regex("(^|\\s|:)'", RegexOptions.IgnoreCase);
        static Regex regCommentAsm = new Regex(";", RegexOptions.IgnoreCase);
        static Regex regEnd = new Regex("^\\s*end\\s*$", RegexOptions.IgnoreCase);

        static ZXBasicEditor()
        {
            List<ZXBasicCompletionData> asmMerged = new List<ZXBasicCompletionData>();
            asmMerged.AddRange(assemblerKeywords);
            asmMerged.AddRange(assemblerRegisters);
            assembler = asmMerged.ToArray();

            List<ZXBasicCompletionData> basicMerged = new List<ZXBasicCompletionData>();
            basicMerged.AddRange(basicKeywords);
            basicMerged.AddRange(basicModifiers);
            keywords = basicMerged.ToArray();
        }



        protected override LanguageDefinitionBase? langDef => def;
        protected override IBrush? searchMarkerBrush => Brushes.Red;
        protected override AbstractFoldingStrategy? foldingStrategy => strategy;
        protected override char? commentChar => '\'';
        protected override Regex? regCancelBreakpoint => regCancel;
        protected override bool allowsBreakpoints => true;
        protected override string? HelpUrl => "https://zxbasic.readthedocs.io/en/docs/search.html?q={0}";
        public ZXBasicEditor() : base() { }
        public ZXBasicEditor(string DocumentPath) : base(DocumentPath, ZXBasicDocument.Id) { }

        protected override IEnumerable<ICompletionData>? ShouldComplete(IDocument Document, int Line, int Column, char? RequestedChar, bool ByRequest)
        {
            var line = Document.GetLineByNumber(Line);
            var text = Document.GetText(line.Offset, line.Length);

            string preText = text.Substring(0, Column);
            string postText = text.Substring(Column);

            var context = GetContext(Document, line.Offset + Column);

            if (ByRequest)
            {

                switch (context)
                {
                    case ContextType.Basic:

                        if (regComment.IsMatch(preText))
                            return null;

                        List<ICompletionData> allData = new List<ICompletionData>();
                        allData.AddRange(keywords);
                        allData.AddRange(types);
                        allData.AddRange(directives);
                        return allData;

                    case ContextType.Assembler:

                        if (regCommentAsm.IsMatch(preText))
                            return null;
                        
                        return assembler;

                    case ContextType.Comment:
                        return null;
                }

                
            }

            if (context == ContextType.Comment)
                return null;

            string trimmed = preText.Trim();

            if(context == ContextType.Assembler)
            {
                if (!char.IsLetter(RequestedChar ?? ' ') || regCommentAsm.IsMatch(trimmed))
                    return null;

                if (string.IsNullOrWhiteSpace(trimmed))
                    PrioritizeAssemblerKeywords();
                else
                    PrioritizeAssemblerRegisters();

                return assembler;
            }


            if (string.IsNullOrWhiteSpace(preText))
            {
                if (RequestedChar == '#')
                    return directives;

                if (!char.IsLetter(RequestedChar ?? ' '))
                {
                    return null;
                }

                if (regComment.IsMatch(trimmed))
                    return null;

                PrioritizeBasicKeywords();
                return keywords;
            }

            if (context == ContextType.Assembler)
                return null;

            if (regType.IsMatch(trimmed))
            {
                return types;
            }

            if(regDots.IsMatch(trimmed))
            {
                PrioritizeBasicKeywords();
                return keywords;
            }

            if(regPar.IsMatch(trimmed))
            {
                PrioritizeBasicModifiers();
                return keywords;
            }

            if(regEnd.IsMatch(trimmed))
            {
                PrioritizeBasicKeywords();
                return keywords;
            }

            return null;
        }

        private void PrioritizeBasicModifiers()
        {
            foreach (var item in basicKeywords)
                item.Priority = 5;

            foreach (var item in basicModifiers)
                item.Priority = 10;
        }

        private void PrioritizeBasicKeywords()
        {
            foreach (var item in basicKeywords)
                item.Priority = 10;

            foreach (var item in basicModifiers)
                item.Priority = 5;
        }

        private void PrioritizeAssemblerRegisters()
        {
            foreach (var item in assemblerKeywords)
                item.Priority = 5;

            foreach (var item in assemblerRegisters)
                item.Priority = 10;
        }

        private void PrioritizeAssemblerKeywords()
        {
            foreach (var item in assemblerKeywords)
                item.Priority = 10;

            foreach(var item in assemblerRegisters)
                item.Priority = 5;
        }

        private ContextType GetContext(IDocument Document, int Offset)
        {
            var text = Document.GetText(0, Offset);

            var lastStartMultiComment = regStartMultiComment.Matches(text).LastOrDefault();
            var lastEndMultiComment = regEndMultiComment.Matches(text).LastOrDefault();

            if(lastStartMultiComment != null && (lastEndMultiComment == null || lastStartMultiComment.Index > lastEndMultiComment.Index))
            {
                return ContextType.Comment;
            }

            var lastAsm = regStartAsm.Matches(text).LastOrDefault();
            var lastEndAsm = regEndAsm.Matches(text).LastOrDefault();
            
            if(lastAsm != null && (lastEndAsm == null || lastAsm.Index > lastEndAsm.Index))
            {
                return ContextType.Assembler;
            }

            return ContextType.Basic;
        }

        enum ContextType
        {
            Basic,
            Assembler,
            Comment
        }
    }
}
