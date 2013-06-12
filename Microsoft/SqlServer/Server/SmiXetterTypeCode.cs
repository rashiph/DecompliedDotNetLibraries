namespace Microsoft.SqlServer.Server
{
    using System;

    internal enum SmiXetterTypeCode
    {
        GetVariantMetaData = 13,
        GetXet = 14,
        XetBoolean = 0,
        XetByte = 1,
        XetBytes = 2,
        XetChars = 3,
        XetDateTime = 11,
        XetDateTimeOffset = 0x10,
        XetDouble = 9,
        XetGuid = 12,
        XetInt16 = 5,
        XetInt32 = 6,
        XetInt64 = 7,
        XetSingle = 8,
        XetSqlDecimal = 10,
        XetString = 4,
        XetTime = 15,
        XetTimeSpan = 15
    }
}

