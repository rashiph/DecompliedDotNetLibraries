namespace System.Data.Odbc
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public sealed class OdbcDataReader : DbDataReader
    {
        private string _cmdText;
        private CMDWrapper _cmdWrapper;
        private int _column = -1;
        private CommandBehavior _commandBehavior;
        private FieldNameLookup _fieldNameLookup;
        private HasRowsStatus _hasRows;
        private int _hiddenColumns;
        private bool _isClosed;
        private bool _isRead;
        private bool _isValidResult;
        private bool _noMoreResults;
        private bool _noMoreRows;
        private static int _objectTypeCount;
        private int _row = -1;
        private long _sequentialBytesRead;
        private bool _skipReadOnce;
        private OdbcCommand command;
        private DbCache dataCache;
        private MetaData[] metadata;
        internal readonly int ObjectID = Interlocked.Increment(ref _objectTypeCount);
        private int recordAffected = -1;
        private DataTable schemaTable;

        internal OdbcDataReader(OdbcCommand command, CMDWrapper cmdWrapper, CommandBehavior commandbehavior)
        {
            this.command = command;
            this._commandBehavior = commandbehavior;
            this._cmdText = command.CommandText;
            this._cmdWrapper = cmdWrapper;
        }

        private void BuildMetaDataInfo()
        {
            List<string> list;
            int fieldCount = this.FieldCount;
            MetaData[] dataArray = new MetaData[fieldCount];
            bool needkeyinfo = this.IsCommandBehavior(CommandBehavior.KeyInfo);
            if (needkeyinfo)
            {
                list = new List<string>();
            }
            else
            {
                list = null;
            }
            for (int i = 0; i < fieldCount; i++)
            {
                dataArray[i] = new MetaData();
                dataArray[i].ordinal = i;
                TypeMap typeMap = TypeMap.FromSqlType((ODBC32.SQL_TYPE) ((short) this.GetColAttribute(i, ODBC32.SQL_DESC.CONCISE_TYPE, ODBC32.SQL_COLUMN.TYPE, ODBC32.HANDLER.THROW)));
                if (typeMap._signType)
                {
                    bool unsigned = this.GetColAttribute(i, ODBC32.SQL_DESC.UNSIGNED, ODBC32.SQL_COLUMN.UNSIGNED, ODBC32.HANDLER.THROW).ToInt64() != 0L;
                    typeMap = TypeMap.UpgradeSignedType(typeMap, unsigned);
                }
                dataArray[i].typemap = typeMap;
                dataArray[i].size = this.GetColAttribute(i, ODBC32.SQL_DESC.OCTET_LENGTH, ODBC32.SQL_COLUMN.LENGTH, ODBC32.HANDLER.IGNORE);
                switch (dataArray[i].typemap._sql_type)
                {
                    case ODBC32.SQL_TYPE.WLONGVARCHAR:
                    case ODBC32.SQL_TYPE.WVARCHAR:
                    case ODBC32.SQL_TYPE.WCHAR:
                    {
                        MetaData data1 = dataArray[i];
                        data1.size /= 2;
                        break;
                    }
                }
                dataArray[i].precision = (byte) this.GetColAttribute(i, (ODBC32.SQL_DESC) 4, ODBC32.SQL_COLUMN.PRECISION, ODBC32.HANDLER.IGNORE);
                dataArray[i].scale = (byte) this.GetColAttribute(i, (ODBC32.SQL_DESC) 5, ODBC32.SQL_COLUMN.SCALE, ODBC32.HANDLER.IGNORE);
                dataArray[i].isAutoIncrement = this.GetColAttribute(i, ODBC32.SQL_DESC.AUTO_UNIQUE_VALUE, ODBC32.SQL_COLUMN.AUTO_INCREMENT, ODBC32.HANDLER.IGNORE) == 1;
                dataArray[i].isReadOnly = this.GetColAttribute(i, ODBC32.SQL_DESC.UPDATABLE, ODBC32.SQL_COLUMN.UPDATABLE, ODBC32.HANDLER.IGNORE) == 0;
                ODBC32.SQL_NULLABILITY sql_nullability = (ODBC32.SQL_NULLABILITY) ((ushort) this.GetColAttribute(i, ODBC32.SQL_DESC.NULLABLE, ODBC32.SQL_COLUMN.NULLABLE, ODBC32.HANDLER.IGNORE));
                dataArray[i].isNullable = sql_nullability == ODBC32.SQL_NULLABILITY.NULLABLE;
                switch (dataArray[i].typemap._sql_type)
                {
                    case ODBC32.SQL_TYPE.WLONGVARCHAR:
                    case ODBC32.SQL_TYPE.LONGVARBINARY:
                    case ODBC32.SQL_TYPE.LONGVARCHAR:
                        dataArray[i].isLong = true;
                        break;

                    default:
                        dataArray[i].isLong = false;
                        break;
                }
                if (this.IsCommandBehavior(CommandBehavior.KeyInfo))
                {
                    if (!this.Connection.ProviderInfo.NoSqlCASSColumnKey)
                    {
                        bool flag2 = this.GetColAttribute(i, (ODBC32.SQL_DESC) 0x4bc, ~ODBC32.SQL_COLUMN.COUNT, ODBC32.HANDLER.IGNORE) == 1;
                        if (flag2)
                        {
                            dataArray[i].isKeyColumn = flag2;
                            dataArray[i].isUnique = true;
                            needkeyinfo = false;
                        }
                    }
                    dataArray[i].baseSchemaName = this.GetColAttributeStr(i, ODBC32.SQL_DESC.SCHEMA_NAME, ODBC32.SQL_COLUMN.OWNER_NAME, ODBC32.HANDLER.IGNORE);
                    dataArray[i].baseCatalogName = this.GetColAttributeStr(i, ODBC32.SQL_DESC.CATALOG_NAME, ~ODBC32.SQL_COLUMN.COUNT, ODBC32.HANDLER.IGNORE);
                    dataArray[i].baseTableName = this.GetColAttributeStr(i, ODBC32.SQL_DESC.BASE_TABLE_NAME, ODBC32.SQL_COLUMN.TABLE_NAME, ODBC32.HANDLER.IGNORE);
                    dataArray[i].baseColumnName = this.GetColAttributeStr(i, ODBC32.SQL_DESC.BASE_COLUMN_NAME, ODBC32.SQL_COLUMN.NAME, ODBC32.HANDLER.IGNORE);
                    if (this.Connection.IsV3Driver)
                    {
                        if ((dataArray[i].baseTableName == null) || (dataArray[i].baseTableName.Length == 0))
                        {
                            dataArray[i].baseTableName = this.GetDescFieldStr(i, ODBC32.SQL_DESC.BASE_TABLE_NAME, ODBC32.HANDLER.IGNORE);
                        }
                        if ((dataArray[i].baseColumnName == null) || (dataArray[i].baseColumnName.Length == 0))
                        {
                            dataArray[i].baseColumnName = this.GetDescFieldStr(i, ODBC32.SQL_DESC.BASE_COLUMN_NAME, ODBC32.HANDLER.IGNORE);
                        }
                    }
                    if ((dataArray[i].baseTableName != null) && !list.Contains(dataArray[i].baseTableName))
                    {
                        list.Add(dataArray[i].baseTableName);
                    }
                }
                if ((dataArray[i].isKeyColumn || dataArray[i].isAutoIncrement) && (sql_nullability == ODBC32.SQL_NULLABILITY.UNKNOWN))
                {
                    dataArray[i].isNullable = false;
                }
            }
            if (!this.Connection.ProviderInfo.NoSqlCASSColumnKey)
            {
                for (int j = fieldCount; j < (fieldCount + this._hiddenColumns); j++)
                {
                    if ((this.GetColAttribute(j, (ODBC32.SQL_DESC) 0x4bc, ~ODBC32.SQL_COLUMN.COUNT, ODBC32.HANDLER.IGNORE) == 1) && (this.GetColAttribute(j, (ODBC32.SQL_DESC) 0x4bb, ~ODBC32.SQL_COLUMN.COUNT, ODBC32.HANDLER.IGNORE) == 1))
                    {
                        for (int k = 0; k < fieldCount; k++)
                        {
                            dataArray[k].isKeyColumn = false;
                            dataArray[k].isUnique = false;
                        }
                    }
                }
            }
            this.metadata = dataArray;
            if (this.IsCommandBehavior(CommandBehavior.KeyInfo))
            {
                if ((list != null) && (list.Count > 0))
                {
                    List<string>.Enumerator enumerator = list.GetEnumerator();
                    QualifiedTableName qualifiedTableName = new QualifiedTableName(this.Connection.QuoteChar("GetSchemaTable"));
                    while (enumerator.MoveNext())
                    {
                        qualifiedTableName.Table = enumerator.Current;
                        if (this.RetrieveKeyInfo(needkeyinfo, qualifiedTableName, false) <= 0)
                        {
                            this.RetrieveKeyInfo(needkeyinfo, qualifiedTableName, true);
                        }
                    }
                }
                else
                {
                    QualifiedTableName name = new QualifiedTableName(this.Connection.QuoteChar("GetSchemaTable"), this.GetTableNameFromCommandText());
                    if (!ADP.IsEmpty(name.Table))
                    {
                        this.SetBaseTableNames(name);
                        if (this.RetrieveKeyInfo(needkeyinfo, name, false) <= 0)
                        {
                            this.RetrieveKeyInfo(needkeyinfo, name, true);
                        }
                    }
                }
            }
        }

        internal int CalculateRecordsAffected(int cRowsAffected)
        {
            if (0 <= cRowsAffected)
            {
                if (-1 == this.recordAffected)
                {
                    this.recordAffected = cRowsAffected;
                }
                else
                {
                    this.recordAffected += cRowsAffected;
                }
            }
            return this.recordAffected;
        }

        public override void Close()
        {
            this.Close(false);
        }

        private void Close(bool disposing)
        {
            Exception exception = null;
            CMDWrapper wrapper = this._cmdWrapper;
            if ((wrapper != null) && (wrapper.StatementHandle != null))
            {
                if (this.IsNonCancelingCommand)
                {
                    this.NextResult(disposing, !disposing);
                    if (this.command != null)
                    {
                        if (this.command.HasParameters)
                        {
                            this.command.Parameters.GetOutputValues(this._cmdWrapper);
                        }
                        wrapper.FreeStatementHandle(ODBC32.STMT.CLOSE);
                        this.command.CloseFromDataReader();
                    }
                }
                wrapper.FreeKeyInfoStatementHandle(ODBC32.STMT.CLOSE);
            }
            if (this.command != null)
            {
                this.command.CloseFromDataReader();
                if (this.IsCommandBehavior(CommandBehavior.CloseConnection))
                {
                    this.command.Parameters.RebindCollection = true;
                    this.Connection.Close();
                }
            }
            else if (wrapper != null)
            {
                wrapper.Dispose();
            }
            this.command = null;
            this._isClosed = true;
            this.dataCache = null;
            this.metadata = null;
            this.schemaTable = null;
            this._isRead = false;
            this._hasRows = HasRowsStatus.DontKnow;
            this._isValidResult = false;
            this._noMoreResults = true;
            this._noMoreRows = true;
            this._fieldNameLookup = null;
            this.SetCurrentRowColumnInfo(-1, 0);
            if ((exception != null) && !disposing)
            {
                throw exception;
            }
            this._cmdWrapper = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close(true);
            }
        }

        internal ODBC32.RetCode FieldCountNoThrow(out short cColsAffected)
        {
            if (this.IsCancelingCommand)
            {
                cColsAffected = 0;
                return ODBC32.RetCode.ERROR;
            }
            ODBC32.RetCode code = this.StatementHandle.NumberOfResultColumns(out cColsAffected);
            if (code != ODBC32.RetCode.SUCCESS)
            {
                cColsAffected = 0;
                return code;
            }
            this._hiddenColumns = 0;
            if ((this.IsCommandBehavior(CommandBehavior.KeyInfo) && !this.Connection.ProviderInfo.NoSqlSoptSSNoBrowseTable) && !this.Connection.ProviderInfo.NoSqlSoptSSHiddenColumns)
            {
                for (int i = 0; i < cColsAffected; i++)
                {
                    if (this.GetColAttribute(i, (ODBC32.SQL_DESC) 0x4bb, ~ODBC32.SQL_COLUMN.COUNT, ODBC32.HANDLER.IGNORE).ToInt64() == 1L)
                    {
                        this._hiddenColumns = cColsAffected - i;
                        cColsAffected = (short) i;
                        break;
                    }
                }
            }
            this.dataCache = new DbCache(this, cColsAffected);
            return code;
        }

        internal void FirstResult()
        {
            short num;
            SQLLEN rowCount = this.GetRowCount();
            this.CalculateRecordsAffected((int) rowCount);
            if ((this.FieldCountNoThrow(out num) == ODBC32.RetCode.SUCCESS) && (num == 0))
            {
                this.NextResult();
            }
            else
            {
                this._isValidResult = true;
            }
        }

        public override bool GetBoolean(int i)
        {
            return (bool) this.internalGetBoolean(i);
        }

        public override byte GetByte(int i)
        {
            return (byte) this.internalGetByte(i);
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            return this.GetBytesOrChars(i, dataIndex, buffer, false, bufferIndex, length);
        }

        private long GetBytesOrChars(int i, long dataIndex, Array buffer, bool isCharsBuffer, int bufferIndex, int length)
        {
            if (this.IsClosed)
            {
                throw ADP.DataReaderNoData();
            }
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if (dataIndex < 0L)
            {
                throw ADP.ArgumentOutOfRange("dataIndex");
            }
            if (bufferIndex < 0)
            {
                throw ADP.ArgumentOutOfRange("bufferIndex");
            }
            if (length < 0)
            {
                throw ADP.ArgumentOutOfRange("length");
            }
            string method = isCharsBuffer ? "GetChars" : "GetBytes";
            this.SetCurrentRowColumnInfo(this._row, i);
            object obj2 = null;
            if (isCharsBuffer)
            {
                obj2 = (string) this.dataCache[i];
            }
            else
            {
                obj2 = (byte[]) this.dataCache[i];
            }
            if (!this.IsCommandBehavior(CommandBehavior.SequentialAccess) || (obj2 != null))
            {
                if (0x7fffffffL < dataIndex)
                {
                    throw ADP.ArgumentOutOfRange("dataIndex");
                }
                if (obj2 == null)
                {
                    if (isCharsBuffer)
                    {
                        obj2 = (string) this.internalGetString(i);
                    }
                    else
                    {
                        obj2 = (byte[]) this.internalGetBytes(i);
                    }
                }
                int num2 = isCharsBuffer ? ((string) obj2).Length : ((byte[]) obj2).Length;
                if (buffer == null)
                {
                    return (long) num2;
                }
                if (length == 0)
                {
                    return 0L;
                }
                if (dataIndex >= num2)
                {
                    return 0L;
                }
                int num5 = num2 - ((int) dataIndex);
                int count = Math.Min(Math.Min(num5, length), buffer.Length - bufferIndex);
                if (count <= 0)
                {
                    return 0L;
                }
                if (isCharsBuffer)
                {
                    ((string) obj2).CopyTo((int) dataIndex, (char[]) buffer, bufferIndex, count);
                }
                else
                {
                    Array.Copy((byte[]) obj2, (int) dataIndex, (byte[]) buffer, bufferIndex, count);
                }
                return (long) count;
            }
            if (buffer == null)
            {
                int num3;
                ODBC32.SQL_C sqlctype = isCharsBuffer ? ODBC32.SQL_C.WCHAR : ODBC32.SQL_C.BINARY;
                if (!this.QueryFieldInfo(i, sqlctype, out num3))
                {
                    if (isCharsBuffer)
                    {
                        throw ADP.InvalidCast();
                    }
                    return -1L;
                }
                if (isCharsBuffer)
                {
                    return (long) (num3 / 2);
                }
                return (long) num3;
            }
            if ((isCharsBuffer && (dataIndex < (this._sequentialBytesRead / 2L))) || (!isCharsBuffer && (dataIndex < this._sequentialBytesRead)))
            {
                throw ADP.NonSeqByteAccess(dataIndex, this._sequentialBytesRead, method);
            }
            if (isCharsBuffer)
            {
                dataIndex -= this._sequentialBytesRead / 2L;
            }
            else
            {
                dataIndex -= this._sequentialBytesRead;
            }
            if ((dataIndex <= 0L) || (this.readBytesOrCharsSequentialAccess(i, null, isCharsBuffer, 0, dataIndex) >= dataIndex))
            {
                int num6;
                length = Math.Min(length, buffer.Length - bufferIndex);
                if (length > 0)
                {
                    return (long) this.readBytesOrCharsSequentialAccess(i, buffer, isCharsBuffer, bufferIndex, (long) length);
                }
                if (isCharsBuffer && !this.QueryFieldInfo(i, ODBC32.SQL_C.WCHAR, out num6))
                {
                    throw ADP.InvalidCast();
                }
            }
            return 0L;
        }

        public override char GetChar(int i)
        {
            return (char) this.internalGetChar(i);
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            return this.GetBytesOrChars(i, dataIndex, buffer, true, bufferIndex, length);
        }

        private SQLLEN GetColAttribute(int iColumn, ODBC32.SQL_DESC v3FieldId, ODBC32.SQL_COLUMN v2FieldId, ODBC32.HANDLER handler)
        {
            short stringLength = 0;
            if ((this.Connection != null) && !this._cmdWrapper.Canceling)
            {
                ODBC32.RetCode code;
                SQLLEN sqllen;
                OdbcStatementHandle statementHandle = this.StatementHandle;
                if (this.Connection.IsV3Driver)
                {
                    code = statementHandle.ColumnAttribute(iColumn + 1, (short) v3FieldId, this.Buffer, out stringLength, out sqllen);
                }
                else if (v2FieldId != ~ODBC32.SQL_COLUMN.COUNT)
                {
                    code = statementHandle.ColumnAttribute(iColumn + 1, (short) v2FieldId, this.Buffer, out stringLength, out sqllen);
                }
                else
                {
                    return 0;
                }
                if (code == ODBC32.RetCode.SUCCESS)
                {
                    return sqllen;
                }
                if ((code == ODBC32.RetCode.ERROR) && ("HY091" == this.Command.GetDiagSqlState()))
                {
                    this.Connection.FlagUnsupportedColAttr(v3FieldId, v2FieldId);
                }
                if (handler == ODBC32.HANDLER.THROW)
                {
                    this.Connection.HandleError(statementHandle, code);
                }
            }
            return -1;
        }

        private string GetColAttributeStr(int i, ODBC32.SQL_DESC v3FieldId, ODBC32.SQL_COLUMN v2FieldId, ODBC32.HANDLER handler)
        {
            ODBC32.RetCode code;
            SQLLEN sqllen;
            short stringLength = 0;
            CNativeBuffer characterAttribute = this.Buffer;
            characterAttribute.WriteInt16(0, 0);
            OdbcStatementHandle statementHandle = this.StatementHandle;
            if (((this.Connection == null) || this._cmdWrapper.Canceling) || (statementHandle == null))
            {
                return "";
            }
            if (this.Connection.IsV3Driver)
            {
                code = statementHandle.ColumnAttribute(i + 1, (short) v3FieldId, characterAttribute, out stringLength, out sqllen);
            }
            else if (v2FieldId != ~ODBC32.SQL_COLUMN.COUNT)
            {
                code = statementHandle.ColumnAttribute(i + 1, (short) v2FieldId, characterAttribute, out stringLength, out sqllen);
            }
            else
            {
                return null;
            }
            if ((code == ODBC32.RetCode.SUCCESS) && (stringLength != 0))
            {
                return characterAttribute.PtrToStringUni(0, stringLength / 2);
            }
            if ((code == ODBC32.RetCode.ERROR) && ("HY091" == this.Command.GetDiagSqlState()))
            {
                this.Connection.FlagUnsupportedColAttr(v3FieldId, v2FieldId);
            }
            if (handler == ODBC32.HANDLER.THROW)
            {
                this.Connection.HandleError(statementHandle, code);
            }
            return null;
        }

        private bool GetData(int i, ODBC32.SQL_C sqlctype)
        {
            int num;
            return this.GetData(i, sqlctype, this.Buffer.Length - 4, out num);
        }

        private bool GetData(int i, ODBC32.SQL_C sqlctype, int cb, out int cbLengthOrIndicator)
        {
            IntPtr zero = IntPtr.Zero;
            if (this.IsCancelingCommand)
            {
                throw ADP.DataReaderNoData();
            }
            CNativeBuffer buffer = this.Buffer;
            ODBC32.RetCode retcode = this.StatementHandle.GetData(i + 1, sqlctype, buffer, cb, out zero);
            switch (retcode)
            {
                case ODBC32.RetCode.SUCCESS:
                    break;

                case ODBC32.RetCode.SUCCESS_WITH_INFO:
                    if (((int) zero) != -4)
                    {
                    }
                    break;

                case ODBC32.RetCode.NO_DATA:
                    if ((sqlctype != ODBC32.SQL_C.WCHAR) && (sqlctype != ODBC32.SQL_C.BINARY))
                    {
                        this.Connection.HandleError(this.StatementHandle, retcode);
                    }
                    if (zero == ((IntPtr) (-4)))
                    {
                        zero = IntPtr.Zero;
                    }
                    break;

                default:
                    this.Connection.HandleError(this.StatementHandle, retcode);
                    break;
            }
            this.SetCurrentRowColumnInfo(this._row, i);
            if (zero == ((IntPtr) (-1)))
            {
                this.dataCache[i] = DBNull.Value;
                cbLengthOrIndicator = 0;
                return false;
            }
            cbLengthOrIndicator = (int) zero;
            return true;
        }

        public override string GetDataTypeName(int i)
        {
            if (this.dataCache == null)
            {
                throw ADP.DataReaderNoData();
            }
            DbSchemaInfo schema = this.dataCache.GetSchema(i);
            if (schema._typename == null)
            {
                schema._typename = this.GetColAttributeStr(i, ODBC32.SQL_DESC.TYPE_NAME, ODBC32.SQL_COLUMN.TYPE_NAME, ODBC32.HANDLER.THROW);
            }
            return schema._typename;
        }

        public DateTime GetDate(int i)
        {
            return (DateTime) this.internalGetDate(i);
        }

        public override DateTime GetDateTime(int i)
        {
            return (DateTime) this.internalGetDateTime(i);
        }

        public override decimal GetDecimal(int i)
        {
            return (decimal) this.internalGetDecimal(i);
        }

        private string GetDescFieldStr(int i, ODBC32.SQL_DESC attribute, ODBC32.HANDLER handler)
        {
            int numericAttribute = 0;
            if ((this.Connection == null) || this._cmdWrapper.Canceling)
            {
                return "";
            }
            if (!this.Connection.IsV3Driver)
            {
                return null;
            }
            CNativeBuffer buffer = this.Buffer;
            using (OdbcDescriptorHandle handle = new OdbcDescriptorHandle(this.StatementHandle, ODBC32.SQL_ATTR.APP_PARAM_DESC))
            {
                ODBC32.RetCode retcode = handle.GetDescriptionField(i + 1, attribute, buffer, out numericAttribute);
                if ((retcode != ODBC32.RetCode.SUCCESS) || (numericAttribute == 0))
                {
                    if ((retcode == ODBC32.RetCode.ERROR) && ("HY091" == this.Command.GetDiagSqlState()))
                    {
                        this.Connection.FlagUnsupportedColAttr(attribute, ODBC32.SQL_COLUMN.COUNT);
                    }
                    if (handler == ODBC32.HANDLER.THROW)
                    {
                        this.Connection.HandleError(this.StatementHandle, retcode);
                    }
                    return null;
                }
            }
            return buffer.PtrToStringUni(0, numericAttribute / 2);
        }

        public override double GetDouble(int i)
        {
            return (double) this.internalGetDouble(i);
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this, this.IsCommandBehavior(CommandBehavior.CloseConnection));
        }

        public override Type GetFieldType(int i)
        {
            if (this.dataCache == null)
            {
                throw ADP.DataReaderNoData();
            }
            DbSchemaInfo schema = this.dataCache.GetSchema(i);
            if (schema._type == null)
            {
                schema._type = this.GetSqlType(i)._type;
            }
            return schema._type;
        }

        public override float GetFloat(int i)
        {
            return (float) this.internalGetFloat(i);
        }

        public override Guid GetGuid(int i)
        {
            return (Guid) this.internalGetGuid(i);
        }

        public override short GetInt16(int i)
        {
            return (short) this.internalGetInt16(i);
        }

        public override int GetInt32(int i)
        {
            return (int) this.internalGetInt32(i);
        }

        public override long GetInt64(int i)
        {
            return (long) this.internalGetInt64(i);
        }

        public override string GetName(int i)
        {
            if (this.dataCache == null)
            {
                throw ADP.DataReaderNoData();
            }
            DbSchemaInfo schema = this.dataCache.GetSchema(i);
            if (schema._name == null)
            {
                schema._name = this.GetColAttributeStr(i, ODBC32.SQL_DESC.NAME, ODBC32.SQL_COLUMN.NAME, ODBC32.HANDLER.THROW);
                if (schema._name == null)
                {
                    schema._name = "";
                }
            }
            return schema._name;
        }

        public override int GetOrdinal(string value)
        {
            if (this._fieldNameLookup == null)
            {
                if (this.dataCache == null)
                {
                    throw ADP.DataReaderNoData();
                }
                this._fieldNameLookup = new FieldNameLookup(this, -1);
            }
            return this._fieldNameLookup.GetOrdinal(value);
        }

        internal int GetOrdinalFromBaseColName(string columnname)
        {
            return this.GetOrdinalFromBaseColName(columnname, null);
        }

        internal int GetOrdinalFromBaseColName(string columnname, string tablename)
        {
            if (ADP.IsEmpty(columnname))
            {
                return -1;
            }
            if (this.metadata != null)
            {
                int fieldCount = this.FieldCount;
                for (int i = 0; i < fieldCount; i++)
                {
                    if ((this.metadata[i].baseColumnName != null) && (columnname == this.metadata[i].baseColumnName))
                    {
                        if (ADP.IsEmpty(tablename))
                        {
                            return i;
                        }
                        if (tablename == this.metadata[i].baseTableName)
                        {
                            return i;
                        }
                    }
                }
            }
            return this.IndexOf(columnname);
        }

        private SQLLEN GetRowCount()
        {
            if (!this.IsClosed)
            {
                SQLLEN sqllen;
                ODBC32.RetCode code = this.StatementHandle.RowCount(out sqllen);
                if ((code == ODBC32.RetCode.SUCCESS) || (ODBC32.RetCode.SUCCESS_WITH_INFO == code))
                {
                    return sqllen;
                }
            }
            return -1;
        }

        public override DataTable GetSchemaTable()
        {
            if (this.IsClosed)
            {
                throw ADP.DataReaderClosed("GetSchemaTable");
            }
            if (this._noMoreResults)
            {
                return null;
            }
            if (this.schemaTable != null)
            {
                return this.schemaTable;
            }
            DataTable table = this.NewSchemaTable();
            if (this.FieldCount != 0)
            {
                if (this.metadata == null)
                {
                    this.BuildMetaDataInfo();
                }
                DataColumn column18 = table.Columns["ColumnName"];
                DataColumn column17 = table.Columns["ColumnOrdinal"];
                DataColumn column16 = table.Columns["ColumnSize"];
                DataColumn column15 = table.Columns["NumericPrecision"];
                DataColumn column14 = table.Columns["NumericScale"];
                DataColumn column13 = table.Columns["DataType"];
                DataColumn column12 = table.Columns["ProviderType"];
                DataColumn column11 = table.Columns["IsLong"];
                DataColumn column10 = table.Columns["AllowDBNull"];
                DataColumn column9 = table.Columns["IsReadOnly"];
                DataColumn column8 = table.Columns["IsRowVersion"];
                DataColumn column7 = table.Columns["IsUnique"];
                DataColumn column6 = table.Columns["IsKey"];
                DataColumn column5 = table.Columns["IsAutoIncrement"];
                DataColumn column4 = table.Columns["BaseSchemaName"];
                DataColumn column3 = table.Columns["BaseCatalogName"];
                DataColumn column2 = table.Columns["BaseTableName"];
                DataColumn column = table.Columns["BaseColumnName"];
                int fieldCount = this.FieldCount;
                for (int i = 0; i < fieldCount; i++)
                {
                    DataRow row = table.NewRow();
                    row[column18] = this.GetName(i);
                    row[column17] = i;
                    row[column16] = (int) Math.Min(Math.Max(-2147483648L, this.metadata[i].size.ToInt64()), 0x7fffffffL);
                    row[column15] = this.metadata[i].precision;
                    row[column14] = this.metadata[i].scale;
                    row[column13] = this.metadata[i].typemap._type;
                    row[column12] = this.metadata[i].typemap._odbcType;
                    row[column11] = this.metadata[i].isLong;
                    row[column10] = this.metadata[i].isNullable;
                    row[column9] = this.metadata[i].isReadOnly;
                    row[column8] = this.metadata[i].isRowVersion;
                    row[column7] = this.metadata[i].isUnique;
                    row[column6] = this.metadata[i].isKeyColumn;
                    row[column5] = this.metadata[i].isAutoIncrement;
                    row[column4] = this.metadata[i].baseSchemaName;
                    row[column3] = this.metadata[i].baseCatalogName;
                    row[column2] = this.metadata[i].baseTableName;
                    row[column] = this.metadata[i].baseColumnName;
                    table.Rows.Add(row);
                    row.AcceptChanges();
                }
                this.schemaTable = table;
            }
            return table;
        }

        private TypeMap GetSqlType(int i)
        {
            TypeMap map;
            DbSchemaInfo schema = this.dataCache.GetSchema(i);
            if (!schema._dbtype.HasValue)
            {
                schema._dbtype = new ODBC32.SQL_TYPE?((ODBC32.SQL_TYPE) ((short) this.GetColAttribute(i, ODBC32.SQL_DESC.CONCISE_TYPE, ODBC32.SQL_COLUMN.TYPE, ODBC32.HANDLER.THROW)));
                map = TypeMap.FromSqlType(schema._dbtype.Value);
                if (map._signType)
                {
                    bool unsigned = this.GetColAttribute(i, ODBC32.SQL_DESC.UNSIGNED, ODBC32.SQL_COLUMN.UNSIGNED, ODBC32.HANDLER.THROW).ToInt64() != 0L;
                    map = TypeMap.UpgradeSignedType(map, unsigned);
                    schema._dbtype = new ODBC32.SQL_TYPE?(map._sql_type);
                }
            }
            else
            {
                map = TypeMap.FromSqlType(schema._dbtype.Value);
            }
            this.Connection.SetSupportedType(schema._dbtype.Value);
            return map;
        }

        public override string GetString(int i)
        {
            return (string) this.internalGetString(i);
        }

        internal string GetTableNameFromCommandText()
        {
            int currentPosition;
            if (this.command == null)
            {
                return null;
            }
            string str = this._cmdText;
            if (ADP.IsEmpty(str))
            {
                return null;
            }
            CStringTokenizer tokenizer = new CStringTokenizer(str, this.Connection.QuoteChar("GetSchemaTable")[0], this.Connection.EscapeChar("GetSchemaTable"));
            if (tokenizer.StartsWith("select"))
            {
                currentPosition = tokenizer.FindTokenIndex("from");
            }
            else if ((tokenizer.StartsWith("insert") || tokenizer.StartsWith("update")) || tokenizer.StartsWith("delete"))
            {
                currentPosition = tokenizer.CurrentPosition;
            }
            else
            {
                currentPosition = -1;
            }
            if (currentPosition == -1)
            {
                return null;
            }
            string str2 = tokenizer.NextToken();
            str = tokenizer.NextToken();
            if ((str.Length > 0) && (str[0] == ','))
            {
                return null;
            }
            if ((str.Length == 2) && ((str[0] == 'a') || (str[0] == 'A')))
            {
                if ((str[1] != 's') && (str[1] != 'S'))
                {
                    return str2;
                }
                str = tokenizer.NextToken();
                str = tokenizer.NextToken();
                if ((str.Length > 0) && (str[0] == ','))
                {
                    return null;
                }
            }
            return str2;
        }

        public TimeSpan GetTime(int i)
        {
            return (TimeSpan) this.internalGetTime(i);
        }

        public override object GetValue(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if (this.dataCache.AccessIndex(i) == null)
            {
                this.dataCache[i] = this.GetValue(i, this.GetSqlType(i));
            }
            return this.dataCache[i];
        }

        internal object GetValue(int i, TypeMap typemap)
        {
            switch (typemap._sql_type)
            {
                case ODBC32.SQL_TYPE.GUID:
                    return this.internalGetGuid(i);

                case ODBC32.SQL_TYPE.WLONGVARCHAR:
                case ODBC32.SQL_TYPE.WVARCHAR:
                case ODBC32.SQL_TYPE.WCHAR:
                case ODBC32.SQL_TYPE.LONGVARCHAR:
                case ODBC32.SQL_TYPE.CHAR:
                case ODBC32.SQL_TYPE.VARCHAR:
                    return this.internalGetString(i);

                case ODBC32.SQL_TYPE.BIT:
                    return this.internalGetBoolean(i);

                case ODBC32.SQL_TYPE.TINYINT:
                    return this.internalGetByte(i);

                case ODBC32.SQL_TYPE.BIGINT:
                    return this.internalGetInt64(i);

                case ODBC32.SQL_TYPE.LONGVARBINARY:
                case ODBC32.SQL_TYPE.VARBINARY:
                case ODBC32.SQL_TYPE.BINARY:
                    return this.internalGetBytes(i);

                case ODBC32.SQL_TYPE.NUMERIC:
                case ODBC32.SQL_TYPE.DECIMAL:
                    return this.internalGetDecimal(i);

                case ODBC32.SQL_TYPE.INTEGER:
                    return this.internalGetInt32(i);

                case ODBC32.SQL_TYPE.SMALLINT:
                    return this.internalGetInt16(i);

                case ODBC32.SQL_TYPE.FLOAT:
                case ODBC32.SQL_TYPE.DOUBLE:
                    return this.internalGetDouble(i);

                case ODBC32.SQL_TYPE.REAL:
                    return this.internalGetFloat(i);

                case ODBC32.SQL_TYPE.SS_VARIANT:
                    int num;
                    if (!this._isRead)
                    {
                        throw ADP.DataReaderNoData();
                    }
                    if ((this.dataCache.AccessIndex(i) == null) && this.QueryFieldInfo(i, ODBC32.SQL_C.BINARY, out num))
                    {
                        ODBC32.SQL_TYPE sqltype = (ODBC32.SQL_TYPE) ((short) this.GetColAttribute(i, (ODBC32.SQL_DESC) 0x4c0, ~ODBC32.SQL_COLUMN.COUNT, ODBC32.HANDLER.THROW));
                        return this.GetValue(i, TypeMap.FromSqlType(sqltype));
                    }
                    return this.dataCache[i];

                case ODBC32.SQL_TYPE.TYPE_DATE:
                    return this.internalGetDate(i);

                case ODBC32.SQL_TYPE.TYPE_TIME:
                    return this.internalGetTime(i);

                case ODBC32.SQL_TYPE.TYPE_TIMESTAMP:
                    return this.internalGetDateTime(i);
            }
            return this.internalGetBytes(i);
        }

        public override int GetValues(object[] values)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            int num2 = Math.Min(values.Length, this.FieldCount);
            for (int i = 0; i < num2; i++)
            {
                values[i] = this.GetValue(i);
            }
            return num2;
        }

        private int IndexOf(string value)
        {
            if (this._fieldNameLookup == null)
            {
                if (this.dataCache == null)
                {
                    throw ADP.DataReaderNoData();
                }
                this._fieldNameLookup = new FieldNameLookup(this, -1);
            }
            return this._fieldNameLookup.IndexOf(value);
        }

        private object internalGetBoolean(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.BIT))
            {
                this.dataCache[i] = this.Buffer.MarshalToManaged(0, ODBC32.SQL_C.BIT, -1);
            }
            return this.dataCache[i];
        }

        private object internalGetByte(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.UTINYINT))
            {
                this.dataCache[i] = this.Buffer.ReadByte(0);
            }
            return this.dataCache[i];
        }

        private object internalGetBytes(int i)
        {
            if (this.dataCache.AccessIndex(i) == null)
            {
                int num;
                int cb = this.Buffer.Length - 4;
                int startIndex = 0;
                if (this.GetData(i, ODBC32.SQL_C.BINARY, cb, out num))
                {
                    byte[] buffer;
                    CNativeBuffer buffer3 = this.Buffer;
                    if (-4 != num)
                    {
                        buffer = new byte[num];
                        this.Buffer.ReadBytes(0, buffer, startIndex, Math.Min(num, cb));
                        while (num > cb)
                        {
                            this.GetData(i, ODBC32.SQL_C.BINARY, cb, out num);
                            startIndex += cb;
                            buffer3.ReadBytes(0, buffer, startIndex, Math.Min(num, cb));
                        }
                    }
                    else
                    {
                        List<byte[]> list = new List<byte[]>();
                        int num5 = 0;
                        do
                        {
                            int length = (-4 != num) ? num : cb;
                            buffer = new byte[length];
                            num5 += length;
                            buffer3.ReadBytes(0, buffer, 0, length);
                            list.Add(buffer);
                        }
                        while ((-4 == num) && this.GetData(i, ODBC32.SQL_C.BINARY, cb, out num));
                        buffer = new byte[num5];
                        foreach (byte[] buffer2 in list)
                        {
                            buffer2.CopyTo(buffer, startIndex);
                            startIndex += buffer2.Length;
                        }
                    }
                    this.dataCache[i] = buffer;
                }
            }
            return this.dataCache[i];
        }

        private object internalGetChar(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.WCHAR))
            {
                this.dataCache[i] = this.Buffer.ReadChar(0);
            }
            return this.dataCache[i];
        }

        private object internalGetDate(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.TYPE_DATE))
            {
                this.dataCache[i] = this.Buffer.MarshalToManaged(0, ODBC32.SQL_C.TYPE_DATE, -1);
            }
            return this.dataCache[i];
        }

        private object internalGetDateTime(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.TYPE_TIMESTAMP))
            {
                this.dataCache[i] = this.Buffer.MarshalToManaged(0, ODBC32.SQL_C.TYPE_TIMESTAMP, -1);
            }
            return this.dataCache[i];
        }

        private object internalGetDecimal(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.WCHAR))
            {
                string s = null;
                try
                {
                    s = (string) this.Buffer.MarshalToManaged(0, ODBC32.SQL_C.WCHAR, -3);
                    this.dataCache[i] = decimal.Parse(s, CultureInfo.InvariantCulture);
                }
                catch (OverflowException exception)
                {
                    this.dataCache[i] = s;
                    throw exception;
                }
            }
            return this.dataCache[i];
        }

        private object internalGetDouble(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.DOUBLE))
            {
                this.dataCache[i] = this.Buffer.ReadDouble(0);
            }
            return this.dataCache[i];
        }

        private object internalGetFloat(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.REAL))
            {
                this.dataCache[i] = this.Buffer.ReadSingle(0);
            }
            return this.dataCache[i];
        }

        private object internalGetGuid(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.GUID))
            {
                this.dataCache[i] = this.Buffer.ReadGuid(0);
            }
            return this.dataCache[i];
        }

        private object internalGetInt16(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.SSHORT))
            {
                this.dataCache[i] = this.Buffer.ReadInt16(0);
            }
            return this.dataCache[i];
        }

        private object internalGetInt32(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.SLONG))
            {
                this.dataCache[i] = this.Buffer.ReadInt32(0);
            }
            return this.dataCache[i];
        }

        private object internalGetInt64(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.WCHAR))
            {
                string s = (string) this.Buffer.MarshalToManaged(0, ODBC32.SQL_C.WCHAR, -3);
                this.dataCache[i] = long.Parse(s, CultureInfo.InvariantCulture);
            }
            return this.dataCache[i];
        }

        private object internalGetString(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if (this.dataCache.AccessIndex(i) == null)
            {
                int num;
                CNativeBuffer buffer = this.Buffer;
                int num2 = buffer.Length - 4;
                if (this.GetData(i, ODBC32.SQL_C.WCHAR, buffer.Length - 2, out num))
                {
                    bool flag;
                    if ((num <= num2) && (-4 != num))
                    {
                        string str = buffer.PtrToStringUni(0, Math.Min(num, num2) / 2);
                        this.dataCache[i] = str;
                        return str;
                    }
                    char[] destination = new char[num2 / 2];
                    int num6 = (num == -4) ? num2 : num;
                    StringBuilder builder = new StringBuilder(num6 / 2);
                    int num4 = num2;
                    int num3 = (-4 == num) ? -1 : (num - num4);
                    do
                    {
                        int length = num4 / 2;
                        buffer.ReadChars(0, destination, 0, length);
                        builder.Append(destination, 0, length);
                        if (num3 == 0)
                        {
                            break;
                        }
                        flag = this.GetData(i, ODBC32.SQL_C.WCHAR, buffer.Length - 2, out num);
                        if (-4 != num)
                        {
                            num4 = Math.Min(num, num2);
                            if (0 < num3)
                            {
                                num3 -= num4;
                            }
                            else
                            {
                                num3 = 0;
                            }
                        }
                    }
                    while (flag);
                    this.dataCache[i] = builder.ToString();
                }
            }
            return this.dataCache[i];
        }

        private object internalGetTime(int i)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if ((this.dataCache.AccessIndex(i) == null) && this.GetData(i, ODBC32.SQL_C.TYPE_TIME))
            {
                this.dataCache[i] = this.Buffer.MarshalToManaged(0, ODBC32.SQL_C.TYPE_TIME, -1);
            }
            return this.dataCache[i];
        }

        internal bool IsBehavior(CommandBehavior behavior)
        {
            return this.IsCommandBehavior(behavior);
        }

        private bool IsCommandBehavior(CommandBehavior condition)
        {
            return (condition == (condition & this._commandBehavior));
        }

        public override bool IsDBNull(int i)
        {
            int num;
            if (!this.IsCommandBehavior(CommandBehavior.SequentialAccess))
            {
                return Convert.IsDBNull(this.GetValue(i));
            }
            object obj2 = this.dataCache[i];
            if (obj2 != null)
            {
                return Convert.IsDBNull(obj2);
            }
            TypeMap sqlType = this.GetSqlType(i);
            if (sqlType._bufferSize > 0)
            {
                return Convert.IsDBNull(this.GetValue(i));
            }
            return !this.QueryFieldInfo(i, sqlType._sql_c, out num);
        }

        private DataTable NewSchemaTable()
        {
            DataTable table = new DataTable("SchemaTable") {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = this.FieldCount
            };
            DataColumnCollection columns = table.Columns;
            columns.Add(new DataColumn("ColumnName", typeof(string)));
            columns.Add(new DataColumn("ColumnOrdinal", typeof(int)));
            columns.Add(new DataColumn("ColumnSize", typeof(int)));
            columns.Add(new DataColumn("NumericPrecision", typeof(short)));
            columns.Add(new DataColumn("NumericScale", typeof(short)));
            columns.Add(new DataColumn("DataType", typeof(object)));
            columns.Add(new DataColumn("ProviderType", typeof(int)));
            columns.Add(new DataColumn("IsLong", typeof(bool)));
            columns.Add(new DataColumn("AllowDBNull", typeof(bool)));
            columns.Add(new DataColumn("IsReadOnly", typeof(bool)));
            columns.Add(new DataColumn("IsRowVersion", typeof(bool)));
            columns.Add(new DataColumn("IsUnique", typeof(bool)));
            columns.Add(new DataColumn("IsKey", typeof(bool)));
            columns.Add(new DataColumn("IsAutoIncrement", typeof(bool)));
            columns.Add(new DataColumn("BaseSchemaName", typeof(string)));
            columns.Add(new DataColumn("BaseCatalogName", typeof(string)));
            columns.Add(new DataColumn("BaseTableName", typeof(string)));
            columns.Add(new DataColumn("BaseColumnName", typeof(string)));
            foreach (DataColumn column in columns)
            {
                column.ReadOnly = true;
            }
            return table;
        }

        public override bool NextResult()
        {
            return this.NextResult(false, false);
        }

        private bool NextResult(bool disposing, bool allresults)
        {
            ODBC32.RetCode code;
            bool flag;
            ODBC32.RetCode sUCCESS = ODBC32.RetCode.SUCCESS;
            bool flag3 = false;
            bool flag2 = this.IsCommandBehavior(CommandBehavior.SingleResult);
            if (this.IsClosed)
            {
                throw ADP.DataReaderClosed("NextResult");
            }
            this._fieldNameLookup = null;
            if (this.IsCancelingCommand || this._noMoreResults)
            {
                return false;
            }
            this._isRead = false;
            this._hasRows = HasRowsStatus.DontKnow;
            this._fieldNameLookup = null;
            this.metadata = null;
            this.schemaTable = null;
            int num = 0;
            OdbcErrorCollection errors = null;
            do
            {
                this._isValidResult = false;
                code = this.StatementHandle.MoreResults();
                flag = (code == ODBC32.RetCode.SUCCESS) || (code == ODBC32.RetCode.SUCCESS_WITH_INFO);
                if (code == ODBC32.RetCode.SUCCESS_WITH_INFO)
                {
                    this.Connection.HandleErrorNoThrow(this.StatementHandle, code);
                }
                else if ((!disposing && (code != ODBC32.RetCode.NO_DATA)) && (code != ODBC32.RetCode.SUCCESS))
                {
                    if (errors == null)
                    {
                        sUCCESS = code;
                        errors = new OdbcErrorCollection();
                    }
                    ODBC32.GetDiagErrors(errors, null, this.StatementHandle, code);
                    num++;
                }
                if (!disposing && flag)
                {
                    num = 0;
                    SQLLEN rowCount = this.GetRowCount();
                    this.CalculateRecordsAffected((int) rowCount);
                    if (!flag2)
                    {
                        short num2;
                        this.FieldCountNoThrow(out num2);
                        flag3 = 0 != num2;
                        this._isValidResult = flag3;
                    }
                }
            }
            while ((((!flag2 && flag) && !flag3) || (((ODBC32.RetCode.NO_DATA != code) && allresults) && (num < 0x7d0))) || (flag2 && flag));
            if (0x7d0 <= num)
            {
                Bid.Trace("<odbc.OdbcDataReader.NextResult|INFO> 2000 consecutive failed results");
            }
            if (code == ODBC32.RetCode.NO_DATA)
            {
                this.dataCache = null;
                this._noMoreResults = true;
            }
            if (errors != null)
            {
                errors.SetSource(this.Connection.Driver);
                OdbcException innerException = OdbcException.CreateException(errors, sUCCESS);
                this.Connection.ConnectionIsAlive(innerException);
                throw innerException;
            }
            return flag;
        }

        private bool QueryFieldInfo(int i, ODBC32.SQL_C sqlctype, out int cbLengthOrIndicator)
        {
            int cb = 0;
            if (sqlctype == ODBC32.SQL_C.WCHAR)
            {
                cb = 2;
            }
            return this.GetData(i, sqlctype, cb, out cbLengthOrIndicator);
        }

        public override bool Read()
        {
            if (this.IsClosed)
            {
                throw ADP.DataReaderClosed("Read");
            }
            if (this.IsCancelingCommand)
            {
                this._isRead = false;
                return false;
            }
            if (this._skipReadOnce)
            {
                this._skipReadOnce = false;
                return this._isRead;
            }
            if ((this._noMoreRows || this._noMoreResults) || this.IsCommandBehavior(CommandBehavior.SchemaOnly))
            {
                return false;
            }
            if (!this._isValidResult)
            {
                return false;
            }
            ODBC32.RetCode retcode = this.StatementHandle.Fetch();
            switch (retcode)
            {
                case ODBC32.RetCode.SUCCESS:
                    this._hasRows = HasRowsStatus.HasRows;
                    this._isRead = true;
                    break;

                case ODBC32.RetCode.SUCCESS_WITH_INFO:
                    this.Connection.HandleErrorNoThrow(this.StatementHandle, retcode);
                    this._hasRows = HasRowsStatus.HasRows;
                    this._isRead = true;
                    break;

                case ODBC32.RetCode.NO_DATA:
                    this._isRead = false;
                    if (this._hasRows == HasRowsStatus.DontKnow)
                    {
                        this._hasRows = HasRowsStatus.HasNoRows;
                    }
                    break;

                default:
                    this.Connection.HandleError(this.StatementHandle, retcode);
                    break;
            }
            this.dataCache.FlushValues();
            if (this.IsCommandBehavior(CommandBehavior.SingleRow))
            {
                this._noMoreRows = true;
                this.SetCurrentRowColumnInfo(-1, 0);
            }
            else
            {
                this.SetCurrentRowColumnInfo(this._row + 1, 0);
            }
            return this._isRead;
        }

        private int readBytesOrCharsSequentialAccess(int i, Array buffer, bool isCharsBuffer, int bufferIndex, long bytesOrCharsLength)
        {
            int num4 = 0;
            long num3 = isCharsBuffer ? (bytesOrCharsLength * 2L) : bytesOrCharsLength;
            CNativeBuffer buffer2 = this.Buffer;
            while (num3 > 0L)
            {
                int num;
                int num2;
                int num5;
                bool flag2;
                if (isCharsBuffer)
                {
                    num2 = (int) Math.Min(num3, (long) (buffer2.Length - 4));
                    flag2 = this.GetData(i, ODBC32.SQL_C.WCHAR, num2 + 2, out num5);
                }
                else
                {
                    num2 = (int) Math.Min(num3, (long) (buffer2.Length - 2));
                    flag2 = this.GetData(i, ODBC32.SQL_C.BINARY, num2, out num5);
                }
                if (!flag2)
                {
                    throw ADP.InvalidCast();
                }
                bool flag = false;
                if (num5 == 0)
                {
                    return num4;
                }
                if (-4 == num5)
                {
                    num = num2;
                }
                else if (num5 > num2)
                {
                    num = num2;
                }
                else
                {
                    num = num5;
                    flag = true;
                }
                this._sequentialBytesRead += num;
                if (isCharsBuffer)
                {
                    int length = num / 2;
                    if (buffer != null)
                    {
                        buffer2.ReadChars(0, (char[]) buffer, bufferIndex, length);
                        bufferIndex += length;
                    }
                    num4 += length;
                }
                else
                {
                    if (buffer != null)
                    {
                        buffer2.ReadBytes(0, (byte[]) buffer, bufferIndex, num);
                        bufferIndex += num;
                    }
                    num4 += num;
                }
                num3 -= num;
                if (flag)
                {
                    return num4;
                }
            }
            return num4;
        }

        internal int RetrieveKeyInfo(bool needkeyinfo, QualifiedTableName qualifiedTableName, bool quoted)
        {
            int num2 = 0;
            IntPtr zero = IntPtr.Zero;
            if (this.IsClosed || (this._cmdWrapper == null))
            {
                return 0;
            }
            this._cmdWrapper.CreateKeyInfoStatementHandle();
            CNativeBuffer buffer = this.Buffer;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                int ordinalFromBaseColName;
                ODBC32.RetCode code;
                string str;
                buffer.DangerousAddRef(ref success);
                if (!needkeyinfo)
                {
                    goto Label_0206;
                }
                if (!this.Connection.ProviderInfo.NoSqlPrimaryKeys)
                {
                    code = this.KeyInfoStatementHandle.PrimaryKeys(qualifiedTableName.Catalog, qualifiedTableName.Schema, qualifiedTableName.GetTable(quoted));
                    switch (code)
                    {
                        case ODBC32.RetCode.SUCCESS:
                        case ODBC32.RetCode.SUCCESS_WITH_INFO:
                        {
                            bool flag = false;
                            buffer.WriteInt16(0, 0);
                            code = this.KeyInfoStatementHandle.BindColumn2(4, ODBC32.SQL_C.WCHAR, buffer.PtrOffset(0, 0x100), (IntPtr) 0x100, buffer.PtrOffset(0x100, IntPtr.Size).Handle);
                            while ((code = this.KeyInfoStatementHandle.Fetch()) == ODBC32.RetCode.SUCCESS)
                            {
                                zero = buffer.ReadIntPtr(0x100);
                                str = buffer.PtrToStringUni(0, ((int) zero) / 2);
                                ordinalFromBaseColName = this.GetOrdinalFromBaseColName(str);
                                if (ordinalFromBaseColName != -1)
                                {
                                    num2++;
                                    this.metadata[ordinalFromBaseColName].isKeyColumn = true;
                                    this.metadata[ordinalFromBaseColName].isUnique = true;
                                    this.metadata[ordinalFromBaseColName].isNullable = false;
                                    this.metadata[ordinalFromBaseColName].baseTableName = qualifiedTableName.Table;
                                    if (this.metadata[ordinalFromBaseColName].baseColumnName == null)
                                    {
                                        this.metadata[ordinalFromBaseColName].baseColumnName = str;
                                    }
                                }
                                else
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                foreach (MetaData data in this.metadata)
                                {
                                    data.isKeyColumn = false;
                                }
                            }
                            code = this.KeyInfoStatementHandle.BindColumn3(4, ODBC32.SQL_C.WCHAR, buffer.DangerousGetHandle());
                            goto Label_01E0;
                        }
                    }
                    if ("IM001" == this.Command.GetDiagSqlState())
                    {
                        this.Connection.ProviderInfo.NoSqlPrimaryKeys = true;
                    }
                }
            Label_01E0:
                if (num2 == 0)
                {
                    this.KeyInfoStatementHandle.MoreResults();
                    num2 += this.RetrieveKeyInfoFromStatistics(qualifiedTableName, quoted);
                }
                this.KeyInfoStatementHandle.MoreResults();
            Label_0206:
                code = this.KeyInfoStatementHandle.SpecialColumns(qualifiedTableName.GetTable(quoted));
                if ((code != ODBC32.RetCode.SUCCESS) && (code != ODBC32.RetCode.SUCCESS_WITH_INFO))
                {
                    return num2;
                }
                zero = IntPtr.Zero;
                buffer.WriteInt16(0, 0);
                code = this.KeyInfoStatementHandle.BindColumn2(2, ODBC32.SQL_C.WCHAR, buffer.PtrOffset(0, 0x100), (IntPtr) 0x100, buffer.PtrOffset(0x100, IntPtr.Size).Handle);
                while ((code = this.KeyInfoStatementHandle.Fetch()) == ODBC32.RetCode.SUCCESS)
                {
                    zero = buffer.ReadIntPtr(0x100);
                    str = buffer.PtrToStringUni(0, ((int) zero) / 2);
                    ordinalFromBaseColName = this.GetOrdinalFromBaseColName(str);
                    if (ordinalFromBaseColName != -1)
                    {
                        this.metadata[ordinalFromBaseColName].isRowVersion = true;
                        if (this.metadata[ordinalFromBaseColName].baseColumnName == null)
                        {
                            this.metadata[ordinalFromBaseColName].baseColumnName = str;
                        }
                    }
                }
                code = this.KeyInfoStatementHandle.BindColumn3(2, ODBC32.SQL_C.WCHAR, buffer.DangerousGetHandle());
                code = this.KeyInfoStatementHandle.MoreResults();
            }
            finally
            {
                if (success)
                {
                    buffer.DangerousRelease();
                }
            }
            return num2;
        }

        private int RetrieveKeyInfoFromStatistics(QualifiedTableName qualifiedTableName, bool quoted)
        {
            string columnname = string.Empty;
            string indexname = string.Empty;
            string currentindexname = string.Empty;
            int[] numArray = new int[0x10];
            int[] numArray2 = new int[0x10];
            int num2 = 0;
            int ncols = 0;
            bool flag = false;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            int num8 = 0;
            string tableName = string.Copy(qualifiedTableName.GetTable(quoted));
            if (this.KeyInfoStatementHandle.Statistics(tableName) != ODBC32.RetCode.SUCCESS)
            {
                return 0;
            }
            CNativeBuffer buffer = this.Buffer;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                buffer.DangerousAddRef(ref success);
                HandleRef ref4 = buffer.PtrOffset(0, 0x100);
                HandleRef ref3 = buffer.PtrOffset(0x100, 0x100);
                HandleRef ref2 = buffer.PtrOffset(0x200, 4);
                IntPtr handle = buffer.PtrOffset(520, IntPtr.Size).Handle;
                IntPtr ptr4 = buffer.PtrOffset(0x210, IntPtr.Size).Handle;
                IntPtr ptr3 = buffer.PtrOffset(0x218, IntPtr.Size).Handle;
                buffer.WriteInt16(0x100, 0);
                ODBC32.RetCode code = this.KeyInfoStatementHandle.BindColumn2(6, ODBC32.SQL_C.WCHAR, ref3, (IntPtr) 0x100, ptr4);
                code = this.KeyInfoStatementHandle.BindColumn2(8, ODBC32.SQL_C.SSHORT, ref2, (IntPtr) 4, ptr3);
                buffer.WriteInt16(0x200, 0);
                code = this.KeyInfoStatementHandle.BindColumn2(9, ODBC32.SQL_C.WCHAR, ref4, (IntPtr) 0x100, handle);
                while ((code = this.KeyInfoStatementHandle.Fetch()) == ODBC32.RetCode.SUCCESS)
                {
                    ptr = buffer.ReadIntPtr(520);
                    zero = buffer.ReadIntPtr(0x210);
                    if (buffer.ReadInt16(0x100) != 0)
                    {
                        columnname = buffer.PtrToStringUni(0, ((int) ptr) / 2);
                        indexname = buffer.PtrToStringUni(0x100, ((int) zero) / 2);
                        int ordinal = buffer.ReadInt16(0x200);
                        if (this.SameIndexColumn(currentindexname, indexname, ordinal, ncols))
                        {
                            if (!flag)
                            {
                                ordinal = this.GetOrdinalFromBaseColName(columnname, qualifiedTableName.Table);
                                if (ordinal == -1)
                                {
                                    flag = true;
                                    continue;
                                }
                                if (ncols < 0x10)
                                {
                                    numArray[ncols++] = ordinal;
                                }
                                else
                                {
                                    flag = true;
                                }
                            }
                        }
                        else
                        {
                            if ((!flag && (ncols != 0)) && ((num2 == 0) || (num2 > ncols)))
                            {
                                num2 = ncols;
                                for (int i = 0; i < ncols; i++)
                                {
                                    numArray2[i] = numArray[i];
                                }
                            }
                            ncols = 0;
                            currentindexname = indexname;
                            flag = false;
                            ordinal = this.GetOrdinalFromBaseColName(columnname, qualifiedTableName.Table);
                            if (ordinal == -1)
                            {
                                flag = true;
                            }
                            else
                            {
                                numArray[ncols++] = ordinal;
                            }
                        }
                    }
                }
                if ((!flag && (ncols != 0)) && ((num2 == 0) || (num2 > ncols)))
                {
                    num2 = ncols;
                    for (int j = 0; j < ncols; j++)
                    {
                        numArray2[j] = numArray[j];
                    }
                }
                if (num2 != 0)
                {
                    for (int k = 0; k < num2; k++)
                    {
                        int index = numArray2[k];
                        num8++;
                        this.metadata[index].isKeyColumn = true;
                        this.metadata[index].isNullable = false;
                        this.metadata[index].isUnique = true;
                        if (this.metadata[index].baseTableName == null)
                        {
                            this.metadata[index].baseTableName = qualifiedTableName.Table;
                        }
                        if (this.metadata[index].baseColumnName == null)
                        {
                            this.metadata[index].baseColumnName = columnname;
                        }
                    }
                }
                this._cmdWrapper.FreeKeyInfoStatementHandle(ODBC32.STMT.UNBIND);
            }
            finally
            {
                if (success)
                {
                    buffer.DangerousRelease();
                }
            }
            return num8;
        }

        internal bool SameIndexColumn(string currentindexname, string indexname, int ordinal, int ncols)
        {
            if (ADP.IsEmpty(currentindexname))
            {
                return false;
            }
            return ((currentindexname == indexname) && (ordinal == (ncols + 1)));
        }

        internal void SetBaseTableNames(QualifiedTableName qualifiedTableName)
        {
            int fieldCount = this.FieldCount;
            for (int i = 0; i < fieldCount; i++)
            {
                if (this.metadata[i].baseTableName == null)
                {
                    this.metadata[i].baseTableName = qualifiedTableName.Table;
                    this.metadata[i].baseSchemaName = qualifiedTableName.Schema;
                    this.metadata[i].baseCatalogName = qualifiedTableName.Catalog;
                }
            }
        }

        private void SetCurrentRowColumnInfo(int row, int column)
        {
            if ((this._row != row) || (this._column != column))
            {
                this._row = row;
                this._column = column;
                this._sequentialBytesRead = 0L;
            }
        }

        private CNativeBuffer Buffer
        {
            get
            {
                CNativeBuffer buffer = this._cmdWrapper._dataReaderBuf;
                if (buffer == null)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return buffer;
            }
        }

        internal OdbcCommand Command
        {
            get
            {
                return this.command;
            }
            set
            {
                this.command = value;
            }
        }

        private OdbcConnection Connection
        {
            get
            {
                if (this._cmdWrapper != null)
                {
                    return this._cmdWrapper.Connection;
                }
                return null;
            }
        }

        public override int Depth
        {
            get
            {
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("Depth");
                }
                return 0;
            }
        }

        public override int FieldCount
        {
            get
            {
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("FieldCount");
                }
                if (this._noMoreResults)
                {
                    return 0;
                }
                if (this.dataCache == null)
                {
                    short num;
                    ODBC32.RetCode retcode = this.FieldCountNoThrow(out num);
                    if (retcode != ODBC32.RetCode.SUCCESS)
                    {
                        this.Connection.HandleError(this.StatementHandle, retcode);
                    }
                }
                if (this.dataCache == null)
                {
                    return 0;
                }
                return this.dataCache._count;
            }
        }

        public override bool HasRows
        {
            get
            {
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("HasRows");
                }
                if (this._hasRows == HasRowsStatus.DontKnow)
                {
                    this.Read();
                    this._skipReadOnce = true;
                }
                return (this._hasRows == HasRowsStatus.HasRows);
            }
        }

        internal bool IsCancelingCommand
        {
            get
            {
                return ((this.command != null) && this.command.Canceling);
            }
        }

        public override bool IsClosed
        {
            get
            {
                return this._isClosed;
            }
        }

        internal bool IsNonCancelingCommand
        {
            get
            {
                return ((this.command != null) && !this.command.Canceling);
            }
        }

        public override object this[int i]
        {
            get
            {
                return this.GetValue(i);
            }
        }

        public override object this[string value]
        {
            get
            {
                return this.GetValue(this.GetOrdinal(value));
            }
        }

        private OdbcStatementHandle KeyInfoStatementHandle
        {
            get
            {
                return this._cmdWrapper.KeyInfoStatement;
            }
        }

        public override int RecordsAffected
        {
            get
            {
                return this.recordAffected;
            }
        }

        private OdbcStatementHandle StatementHandle
        {
            get
            {
                return this._cmdWrapper.StatementHandle;
            }
        }

        private enum HasRowsStatus
        {
            DontKnow,
            HasRows,
            HasNoRows
        }

        private sealed class MetaData
        {
            internal string baseCatalogName;
            internal string baseColumnName;
            internal string baseSchemaName;
            internal string baseTableName;
            internal bool isAutoIncrement;
            internal bool isKeyColumn;
            internal bool isLong;
            internal bool isNullable;
            internal bool isReadOnly;
            internal bool isRowVersion;
            internal bool isUnique;
            internal int ordinal;
            internal byte precision;
            internal byte scale;
            internal SQLLEN size;
            internal TypeMap typemap;
        }

        internal sealed class QualifiedTableName
        {
            private string _catalogName;
            private string _quoteChar;
            private string _quotedTableName;
            private string _schemaName;
            private string _tableName;

            internal QualifiedTableName(string quoteChar)
            {
                this._quoteChar = quoteChar;
            }

            internal QualifiedTableName(string quoteChar, string qualifiedname)
            {
                this._quoteChar = quoteChar;
                string[] strArray = DbCommandBuilder.ParseProcedureName(qualifiedname, quoteChar, quoteChar);
                this._catalogName = this.UnQuote(strArray[1]);
                this._schemaName = this.UnQuote(strArray[2]);
                this._quotedTableName = strArray[3];
                this._tableName = this.UnQuote(strArray[3]);
            }

            internal string GetTable(bool flag)
            {
                if (!flag)
                {
                    return this.Table;
                }
                return this.QuotedTable;
            }

            private string UnQuote(string str)
            {
                if ((str != null) && (str.Length > 0))
                {
                    char ch = this._quoteChar[0];
                    if (((str[0] == ch) && (str.Length > 1)) && (str[str.Length - 1] == ch))
                    {
                        str = str.Substring(1, str.Length - 2);
                    }
                }
                return str;
            }

            internal string Catalog
            {
                get
                {
                    return this._catalogName;
                }
            }

            internal string QuotedTable
            {
                get
                {
                    return this._quotedTableName;
                }
            }

            internal string Schema
            {
                get
                {
                    return this._schemaName;
                }
            }

            internal string Table
            {
                get
                {
                    return this._tableName;
                }
                set
                {
                    this._quotedTableName = value;
                    this._tableName = this.UnQuote(value);
                }
            }
        }
    }
}

