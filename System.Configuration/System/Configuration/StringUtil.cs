namespace System.Configuration
{
    using System;

    internal static class StringUtil
    {
        internal static bool EqualsIgnoreCase(string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EqualsNE(string s1, string s2)
        {
            if (s1 == null)
            {
                s1 = string.Empty;
            }
            if (s2 == null)
            {
                s2 = string.Empty;
            }
            return string.Equals(s1, s2, StringComparison.Ordinal);
        }

        internal static string[] ObjectArrayToStringArray(object[] objectArray)
        {
            string[] array = new string[objectArray.Length];
            objectArray.CopyTo(array, 0);
            return array;
        }

        internal static bool StartsWith(string s1, string s2)
        {
            if (s2 == null)
            {
                return false;
            }
            return (0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.Ordinal));
        }

        internal static bool StartsWithIgnoreCase(string s1, string s2)
        {
            if (s2 == null)
            {
                return false;
            }
            return (0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase));
        }
    }
}

