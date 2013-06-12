namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum NumberStyles
    {
        AllowCurrencySymbol = 0x100,
        AllowDecimalPoint = 0x20,
        AllowExponent = 0x80,
        AllowHexSpecifier = 0x200,
        AllowLeadingSign = 4,
        AllowLeadingWhite = 1,
        AllowParentheses = 0x10,
        AllowThousands = 0x40,
        AllowTrailingSign = 8,
        AllowTrailingWhite = 2,
        Any = 0x1ff,
        Currency = 0x17f,
        Float = 0xa7,
        HexNumber = 0x203,
        Integer = 7,
        None = 0,
        Number = 0x6f
    }
}

