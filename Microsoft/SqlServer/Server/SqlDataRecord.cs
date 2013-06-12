namespace Microsoft.SqlServer.Server
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Reflection;

    public class SqlDataRecord : IDataRecord
    {
        private static readonly SmiMetaData __maxNVarCharForXml = new SmiMetaData(SqlDbType.NVarChar, -1L, SmiMetaData.DefaultNVarChar_NoCollation.Precision, SmiMetaData.DefaultNVarChar_NoCollation.Scale, SmiMetaData.DefaultNVarChar.LocaleId, SmiMetaData.DefaultNVarChar.CompareOptions, null);
        private SqlMetaData[] _columnMetaData;
        private SmiExtendedMetaData[] _columnSmiMetaData;
        private SmiEventSink_Default _eventSink;
        private FieldNameLookup _fieldNameLookup;
        private SmiRecordBuffer _recordBuffer;
        private SmiContext _recordContext;
        private bool _usesStringStorageForXml;

        public SqlDataRecord(params SqlMetaData[] metaData)
        {
            if (metaData == null)
            {
                throw ADP.ArgumentNull("metadata");
            }
            this._columnMetaData = new SqlMetaData[metaData.Length];
            this._columnSmiMetaData = new SmiExtendedMetaData[metaData.Length];
            ulong smiVersion = this.SmiVersion;
            for (int i = 0; i < this._columnSmiMetaData.Length; i++)
            {
                if (metaData[i] == null)
                {
                    throw ADP.ArgumentNull("metadata[" + i + "]");
                }
                this._columnMetaData[i] = metaData[i];
                this._columnSmiMetaData[i] = MetaDataUtilsSmi.SqlMetaDataToSmiExtendedMetaData(this._columnMetaData[i]);
                if (!MetaDataUtilsSmi.IsValidForSmiVersion(this._columnSmiMetaData[i], smiVersion))
                {
                    throw ADP.VersionDoesNotSupportDataType(this._columnSmiMetaData[i].TypeName);
                }
            }
            this._eventSink = new SmiEventSink_Default();
            if (InOutOfProcHelper.InProc)
            {
                this._recordContext = SmiContextFactory.Instance.GetCurrentContext();
                this._recordBuffer = this._recordContext.CreateRecordBuffer(this._columnSmiMetaData, this._eventSink);
                this._usesStringStorageForXml = false;
            }
            else
            {
                this._recordContext = null;
                this._recordBuffer = new MemoryRecordBuffer(this._columnSmiMetaData);
                this._usesStringStorageForXml = true;
            }
            this._eventSink.ProcessMessagesAndThrow();
        }

        internal SqlDataRecord(SmiRecordBuffer recordBuffer, params SmiExtendedMetaData[] metaData)
        {
            this._columnMetaData = new SqlMetaData[metaData.Length];
            this._columnSmiMetaData = new SmiExtendedMetaData[metaData.Length];
            for (int i = 0; i < this._columnSmiMetaData.Length; i++)
            {
                this._columnSmiMetaData[i] = metaData[i];
                this._columnMetaData[i] = MetaDataUtilsSmi.SmiExtendedMetaDataToSqlMetaData(this._columnSmiMetaData[i]);
            }
            this._eventSink = new SmiEventSink_Default();
            if (InOutOfProcHelper.InProc)
            {
                this._recordContext = SmiContextFactory.Instance.GetCurrentContext();
            }
            else
            {
                this._recordContext = null;
            }
            this._recordBuffer = recordBuffer;
            this._eventSink.ProcessMessagesAndThrow();
        }

        private void EnsureSubclassOverride()
        {
            if (this._recordBuffer == null)
            {
                throw SQL.SubclassMustOverride();
            }
        }

        public virtual bool GetBoolean(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetBoolean(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual byte GetByte(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetByte(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetBytes(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), fieldOffset, buffer, bufferOffset, length, true);
        }

        public virtual char GetChar(int ordinal)
        {
            this.EnsureSubclassOverride();
            throw ADP.NotSupported();
        }

        public virtual long GetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetChars(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), fieldOffset, buffer, bufferOffset, length);
        }

        public virtual string GetDataTypeName(int ordinal)
        {
            this.EnsureSubclassOverride();
            SqlMetaData sqlMetaData = this.GetSqlMetaData(ordinal);
            if (SqlDbType.Udt == sqlMetaData.SqlDbType)
            {
                return sqlMetaData.UdtTypeName;
            }
            return MetaType.GetMetaTypeFromSqlDbType(sqlMetaData.SqlDbType, false).TypeName;
        }

        public virtual DateTime GetDateTime(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetDateTime(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual DateTimeOffset GetDateTimeOffset(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetDateTimeOffset(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual decimal GetDecimal(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetDecimal(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual double GetDouble(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetDouble(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual Type GetFieldType(int ordinal)
        {
            this.EnsureSubclassOverride();
            if (SqlDbType.Udt == this.GetSqlMetaData(ordinal).SqlDbType)
            {
                return this.GetSqlMetaData(ordinal).Type;
            }
            return MetaType.GetMetaTypeFromSqlDbType(this.GetSqlMetaData(ordinal).SqlDbType, false).ClassType;
        }

        public virtual float GetFloat(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSingle(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual Guid GetGuid(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetGuid(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual short GetInt16(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetInt16(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual int GetInt32(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetInt32(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual long GetInt64(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetInt64(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual string GetName(int ordinal)
        {
            this.EnsureSubclassOverride();
            return this.GetSqlMetaData(ordinal).Name;
        }

        public virtual int GetOrdinal(string name)
        {
            this.EnsureSubclassOverride();
            if (this._fieldNameLookup == null)
            {
                string[] fieldNames = new string[this.FieldCount];
                for (int i = 0; i < fieldNames.Length; i++)
                {
                    fieldNames[i] = this.GetSqlMetaData(i).Name;
                }
                this._fieldNameLookup = new FieldNameLookup(fieldNames, -1);
            }
            return this._fieldNameLookup.GetOrdinal(name);
        }

        internal SmiExtendedMetaData GetSmiMetaData(int ordinal)
        {
            return this._columnSmiMetaData[ordinal];
        }

        public virtual SqlBinary GetSqlBinary(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlBinary(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlBoolean GetSqlBoolean(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlBoolean(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlByte GetSqlByte(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlByte(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlBytes GetSqlBytes(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlBytes(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), this._recordContext);
        }

        public virtual SqlChars GetSqlChars(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlChars(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), this._recordContext);
        }

        public virtual SqlDateTime GetSqlDateTime(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlDateTime(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlDecimal GetSqlDecimal(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlDecimal(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlDouble GetSqlDouble(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlDouble(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual Type GetSqlFieldType(int ordinal)
        {
            this.EnsureSubclassOverride();
            return MetaType.GetMetaTypeFromSqlDbType(this.GetSqlMetaData(ordinal).SqlDbType, false).SqlType;
        }

        public virtual SqlGuid GetSqlGuid(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlGuid(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlInt16 GetSqlInt16(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlInt16(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlInt32 GetSqlInt32(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlInt32(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlInt64 GetSqlInt64(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlInt64(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlMetaData GetSqlMetaData(int ordinal)
        {
            this.EnsureSubclassOverride();
            return this._columnMetaData[ordinal];
        }

        public virtual SqlMoney GetSqlMoney(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlMoney(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlSingle GetSqlSingle(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlSingle(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual SqlString GetSqlString(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlString(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual object GetSqlValue(int ordinal)
        {
            this.EnsureSubclassOverride();
            SmiMetaData smiMetaData = this.GetSmiMetaData(ordinal);
            if (this.SmiVersion >= 210L)
            {
                return ValueUtilsSmi.GetSqlValue200(this._eventSink, this._recordBuffer, ordinal, smiMetaData, this._recordContext);
            }
            return ValueUtilsSmi.GetSqlValue(this._eventSink, this._recordBuffer, ordinal, smiMetaData, this._recordContext);
        }

        public virtual int GetSqlValues(object[] values)
        {
            this.EnsureSubclassOverride();
            if (values == null)
            {
                throw ADP.ArgumentNull("values");
            }
            int num2 = (values.Length < this.FieldCount) ? values.Length : this.FieldCount;
            for (int i = 0; i < num2; i++)
            {
                values[i] = this.GetSqlValue(i);
            }
            return num2;
        }

        public virtual SqlXml GetSqlXml(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetSqlXml(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), this._recordContext);
        }

        public virtual string GetString(int ordinal)
        {
            this.EnsureSubclassOverride();
            SmiMetaData smiMetaData = this.GetSmiMetaData(ordinal);
            if (this._usesStringStorageForXml && (SqlDbType.Xml == smiMetaData.SqlDbType))
            {
                return ValueUtilsSmi.GetString(this._eventSink, this._recordBuffer, ordinal, __maxNVarCharForXml);
            }
            return ValueUtilsSmi.GetString(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual TimeSpan GetTimeSpan(int ordinal)
        {
            this.EnsureSubclassOverride();
            return ValueUtilsSmi.GetTimeSpan(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal));
        }

        public virtual object GetValue(int ordinal)
        {
            this.EnsureSubclassOverride();
            SmiMetaData smiMetaData = this.GetSmiMetaData(ordinal);
            if (this.SmiVersion >= 210L)
            {
                return ValueUtilsSmi.GetValue200(this._eventSink, this._recordBuffer, ordinal, smiMetaData, this._recordContext);
            }
            return ValueUtilsSmi.GetValue(this._eventSink, this._recordBuffer, ordinal, smiMetaData, this._recordContext);
        }

        public virtual int GetValues(object[] values)
        {
            this.EnsureSubclassOverride();
            if (values == null)
            {
                throw ADP.ArgumentNull("values");
            }
            int num2 = (values.Length < this.FieldCount) ? values.Length : this.FieldCount;
            for (int i = 0; i < num2; i++)
            {
                values[i] = this.GetValue(i);
            }
            return num2;
        }

        internal SqlMetaData[] InternalGetMetaData()
        {
            return this._columnMetaData;
        }

        internal SmiExtendedMetaData[] InternalGetSmiMetaData()
        {
            return this._columnSmiMetaData;
        }

        public virtual bool IsDBNull(int ordinal)
        {
            this.EnsureSubclassOverride();
            this.ThrowIfInvalidOrdinal(ordinal);
            return ValueUtilsSmi.IsDBNull(this._eventSink, this._recordBuffer, ordinal);
        }

        public virtual void SetBoolean(int ordinal, bool value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetBoolean(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetByte(int ordinal, byte value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetByte(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetBytes(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), fieldOffset, buffer, bufferOffset, length);
        }

        public virtual void SetChar(int ordinal, char value)
        {
            this.EnsureSubclassOverride();
            throw ADP.NotSupported();
        }

        public virtual void SetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetChars(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), fieldOffset, buffer, bufferOffset, length);
        }

        public virtual void SetDateTime(int ordinal, DateTime value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetDateTime(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetDateTimeOffset(int ordinal, DateTimeOffset value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetDateTimeOffset(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value, this.SmiVersion >= 210L);
        }

        public virtual void SetDBNull(int ordinal)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetDBNull(this._eventSink, this._recordBuffer, ordinal, true);
        }

        public virtual void SetDecimal(int ordinal, decimal value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetDecimal(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetDouble(int ordinal, double value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetDouble(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetFloat(int ordinal, float value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSingle(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetGuid(int ordinal, Guid value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetGuid(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetInt16(int ordinal, short value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetInt16(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetInt32(int ordinal, int value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetInt32(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetInt64(int ordinal, long value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetInt64(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlBinary(int ordinal, SqlBinary value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlBinary(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlBoolean(int ordinal, SqlBoolean value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlBoolean(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlByte(int ordinal, SqlByte value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlByte(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlBytes(int ordinal, SqlBytes value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlBytes(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlChars(int ordinal, SqlChars value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlChars(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlDateTime(int ordinal, SqlDateTime value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlDateTime(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlDecimal(int ordinal, SqlDecimal value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlDecimal(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlDouble(int ordinal, SqlDouble value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlDouble(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlGuid(int ordinal, SqlGuid value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlGuid(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlInt16(int ordinal, SqlInt16 value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlInt16(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlInt32(int ordinal, SqlInt32 value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlInt32(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlInt64(int ordinal, SqlInt64 value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlInt64(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlMoney(int ordinal, SqlMoney value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlMoney(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlSingle(int ordinal, SqlSingle value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlSingle(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlString(int ordinal, SqlString value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlString(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetSqlXml(int ordinal, SqlXml value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetSqlXml(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetString(int ordinal, string value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetString(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value);
        }

        public virtual void SetTimeSpan(int ordinal, TimeSpan value)
        {
            this.EnsureSubclassOverride();
            ValueUtilsSmi.SetTimeSpan(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value, this.SmiVersion >= 210L);
        }

        public virtual void SetValue(int ordinal, object value)
        {
            this.EnsureSubclassOverride();
            SqlMetaData sqlMetaData = this.GetSqlMetaData(ordinal);
            ExtendedClrTypeCode typeCode = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(sqlMetaData.SqlDbType, false, value, sqlMetaData.Type, this.SmiVersion);
            if (ExtendedClrTypeCode.Invalid == typeCode)
            {
                throw ADP.InvalidCast();
            }
            if (this.SmiVersion >= 210L)
            {
                ValueUtilsSmi.SetCompatibleValueV200(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value, typeCode, 0, 0, null);
            }
            else
            {
                ValueUtilsSmi.SetCompatibleValue(this._eventSink, this._recordBuffer, ordinal, this.GetSmiMetaData(ordinal), value, typeCode, 0);
            }
        }

        public virtual int SetValues(params object[] values)
        {
            this.EnsureSubclassOverride();
            if (values == null)
            {
                throw ADP.ArgumentNull("values");
            }
            int num3 = (values.Length > this.FieldCount) ? this.FieldCount : values.Length;
            ExtendedClrTypeCode[] codeArray = new ExtendedClrTypeCode[num3];
            for (int i = 0; i < num3; i++)
            {
                SqlMetaData sqlMetaData = this.GetSqlMetaData(i);
                codeArray[i] = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(sqlMetaData.SqlDbType, false, values[i], sqlMetaData.Type, this.SmiVersion);
                if (ExtendedClrTypeCode.Invalid == codeArray[i])
                {
                    throw ADP.InvalidCast();
                }
            }
            for (int j = 0; j < num3; j++)
            {
                if (this.SmiVersion >= 210L)
                {
                    ValueUtilsSmi.SetCompatibleValueV200(this._eventSink, this._recordBuffer, j, this.GetSmiMetaData(j), values[j], codeArray[j], 0, 0, null);
                }
                else
                {
                    ValueUtilsSmi.SetCompatibleValue(this._eventSink, this._recordBuffer, j, this.GetSmiMetaData(j), values[j], codeArray[j], 0);
                }
            }
            return num3;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IDataReader IDataRecord.GetData(int ordinal)
        {
            throw ADP.NotSupported();
        }

        internal void ThrowIfInvalidOrdinal(int ordinal)
        {
            if ((0 > ordinal) || (this.FieldCount <= ordinal))
            {
                throw ADP.IndexOutOfRange(ordinal);
            }
        }

        public virtual int FieldCount
        {
            get
            {
                this.EnsureSubclassOverride();
                return this._columnMetaData.Length;
            }
        }

        public virtual object this[int ordinal]
        {
            get
            {
                this.EnsureSubclassOverride();
                return this.GetValue(ordinal);
            }
        }

        public virtual object this[string name]
        {
            get
            {
                this.EnsureSubclassOverride();
                return this.GetValue(this.GetOrdinal(name));
            }
        }

        internal SmiRecordBuffer RecordBuffer
        {
            get
            {
                return this._recordBuffer;
            }
        }

        internal SmiContext RecordContext
        {
            get
            {
                return this._recordContext;
            }
        }

        private ulong SmiVersion
        {
            get
            {
                if (!InOutOfProcHelper.InProc)
                {
                    return 210L;
                }
                return SmiContextFactory.Instance.NegotiatedSmiVersion;
            }
        }
    }
}

