namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class Scanner
    {
        private int currentPosition;
        private TokenID currentToken;
        private string inputString;
        private int inputStringLength;
        private static Dictionary<string, KeywordInfo> keywordMap = CreateKeywordMap();
        private int tokenStartPosition;
        private object tokenValue;

        internal Scanner(string inputString)
        {
            this.inputString = inputString;
            this.inputStringLength = inputString.Length;
        }

        internal static void AddKeywordsStartingWith(char upperFirstCharacter, ArrayList list)
        {
            foreach (KeyValuePair<string, KeywordInfo> pair in keywordMap)
            {
                if (char.ToUpper(pair.Key[0], CultureInfo.InvariantCulture) == upperFirstCharacter)
                {
                    list.Add(new IntellisenseKeyword(pair.Key));
                }
            }
        }

        private static Dictionary<string, KeywordInfo> CreateKeywordMap()
        {
            Dictionary<string, KeywordInfo> dictionary = new Dictionary<string, KeywordInfo>(0x1b);
            dictionary.Add("mod", new KeywordInfo(TokenID.Modulus));
            dictionary.Add("and", new KeywordInfo(TokenID.And));
            dictionary.Add("or", new KeywordInfo(TokenID.Or));
            dictionary.Add("not", new KeywordInfo(TokenID.Not));
            dictionary.Add("true", new KeywordInfo(TokenID.True, true));
            dictionary.Add("false", new KeywordInfo(TokenID.False, false));
            dictionary.Add("null", new KeywordInfo(TokenID.Null, null));
            dictionary.Add("nothing", new KeywordInfo(TokenID.Null, null));
            dictionary.Add("this", new KeywordInfo(TokenID.This));
            dictionary.Add("me", new KeywordInfo(TokenID.This));
            dictionary.Add("in", new KeywordInfo(TokenID.In));
            dictionary.Add("out", new KeywordInfo(TokenID.Out));
            dictionary.Add("ref", new KeywordInfo(TokenID.Ref));
            dictionary.Add("halt", new KeywordInfo(TokenID.Halt));
            dictionary.Add("update", new KeywordInfo(TokenID.Update));
            dictionary.Add("new", new KeywordInfo(TokenID.New));
            dictionary.Add("char", new KeywordInfo(TokenID.TypeName, typeof(char)));
            dictionary.Add("byte", new KeywordInfo(TokenID.TypeName, typeof(byte)));
            dictionary.Add("sbyte", new KeywordInfo(TokenID.TypeName, typeof(sbyte)));
            dictionary.Add("short", new KeywordInfo(TokenID.TypeName, typeof(short)));
            dictionary.Add("ushort", new KeywordInfo(TokenID.TypeName, typeof(ushort)));
            dictionary.Add("int", new KeywordInfo(TokenID.TypeName, typeof(int)));
            dictionary.Add("uint", new KeywordInfo(TokenID.TypeName, typeof(uint)));
            dictionary.Add("long", new KeywordInfo(TokenID.TypeName, typeof(long)));
            dictionary.Add("ulong", new KeywordInfo(TokenID.TypeName, typeof(ulong)));
            dictionary.Add("float", new KeywordInfo(TokenID.TypeName, typeof(float)));
            dictionary.Add("double", new KeywordInfo(TokenID.TypeName, typeof(double)));
            dictionary.Add("decimal", new KeywordInfo(TokenID.TypeName, typeof(decimal)));
            dictionary.Add("bool", new KeywordInfo(TokenID.TypeName, typeof(bool)));
            dictionary.Add("string", new KeywordInfo(TokenID.TypeName, typeof(string)));
            dictionary.Add("object", new KeywordInfo(TokenID.TypeName, typeof(object)));
            return dictionary;
        }

        private char CurrentChar()
        {
            if (this.currentPosition >= this.inputStringLength)
            {
                return '\0';
            }
            return this.inputString[this.currentPosition];
        }

        private static int HexValue(char ch)
        {
            int num = -1;
            if (char.IsDigit(ch))
            {
                return (ch - '0');
            }
            if ((ch >= 'a') && (ch <= 'f'))
            {
                return ((ch - 'a') + 10);
            }
            if ((ch >= 'A') && (ch <= 'F'))
            {
                num = (ch - 'A') + 10;
            }
            return num;
        }

        private char NextChar()
        {
            if (this.currentPosition == (this.inputStringLength - 1))
            {
                this.currentPosition++;
                return '\0';
            }
            this.currentPosition++;
            return this.CurrentChar();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private Token NextToken()
        {
            string message = null;
            Token token;
            TokenID unknown = TokenID.Unknown;
            char ch = this.CurrentChar();
            ch = this.SkipWhitespace(ch);
            if (ch == '\0')
            {
                return new Token(TokenID.EndOfInput, this.currentPosition, null);
            }
            this.tokenStartPosition = this.currentPosition;
            this.tokenValue = null;
            if (char.IsDigit(ch))
            {
                unknown = this.ScanNumber();
            }
            else if (char.IsLetter(ch))
            {
                unknown = this.ScanKeywordOrIdentifier();
            }
            else
            {
                switch (ch)
                {
                    case '!':
                        unknown = TokenID.Not;
                        if (this.NextChar() == '=')
                        {
                            this.NextChar();
                            unknown = TokenID.NotEqual;
                        }
                        goto Label_0397;

                    case '"':
                        unknown = this.ScanStringLiteral();
                        this.NextChar();
                        goto Label_0397;

                    case '%':
                        unknown = TokenID.Modulus;
                        this.NextChar();
                        goto Label_0397;

                    case '&':
                        unknown = TokenID.BitAnd;
                        if (this.NextChar() == '&')
                        {
                            this.NextChar();
                            unknown = TokenID.And;
                        }
                        goto Label_0397;

                    case '\'':
                        unknown = this.ScanCharacterLiteral();
                        this.NextChar();
                        goto Label_0397;

                    case '(':
                        unknown = TokenID.LParen;
                        this.NextChar();
                        goto Label_0397;

                    case ')':
                        unknown = TokenID.RParen;
                        this.NextChar();
                        goto Label_0397;

                    case '*':
                        unknown = TokenID.Multiply;
                        this.NextChar();
                        goto Label_0397;

                    case '+':
                        unknown = TokenID.Plus;
                        this.NextChar();
                        goto Label_0397;

                    case ',':
                        unknown = TokenID.Comma;
                        this.NextChar();
                        goto Label_0397;

                    case '-':
                        unknown = TokenID.Minus;
                        this.NextChar();
                        goto Label_0397;

                    case '.':
                        unknown = TokenID.Dot;
                        if (!char.IsDigit(this.PeekNextChar()))
                        {
                            this.NextChar();
                        }
                        else
                        {
                            unknown = this.ScanDecimal();
                        }
                        goto Label_0397;

                    case '/':
                        unknown = TokenID.Divide;
                        this.NextChar();
                        goto Label_0397;

                    case ';':
                        unknown = TokenID.Semicolon;
                        this.NextChar();
                        goto Label_0397;

                    case '<':
                        unknown = TokenID.Less;
                        switch (this.NextChar())
                        {
                            case '=':
                                this.NextChar();
                                unknown = TokenID.LessEqual;
                                break;

                            case '>':
                                this.NextChar();
                                unknown = TokenID.NotEqual;
                                break;
                        }
                        goto Label_0397;

                    case '=':
                        unknown = TokenID.Assign;
                        if (this.NextChar() == '=')
                        {
                            this.NextChar();
                            unknown = TokenID.Equal;
                        }
                        goto Label_0397;

                    case '>':
                        unknown = TokenID.Greater;
                        if (this.NextChar() == '=')
                        {
                            this.NextChar();
                            unknown = TokenID.GreaterEqual;
                        }
                        goto Label_0397;

                    case '@':
                        ch = this.NextChar();
                        if (ch != '"')
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidCharacter, new object[] { ch });
                            throw new RuleSyntaxException(0x17b, message, this.tokenStartPosition);
                        }
                        unknown = this.ScanVerbatimStringLiteral();
                        this.NextChar();
                        goto Label_0397;

                    case '[':
                        unknown = TokenID.LBracket;
                        this.NextChar();
                        goto Label_0397;

                    case ']':
                        unknown = TokenID.RBracket;
                        this.NextChar();
                        goto Label_0397;

                    case '_':
                        unknown = this.ScanKeywordOrIdentifier();
                        goto Label_0397;

                    case '{':
                        unknown = TokenID.LCurlyBrace;
                        this.NextChar();
                        goto Label_0397;

                    case '|':
                        unknown = TokenID.BitOr;
                        if (this.NextChar() == '|')
                        {
                            this.NextChar();
                            unknown = TokenID.Or;
                        }
                        goto Label_0397;

                    case '}':
                        unknown = TokenID.RCurlyBrace;
                        this.NextChar();
                        goto Label_0397;
                }
                this.NextChar();
                message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidCharacter, new object[] { ch });
                throw new RuleSyntaxException(0x17b, message, this.tokenStartPosition);
            }
        Label_0397:
            token = new Token(unknown, this.tokenStartPosition, this.tokenValue);
            this.currentToken = unknown;
            return token;
        }

        private char PeekNextChar()
        {
            if (this.currentPosition == (this.inputStringLength - 1))
            {
                return '\0';
            }
            int num = this.currentPosition + 1;
            if (num >= this.inputStringLength)
            {
                return '\0';
            }
            return this.inputString[num];
        }

        private char ScanCharacter(out bool isEscaped)
        {
            isEscaped = false;
            char ch = this.NextChar();
            if (ch != '\\')
            {
                return ch;
            }
            isEscaped = true;
            ch = this.NextChar();
            switch (ch)
            {
                case '"':
                case '\'':
                    return ch;

                case '0':
                    return '\0';

                case 'n':
                    return '\n';

                case 'r':
                    return '\r';

                case 't':
                    return '\t';

                case 'u':
                    return this.ScanUnicodeEscapeSequence();

                case 'v':
                    return '\v';

                case 'f':
                    return '\f';

                case 'a':
                    return '\a';

                case 'b':
                    return '\b';

                case '\\':
                    return ch;
            }
            string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidEscapeSequence, new object[] { ch });
            throw new RuleSyntaxException(380, message, this.currentPosition - 1);
        }

        private TokenID ScanCharacterLiteral()
        {
            bool isEscaped = false;
            char ch = this.ScanCharacter(out isEscaped);
            this.tokenValue = ch;
            if (this.NextChar() != '\'')
            {
                throw new RuleSyntaxException(0x17d, Messages.Parser_UnterminatedCharacterLiteral, this.currentPosition);
            }
            return TokenID.CharacterLiteral;
        }

        private TokenID ScanDecimal()
        {
            string str;
            TokenID integerLiteral;
            NumberKind unsuffixedInteger = NumberKind.UnsuffixedInteger;
            StringBuilder buffer = new StringBuilder();
            char c = this.CurrentChar();
            while (char.IsDigit(c))
            {
                buffer.Append(c);
                c = this.NextChar();
            }
            switch (c)
            {
                case 'd':
                case 'D':
                    unsuffixedInteger = NumberKind.Double;
                    this.NextChar();
                    break;

                case 'e':
                case 'E':
                    buffer.Append('e');
                    this.NextChar();
                    unsuffixedInteger = this.ScanExponent(buffer);
                    break;

                case 'f':
                case 'F':
                    unsuffixedInteger = NumberKind.Float;
                    this.NextChar();
                    break;

                case 'm':
                case 'M':
                    unsuffixedInteger = NumberKind.Decimal;
                    this.NextChar();
                    break;

                case '.':
                    unsuffixedInteger = NumberKind.Double;
                    buffer.Append('.');
                    this.NextChar();
                    unsuffixedInteger = this.ScanFraction(buffer);
                    break;

                default:
                    unsuffixedInteger = this.ScanOptionalIntegerSuffix();
                    break;
            }
            string s = buffer.ToString();
            switch (unsuffixedInteger)
            {
                case NumberKind.Float:
                    integerLiteral = TokenID.FloatLiteral;
                    try
                    {
                        this.tokenValue = float.Parse(s, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        return integerLiteral;
                    }
                    catch (Exception exception)
                    {
                        str = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidFloatingPointConstant, new object[] { exception.Message });
                        throw new RuleSyntaxException(0x1a7, str, this.tokenStartPosition);
                    }
                    break;

                case NumberKind.Double:
                    integerLiteral = TokenID.FloatLiteral;
                    try
                    {
                        this.tokenValue = double.Parse(s, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        return integerLiteral;
                    }
                    catch (Exception exception2)
                    {
                        str = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidFloatingPointConstant, new object[] { exception2.Message });
                        throw new RuleSyntaxException(0x1a7, str, this.tokenStartPosition);
                    }
                    break;

                case NumberKind.Decimal:
                    integerLiteral = TokenID.DecimalLiteral;
                    try
                    {
                        this.tokenValue = decimal.Parse(s, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        return integerLiteral;
                    }
                    catch (Exception exception3)
                    {
                        str = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidDecimalConstant, new object[] { exception3.Message });
                        throw new RuleSyntaxException(0x1a7, str, this.tokenStartPosition);
                    }
                    break;
            }
            integerLiteral = TokenID.IntegerLiteral;
            ulong num = 0L;
            try
            {
                num = ulong.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (Exception exception4)
            {
                str = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidIntegerConstant, new object[] { exception4.Message });
                throw new RuleSyntaxException(0x192, str, this.tokenStartPosition);
            }
            switch (unsuffixedInteger)
            {
                case NumberKind.UnsuffixedInteger:
                    if (num <= 0x7fffffffffffffffL)
                    {
                        if (num <= 0x7fffffffL)
                        {
                            this.tokenValue = (int) num;
                            return integerLiteral;
                        }
                        this.tokenValue = (long) num;
                        return integerLiteral;
                    }
                    this.tokenValue = num;
                    return integerLiteral;

                case NumberKind.Long:
                    this.tokenValue = (long) num;
                    return integerLiteral;

                case (NumberKind.Long | NumberKind.UnsuffixedInteger):
                case (NumberKind.Unsigned | NumberKind.UnsuffixedInteger):
                    return integerLiteral;

                case NumberKind.Unsigned:
                    if (num > 0xffffffffL)
                    {
                        this.tokenValue = num;
                        return integerLiteral;
                    }
                    this.tokenValue = (uint) num;
                    return integerLiteral;

                case (NumberKind.Unsigned | NumberKind.Long):
                    this.tokenValue = num;
                    return integerLiteral;
            }
            return integerLiteral;
        }

        private NumberKind ScanExponent(StringBuilder buffer)
        {
            char ch = this.CurrentChar();
            switch (ch)
            {
                case '-':
                case '+':
                    buffer.Append(ch);
                    ch = this.NextChar();
                    break;
            }
            if (!char.IsDigit(ch))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidExponentDigit, new object[] { ch });
                throw new RuleSyntaxException(0x17e, message, this.currentPosition);
            }
            do
            {
                buffer.Append(ch);
                ch = this.NextChar();
            }
            while (char.IsDigit(ch));
            NumberKind kind = NumberKind.Double;
            char ch2 = ch;
            if (ch2 <= 'M')
            {
                switch (ch2)
                {
                    case 'D':
                        goto Label_00B9;

                    case 'E':
                        return kind;

                    case 'F':
                        goto Label_00C4;

                    case 'M':
                        goto Label_00D0;
                }
                return kind;
            }
            switch (ch2)
            {
                case 'd':
                    break;

                case 'e':
                    return kind;

                case 'f':
                    goto Label_00C4;

                case 'm':
                    goto Label_00D0;

                default:
                    return kind;
            }
        Label_00B9:
            kind = NumberKind.Double;
            this.NextChar();
            return kind;
        Label_00C4:
            kind = NumberKind.Float;
            this.NextChar();
            return kind;
        Label_00D0:
            kind = NumberKind.Decimal;
            this.NextChar();
            return kind;
        }

        private NumberKind ScanFraction(StringBuilder buffer)
        {
            char c = this.CurrentChar();
            while (char.IsDigit(c))
            {
                buffer.Append(c);
                c = this.NextChar();
            }
            NumberKind kind = NumberKind.Double;
            char ch2 = c;
            if (ch2 <= 'M')
            {
                switch (ch2)
                {
                    case 'D':
                        goto Label_007B;

                    case 'E':
                        goto Label_0061;

                    case 'F':
                        goto Label_0086;

                    case 'M':
                        goto Label_0092;
                }
                return kind;
            }
            switch (ch2)
            {
                case 'd':
                    goto Label_007B;

                case 'e':
                    break;

                case 'f':
                    goto Label_0086;

                case 'm':
                    goto Label_0092;

                default:
                    return kind;
            }
        Label_0061:
            buffer.Append('E');
            this.NextChar();
            return this.ScanExponent(buffer);
        Label_007B:
            kind = NumberKind.Double;
            this.NextChar();
            return kind;
        Label_0086:
            kind = NumberKind.Float;
            this.NextChar();
            return kind;
        Label_0092:
            kind = NumberKind.Decimal;
            this.NextChar();
            return kind;
        }

        private TokenID ScanHexNumber()
        {
            char ch = this.CurrentChar();
            int num = HexValue(ch);
            if (num < 0)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidHexDigit, new object[] { ch });
                throw new RuleSyntaxException(0x17f, message, this.currentPosition);
            }
            int num2 = 1;
            ulong num3 = (ulong) num;
            for (num = HexValue(this.NextChar()); num >= 0; num = HexValue(this.NextChar()))
            {
                num2++;
                num3 = (num3 * ((ulong) 0x10L)) + num;
            }
            if (num2 > 0x10)
            {
                string str2 = string.Format(CultureInfo.CurrentCulture, Messages.Parser_InvalidIntegerConstant, new object[] { string.Empty });
                throw new RuleSyntaxException(0x192, str2, this.tokenStartPosition);
            }
            TokenID integerLiteral = TokenID.IntegerLiteral;
            switch (this.ScanOptionalIntegerSuffix())
            {
                case NumberKind.UnsuffixedInteger:
                    if (num3 <= 0x7fffffffffffffffL)
                    {
                        if (num3 <= 0x7fffffffL)
                        {
                            this.tokenValue = (int) num3;
                            return integerLiteral;
                        }
                        this.tokenValue = (long) num3;
                        return integerLiteral;
                    }
                    this.tokenValue = num3;
                    return integerLiteral;

                case NumberKind.Long:
                    this.tokenValue = (long) num3;
                    return integerLiteral;

                case (NumberKind.Long | NumberKind.UnsuffixedInteger):
                case (NumberKind.Unsigned | NumberKind.UnsuffixedInteger):
                    return integerLiteral;

                case NumberKind.Unsigned:
                    if (num3 > 0xffffffffL)
                    {
                        this.tokenValue = num3;
                        return integerLiteral;
                    }
                    this.tokenValue = (uint) num3;
                    return integerLiteral;

                case (NumberKind.Unsigned | NumberKind.Long):
                    this.tokenValue = num3;
                    return integerLiteral;
            }
            return integerLiteral;
        }

        private void ScanIdentifier(StringBuilder sb, out bool hasLettersOnly)
        {
            char c = this.CurrentChar();
            hasLettersOnly = char.IsLetter(c);
            sb.Append(c);
            for (c = this.NextChar(); c != '\0'; c = this.NextChar())
            {
                bool flag = false;
                if (char.IsLetter(c))
                {
                    flag = true;
                }
                else if (char.IsDigit(c))
                {
                    flag = true;
                    hasLettersOnly = false;
                }
                else if (c == '_')
                {
                    flag = true;
                    hasLettersOnly = false;
                }
                if (!flag)
                {
                    return;
                }
                sb.Append(c);
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private TokenID ScanKeywordOrIdentifier()
        {
            bool flag;
            StringBuilder sb = new StringBuilder();
            this.ScanIdentifier(sb, out flag);
            string str = sb.ToString();
            TokenID unknown = TokenID.Unknown;
            if (flag && (this.currentToken != TokenID.Dot))
            {
                KeywordInfo info = null;
                if (keywordMap.TryGetValue(str.ToLowerInvariant(), out info))
                {
                    unknown = info.Token;
                    this.tokenValue = info.TokenValue;
                    return unknown;
                }
            }
            unknown = TokenID.Identifier;
            this.tokenValue = str;
            return unknown;
        }

        private TokenID ScanNumber()
        {
            if ((this.CurrentChar() == '0') && (this.PeekNextChar() == 'x'))
            {
                this.NextChar();
                this.NextChar();
                return this.ScanHexNumber();
            }
            return this.ScanDecimal();
        }

        private NumberKind ScanOptionalIntegerSuffix()
        {
            NumberKind unsuffixedInteger = NumberKind.UnsuffixedInteger;
            char ch2 = this.CurrentChar();
            if (ch2 <= 'U')
            {
                switch (ch2)
                {
                    case 'L':
                        goto Label_0028;

                    case 'U':
                        goto Label_0048;
                }
                return unsuffixedInteger;
            }
            switch (ch2)
            {
                case 'l':
                    break;

                case 'u':
                    goto Label_0048;

                default:
                    return unsuffixedInteger;
            }
        Label_0028:
            switch (this.NextChar())
            {
                case 'u':
                case 'U':
                    unsuffixedInteger = NumberKind.Unsigned | NumberKind.Long;
                    this.NextChar();
                    return unsuffixedInteger;

                default:
                    return NumberKind.Long;
            }
        Label_0048:
            switch (this.NextChar())
            {
                case 'l':
                case 'L':
                    unsuffixedInteger = NumberKind.Unsigned | NumberKind.Long;
                    this.NextChar();
                    return unsuffixedInteger;
            }
            return NumberKind.Unsigned;
        }

        private TokenID ScanStringLiteral()
        {
            StringBuilder builder = new StringBuilder();
            bool isEscaped = false;
            char ch = this.ScanCharacter(out isEscaped);
            while (true)
            {
                if ((ch == '\0') && !isEscaped)
                {
                    throw new RuleSyntaxException(0x193, Messages.Parser_UnterminatedStringLiteral, this.tokenStartPosition);
                }
                if ((ch == '"') && !isEscaped)
                {
                    break;
                }
                builder.Append(ch);
                ch = this.ScanCharacter(out isEscaped);
            }
            this.tokenValue = builder.ToString();
            return TokenID.StringLiteral;
        }

        private char ScanUnicodeEscapeSequence()
        {
            uint num = 0;
            for (int i = 0; i < 4; i++)
            {
                int num3 = HexValue(this.NextChar());
                num = (0x10 * num) + ((uint) num3);
            }
            return (char) num;
        }

        private TokenID ScanVerbatimStringLiteral()
        {
            StringBuilder builder = new StringBuilder();
            char ch = this.NextChar();
            while (true)
            {
                switch (ch)
                {
                    case '\0':
                        throw new RuleSyntaxException(0x193, Messages.Parser_UnterminatedStringLiteral, this.tokenStartPosition);

                    case '"':
                        if (this.PeekNextChar() != '"')
                        {
                            this.tokenValue = builder.ToString();
                            return TokenID.StringLiteral;
                        }
                        this.NextChar();
                        builder.Append('"');
                        break;

                    default:
                        builder.Append(ch);
                        break;
                }
                ch = this.NextChar();
            }
        }

        private char SkipWhitespace(char ch)
        {
            while (char.IsWhiteSpace(ch))
            {
                ch = this.NextChar();
            }
            return ch;
        }

        internal void Tokenize(List<Token> tokenList)
        {
            Token item = null;
            do
            {
                item = this.NextToken();
                tokenList.Add(item);
            }
            while (item.TokenID != TokenID.EndOfInput);
        }

        internal void TokenizeForIntellisense(List<Token> tokenList)
        {
            Token item = null;
            do
            {
                try
                {
                    item = this.NextToken();
                    tokenList.Add(item);
                }
                catch (RuleSyntaxException)
                {
                    item = new Token(TokenID.Illegal, 0, null);
                    tokenList.Add(item);
                }
            }
            while ((item != null) && (item.TokenID != TokenID.EndOfInput));
        }

        private class KeywordInfo
        {
            internal TokenID Token;
            internal object TokenValue;

            internal KeywordInfo(TokenID token) : this(token, null)
            {
            }

            internal KeywordInfo(TokenID token, object tokenValue)
            {
                this.Token = token;
                this.TokenValue = tokenValue;
            }
        }

        [Flags]
        private enum NumberKind
        {
            Decimal = 0x10,
            Double = 8,
            Float = 12,
            Long = 2,
            Unsigned = 4,
            UnsuffixedInteger = 1
        }
    }
}

