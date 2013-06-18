namespace Microsoft.Build.Shared.LanguageParser
{
    using System;
    using System.IO;

    internal sealed class CSharpTokenEnumerator : TokenEnumerator
    {
        private static readonly string[] keywordList = new string[] { 
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", 
            "do", "double", "else", "enum", "event", "explicit", "extern", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", 
            "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "object", "operator", "out", "override", "params", "private", "protected", "public", 
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "try", "typeof", 
            "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
         };
        private CSharpTokenCharReader reader;

        internal CSharpTokenEnumerator(Stream binaryStream, bool forceANSI)
        {
            this.reader = new CSharpTokenCharReader(binaryStream, forceANSI);
        }

        internal override bool FindNextToken()
        {
            int position = this.reader.Position;
            if (this.reader.SinkMultipleWhiteSpace())
            {
                base.current = new WhitespaceToken();
                return true;
            }
            if (this.reader.Sink("//"))
            {
                this.reader.SinkToEndOfLine();
                base.current = new CommentToken();
                return true;
            }
            if (this.reader.Sink("/*"))
            {
                this.reader.SinkUntil("*/");
                if (this.reader.EndOfLines)
                {
                    base.current = new CSharpTokenizer.EndOfFileInsideCommentToken();
                    return true;
                }
                base.current = new CommentToken();
                return true;
            }
            if (this.reader.Sink("'"))
            {
                while (this.reader.CurrentCharacter != '\'')
                {
                    this.reader.Sink(@"\");
                    this.reader.SinkCharacter();
                }
                this.reader.SinkCharacter();
                base.current = new CSharpTokenizer.CharLiteralToken();
                return true;
            }
            if (this.reader.Sink("@\""))
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
                base.current.InnerText = this.reader.GetCurrentMatchedString(position).Substring(1);
                return true;
            }
            if (this.reader.Sink("\""))
            {
                while ((this.reader.CurrentCharacter == '\\') || this.reader.MatchRegularStringLiteral())
                {
                    if ((this.reader.SinkCharacter() == '\\') && !this.reader.SinkStringEscape())
                    {
                        this.reader.SinkCharacter();
                        base.current = new CSharpTokenizer.UnrecognizedStringEscapeToken();
                        return true;
                    }
                }
                if (TokenChar.IsNewLine(this.reader.CurrentCharacter))
                {
                    base.current = new CSharpTokenizer.NewlineInsideStringToken();
                    return true;
                }
                this.reader.SinkCharacter();
                base.current = new StringLiteralToken();
                return true;
            }
            if ((this.reader.CurrentCharacter == '@') || this.reader.MatchNextIdentifierStart())
            {
                if (this.reader.CurrentCharacter == '@')
                {
                    this.reader.SinkCharacter();
                }
                if (!this.reader.SinkIdentifierStart())
                {
                    base.current = new ExpectedIdentifierToken();
                    return true;
                }
                while (this.reader.SinkIdentifierPart())
                {
                }
                string currentMatchedString = this.reader.GetCurrentMatchedString(position);
                switch (currentMatchedString)
                {
                    case "false":
                    case "true":
                        base.current = new BooleanLiteralToken();
                        return true;

                    case "null":
                        base.current = new CSharpTokenizer.NullLiteralToken();
                        return true;
                }
                if (Array.IndexOf<string>(keywordList, currentMatchedString) >= 0)
                {
                    base.current = new KeywordToken();
                    return true;
                }
                string str2 = this.reader.GetCurrentMatchedString(position);
                if (str2.StartsWith("@", StringComparison.Ordinal))
                {
                    str2 = str2.Substring(1);
                }
                base.current = new IdentifierToken();
                base.current.InnerText = str2;
                return true;
            }
            if (this.reader.Sink("{"))
            {
                base.current = new CSharpTokenizer.OpenScopeToken();
                return true;
            }
            if (this.reader.Sink("}"))
            {
                base.current = new CSharpTokenizer.CloseScopeToken();
                return true;
            }
            if (this.reader.SinkIgnoreCase("0x"))
            {
                if (!this.reader.SinkMultipleHexDigits())
                {
                    base.current = new ExpectedValidHexDigitToken();
                    return true;
                }
                this.reader.SinkLongIntegerSuffix();
                base.current = new HexIntegerLiteralToken();
                return true;
            }
            if (this.reader.SinkMultipleDecimalDigits())
            {
                this.reader.SinkLongIntegerSuffix();
                base.current = new DecimalIntegerLiteralToken();
                return true;
            }
            if (this.reader.SinkOperatorOrPunctuator())
            {
                base.current = new OperatorOrPunctuatorToken();
                return true;
            }
            if (this.reader.CurrentCharacter == '#')
            {
                if (this.reader.Sink("#if"))
                {
                    base.current = new OpenConditionalDirectiveToken();
                }
                else if (this.reader.Sink("#endif"))
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

