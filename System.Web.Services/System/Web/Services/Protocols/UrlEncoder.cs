namespace System.Web.Services.Protocols
{
    using System;
    using System.Text;

    internal class UrlEncoder
    {
        internal static readonly char[] HexUpperChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private const int Max16BitUtf8SequenceLength = 4;

        private UrlEncoder()
        {
        }

        internal static string EscapeString(string s, Encoding e)
        {
            return EscapeStringInternal(s, (e == null) ? new ASCIIEncoding() : e, false);
        }

        private static string EscapeStringInternal(string s, Encoding e, bool escapeUriStuff)
        {
            if (s == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(s);
            StringBuilder sb = new StringBuilder(bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
            {
                byte num2 = bytes[i];
                char ch = (char) num2;
                if (((num2 > 0x7f) || (num2 < 0x20)) || ((ch == '%') || (escapeUriStuff && !IsSafe(ch))))
                {
                    HexEscape8(sb, ch);
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        private static void HexEscape16(StringBuilder sb, char c)
        {
            sb.Append("%u");
            sb.Append(HexUpperChars[(c >> 12) & '\x000f']);
            sb.Append(HexUpperChars[(c >> 8) & '\x000f']);
            sb.Append(HexUpperChars[(c >> 4) & '\x000f']);
            sb.Append(HexUpperChars[c & '\x000f']);
        }

        private static void HexEscape8(StringBuilder sb, char c)
        {
            sb.Append('%');
            sb.Append(HexUpperChars[(c >> 4) & '\x000f']);
            sb.Append(HexUpperChars[c & '\x000f']);
        }

        private static bool IsSafe(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                case '\'':
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }

        internal static string UrlEscapeString(string s, Encoding e)
        {
            return EscapeStringInternal(s, (e == null) ? new ASCIIEncoding() : e, true);
        }

        internal static string UrlEscapeStringUnicode(string s)
        {
            int length = s.Length;
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                if (IsSafe(ch))
                {
                    sb.Append(ch);
                }
                else if (ch == ' ')
                {
                    sb.Append('+');
                }
                else if ((ch & 0xff80) == 0)
                {
                    HexEscape8(sb, ch);
                }
                else
                {
                    HexEscape16(sb, ch);
                }
            }
            return sb.ToString();
        }
    }
}

