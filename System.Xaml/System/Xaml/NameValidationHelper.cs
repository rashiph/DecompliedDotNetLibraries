namespace System.Xaml
{
    using System;
    using System.Globalization;

    internal static class NameValidationHelper
    {
        internal static bool IsValidIdentifierName(string name)
        {
            for (int i = 0; i < name.Length; i++)
            {
                UnicodeCategory unicodeCategory = char.GetUnicodeCategory(name[i]);
                bool flag = ((((unicodeCategory == UnicodeCategory.UppercaseLetter) || (unicodeCategory == UnicodeCategory.LowercaseLetter)) || ((unicodeCategory == UnicodeCategory.TitlecaseLetter) || (unicodeCategory == UnicodeCategory.OtherLetter))) || (unicodeCategory == UnicodeCategory.LetterNumber)) || (name[i] == '_');
                bool flag2 = (((unicodeCategory == UnicodeCategory.NonSpacingMark) || (unicodeCategory == UnicodeCategory.SpacingCombiningMark)) || (unicodeCategory == UnicodeCategory.ModifierLetter)) || (unicodeCategory == UnicodeCategory.DecimalDigitNumber);
                if (i == 0)
                {
                    if (!flag)
                    {
                        return false;
                    }
                }
                else if (!flag && !flag2)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

