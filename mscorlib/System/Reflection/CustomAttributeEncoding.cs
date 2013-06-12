namespace System.Reflection
{
    using System;

    [Serializable]
    internal enum CustomAttributeEncoding
    {
        Array = 0x1d,
        Boolean = 2,
        Byte = 5,
        Char = 3,
        Double = 13,
        Enum = 0x55,
        Field = 0x53,
        Float = 12,
        Int16 = 6,
        Int32 = 8,
        Int64 = 10,
        Object = 0x51,
        Property = 0x54,
        SByte = 4,
        String = 14,
        Type = 80,
        UInt16 = 7,
        UInt32 = 9,
        UInt64 = 11,
        Undefined = 0
    }
}

