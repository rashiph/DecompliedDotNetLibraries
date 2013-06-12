namespace Microsoft.SqlServer.Server
{
    using System;

    internal enum ExtendedClrTypeCode
    {
        Boolean = 0,
        Byte = 1,
        ByteArray = 0x12,
        Char = 2,
        CharArray = 0x13,
        DataTable = 0x25,
        DateTime = 3,
        DateTimeOffset = 0x29,
        DbDataReader = 0x26,
        DBNull = 4,
        Decimal = 5,
        Double = 6,
        Empty = 7,
        First = 0,
        Guid = 20,
        IEnumerableOfSqlDataRecord = 0x27,
        Int16 = 8,
        Int32 = 9,
        Int64 = 10,
        Invalid = -1,
        Last = 0x29,
        Object = 0x11,
        SByte = 11,
        Single = 12,
        SqlBinary = 0x15,
        SqlBoolean = 0x16,
        SqlByte = 0x17,
        SqlBytes = 0x23,
        SqlChars = 0x22,
        SqlDateTime = 0x18,
        SqlDecimal = 0x1f,
        SqlDouble = 0x19,
        SqlGuid = 0x1a,
        SqlInt16 = 0x1b,
        SqlInt32 = 0x1c,
        SqlInt64 = 0x1d,
        SqlMoney = 30,
        SqlSingle = 0x20,
        SqlString = 0x21,
        SqlXml = 0x24,
        String = 13,
        TimeSpan = 40,
        UInt16 = 14,
        UInt32 = 15,
        UInt64 = 0x10
    }
}

