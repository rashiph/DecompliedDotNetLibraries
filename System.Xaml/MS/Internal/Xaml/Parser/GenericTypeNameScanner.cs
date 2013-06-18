namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Runtime.InteropServices;

    internal class GenericTypeNameScanner : Sample_StringParserBase
    {
        private char _lastChar;
        private int _multiCharTokenLength;
        private int _multiCharTokenStartIdx;
        private GenericTypeNameScannerToken _pushedBackSymbol;
        private State _state;
        private GenericTypeNameScannerToken _token;
        private string _tokenText;
        public const char CloseBracket = ']';
        public const char CloseParen = ')';
        public const char Colon = ':';
        public const char Comma = ',';
        public const char OpenBracket = '[';
        public const char OpenParen = '(';
        public const char Space = ' ';

        public GenericTypeNameScanner(string text) : base(text)
        {
            this._state = State.START;
            this._pushedBackSymbol = GenericTypeNameScannerToken.NONE;
        }

        private void AddToMultiCharToken()
        {
            this._multiCharTokenLength++;
        }

        private string CollectMultiCharToken()
        {
            if ((this._multiCharTokenStartIdx == 0) && (this._multiCharTokenLength == base._inputText.Length))
            {
                return base._inputText;
            }
            return base._inputText.Substring(this._multiCharTokenStartIdx, this._multiCharTokenLength);
        }

        internal static int ParseSubscriptSegment(string subscript, ref int pos)
        {
            bool flag = false;
            int num = 1;
            do
            {
                switch (subscript[pos])
                {
                    case '[':
                        if (!flag)
                        {
                            flag = true;
                            break;
                        }
                        return 0;

                    case ']':
                        if (flag)
                        {
                            pos++;
                            return num;
                        }
                        return 0;

                    case ',':
                        if (!flag)
                        {
                            return 0;
                        }
                        num++;
                        break;

                    default:
                        if (!Sample_StringParserBase.IsWhitespaceChar(subscript[pos]))
                        {
                            return 0;
                        }
                        break;
                }
                pos++;
            }
            while (pos < subscript.Length);
            return 0;
        }

        public void Read()
        {
            if (this._pushedBackSymbol != GenericTypeNameScannerToken.NONE)
            {
                this._token = this._pushedBackSymbol;
                this._pushedBackSymbol = GenericTypeNameScannerToken.NONE;
            }
            else
            {
                this._token = GenericTypeNameScannerToken.NONE;
                this._tokenText = string.Empty;
                this._multiCharTokenStartIdx = -1;
                this._multiCharTokenLength = 0;
                while (this._token == GenericTypeNameScannerToken.NONE)
                {
                    if (base.IsAtEndOfInput)
                    {
                        if (this._state == State.INNAME)
                        {
                            this._token = GenericTypeNameScannerToken.NAME;
                            this._state = State.START;
                        }
                        if (this._state == State.INSUBSCRIPT)
                        {
                            this._token = GenericTypeNameScannerToken.ERROR;
                            this._state = State.START;
                        }
                        break;
                    }
                    switch (this._state)
                    {
                        case State.START:
                            this.State_Start();
                            break;

                        case State.INNAME:
                            this.State_InName();
                            break;

                        case State.INSUBSCRIPT:
                            this.State_InSubscript();
                            break;
                    }
                }
                if ((this._token == GenericTypeNameScannerToken.NAME) || (this._token == GenericTypeNameScannerToken.SUBSCRIPT))
                {
                    this._tokenText = this.CollectMultiCharToken();
                }
            }
        }

        private void StartMultiCharToken()
        {
            this._multiCharTokenStartIdx = base._idx;
            this._multiCharTokenLength = 1;
        }

        private void State_InName()
        {
            if ((base.IsAtEndOfInput || Sample_StringParserBase.IsWhitespaceChar(base.CurrentChar)) || (base.CurrentChar == '['))
            {
                this._token = GenericTypeNameScannerToken.NAME;
                this._state = State.START;
            }
            else
            {
                switch (base.CurrentChar)
                {
                    case '(':
                        this._pushedBackSymbol = GenericTypeNameScannerToken.OPEN;
                        this._token = GenericTypeNameScannerToken.NAME;
                        this._state = State.START;
                        break;

                    case ')':
                        this._pushedBackSymbol = GenericTypeNameScannerToken.CLOSE;
                        this._token = GenericTypeNameScannerToken.NAME;
                        this._state = State.START;
                        break;

                    case ',':
                        this._pushedBackSymbol = GenericTypeNameScannerToken.COMMA;
                        this._token = GenericTypeNameScannerToken.NAME;
                        this._state = State.START;
                        break;

                    case ':':
                        this._pushedBackSymbol = GenericTypeNameScannerToken.COLON;
                        this._token = GenericTypeNameScannerToken.NAME;
                        this._state = State.START;
                        break;

                    default:
                        if (XamlName.IsValidQualifiedNameChar(base.CurrentChar))
                        {
                            this.AddToMultiCharToken();
                        }
                        else
                        {
                            this._token = GenericTypeNameScannerToken.ERROR;
                        }
                        break;
                }
                this._lastChar = base.CurrentChar;
                base.Advance();
            }
        }

        private void State_InSubscript()
        {
            if (base.IsAtEndOfInput)
            {
                this._token = GenericTypeNameScannerToken.ERROR;
                this._state = State.START;
            }
            else
            {
                switch (base.CurrentChar)
                {
                    case ',':
                        this.AddToMultiCharToken();
                        break;

                    case ']':
                        this.AddToMultiCharToken();
                        this._token = GenericTypeNameScannerToken.SUBSCRIPT;
                        this._state = State.START;
                        break;

                    default:
                        if (Sample_StringParserBase.IsWhitespaceChar(base.CurrentChar))
                        {
                            this.AddToMultiCharToken();
                        }
                        else
                        {
                            this._token = GenericTypeNameScannerToken.ERROR;
                        }
                        break;
                }
                this._lastChar = base.CurrentChar;
                base.Advance();
            }
        }

        private void State_Start()
        {
            base.AdvanceOverWhitespace();
            if (base.IsAtEndOfInput)
            {
                this._token = GenericTypeNameScannerToken.NONE;
            }
            else
            {
                switch (base.CurrentChar)
                {
                    case '(':
                        this._token = GenericTypeNameScannerToken.OPEN;
                        break;

                    case ')':
                        this._token = GenericTypeNameScannerToken.CLOSE;
                        break;

                    case ',':
                        this._token = GenericTypeNameScannerToken.COMMA;
                        break;

                    case ':':
                        this._token = GenericTypeNameScannerToken.COLON;
                        break;

                    case '[':
                        this.StartMultiCharToken();
                        this._state = State.INSUBSCRIPT;
                        break;

                    default:
                        if (XamlName.IsValidNameStartChar(base.CurrentChar))
                        {
                            this.StartMultiCharToken();
                            this._state = State.INNAME;
                        }
                        else
                        {
                            this._token = GenericTypeNameScannerToken.ERROR;
                        }
                        break;
                }
                this._lastChar = base.CurrentChar;
                base.Advance();
            }
        }

        internal static string StripSubscript(string typeName, out string subscript)
        {
            int index = typeName.IndexOf('[');
            if (index < 0)
            {
                subscript = null;
                return typeName;
            }
            subscript = typeName.Substring(index);
            return typeName.Substring(0, index);
        }

        public char ErrorCurrentChar
        {
            get
            {
                return this._lastChar;
            }
        }

        public string MultiCharTokenText
        {
            get
            {
                return this._tokenText;
            }
        }

        public GenericTypeNameScannerToken Token
        {
            get
            {
                return this._token;
            }
        }

        internal enum State
        {
            START,
            INNAME,
            INSUBSCRIPT
        }
    }
}

