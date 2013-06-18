namespace Microsoft.Build.Shared
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class EscapingUtilities
    {
        private static char[] charsToEscape = new char[] { '%', '*', '?', '@', '$', '(', ')', ';', '\'' };

        internal static bool ContainsEscapedWildcards(string escapedString)
        {
            if ((-1 == escapedString.IndexOf('%')) || ((-1 == escapedString.IndexOf("%2", StringComparison.Ordinal)) && (-1 == escapedString.IndexOf("%3", StringComparison.Ordinal))))
            {
                return false;
            }
            if (((-1 == escapedString.IndexOf("%2a", StringComparison.Ordinal)) && (-1 == escapedString.IndexOf("%2A", StringComparison.Ordinal))) && (-1 == escapedString.IndexOf("%3f", StringComparison.Ordinal)))
            {
                return (-1 != escapedString.IndexOf("%3F", StringComparison.Ordinal));
            }
            return true;
        }

        private static bool ContainsReservedCharacters(string unescapedString)
        {
            return (-1 != unescapedString.IndexOfAny(charsToEscape));
        }

        internal static string Escape(string unescapedString)
        {
            ErrorUtilities.VerifyThrowArgumentNull(unescapedString, "unescapedString");
            if (!ContainsReservedCharacters(unescapedString))
            {
                return unescapedString;
            }
            StringBuilder builder = new StringBuilder(unescapedString, unescapedString.Length * 2);
            foreach (char ch in charsToEscape)
            {
                int num = Convert.ToInt32(ch);
                string newValue = string.Format(CultureInfo.InvariantCulture, "%{0:x00}", new object[] { num });
                builder.Replace(ch.ToString(CultureInfo.InvariantCulture), newValue);
            }
            return builder.ToString();
        }

        internal static string UnescapeAll(string escapedString)
        {
            bool flag;
            return UnescapeAll(escapedString, out flag);
        }

        internal static string UnescapeAll(string escapedString, out bool escapingWasNecessary)
        {
            ErrorUtilities.VerifyThrow(escapedString != null, "Null strings not allowed.");
            escapingWasNecessary = false;
            int index = escapedString.IndexOf('%');
            if (index == -1)
            {
                return escapedString;
            }
            StringBuilder builder = new StringBuilder(escapedString.Length);
            int startIndex = 0;
            while (index != -1)
            {
                if (((index <= (escapedString.Length - 3)) && Uri.IsHexDigit(escapedString[index + 1])) && Uri.IsHexDigit(escapedString[index + 2]))
                {
                    builder.Append(escapedString, startIndex, index - startIndex);
                    char ch = (char) int.Parse(escapedString.Substring(index + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    builder.Append(ch);
                    startIndex = index + 3;
                    escapingWasNecessary = true;
                }
                index = escapedString.IndexOf('%', index + 1);
            }
            builder.Append(escapedString, startIndex, escapedString.Length - startIndex);
            return builder.ToString();
        }
    }
}

