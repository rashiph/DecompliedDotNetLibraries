namespace Microsoft.Build.Shared.LanguageParser
{
    using System;
    using System.Globalization;

    internal static class TokenChar
    {
        internal static bool IsCombining(char c)
        {
            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
            if ((unicodeCategory != UnicodeCategory.NonSpacingMark) && (unicodeCategory != UnicodeCategory.SpacingCombiningMark))
            {
                return false;
            }
            return true;
        }

        internal static bool IsConnecting(char c)
        {
            return (char.GetUnicodeCategory(c) == UnicodeCategory.ConnectorPunctuation);
        }

        internal static bool IsDecimalDigit(char c)
        {
            return (char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber);
        }

        internal static bool IsFormatting(char c)
        {
            return (char.GetUnicodeCategory(c) == UnicodeCategory.Format);
        }

        internal static bool IsHexDigit(char c)
        {
            if ((((c < '0') || (c > '9')) && ((c < 'A') || (c > 'F'))) && ((c < 'a') || (c > 'f')))
            {
                return false;
            }
            return true;
        }

        internal static bool IsLetter(char c)
        {
            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
            if ((((unicodeCategory != UnicodeCategory.UppercaseLetter) && (unicodeCategory != UnicodeCategory.LowercaseLetter)) && ((unicodeCategory != UnicodeCategory.TitlecaseLetter) && (unicodeCategory != UnicodeCategory.ModifierLetter))) && ((unicodeCategory != UnicodeCategory.OtherLetter) && (unicodeCategory != UnicodeCategory.LetterNumber)))
            {
                return false;
            }
            return true;
        }

        internal static bool IsNewLine(char c)
        {
            if (((c != '\r') && (c != '\n')) && (c != '\u2028'))
            {
                return (c == '\u2029');
            }
            return true;
        }

        internal static bool IsOctalDigit(char c)
        {
            return ((c >= '0') && (c <= '7'));
        }
    }
}

