namespace MS.Internal.Xaml.Parser
{
    using MS.Internal.Xaml.Context;
    using System;
    using System.Text;
    using System.Xaml;
    using System.Xaml.MS.Impl;
    using System.Xaml.Schema;

    internal class MeScanner
    {
        private XamlParserContext _context;
        private bool _hasTrailingWhitespace;
        private int _idx;
        private string _inputText;
        private int _lineNumber;
        private int _startPosition;
        private StringState _state;
        private MeTokenType _token;
        private string _tokenNamespace;
        private XamlMember _tokenProperty;
        private string _tokenText;
        private XamlType _tokenXamlType;
        public const char Backslash = '\\';
        public const char CloseCurlie = '}';
        public const char Comma = ',';
        public const char EqualSign = '=';
        public const char NullChar = '\0';
        public const char OpenCurlie = '{';
        public const char Quote1 = '\'';
        public const char Quote2 = '"';
        public const char Space = ' ';

        public MeScanner(XamlParserContext context, string text, int lineNumber, int linePosition)
        {
            this._context = context;
            this._inputText = text;
            this._lineNumber = lineNumber;
            this._startPosition = linePosition;
            this._idx = -1;
            this._state = StringState.Value;
        }

        private bool Advance()
        {
            this._idx++;
            if (this.IsAtEndOfInput)
            {
                this._idx = this._inputText.Length;
                return false;
            }
            return true;
        }

        private void AdvanceOverWhitespace()
        {
            bool flag = false;
            while (!this.IsAtEndOfInput && IsWhitespaceChar(this.CurrentChar))
            {
                flag = true;
                this.Advance();
            }
            if (this.IsAtEndOfInput && flag)
            {
                this._hasTrailingWhitespace = true;
            }
        }

        private static bool IsWhitespaceChar(char ch)
        {
            if (((ch != KnownStrings.WhitespaceChars[0]) && (ch != KnownStrings.WhitespaceChars[1])) && (((ch != KnownStrings.WhitespaceChars[2]) && (ch != KnownStrings.WhitespaceChars[3])) && (ch != KnownStrings.WhitespaceChars[4])))
            {
                return false;
            }
            return true;
        }

        private void PushBack()
        {
            this._idx--;
        }

        public void Read()
        {
            bool flag = false;
            bool flag2 = false;
            this._tokenText = string.Empty;
            this._tokenXamlType = null;
            this._tokenProperty = null;
            this._tokenNamespace = null;
            this.Advance();
            this.AdvanceOverWhitespace();
            if (this.IsAtEndOfInput)
            {
                this._token = MeTokenType.None;
            }
            else
            {
                switch (this.CurrentChar)
                {
                    case '{':
                        this._token = MeTokenType.Open;
                        this._state = StringState.Type;
                        break;

                    case '}':
                        this._token = MeTokenType.Close;
                        this._state = StringState.Value;
                        break;

                    case '=':
                        this._token = MeTokenType.EqualSign;
                        this._state = StringState.Value;
                        break;

                    case ',':
                        this._token = MeTokenType.Comma;
                        this._state = StringState.Value;
                        break;

                    case '"':
                    case '\'':
                        if (this.NextChar == '{')
                        {
                            flag = true;
                        }
                        flag2 = true;
                        break;

                    default:
                        flag2 = true;
                        break;
                }
                if (flag2)
                {
                    string longName = this.ReadString();
                    this._token = flag ? MeTokenType.QuotedMarkupExtension : MeTokenType.String;
                    switch (this._state)
                    {
                        case StringState.Type:
                            this._token = MeTokenType.TypeName;
                            this.ResolveTypeName(longName);
                            break;

                        case StringState.Property:
                            this._token = MeTokenType.PropertyName;
                            this.ResolvePropertyName(longName);
                            break;
                    }
                    this._state = StringState.Value;
                    this._tokenText = RemoveEscapes(longName);
                }
            }
        }

