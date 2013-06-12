namespace System
{
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct __DTString
    {
        internal string Value;
        internal int Index;
        internal int len;
        internal char m_current;
        private System.Globalization.CompareInfo m_info;
        private bool m_checkDigitToken;
        private static char[] WhiteSpaceChecks;
        internal __DTString(string str, DateTimeFormatInfo dtfi, bool checkDigitToken) : this(str, dtfi)
        {
            this.m_checkDigitToken = checkDigitToken;
        }

        internal __DTString(string str, DateTimeFormatInfo dtfi)
        {
            this.Index = -1;
            this.Value = str;
            this.len = this.Value.Length;
            this.m_current = '\0';
            if (dtfi != null)
            {
                this.m_info = dtfi.CompareInfo;
                this.m_checkDigitToken = (dtfi.FormatFlags & DateTimeFormatFlags.UseDigitPrefixInTokens) != DateTimeFormatFlags.None;
            }
            else
            {
                this.m_info = Thread.CurrentThread.CurrentCulture.CompareInfo;
                this.m_checkDigitToken = false;
            }
        }

        internal System.Globalization.CompareInfo CompareInfo
        {
            get
            {
                return this.m_info;
            }
        }
        internal bool GetNext()
        {
            this.Index++;
            if (this.Index < this.len)
            {
                this.m_current = this.Value[this.Index];
                return true;
            }
            return false;
        }

        internal bool Advance(int count)
        {
            this.Index += count;
            if (this.Index < this.len)
            {
                this.m_current = this.Value[this.Index];
                return true;
            }
            return false;
        }

        [SecurityCritical]
        internal void GetRegularToken(out TokenType tokenType, out int tokenValue, DateTimeFormatInfo dtfi)
        {
            tokenValue = 0;
            if (this.Index >= this.len)
            {
                tokenType = TokenType.EndOfString;
                return;
            }
            tokenType = TokenType.UnknownToken;
        Label_0019:
            if (!DateTimeParse.IsDigit(this.m_current))
            {
                if (char.IsWhiteSpace(this.m_current))
                {
                    while (++this.Index < this.len)
                    {
                        this.m_current = this.Value[this.Index];
                        if (!char.IsWhiteSpace(this.m_current))
                        {
                            goto Label_0019;
                        }
                    }
                    tokenType = TokenType.EndOfString;
                    return;
                }
                dtfi.Tokenize(TokenType.RegularTokenMask, out tokenType, out tokenValue, ref this);
            }
            else
            {
                tokenValue = this.m_current - '0';
                int index = this.Index;
                while (++this.Index < this.len)
                {
                    this.m_current = this.Value[this.Index];
                    int num = this.m_current - '0';
                    if ((num < 0) || (num > 9))
                    {
                        break;
                    }
                    tokenValue = (tokenValue * 10) + num;
                }
                if ((this.Index - index) > 8)
                {
                    tokenType = TokenType.NumberToken;
                    tokenValue = -1;
                }
                else if ((this.Index - index) < 3)
                {
                    tokenType = TokenType.NumberToken;
                }
                else
                {
                    tokenType = TokenType.YearNumberToken;
                }
                if (this.m_checkDigitToken)
                {
                    TokenType type;
                    int num4;
                    int num3 = this.Index;
                    char current = this.m_current;
                    this.Index = index;
                    this.m_current = this.Value[this.Index];
                    if (dtfi.Tokenize(TokenType.RegularTokenMask, out type, out num4, ref this))
                    {
                        tokenType = type;
                        tokenValue = num4;
                        return;
                    }
                    this.Index = num3;
                    this.m_current = current;
                }
            }
        }

        [SecurityCritical]
        internal TokenType GetSeparatorToken(DateTimeFormatInfo dtfi, out int indexBeforeSeparator, out char charBeforeSeparator)
        {
            indexBeforeSeparator = this.Index;
            charBeforeSeparator = this.m_current;
            if (!this.SkipWhiteSpaceCurrent())
            {
                return TokenType.SEP_End;
            }
            if (!DateTimeParse.IsDigit(this.m_current))
            {
                TokenType type;
                int num;
                if (!dtfi.Tokenize(TokenType.SeparatorTokenMask, out type, out num, ref this))
                {
                    type = TokenType.SEP_Space;
                }
                return type;
            }
            return TokenType.SEP_Space;
        }

        internal bool MatchSpecifiedWord(string target)
        {
            return this.MatchSpecifiedWord(target, target.Length + this.Index);
        }

        internal bool MatchSpecifiedWord(string target, int endIndex)
        {
            int num = endIndex - this.Index;
            if (num != target.Length)
            {
                return false;
            }
            if ((this.Index + num) > this.len)
            {
                return false;
            }
            return (this.m_info.Compare(this.Value, this.Index, num, target, 0, num, CompareOptions.IgnoreCase) == 0);
        }

        internal bool MatchSpecifiedWords(string target, bool checkWordBoundary, ref int matchLength)
        {
            int num = this.Value.Length - this.Index;
            matchLength = target.Length;
            if ((matchLength > num) || (this.m_info.Compare(this.Value, this.Index, matchLength, target, 0, matchLength, CompareOptions.IgnoreCase) != 0))
            {
                int startIndex = 0;
                int index = this.Index;
                int num4 = target.IndexOfAny(WhiteSpaceChecks, startIndex);
                if (num4 == -1)
                {
                    return false;
                }
                do
                {
                    int num5 = num4 - startIndex;
                    if (index >= (this.Value.Length - num5))
                    {
                        return false;
                    }
                    if (num5 == 0)
                    {
                        matchLength--;
                    }
                    else
                    {
                        if (!char.IsWhiteSpace(this.Value[index + num5]))
                        {
                            return false;
                        }
                        if (this.m_info.Compare(this.Value, index, num5, target, startIndex, num5, CompareOptions.IgnoreCase) != 0)
                        {
                            return false;
                        }
                        index = (index + num5) + 1;
                    }
                    startIndex = num4 + 1;
                    while ((index < this.Value.Length) && char.IsWhiteSpace(this.Value[index]))
                    {
                        index++;
                        matchLength++;
                    }
                }
                while ((num4 = target.IndexOfAny(WhiteSpaceChecks, startIndex)) >= 0);
                if (startIndex < target.Length)
                {
                    int num6 = target.Length - startIndex;
                    if (index > (this.Value.Length - num6))
                    {
                        return false;
                    }
                    if (this.m_info.Compare(this.Value, index, num6, target, startIndex, num6, CompareOptions.IgnoreCase) != 0)
                    {
                        return false;
                    }
                }
            }
            if (checkWordBoundary)
            {
                int num7 = this.Index + matchLength;
                if ((num7 < this.Value.Length) && char.IsLetter(this.Value[num7]))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool Match(string str)
        {
            if (++this.Index < this.len)
            {
                if (str.Length > (this.Value.Length - this.Index))
                {
                    return false;
                }
                if (this.m_info.Compare(this.Value, this.Index, str.Length, str, 0, str.Length, CompareOptions.Ordinal) == 0)
                {
                    this.Index += str.Length - 1;
                    return true;
                }
            }
            return false;
        }

        internal bool Match(char ch)
        {
            if (++this.Index < this.len)
            {
                if (this.Value[this.Index] == ch)
                {
                    this.m_current = ch;
                    return true;
                }
                this.Index--;
            }
            return false;
        }

        internal int MatchLongestWords(string[] words, ref int maxMatchStrLen)
        {
            int num = -1;
            for (int i = 0; i < words.Length; i++)
            {
                string target = words[i];
                int length = target.Length;
                if (this.MatchSpecifiedWords(target, false, ref length) && (length > maxMatchStrLen))
                {
                    maxMatchStrLen = length;
                    num = i;
                }
            }
            return num;
        }

        internal int GetRepeatCount()
        {
            char ch = this.Value[this.Index];
            int num = this.Index + 1;
            while ((num < this.len) && (this.Value[num] == ch))
            {
                num++;
            }
            int num2 = num - this.Index;
            this.Index = num - 1;
            return num2;
        }

        internal bool GetNextDigit()
        {
            if (++this.Index >= this.len)
            {
                return false;
            }
            return DateTimeParse.IsDigit(this.Value[this.Index]);
        }

        internal char GetChar()
        {
            return this.Value[this.Index];
        }

        internal int GetDigit()
        {
            return (this.Value[this.Index] - '0');
        }

        internal void SkipWhiteSpaces()
        {
            while ((this.Index + 1) < this.len)
            {
                char c = this.Value[this.Index + 1];
                if (!char.IsWhiteSpace(c))
                {
                    return;
                }
                this.Index++;
            }
        }

        internal bool SkipWhiteSpaceCurrent()
        {
            if (this.Index >= this.len)
            {
                return false;
            }
            if (char.IsWhiteSpace(this.m_current))
            {
                while (++this.Index < this.len)
                {
                    this.m_current = this.Value[this.Index];
                    if (!char.IsWhiteSpace(this.m_current))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        internal void TrimTail()
        {
            int num = this.len - 1;
            while ((num >= 0) && char.IsWhiteSpace(this.Value[num]))
            {
                num--;
            }
            this.Value = this.Value.Substring(0, num + 1);
            this.len = this.Value.Length;
        }

        internal void RemoveTrailingInQuoteSpaces()
        {
            int startIndex = this.len - 1;
            if (startIndex > 1)
            {
                char ch = this.Value[startIndex];
                if (((ch == '\'') || (ch == '"')) && char.IsWhiteSpace(this.Value[startIndex - 1]))
                {
                    startIndex--;
                    while ((startIndex >= 1) && char.IsWhiteSpace(this.Value[startIndex - 1]))
                    {
                        startIndex--;
                    }
                    this.Value = this.Value.Remove(startIndex, (this.Value.Length - 1) - startIndex);
                    this.len = this.Value.Length;
                }
            }
        }

        internal void RemoveLeadingInQuoteSpaces()
        {
            if (this.len > 2)
            {
                int count = 0;
                switch (this.Value[count])
                {
                    case '\'':
                    case '"':
                        while (((count + 1) < this.len) && char.IsWhiteSpace(this.Value[count + 1]))
                        {
                            count++;
                        }
                        if (count != 0)
                        {
                            this.Value = this.Value.Remove(1, count);
                            this.len = this.Value.Length;
                        }
                        break;
                }
            }
        }

        internal DTSubString GetSubString()
        {
            DTSubString str = new DTSubString {
                index = this.Index,
                s = this.Value
            };
            while ((this.Index + str.length) < this.len)
            {
                DTSubStringType number;
                char ch = this.Value[this.Index + str.length];
                if ((ch >= '0') && (ch <= '9'))
                {
                    number = DTSubStringType.Number;
                }
                else
                {
                    number = DTSubStringType.Other;
                }
                if (str.length == 0)
                {
                    str.type = number;
                }
                else if (str.type != number)
                {
                    break;
                }
                str.length++;
                if (number != DTSubStringType.Number)
                {
                    break;
                }
                if (str.length > 8)
                {
                    str.type = DTSubStringType.Invalid;
                    return str;
                }
                int num = ch - '0';
                str.value = (str.value * 10) + num;
            }
            if (str.length == 0)
            {
                str.type = DTSubStringType.End;
                return str;
            }
            return str;
        }

        internal void ConsumeSubString(DTSubString sub)
        {
            this.Index = sub.index + sub.length;
            if (this.Index < this.len)
            {
                this.m_current = this.Value[this.Index];
            }
        }

        static __DTString()
        {
            WhiteSpaceChecks = new char[] { ' ', '\x00a0' };
        }
    }
}

