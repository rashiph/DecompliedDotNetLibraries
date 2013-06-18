namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.Data.SqlTypes;

    internal interface ITypedGetters
    {
        bool GetBoolean(int ordinal);
        byte GetByte(int ordinal);
        long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length);
        char GetChar(int ordinal);
        long GetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length);
        DateTime GetDateTime(int ordinal);
        decimal GetDecimal(int ordinal);
        double GetDouble(int ordinal);
        float GetFloat(int ordinal);
        Guid GetGuid(int ordinal);
        short GetInt16(int ordinal);
        int GetInt32(int ordinal);
        long GetInt64(int ordinal);
        SqlBinary GetSqlBinary(int ordinal);
        SqlBoolean GetSqlBoolean(int ordinal);
        SqlByte GetSqlByte(int ordinal);
        SqlBytes GetSqlBytes(int ordinal);
        SqlBytes GetSqlBytesRef(int ordinal);
        SqlChars GetSqlChars(int ordinal);
        SqlChars GetSqlCharsRef(int ordinal);
        SqlDateTime GetSqlDateTime(int ordinal);
        SqlDecimal GetSqlDecimal(int ordinal);
        SqlDouble GetSqlDouble(int ordinal);
        SqlGuid GetSqlGuid(int ordinal);
        SqlInt16 GetSqlInt16(int ordinal);
        SqlInt32 GetSqlInt32(int ordinal);
        SqlInt64 GetSqlInt64(int ordinal);
        SqlMoney GetSqlMoney(int ordinal);
        SqlSingle GetSqlSingle(int ordinal);
        SqlString GetSqlString(int ordinal);
        SqlXml GetSqlXml(int ordinal);
        SqlXml GetSqlXmlRef(int ordinal);
        string GetString(int ordinal);
        SqlDbType GetVariantType(int ordinal);
        bool IsDBNull(int ordinal);
    }
}

