namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.SqlTypes;

    internal interface ITypedSetters
    {
        void SetBoolean(int ordinal, bool value);
        void SetByte(int ordinal, byte value);
        void SetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length);
        void SetChar(int ordinal, char value);
        void SetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length);
        void SetDateTime(int ordinal, DateTime value);
        void SetDBNull(int ordinal);
        void SetDecimal(int ordinal, decimal value);
        void SetDouble(int ordinal, double value);
        void SetFloat(int ordinal, float value);
        void SetGuid(int ordinal, Guid value);
        void SetInt16(int ordinal, short value);
        void SetInt32(int ordinal, int value);
        void SetInt64(int ordinal, long value);
        [Obsolete("Not supported as of SMI v2.  Will be removed when v1 support dropped.  Use setter with offset.")]
        void SetSqlBinary(int ordinal, SqlBinary value);
        void SetSqlBinary(int ordinal, SqlBinary value, int offset);
        void SetSqlBoolean(int ordinal, SqlBoolean value);
        void SetSqlByte(int ordinal, SqlByte value);
        [Obsolete("Not supported as of SMI v2.  Will be removed when v1 support dropped.  Use setter with offset.")]
        void SetSqlBytes(int ordinal, SqlBytes value);
        void SetSqlBytes(int ordinal, SqlBytes value, int offset);
        [Obsolete("Not supported as of SMI v2.  Will be removed when v1 support dropped.  Use setter with offset.")]
        void SetSqlChars(int ordinal, SqlChars value);
        void SetSqlChars(int ordinal, SqlChars value, int offset);
        void SetSqlDateTime(int ordinal, SqlDateTime value);
        void SetSqlDecimal(int ordinal, SqlDecimal value);
        void SetSqlDouble(int ordinal, SqlDouble value);
        void SetSqlGuid(int ordinal, SqlGuid value);
        void SetSqlInt16(int ordinal, SqlInt16 value);
        void SetSqlInt32(int ordinal, SqlInt32 value);
        void SetSqlInt64(int ordinal, SqlInt64 value);
        void SetSqlMoney(int ordinal, SqlMoney value);
        void SetSqlSingle(int ordinal, SqlSingle value);
        [Obsolete("Not supported as of SMI v2.  Will be removed when v1 support dropped.  Use setter with offset.")]
        void SetSqlString(int ordinal, SqlString value);
        void SetSqlString(int ordinal, SqlString value, int offset);
        void SetSqlXml(int ordinal, SqlXml value);
        [Obsolete("Not supported as of SMI v2.  Will be removed when v1 support dropped.  Use setter with offset.")]
        void SetString(int ordinal, string value);
        void SetString(int ordinal, string value, int offset);
    }
}

