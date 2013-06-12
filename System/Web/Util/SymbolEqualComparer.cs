namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.Globalization;

    internal class SymbolEqualComparer : IComparer
    {
        internal static readonly IComparer Default = new SymbolEqualComparer();

        internal SymbolEqualComparer()
        {
        }

        int IComparer.Compare(object keyLeft, object keyRight)
        {
            string str = keyLeft as string;
            string str2 = keyRight as string;
            if (str == null)
            {
                throw new ArgumentNullException("keyLeft");
            }
            if (str2 == null)
            {
                throw new ArgumentNullException("keyRight");
            }
            int length = str.Length;
            int num2 = str2.Length;
            if (length != num2)
            {
                return 1;
            }
            for (int i = 0; i < length; i++)
            {
                char c = str[i];
                char ch2 = str2[i];
                if (c == ch2)
                {
                    continue;
                }
                UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
                UnicodeCategory category2 = char.GetUnicodeCategory(ch2);
                if ((unicodeCategory == UnicodeCategory.UppercaseLetter) && (category2 == UnicodeCategory.LowercaseLetter))
                {
                    if (char.ToLower(c, CultureInfo.InvariantCulture) != ch2)
                    {
                        goto Label_00A3;
                    }
                    continue;
                }
                if (((category2 == UnicodeCategory.UppercaseLetter) && (unicodeCategory == UnicodeCategory.LowercaseLetter)) && (char.ToLower(ch2, CultureInfo.InvariantCulture) == c))
                {
                    continue;
                }
            Label_00A3:
                return 1;
            }
            return 0;
        }
    }
}

