namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.SqlTypes;

    internal interface ITypedSettersV3
    {
        void SetBoolean(SmiEventSink sink, int ordinal, bool value);
        void SetByte(SmiEventSink sink, int ordinal, byte value);
        int SetBytes(SmiEventSink sink, int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length);
        void SetBytesLength(SmiEventSink sink, int ordinal, long length);
        int SetChars(SmiEventSink sink, int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length);
        void SetCharsLength(SmiEventSink sink, int ordinal, long length);
        void SetDateTime(SmiEventSink sink, int ordinal, DateTime value);
        void SetDBNull(SmiEventSink sink, int ordinal);
        void SetDouble(SmiEventSink sink, int ordinal, double value);
        void SetGuid(SmiEventSink sink, int ordinal, Guid value);
        void SetInt16(SmiEventSink sink, int ordinal, short value);
        void SetInt32(SmiEventSink sink, int ordinal, int value);
        void SetInt64(SmiEventSink sink, int ordinal, long value);
        void SetSingle(SmiEventSink sink, int ordinal, float value);
        void SetSqlDecimal(SmiEventSink sink, int ordinal, SqlDecimal value);
        void SetString(SmiEventSink sink, int ordinal, string value, int offset, int length);
        void SetVariantMetaData(SmiEventSink sink, int ordinal, SmiMetaData metaData);
    }
}

