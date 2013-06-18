namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Xaml.MS.Impl;

    internal class Sample_StringParserBase
    {
        protected int _idx;
        protected string _inputText;
        protected const char NullChar = '\0';

        public Sample_StringParserBase(string text)
        {
            this._inputText = text;
            this._idx = 0;
        }

        protected bool Advance()
        {
            this._idx++;
            if (this.IsAtEndOfInput)
            {
                this._idx = this._inputText.Length;
                return false;
            }
            return true;
        }

        protected bool AdvanceOverWhitespace()
        {
            bool flag = true;
            while (!this.IsAtEndOfInput && IsWhitespaceChar(this.CurrentChar))
            {
                flag = true;
                this.Advance();
            }
            return flag;
        }

        protected static bool IsWhitespaceChar(char ch)
        {
            if (((ch != KnownStrings.WhitespaceChars[0]) && (ch != KnownStrings.WhitespaceChars[1])) && (((ch != KnownStrings.WhitespaceChars[2]) && (ch != KnownStrings.WhitespaceChars[3])) && (ch != KnownStrings.WhitespaceChars[4])))
            {
                return false;
            }
            return true;
        }

        protected char CurrentChar
        {
            get
            {
                return this._inputText[this._idx];
            }
        }

        public bool IsAtEndOfInput
        {
            get
            {
                return (this._idx >= this._inputText.Length);
            }
        }
    }
}