        private string ReadString()
        {
            bool flag = false;
            char ch = '\0';
            bool flag2 = true;
            bool flag3 = false;
            uint num = 0;
            StringBuilder builder = new StringBuilder();
            while (!this.IsAtEndOfInput)
            {
                char currentChar = this.CurrentChar;
                if (flag)
                {
                    builder.Append('\\');
                    builder.Append(currentChar);
                    flag = false;
                    goto Label_016E;
                }
                if (ch != '\0')
                {
                    if (currentChar == '\\')
                    {
                        flag = true;
                        goto Label_016E;
                    }
                    if (currentChar != ch)
                    {
                        builder.Append(currentChar);
                        goto Label_016E;
                    }
                    currentChar = this.CurrentChar;
                    ch = '\0';
                    break;
                }
                bool flag4 = false;
                switch (currentChar)
                {
                    case '{':
                        num++;
                        builder.Append(currentChar);
                        goto Label_014C;

                    case '}':
                        if (num != 0)
                        {
                            goto Label_0100;
                        }
                        flag4 = true;
                        goto Label_014C;

                    case '\\':
                        flag = true;
                        goto Label_014C;

                    case '=':
                        this._state = StringState.Property;
                        flag4 = true;
                        goto Label_014C;

                    case ' ':
                        if (this._state != StringState.Type)
                        {
                            break;
                        }
                        flag4 = true;
                        goto Label_014C;

                    case '"':
                    case '\'':
                        if (!flag2)
                        {
                            throw new XamlParseException(this, System.Xaml.SR.Get("QuoteCharactersOutOfPlace"));
                        }
                        ch = currentChar;
                        flag3 = true;
                        goto Label_014C;

                    case ',':
                        flag4 = true;
                        goto Label_014C;

                    default:
                        builder.Append(currentChar);
                        goto Label_014C;
                }
                builder.Append(currentChar);
                goto Label_014C;
            Label_0100:
                num--;
                builder.Append(currentChar);
            Label_014C:
                if (flag4)
                {
                    if (num > 0)
                    {
                        throw new XamlParseException(this, System.Xaml.SR.Get("UnexpectedTokenAfterME"));
                    }
                    this.PushBack();
                    break;
                }
            Label_016E:
                flag2 = false;
                this.Advance();
            }
            if (ch != '\0')
            {
                throw new XamlParseException(this, System.Xaml.SR.Get("UnclosedQuote"));
            }
            string str = builder.ToString();
            if (!flag3)
            {
                str = str.TrimEnd(KnownStrings.WhitespaceChars).TrimStart(KnownStrings.WhitespaceChars);
            }
            return str;
        }

        private static string RemoveEscapes(string value)
        {
            if (!value.Contains(@"\"))
            {
                return value;
            }
            StringBuilder builder = new StringBuilder(value.Length);
            int startIndex = 0;
            do
            {
                int index = value.IndexOf('\\', startIndex);
                if (index < 0)
                {
                    builder.Append(value.Substring(startIndex));
                    break;
                }
                int length = index - startIndex;
                builder.Append(value.Substring(startIndex, length));
                if ((index + 1) < value.Length)
                {
                    builder.Append(value[index + 1]);
                }
                startIndex = index + 2;
            }
            while (startIndex < value.Length);
            return builder.ToString();
        }

        private void ResolvePropertyName(string longName)
        {
            XamlPropertyName propName = XamlPropertyName.Parse(longName);
            if (propName == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("MalformedPropertyName"));
            }
            XamlMember member = null;
            XamlType currentType = this._context.CurrentType;
            string currentTypeNamespace = this._context.CurrentTypeNamespace;
            if (propName.IsDotted)
            {
                member = this._context.GetDottedProperty(currentType, currentTypeNamespace, propName, false);
            }
            else
            {
                string attributeNamespace = this._context.GetAttributeNamespace(propName, this._tokenNamespace);
                XamlType tagType = this._context.CurrentType;
                member = this._context.GetNoDotAttributeProperty(tagType, propName, this._tokenNamespace, attributeNamespace, false);
            }
            this._tokenProperty = member;
        }

        private void ResolveTypeName(string longName)
        {
            string str;
            XamlTypeName typeName = XamlTypeName.ParseInternal(longName, new Func<string, string>(this._context.FindNamespaceByPrefix), out str);
            if (typeName == null)
            {
                throw new XamlParseException(this, str);
            }
            string name = typeName.Name;
            typeName.Name = typeName.Name + "Extension";
            XamlType xamlType = this._context.GetXamlType(typeName, false);
            if ((xamlType == null) || ((xamlType.UnderlyingType != null) && KS.Eq(xamlType.UnderlyingType.Name, typeName.Name + "Extension")))
            {
                typeName.Name = name;
                xamlType = this._context.GetXamlType(typeName, true);
            }
            this._tokenXamlType = xamlType;
            this._tokenNamespace = typeName.Namespace;
        }

        private char CurrentChar
        {
            get
            {
                return this._inputText[this._idx];
            }
        }

        public bool HasTrailingWhitespace
        {
            get
            {
                return this._hasTrailingWhitespace;
            }
        }

        public bool IsAtEndOfInput
        {
            get
            {
                return (this._idx >= this._inputText.Length);
            }
        }

        public int LineNumber
        {
            get
            {
                return this._lineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                int num = (this._idx < 0) ? 0 : this._idx;
                return (this._startPosition + num);
            }
        }

        public string Namespace
        {
            get
            {
                return this._tokenNamespace;
            }
        }

        private char NextChar
        {
            get
            {
                if ((this._idx + 1) < this._inputText.Length)
                {
                    return this._inputText[this._idx + 1];
                }
                return '\0';
            }
        }

        public MeTokenType Token
        {
            get
            {
                return this._token;
            }
        }

        public XamlMember TokenProperty
        {
            get
            {
                return this._tokenProperty;
            }
        }

        public string TokenText
        {
            get
            {
                return this._tokenText;
            }
        }

        public XamlType TokenType
        {
            get
            {
                return this._tokenXamlType;
            }
        }

        private enum StringState
        {
            Value,
            Type,
            Property
        }
    }
}

