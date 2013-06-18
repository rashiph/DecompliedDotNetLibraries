namespace System.Data.OracleClient
{
    using System;

    public enum OracleType
    {
        BFile = 1,
        Blob = 2,
        Byte = 0x17,
        Char = 3,
        Clob = 4,
        Cursor = 5,
        DateTime = 6,
        Double = 30,
        Float = 0x1d,
        Int16 = 0x1b,
        Int32 = 0x1c,
        IntervalDayToSecond = 7,
        IntervalYearToMonth = 8,
        LongRaw = 9,
        LongVarChar = 10,
        NChar = 11,
        NClob = 12,
        Number = 13,
        NVarChar = 14,
        Raw = 15,
        RowId = 0x10,
        SByte = 0x1a,
        Timestamp = 0x12,
        TimestampLocal = 0x13,
        TimestampWithTZ = 20,
        UInt16 = 0x18,
        UInt32 = 0x19,
        VarChar = 0x16
    }
}

