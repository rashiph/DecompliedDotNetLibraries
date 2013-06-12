namespace System.Xml.Schema
{
    using System;

    [Flags]
    internal enum RestrictionFlags
    {
        Enumeration = 0x10,
        FractionDigits = 0x800,
        Length = 1,
        MaxExclusive = 0x80,
        MaxInclusive = 0x40,
        MaxLength = 4,
        MinExclusive = 0x200,
        MinInclusive = 0x100,
        MinLength = 2,
        Pattern = 8,
        TotalDigits = 0x400,
        WhiteSpace = 0x20
    }
}

