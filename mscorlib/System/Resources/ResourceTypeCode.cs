namespace System.Resources
{
    using System;

    [Serializable]
    internal enum ResourceTypeCode
    {
        Boolean = 2,
        Byte = 4,
        ByteArray = 0x20,
        Char = 3,
        DateTime = 15,
        Decimal = 14,
        Double = 13,
        Int16 = 6,
        Int32 = 8,
        Int64 = 10,
        LastPrimitive = 0x10,
        Null = 0,
        SByte = 5,
        Single = 12,
        StartOfUserTypes = 0x40,
        Stream = 0x21,
        String = 1,
        TimeSpan = 0x10,
        UInt16 = 7,
        UInt32 = 9,
        UInt64 = 11
    }
}

