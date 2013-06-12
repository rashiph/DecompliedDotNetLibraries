namespace System.Net
{
    using System;

    internal class CookieParser
    {
        private Cookie m_savedCookie;
        private CookieTokenizer m_tokenizer;

        internal CookieParser(string cookieString)
        {
            this.m_tokenizer = new CookieTokenizer(cookieString);
        }

        internal static string CheckQuoted(string value)
        {
            if (((value.Length < 2) || (value[0] != '"')) || (value[value.Length - 1] != '"'))
            {
                return value;
            }
            if (value.Length != 2)
            {
                return value.Substring(1, value.Length - 2);
            }
            return string.Empty;
        }

        internal Cookie Get()
        {
            Cookie cookie = null;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            bool flag8 = false;
            bool flag9 = false;
            do
            {
                CookieToken token = this.m_tokenizer.Next(cookie == null, true);
                if ((cookie == null) && ((token == CookieToken.NameValuePair) || (token == CookieToken.Attribute)))
                {
                    cookie = new Cookie();
                    if (!cookie.InternalSetName(this.m_tokenizer.Name))
                    {
                        cookie.InternalSetName(string.Empty);
                    }
                    cookie.Value = this.m_tokenizer.Value;
                }
                else
                {
                    switch (token)
                    {
                        case CookieToken.NameValuePair:
                            switch (this.m_tokenizer.Token)
                            {
                                case CookieToken.Comment:
                                    if (!flag)
                                    {
                                        flag = true;
                                        cookie.Comment = this.m_tokenizer.Value;
                                    }
                                    break;

                                case CookieToken.CommentUrl:
                                    if (!flag2)
                                    {
                                        Uri uri;
                                        flag2 = true;
                                        if (Uri.TryCreate(CheckQuoted(this.m_tokenizer.Value), UriKind.Absolute, out uri))
                                        {
                                            cookie.CommentUri = uri;
                                        }
                                    }
                                    break;

                                case CookieToken.Domain:
                                    if (!flag3)
                                    {
                                        flag3 = true;
                                        cookie.Domain = CheckQuoted(this.m_tokenizer.Value);
                                        cookie.IsQuotedDomain = this.m_tokenizer.Quoted;
                                    }
                                    break;

                                case CookieToken.Expires:
                                    if (!flag4)
                                    {
                                        DateTime time;
                                        flag4 = true;
                                        if (DateTime.TryParse(CheckQuoted(this.m_tokenizer.Value), out time))
                                        {
                                            cookie.Expires = time;
                                        }
                                        else
                                        {
                                            cookie.InternalSetName(string.Empty);
                                        }
                                    }
                                    break;

                                case CookieToken.MaxAge:
                                    if (!flag4)
                                    {
                                        int num;
                                        flag4 = true;
                                        if (int.TryParse(CheckQuoted(this.m_tokenizer.Value), out num))
                                        {
                                            cookie.Expires = DateTime.Now.AddSeconds((double) num);
                                        }
                                        else
                                        {
                                            cookie.InternalSetName(string.Empty);
                                        }
                                    }
                                    break;

                                case CookieToken.Path:
                                    if (!flag5)
                                    {
                                        flag5 = true;
                                        cookie.Path = this.m_tokenizer.Value;
                                    }
                                    break;

                                case CookieToken.Port:
                                    if (!flag6)
                                    {
                                        flag6 = true;
                                        try
                                        {
                                            cookie.Port = this.m_tokenizer.Value;
                                        }
                                        catch
                                        {
                                            cookie.InternalSetName(string.Empty);
                                        }
                                    }
                                    break;

                                case CookieToken.Version:
                                    if (!flag7)
                                    {
                                        int num2;
                                        flag7 = true;
                                        if (int.TryParse(CheckQuoted(this.m_tokenizer.Value), out num2))
                                        {
                                            cookie.Version = num2;
                                            cookie.IsQuotedVersion = this.m_tokenizer.Quoted;
                                        }
                                        else
                                        {
                                            cookie.InternalSetName(string.Empty);
                                        }
                                    }
                                    break;
                            }
                            break;

                        case CookieToken.Attribute:
                            switch (this.m_tokenizer.Token)
                            {
                                case CookieToken.Port:
                                    if (!flag6)
                                    {
                                        flag6 = true;
                                        cookie.Port = string.Empty;
                                    }
                                    break;

                                case CookieToken.Secure:
                                    if (!flag8)
                                    {
                                        flag8 = true;
                                        cookie.Secure = true;
                                    }
                                    break;

                                case CookieToken.HttpOnly:
                                    cookie.HttpOnly = true;
                                    break;

                                case CookieToken.Discard:
                                    if (!flag9)
                                    {
                                        flag9 = true;
                                        cookie.Discard = true;
                                    }
                                    break;
                            }
                            break;
                    }
                }
            }
            while (!this.m_tokenizer.Eof && !this.m_tokenizer.EndOfCookie);
            return cookie;
        }

        internal Cookie GetServer()
        {
            Cookie savedCookie = this.m_savedCookie;
            this.m_savedCookie = null;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            do
            {
                bool first = ((savedCookie == null) || (savedCookie.Name == null)) || (savedCookie.Name.Length == 0);
                CookieToken token = this.m_tokenizer.Next(first, false);
                if (first && ((token == CookieToken.NameValuePair) || (token == CookieToken.Attribute)))
                {
                    if (savedCookie == null)
                    {
                        savedCookie = new Cookie();
                    }
                    if (!savedCookie.InternalSetName(this.m_tokenizer.Name))
                    {
                        savedCookie.InternalSetName(string.Empty);
                    }
                    savedCookie.Value = this.m_tokenizer.Value;
                }
                else
                {
                    switch (token)
                    {
                        case CookieToken.NameValuePair:
                            switch (this.m_tokenizer.Token)
                            {
                                case CookieToken.Domain:
                                    if (!flag)
                                    {
                                        flag = true;
                                        savedCookie.Domain = CheckQuoted(this.m_tokenizer.Value);
                                        savedCookie.IsQuotedDomain = this.m_tokenizer.Quoted;
                                    }
                                    break;

                                case CookieToken.Path:
                                    if (!flag2)
                                    {
                                        flag2 = true;
                                        savedCookie.Path = this.m_tokenizer.Value;
                                    }
                                    break;

                                case CookieToken.Port:
                                    if (!flag3)
                                    {
                                        flag3 = true;
                                        try
                                        {
                                            savedCookie.Port = this.m_tokenizer.Value;
                                        }
                                        catch (CookieException)
                                        {
                                            savedCookie.InternalSetName(string.Empty);
                                        }
                                    }
                                    break;

                                case CookieToken.Unknown:
                                    this.m_savedCookie = new Cookie();
                                    if (!this.m_savedCookie.InternalSetName(this.m_tokenizer.Name))
                                    {
                                        this.m_savedCookie.InternalSetName(string.Empty);
                                    }
                                    this.m_savedCookie.Value = this.m_tokenizer.Value;
                                    return savedCookie;

                                case CookieToken.Version:
                                    int num;
                                    this.m_savedCookie = new Cookie();
                                    if (int.TryParse(this.m_tokenizer.Value, out num))
                                    {
                                        this.m_savedCookie.Version = num;
                                    }
                                    return savedCookie;
                            }
                            break;

                        case CookieToken.Attribute:
                            if ((this.m_tokenizer.Token == CookieToken.Port) && !flag3)
                            {
                                flag3 = true;
                                savedCookie.Port = string.Empty;
                            }
                            break;
                    }
                }
            }
            while (!this.m_tokenizer.Eof && !this.m_tokenizer.EndOfCookie);
            return savedCookie;
        }
    }
}

