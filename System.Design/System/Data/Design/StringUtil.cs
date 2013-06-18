namespace System.Data.Design
{
    using System;

    internal sealed class StringUtil
    {
        private StringUtil()
        {
        }

        internal static bool Empty(string str)
        {
            if (str != null)
            {
                return (0 >= str.Length);
            }
            return true;
        }

        internal static bool EmptyOrSpace(string str)
        {
            if (str != null)
            {
                return (0 >= str.Trim().Length);
            }
            return true;
        }

        internal static bool EqualValue(string str1, string str2)
        {
            return EqualValue(str1, str2, false);
        }

        internal static bool EqualValue(string str1, string str2, bool caseInsensitive)
        {
            if ((str1 != null) && (str2 != null))
            {
                StringComparison comparisonType = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return string.Equals(str1, str2, comparisonType);
            }
            return (str1 == str2);
        }

        internal static bool NotEmpty(string str)
        {
            return !Empty(str);
        }

        public static bool NotEmptyAfterTrim(string str)
        {
            return !EmptyOrSpace(str);
        }
    }
}

