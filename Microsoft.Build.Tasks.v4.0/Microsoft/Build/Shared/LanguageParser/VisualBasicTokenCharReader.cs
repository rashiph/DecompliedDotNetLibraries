namespace Microsoft.Build.Shared.LanguageParser
{
    using System;
    using System.IO;

    internal sealed class VisualBasicTokenCharReader : TokenCharReader
    {
        internal VisualBasicTokenCharReader(Stream binaryStream, bool forceANSI) : base(binaryStream, forceANSI)
        {
        }

        internal bool SinkDecimalIntegerSuffix()
        {
            switch (base.CurrentCharacter)
            {
                case 'I':
                case 'L':
                case '!':
                case '#':
                case '%':
                case '&':
                case '@':
                case 'l':
                case 's':
                case 'S':
                case 'i':
                    base.Skip();
                    return true;
            }
            return true;
        }

        internal bool SinkHexIntegerPrefix()
        {
            return base.SinkIgnoreCase("&H");
        }

        internal bool SinkIntegerSuffix()
        {
            switch (base.CurrentCharacter)
            {
                case 'i':
                case 'l':
                case 's':
                case 'I':
                case 'L':
                case 'S':
                    base.Skip();
                    return true;
            }
            return true;
        }

        internal bool SinkLineCommentStart()
        {
            if (base.Sink("'"))
            {
                return true;
            }
            int position = base.Position;
            if (base.SinkIgnoreCase("rem"))
            {
                if (this.SinkWhiteSpace())
                {
                    return true;
                }
                base.Position = position;
            }
            return false;
        }

        internal bool SinkLineContinuationCharacter()
        {
            if (base.CurrentCharacter == '_')
            {
                base.Skip();
                return true;
            }
            return false;
        }

        internal bool SinkMultipleOctalDigits()
        {
            int num = 0;
            while (TokenChar.IsOctalDigit(base.CurrentCharacter))
            {
                num++;
                base.Skip();
            }
            return (num > 0);
        }

        internal bool SinkOctalIntegerPrefix()
        {
            return base.SinkIgnoreCase("&O");
        }

        internal bool SinkOperator()
        {
            if (@"&|*+-/\^<=>".IndexOf(base.CurrentCharacter) == -1)
            {
                return false;
            }
            base.Skip();
            return true;
        }

        internal bool SinkSeparatorCharacter()
        {
            if ((((base.CurrentCharacter != '(') && (base.CurrentCharacter != ')')) && ((base.CurrentCharacter != '!') && (base.CurrentCharacter != '#'))) && (((base.CurrentCharacter != ',') && (base.CurrentCharacter != '.')) && (((base.CurrentCharacter != ':') && (base.CurrentCharacter != '{')) && (base.CurrentCharacter != '}'))))
            {
                return false;
            }
            base.Skip();
            return true;
        }

        internal bool SinkTypeCharacter()
        {
            if ("%&@!#$".IndexOf(base.CurrentCharacter) == -1)
            {
                return false;
            }
            base.Skip();
            return true;
        }

        internal bool SinkWhiteSpace()
        {
            if (char.IsWhiteSpace(base.CurrentCharacter) && !TokenChar.IsNewLine(base.CurrentCharacter))
            {
                base.Skip();
                return true;
            }
            return false;
        }
    }
}

