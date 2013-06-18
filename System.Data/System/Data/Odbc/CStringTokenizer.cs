namespace System.Data.Odbc
{
    using System;
    using System.Data.Common;
    using System.Text;

    internal sealed class CStringTokenizer
    {
        private readonly char _escape;
        private int _idx;
        private int _len;
        private readonly char _quote;
        private readonly string _sqlstatement;
        private readonly StringBuilder _token = new StringBuilder();

        internal CStringTokenizer(string text, char quote, char escape)
        {
            this._quote = quote;
            this._escape = escape;
            this._sqlstatement = text;
            if (text != null)
            {
                int index = text.IndexOf('\0');
                this._len = (0 > index) ? text.Length : index;
            }
            else
            {
                this._len = 0;
            }
        }

        internal int FindTokenIndex(string tokenString)
        {
            string str;
            do
            {
                str = this.NextToken();
                if ((this._idx == this._len) || ADP.IsEmpty(str))
                {
                    return -1;
                }
            }
            while (string.Compare(tokenString, str, StringComparison.OrdinalIgnoreCase) != 0);
            return this._idx;
        }

        private int GetTokenFromBracket(int curidx)
        {
            while (curidx < this._len)
            {
                this._token.Append(this._sqlstatement[curidx]);
                curidx++;
                if (this._sqlstatement[curidx - 1] == ']')
                {
                    return curidx;
                }
            }
            return curidx;
        }

        private int GetTokenFromQuote(int curidx)
        {
            int num = curidx;
            while (num < this._len)
            {
                this._token.Append(this._sqlstatement[num]);
                if ((((this._sqlstatement[num] == this._quote) && (num > curidx)) && ((this._sqlstatement[num - 1] != this._escape) && ((num + 1) < this._len))) && (this._sqlstatement[num + 1] != this._quote))
                {
                    return (num + 1);
                }
                num++;
            }
            return num;
        }

        private bool IsValidNameChar(char ch)
        {
            if ((((!char.IsLetterOrDigit(ch) && (ch != '_')) && ((ch != '-') && (ch != '.'))) && (((ch != '$') && (ch != '#')) && ((ch != '@') && (ch != '~')))) && (((ch != '`') && (ch != '%')) && ((ch != '^') && (ch != '&'))))
            {
                return (ch == '|');
            }
            return true;
        }

        internal string NextToken()
        {
            if (this._token.Length != 0)
            {
                this._idx += this._token.Length;
                this._token.Remove(0, this._token.Length);
            }
            while ((this._idx < this._len) && char.IsWhiteSpace(this._sqlstatement[this._idx]))
            {
                this._idx++;
            }
            if (this._idx == this._len)
            {
                return string.Empty;
            }
            int curidx = this._idx;
            bool flag = false;
            while (!flag && (curidx < this._len))
            {
                if (this.IsValidNameChar(this._sqlstatement[curidx]))
                {
                    while ((curidx < this._len) && this.IsValidNameChar(this._sqlstatement[curidx]))
                    {
                        this._token.Append(this._sqlstatement[curidx]);
                        curidx++;
                    }
                }
                else
                {
                    char c = this._sqlstatement[curidx];
                    if (c == '[')
                    {
                        curidx = this.GetTokenFromBracket(curidx);
                    }
                    else
                    {
                        if ((' ' != this._quote) && (c == this._quote))
                        {
                            curidx = this.GetTokenFromQuote(curidx);
                            continue;
                        }
                        if (!char.IsWhiteSpace(c))
                        {
                            if (c == ',')
                            {
                                if (curidx == this._idx)
                                {
                                    this._token.Append(c);
                                }
                            }
                            else
                            {
                                this._token.Append(c);
                            }
                        }
                        flag = true;
                        break;
                    }
                }
            }
            if (this._token.Length <= 0)
            {
                return string.Empty;
            }
            return this._token.ToString();
        }

        internal bool StartsWith(string tokenString)
        {
            int indexA = 0;
            while ((indexA < this._len) && char.IsWhiteSpace(this._sqlstatement[indexA]))
            {
                indexA++;
            }
            if (((this._len - indexA) >= tokenString.Length) && (string.Compare(this._sqlstatement, indexA, tokenString, 0, tokenString.Length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                this._idx = 0;
                this.NextToken();
                return true;
            }
            return false;
        }

        internal int CurrentPosition
        {
            get
            {
                return this._idx;
            }
        }
    }
}

