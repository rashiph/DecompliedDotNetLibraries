namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum DateTimeStyles
    {
        AdjustToUniversal = 0x10,
        AllowInnerWhite = 4,
        AllowLeadingWhite = 1,
        AllowTrailingWhite = 2,
        AllowWhiteSpaces = 7,
        AssumeLocal = 0x20,
        AssumeUniversal = 0x40,
        NoCurrentDateDefault = 8,
        None = 0,
        RoundtripKind = 0x80
    }
}

