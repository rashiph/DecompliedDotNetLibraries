namespace Microsoft.Build.Shared.LanguageParser
{
    using System;
    using System.Globalization;
    using System.IO;

    internal sealed class VisualBasicTokenEnumerator : TokenEnumerator
    {
        private static readonly string[] keywordList = new string[] { 
            "ADDHANDLER", "ADDRESSOF", "ANDALSO", "ALIAS", "AND", "ANSI", "AS", "ASSEMBLY", "AUTO", "BOOLEAN", "BYREF", "BYTE", "BYVAL", "CALL", "CASE", "CATCH", 
            "CBOOL", "CBYTE", "CCHAR", "CDATE", "CDEC", "CDBL", "CHAR", "CINT", "CLASS", "CLNG", "COBJ", "CONST", "CONTINUE", "CSBYTE", "CSHORT", "CSNG", 
            "CSTR", "CTYPE", "CUINT", "CULNG", "CUSHORT", "DATE", "DECIMAL", "DECLARE", "DEFAULT", "DELEGATE", "DIM", "DIRECTCAST", "DO", "DOUBLE", "EACH", "ELSE", 
            "ELSEIF", "END", "ENDIF", "ENUM", "ERASE", "ERROR", "EVENT", "EXIT", "FALSE", "FINALLY", "FOR", "FRIEND", "FUNCTION", "GET", "GETTYPE", "GLOBAL", 
            "GOSUB", "GOTO", "HANDLES", "IF", "IMPLEMENTS", "IMPORTS", "IN", "INHERITS", "INTEGER", "INTERFACE", "IS", "ISNOT", "LET", "LIB", "LIKE", "LONG", 
            "LOOP", "ME", "MOD", "MODULE", "MUSTINHERIT", "MUSTOVERRIDE", "MYBASE", "MYCLASS", "NAMESPACE", "NARROWING", "NEW", "NEXT", "NOT", "NOTHING", "NOTINHERITABLE", "NOTOVERRIDABLE", 
            "OBJECT", "OF", "ON", "OPERATOR", "OPTION", "OPTIONAL", "OR", "ORELSE", "OVERLOADS", "OVERRIDABLE", "OVERRIDES", "PARAMARRAY", "PARTIAL", "PRESERVE", "PRIVATE", "PROPERTY", 
            "PROTECTED", "PUBLIC", "RAISEEVENT", "READONLY", "REDIM", "REM", "REMOVEHANDLER", "RESUME", "RETURN", "SBYTE", "SELECT", "SET", "SHADOWS", "SHARED", "SHORT", "SINGLE", 
            "STATIC", "STEP", "STOP", "STRING", "STRUCTURE", "SUB", "SYNCLOCK", "THEN", "THROW", "TO", "TRUE", "TRY", "TRYCAST", "TYPEOF", "UNICODE", "UINTEGER", 
            "ULONG", "UNTIL", "USHORT", "USING", "VARIANT", "WEND", "WHEN", "WHILE", "WIDENING", "WITH", "WITHEVENTS", "WRITEONLY", "XOR"
         };
        private VisualBasicTokenCharReader reader;

        internal VisualBasicTokenEnumerator(Stream binaryStream, bool forceANSI)
        {
            this.reader = new VisualBasicTokenCharReader(binaryStream, forceANSI);
        }

        internal override bool FindNextToken()
        {
            int position = this.reader.Position;
            if (this.reader.SinkWhiteSpace())
            {
                while (this.reader.SinkWhiteSpace())
                {
                }
                if (this.reader.SinkLineContinuationCharacter())
                {
                    int num2 = this.reader.Position - 1;
                    while (this.reader.SinkWhiteSpace())
                    {
                    }
                    int num3 = 0;
                    while (this.reader.SinkNewLine())
                    {
                        num3++;
                    }
                    if (num3 > 0)
                    {
                        base.current = new VisualBasicTokenizer.LineContinuationToken();
                        return true;
                    }
                    this.reader.Position = num2;
                }
                base.current = new WhitespaceToken();
                return true;
            }
            if (this.reader.SinkNewLine())
            {
                base.current = new VisualBasicTokenizer.LineTerminatorToken();
                return true;
            }
            if (this.reader.SinkLineCommentStart())
            {
                this.reader.SinkToEndOfLine();
                base.current = new CommentToken();
                return true;
            }
            if ((this.reader.CurrentCharacter == '[') || this.reader.MatchNextIdentifierStart())
            {
                string str3;
                bool flag = false;
                if (this.reader.CurrentCharacter == '[')
                {
                    flag = true;
                    this.reader.SinkCharacter();
                    if (!this.reader.SinkIdentifierStart())
                    {
                        base.current = new ExpectedIdentifierToken();
                        return true;
                    }
                }
                while (this.reader.SinkIdentifierPart())
                {
                }
                if (flag)
                {
                    if (!this.reader.Sink("]"))
                    {
                        base.current = new ExpectedIdentifierToken();
                        return true;
                    }
                }
                else
                {
                    this.reader.SinkTypeCharacter();
                }
                string currentMatchedString = this.reader.GetCurrentMatchedString(position);
                switch (currentMatchedString)
                {
                    case "_":
                    case "[_]":
                    case "[]":
                        base.current = new ExpectedIdentifierToken();
                        return true;
                }
                string str2 = currentMatchedString.ToUpper(CultureInfo.InvariantCulture);
                if (((str3 = str2) == null) || ((str3 != "FALSE") && (str3 != "TRUE")))
                {
                    if (Array.IndexOf<string>(keywordList, str2) >= 0)
                    {
                        base.current = new KeywordToken();
                        return true;
                    }
                    base.current = new IdentifierToken();
                    if (flag)
                    {
                        base.current.InnerText = currentMatchedString.Substring(1, currentMatchedString.Length - 2);
                    }
                    return true;
                }
                base.current = new BooleanLiteralToken();
                return true;
            }
            if (this.reader.SinkHexIntegerPrefix())
            {
                if (!this.reader.SinkMultipleHexDigits())
                {
                    base.current = new ExpectedValidHexDigitToken();
                    return true;
                }
                this.reader.SinkIntegerSuffix();
                base.current = new HexIntegerLiteralToken();
                return true;
            }
            if (this.reader.SinkOctalIntegerPrefix())
            {
                if (!this.reader.SinkMultipleOctalDigits())
                {
                    base.current = new VisualBasicTokenizer.ExpectedValidOctalDigitToken();
                    return true;
                }
                this.reader.SinkIntegerSuffix();
                base.current = new VisualBasicTokenizer.OctalIntegerLiteralToken();
                return true;
            }
            if (this.reader.SinkMultipleDecimalDigits())
            {
                this.reader.SinkDecimalIntegerSuffix();
                base.current = new DecimalIntegerLiteralToken();
                return true;
            }
            if (this.reader.CurrentCharacter == '#')
            {
                if (this.reader.SinkIgnoreCase("#if"))
                {
                    base.current = new OpenConditionalDirectiveToken();
                }
                else if (this.reader.SinkIgnoreCase("#end if"))
                {
                    base.current = new CloseConditionalDirectiveToken();
                }
                else
                {
                    base.current = new PreprocessorToken();
                }
                this.reader.SinkToEndOfLine();
                return true;
            }
            if (this.reader.SinkSeparatorCharacter())
            {
                base.current = new VisualBasicTokenizer.SeparatorToken();
                return true;
            }
            if (this.reader.SinkOperator())
            {
                base.current = new OperatorToken();
                return true;
            }
            if (this.reader.Sink("\""))
            {
                while (this.reader.Sink("\"\"") || (!this.reader.EndOfLines && (this.reader.SinkCharacter() != '"')))
                {
                }
                if (this.reader.EndOfLines)
                {
                    base.current = new Microsoft.Build.Shared.LanguageParser.EndOfFileInsideStringToken();
                    return true;
                }
                base.current = new StringLiteralToken();
                return true;
            }
            this.reader.SinkCharacter();
            base.current = new UnrecognizedToken();
            return true;
        }

        internal override TokenCharReader Reader
        {
            get
            {
                return this.reader;
            }
        }
    }
}

