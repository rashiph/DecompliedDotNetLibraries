namespace System.Globalization
{
    using System;

    [Flags]
    internal enum DateTimeFormatFlags
    {
        None = 0,
        NotInitialized = -1,
        UseDigitPrefixInTokens = 0x20,
        UseGenitiveMonth = 1,
        UseHebrewRule = 8,
        UseLeapYearMonth = 2,
        UseSpacesInDayNames = 0x10,
        UseSpacesInMonthNames = 4
    }
}

