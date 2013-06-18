namespace Microsoft.Build.Shared.LanguageParser
{
    using System;
    using System.IO;

    internal sealed class CSharpTokenCharReader : TokenCharReader
    {
        internal CSharpTokenCharReader(Stream binaryStream, bool forceANSI) : base(binaryStream, forceANSI)
        {
        }

        internal bool MatchRegularStringLiteral()
        {
            return (((base.CurrentCharacter != '"') && (base.CurrentCharacter != '\\')) && !TokenChar.IsNewLine(base.CurrentCharacter));
        }

        internal bool SinkLongIntegerSuffix()
        {
            if ((base.CurrentCharacter == 'U') || (base.CurrentCharacter == 'u'))
            {
                base.Skip();
                if ((base.CurrentCharacter == 'L') || (base.CurrentCharacter == 'l'))
                {
                    base.Skip();
                }
            }
            else if ((base.CurrentCharacter == 'L') || (base.CurrentCharacter == 'l'))
            {
                base.Skip();
                if ((base.CurrentCharacter == 'U') || (base.CurrentCharacter == 'u'))
                {
                    base.Skip();
                }
            }
            return true;
        }

        internal bool SinkMultipleWhiteSpace()
        {
            int num = 0;
            while (!base.EndOfLines && char.IsWhiteSpace(base.CurrentCharacter))
            {
                base.Skip();
                num++;
            }
            return (num > 0);
        }

        internal bool SinkOperatorOrPunctuator()
        {
            if ("{}[]().,:;+-*/%&|^!~=<>?".IndexOf(base.CurrentCharacter) == -1)
            {
                return false;
            }
            base.Skip();
            return true;
        }

        internal bool SinkStringEscape()
        {
            switch (base.CurrentCharacter)
            {
                case '0':
                case 'U':
                case '"':
                case '\'':
                case 'a':
                case 'b':
                case '\\':
                case 'n':
                case 'r':
                case 't':
                case 'u':
                case 'v':
                case 'x':
                case 'f':
                    base.Skip();
                    return true;
            }
            return false;
        }
    }
}

