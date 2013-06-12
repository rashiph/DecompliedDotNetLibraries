namespace System.Data.SqlTypes
{
    using System;

    [Serializable, Flags]
    public enum SqlCompareOptions
    {
        BinarySort = 0x8000,
        BinarySort2 = 0x4000,
        IgnoreCase = 1,
        IgnoreKanaType = 8,
        IgnoreNonSpace = 2,
        IgnoreWidth = 0x10,
        None = 0
    }
}

