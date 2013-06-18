namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xaml;

    [DebuggerDisplay("{Text}")]
    internal class XamlText
    {
        private readonly bool _isSpacePreserve;
        private bool _isWhiteSpaceOnly;
        private StringBuilder _sb = new StringBuilder();
        private const char CLOSECURLIE = '}';
        private static CodePointRange[] EastAsianCodePointRanges = new CodePointRange[] { new CodePointRange(0x1100, 0x11ff), new CodePointRange(0x2e80, 0x2fd5), new CodePointRange(0x2ff0, 0x2ffb), new CodePointRange(0x3040, 0x319f), new CodePointRange(0x31f0, 0xa4cf), new CodePointRange(0xac00, 0xd7a3), new CodePointRange(0xf900, 0xfaff), new CodePointRange(0xff00, 0xffef), new CodePointRange(0x20000, 0x2a6d6), new CodePointRange(0x2f800, 0x2fa1d) };
        private const string ME_ESCAPE = "{}";
        private const char NEWLINE = '\n';
        private const char OPENCURLIE = '{';
        private const char RETURN = '\r';
        private const string RETURN_STRING = "\r";
        private const char SPACE = ' ';
        private const char TAB = '\t';

        public XamlText(bool spacePreserve)
        {
            this._isSpacePreserve = spacePreserve;
            this._isWhiteSpaceOnly = true;
        }

        private static string CollapseWhitespace(string text)
        {
            StringBuilder builder = new StringBuilder(text.Length);
            int start = 0;
            while (start < text.Length)
            {
                char ch = text[start];
                if (!IsWhitespaceChar(ch))
                {
                    builder.Append(ch);
                    start++;
                    continue;
                }
                int end = start;
                while (++end < text.Length)
                {
                    if (!IsWhitespaceChar(text[end]))
                    {
                        break;
                    }
                }
                if ((start != 0) && (end != text.Length))
                {
                    bool flag = false;
                    if (((ch == '\n') && ((end - start) == 2)) && ((text[start - 1] >= 'ᄀ') && HasSurroundingEastAsianChars(start, end, text)))
                    {
                        flag = true;
                    }
                    if (!flag)
                    {
                        builder.Append(' ');
                    }
                }
                start = end;
            }
            return builder.ToString();
        }

        private static int ComputeUnicodeScalarValue(int takeOneIdx, int takeTwoIdx, string text)
        {
            int num = 0;
            bool flag = false;
            char c = text[takeTwoIdx];
            if (char.IsHighSurrogate(c))
            {
                char ch2 = text[takeTwoIdx + 1];
                if (char.IsLowSurrogate(ch2))
                {
                    flag = true;
                    num = (((c & 0x3ff) << 10) | (ch2 & 'Ͽ')) + 0x1000;
                }
            }
            if (!flag)
            {
                num = text[takeOneIdx];
            }
            return num;
        }

        private static bool HasSurroundingEastAsianChars(int start, int end, string text)
        {
            int num;
            if ((start - 2) < 0)
            {
                num = text[0];
            }
            else
            {
                num = ComputeUnicodeScalarValue(start - 1, start - 2, text);
            }
            if (IsEastAsianCodePoint(num))
            {
                int num2;
                if ((end + 1) >= text.Length)
                {
                    num2 = text[end];
                }
                else
                {
                    num2 = ComputeUnicodeScalarValue(end, end, text);
                }
                if (IsEastAsianCodePoint(num2))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsEastAsianCodePoint(int unicodeScalarValue)
        {
            for (int i = 0; i < EastAsianCodePointRanges.Length; i++)
            {
                if ((unicodeScalarValue >= EastAsianCodePointRanges[i].Min) && (unicodeScalarValue <= EastAsianCodePointRanges[i].Max))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsWhitespace(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (!IsWhitespaceChar(text[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsWhitespaceChar(char ch)
        {
            if (((ch != ' ') && (ch != '\t')) && (ch != '\n'))
            {
                return (ch == '\r');
            }
            return true;
        }

        public void Paste(string text, bool trimLeadingWhitespace)
        {
            bool flag = IsWhitespace(text);
            if (this._isSpacePreserve)
            {
                this._sb.Append(text.Replace("\r", ""));
            }
            else if (flag)
            {
                if (this.IsEmpty && !trimLeadingWhitespace)
                {
                    this._sb.Append(' ');
                }
            }
            else
            {
                bool flag2 = IsWhitespaceChar(text[0]);
                bool flag3 = IsWhitespaceChar(text[text.Length - 1]);
                bool flag4 = false;
                string str = CollapseWhitespace(text);
                if (this._sb.Length > 0)
                {
                    if (this._isWhiteSpaceOnly)
                    {
                        this._sb = new StringBuilder();
                    }
                    else if (IsWhitespaceChar(this._sb[this._sb.Length - 1]))
                    {
                        flag4 = true;
                    }
                }
                if ((flag2 && !trimLeadingWhitespace) && !flag4)
                {
                    this._sb.Append(' ');
                }
                this._sb.Append(str);
                if (flag3)
                {
                    this._sb.Append(' ');
                }
            }
            this._isWhiteSpaceOnly = this._isWhiteSpaceOnly && flag;
        }

        public static string TrimLeadingWhitespace(string source)
        {
            return source.TrimStart(new char[] { ' ', '\t', '\n' });
        }

        public static string TrimTrailingWhitespace(string source)
        {
            return source.TrimEnd(new char[] { ' ', '\t', '\n' });
        }

        public string AttributeText
        {
            get
            {
                string text = this.Text;
                if (text.StartsWith("{}", false, TypeConverterHelper.InvariantEnglishUS))
                {
                    return text.Remove(0, "{}".Length);
                }
                return text;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this._sb.Length == 0);
            }
        }

        public bool IsSpacePreserved
        {
            get
            {
                return this._isSpacePreserve;
            }
        }

        public bool IsWhiteSpaceOnly
        {
            get
            {
                return this._isWhiteSpaceOnly;
            }
        }

        public bool LooksLikeAMarkupExtension
        {
            get
            {
                int length = this._sb.Length;
                if ((length <= 0) || (this._sb[0] != '{'))
                {
                    return false;
                }
                if ((length > 1) && (this._sb[1] == '}'))
                {
                    return false;
                }
                return true;
            }
        }

        public string Text
        {
            get
            {
                return this._sb.ToString();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CodePointRange
        {
            public readonly int Min;
            public readonly int Max;
            public CodePointRange(int min, int max)
            {
                this.Min = min;
                this.Max = max;
            }
        }
    }
}

