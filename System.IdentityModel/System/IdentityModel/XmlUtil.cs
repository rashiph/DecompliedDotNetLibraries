namespace System.IdentityModel
{
    using System;

    internal static class XmlUtil
    {
        public const string XmlNs = "http://www.w3.org/XML/1998/namespace";
        public const string XmlNsNs = "http://www.w3.org/2000/xmlns/";

        public static bool IsWhitespace(char ch)
        {
            if (((ch != ' ') && (ch != '\t')) && (ch != '\r'))
            {
                return (ch == '\n');
            }
            return true;
        }

        public static string Trim(string s)
        {
            int startIndex = 0;
            while ((startIndex < s.Length) && IsWhitespace(s[startIndex]))
            {
                startIndex++;
            }
            if (startIndex >= s.Length)
            {
                return string.Empty;
            }
            int length = s.Length;
            while ((length > 0) && IsWhitespace(s[length - 1]))
            {
                length--;
            }
            if ((startIndex == 0) && (length == s.Length))
            {
                return s;
            }
            return s.Substring(startIndex, length - startIndex);
        }

        public static string TrimEnd(string s)
        {
            int length = s.Length;
            while ((length > 0) && IsWhitespace(s[length - 1]))
            {
                length--;
            }
            if (length != s.Length)
            {
                return s.Substring(0, length);
            }
            return s;
        }

        public static string TrimStart(string s)
        {
            int startIndex = 0;
            while ((startIndex < s.Length) && IsWhitespace(s[startIndex]))
            {
                startIndex++;
            }
            if (startIndex != 0)
            {
                return s.Substring(startIndex);
            }
            return s;
        }
    }
}

