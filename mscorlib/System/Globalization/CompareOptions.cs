namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum CompareOptions
    {
        IgnoreCase = 1,
        IgnoreKanaType = 8,
        IgnoreNonSpace = 2,
        IgnoreSymbols = 4,
        IgnoreWidth = 0x10,
        None = 0,
        Ordinal = 0x40000000,
        OrdinalIgnoreCase = 0x10000000,
        StringSort = 0x20000000
    }
}

