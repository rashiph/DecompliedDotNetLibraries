namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    internal class CookieTokenizer
    {
        private bool m_eofCookie;
        private int m_index;
        private int m_length;
        private string m_name;
        private bool m_quoted;
        private int m_start;
        private CookieToken m_token;
        private int m_tokenLength;
        private string m_tokenStream;
        private string m_value;
        private static RecognizedAttribute[] RecognizedAttributes = new RecognizedAttribute[] { new RecognizedAttribute("Path", CookieToken.Path), new RecognizedAttribute("Max-Age", CookieToken.MaxAge), new RecognizedAttribute("Expires", CookieToken.Expires), new RecognizedAttribute("Version", CookieToken.Version), new RecognizedAttribute("Domain", CookieToken.Domain), new RecognizedAttribute("Secure", CookieToken.Secure), new RecognizedAttribute("Discard", CookieToken.Discard), new RecognizedAttribute("Port", CookieToken.Port), new RecognizedAttribute("Comment", CookieToken.Comment), new RecognizedAttribute("CommentURL", CookieToken.CommentUrl), new RecognizedAttribute("HttpOnly", CookieToken.HttpOnly) };
        private static RecognizedAttribute[] RecognizedServerAttributes = new RecognizedAttribute[] { new RecognizedAttribute('$' + "Path", CookieToken.Path), new RecognizedAttribute('$' + "Version", CookieToken.Version), new RecognizedAttribute('$' + "Domain", CookieToken.Domain), new RecognizedAttribute('$' + "Port", CookieToken.Port), new RecognizedAttribute('$' + "HttpOnly", CookieToken.HttpOnly) };

        internal CookieTokenizer(string tokenStream)
        {
            this.m_length = tokenStream.Length;
            this.m_tokenStream = tokenStream;
        }

        internal string Extract()
        {
            string str = string.Empty;
            if (this.m_tokenLength != 0)
            {
                str = this.m_tokenStream.Substring(this.m_start, this.m_tokenLength);
                if (!this.Quoted)
                {
                    str = str.Trim();
                }
            }
            return str;
        }

        internal CookieToken FindNext(bool ignoreComma, bool ignoreEquals)
        {
            this.m_tokenLength = 0;
            this.m_start = this.m_index;
            while ((this.m_index < this.m_length) && char.IsWhiteSpace(this.m_tokenStream[this.m_index]))
            {
                this.m_index++;
                this.m_start++;
            }
            CookieToken end = CookieToken.End;
            int num = 1;
            if (!this.Eof)
            {
                if (this.m_tokenStream[this.m_index] == '"')
                {
                    this.Quoted = true;
                    this.m_index++;
                    bool flag = false;
                    while (this.m_index < this.m_length)
                    {
                        char ch = this.m_tokenStream[this.m_index];
                        if (!flag && (ch == '"'))
                        {
                            break;
                        }
                        if (flag)
                        {
                            flag = false;
                        }
                        else if (ch == '\\')
                        {
                            flag = true;
                        }
                        this.m_index++;
                    }
                    if (this.m_index < this.m_length)
                    {
                        this.m_index++;
                    }
                    this.m_tokenLength = this.m_index - this.m_start;
                    num = 0;
                    ignoreComma = false;
                }
                while ((((this.m_index < this.m_length) && (this.m_tokenStream[this.m_index] != ';')) && (ignoreEquals || (this.m_tokenStream[this.m_index] != '='))) && (ignoreComma || (this.m_tokenStream[this.m_index] != ',')))
                {
                    if (this.m_tokenStream[this.m_index] == ',')
                    {
                        this.m_start = this.m_index + 1;
                        this.m_tokenLength = -1;
                        ignoreComma = false;
                    }
                    this.m_index++;
                    this.m_tokenLength += num;
                }
                if (this.Eof)
                {
                    return end;
                }
                switch (this.m_tokenStream[this.m_index])
                {
                    case ';':
                        end = CookieToken.EndToken;
                        break;

                    case '=':
                        end = CookieToken.Equals;
                        break;

                    default:
                        end = CookieToken.EndCookie;
                        break;
                }
                this.m_index++;
            }
            return end;
        }

        internal CookieToken Next(bool first, bool parseResponseCookies)
        {
            this.Reset();
            CookieToken token = this.FindNext(false, false);
            switch (token)
            {
                case CookieToken.EndCookie:
                    this.EndOfCookie = true;
                    break;

                case CookieToken.End:
                case CookieToken.EndCookie:
                    string str;
                    this.Name = str = this.Extract();
                    if (str.Length != 0)
                    {
                        this.Token = this.TokenFromName(parseResponseCookies);
                        return CookieToken.Attribute;
                    }
                    return token;
            }
            this.Name = this.Extract();
            if (first)
            {
                this.Token = CookieToken.CookieName;
            }
            else
            {
                this.Token = this.TokenFromName(parseResponseCookies);
            }
            if (token != CookieToken.Equals)
            {
                return CookieToken.Attribute;
            }
            if (this.FindNext(!first && (this.Token == CookieToken.Expires), true) == CookieToken.EndCookie)
            {
                this.EndOfCookie = true;
            }
            this.Value = this.Extract();
            return CookieToken.NameValuePair;
        }

        internal void Reset()
        {
            this.m_eofCookie = false;
            this.m_name = string.Empty;
            this.m_quoted = false;
            this.m_start = this.m_index;
            this.m_token = CookieToken.Nothing;
            this.m_tokenLength = 0;
            this.m_value = string.Empty;
        }

        internal CookieToken TokenFromName(bool parseResponseCookies)
        {
            if (!parseResponseCookies)
            {
                for (int i = 0; i < RecognizedServerAttributes.Length; i++)
                {
                    if (RecognizedServerAttributes[i].IsEqualTo(this.Name))
                    {
                        return RecognizedServerAttributes[i].Token;
                    }
                }
            }
            else
            {
                for (int j = 0; j < RecognizedAttributes.Length; j++)
                {
                    if (RecognizedAttributes[j].IsEqualTo(this.Name))
                    {
                        return RecognizedAttributes[j].Token;
                    }
                }
            }
            return CookieToken.Unknown;
        }

        internal bool EndOfCookie
        {
            get
            {
                return this.m_eofCookie;
            }
            set
            {
                this.m_eofCookie = value;
            }
        }

        internal bool Eof
        {
            get
            {
                return (this.m_index >= this.m_length);
            }
        }

        internal string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                this.m_name = value;
            }
        }

        internal bool Quoted
        {
            get
            {
                return this.m_quoted;
            }
            set
            {
                this.m_quoted = value;
            }
        }

        internal CookieToken Token
        {
            get
            {
                return this.m_token;
            }
            set
            {
                this.m_token = value;
            }
        }

        internal string Value
        {
            get
            {
                return this.m_value;
            }
            set
            {
                this.m_value = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RecognizedAttribute
        {
            private string m_name;
            private CookieToken m_token;
            internal RecognizedAttribute(string name, CookieToken token)
            {
                this.m_name = name;
                this.m_token = token;
            }

            internal CookieToken Token
            {
                get
                {
                    return this.m_token;
                }
            }
            internal bool IsEqualTo(string value)
            {
                return (string.Compare(this.m_name, value, StringComparison.OrdinalIgnoreCase) == 0);
            }
        }
    }
}

