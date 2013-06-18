namespace System.Runtime.Remoting.Channels
{
    using System;

    internal static class StringHelper
    {
        internal static bool StartsWithAsciiIgnoreCasePrefixLower(string str, string asciiPrefix)
        {
            int length = asciiPrefix.Length;
            if (str.Length < length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (ToLowerAscii(str[i]) != asciiPrefix[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool StartsWithDoubleUnderscore(string str)
        {
            if (str.Length < 2)
            {
                return false;
            }
            return ((str[0] == '_') && (str[1] == '_'));
        }

        private static char ToLowerAscii(char ch)
        {
            if ((ch >= 'A') && (ch <= 'Z'))
            {
                return (char) (ch + ' ');
            }
            return ch;
        }
    }
}

