namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;

    internal abstract class SmiRecordBuffer : SmiTypedGetterSetter, ITypedGettersV3, ITypedSettersV3, ITypedGetters, ITypedSetters, IDisposable
    {
        protected SmiRecordBuffer()
        {
        }

        public virtual void Close(SmiEventSink eventSink)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void Dispose()
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual bool GetBoolean(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual byte GetByte(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual char GetChar(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual long GetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual DateTime GetDateTime(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual decimal GetDecimal(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual double GetDouble(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual float GetFloat(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual Guid GetGuid(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual short GetInt16(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual int GetInt32(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual long GetInt64(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlBinary GetSqlBinary(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlBoolean GetSqlBoolean(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlByte GetSqlByte(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlBytes GetSqlBytes(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlBytes GetSqlBytesRef(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlChars GetSqlChars(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlChars GetSqlCharsRef(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlDateTime GetSqlDateTime(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlDecimal GetSqlDecimal(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlDouble GetSqlDouble(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlGuid GetSqlGuid(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlInt16 GetSqlInt16(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlInt32 GetSqlInt32(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlInt64 GetSqlInt64(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlMoney GetSqlMoney(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlSingle GetSqlSingle(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlString GetSqlString(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlXml GetSqlXml(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlXml GetSqlXmlRef(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual string GetString(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlDbType GetVariantType(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual bool IsDBNull(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetBoolean(int ordinal, bool value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetByte(int ordinal, byte value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetChar(int ordinal, char value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetDateTime(int ordinal, DateTime value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetDBNull(int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetDecimal(int ordinal, decimal value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetDouble(int ordinal, double value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetFloat(int ordinal, float value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetGuid(int ordinal, Guid value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetInt16(int ordinal, short value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetInt32(int ordinal, int value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetInt64(int ordinal, long value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlBinary(int ordinal, SqlBinary value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlBinary(int ordinal, SqlBinary value, int offset)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlBoolean(int ordinal, SqlBoolean value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlByte(int ordinal, SqlByte value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlBytes(int ordinal, SqlBytes value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlBytes(int ordinal, SqlBytes value, int offset)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlChars(int ordinal, SqlChars value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlChars(int ordinal, SqlChars value, int offset)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlDateTime(int ordinal, SqlDateTime value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlDecimal(int ordinal, SqlDecimal value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlDouble(int ordinal, SqlDouble value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlGuid(int ordinal, SqlGuid value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlInt16(int ordinal, SqlInt16 value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlInt32(int ordinal, SqlInt32 value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlInt64(int ordinal, SqlInt64 value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlMoney(int ordinal, SqlMoney value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlSingle(int ordinal, SqlSingle value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlString(int ordinal, SqlString value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlString(int ordinal, SqlString value, int offset)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlXml(int ordinal, SqlXml value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetString(int ordinal, string value)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetString(int ordinal, string value, int offset)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal override bool CanGet
        {
            get
            {
                return true;
            }
        }

        internal override bool CanSet
        {
            get
            {
                return true;
            }
        }
    }
}

