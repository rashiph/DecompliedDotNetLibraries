namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.SqlTypes;

    internal interface ITypedGettersV3
    {
        bool GetBoolean(SmiEventSink sink, int ordinal);
        byte GetByte(SmiEventSink sink, int ordinal);
        int GetBytes(SmiEventSink sink, int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length);
        long GetBytesLength(SmiEventSink sink, int ordinal);
        int GetChars(SmiEventSink sink, int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length);
        long GetCharsLength(SmiEventSink sink, int ordinal);
        DateTime GetDateTime(SmiEventSink sink, int ordinal);
        double GetDouble(SmiEventSink sink, int ordinal);
        Guid GetGuid(SmiEventSink sink, int ordinal);
        short GetInt16(SmiEventSink sink, int ordinal);
        int GetInt32(SmiEventSink sink, int ordinal);
        long GetInt64(SmiEventSink sink, int ordinal);
        float GetSingle(SmiEventSink sink, int ordinal);
        SqlDecimal GetSqlDecimal(SmiEventSink sink, int ordinal);
        string GetString(SmiEventSink sink, int ordinal);
        SmiMetaData GetVariantType(SmiEventSink sink, int ordinal);
        bool IsDBNull(SmiEventSink sink, int ordinal);
    }
}

