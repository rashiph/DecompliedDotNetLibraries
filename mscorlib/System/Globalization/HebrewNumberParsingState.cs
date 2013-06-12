namespace System.Globalization
{
    using System;

    internal enum HebrewNumberParsingState
    {
        InvalidHebrewNumber,
        NotHebrewDigit,
        FoundEndOfHebrewNumber,
        ContinueParsing
    }
}

