namespace System.Globalization
{
    using System;

    internal enum FORMATFLAGS
    {
        None = 0,
        UseDigitPrefixInTokens = 0x20,
        UseGenitiveMonth = 1,
        UseHebrewParsing = 8,
        UseLeapYearMonth = 2,
        UseSpacesInDayNames = 0x10,
        UseSpacesInMonthNames = 4
    }
}

