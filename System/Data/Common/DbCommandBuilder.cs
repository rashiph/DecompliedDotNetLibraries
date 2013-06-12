namespace System.Data.Common
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    public abstract class DbCommandBuilder : Component
    {
        private System.Data.Common.CatalogLocation _catalogLocation = System.Data.Common.CatalogLocation.Start;
        private string _catalogSeparator = ".";
        private System.Data.ConflictOption _conflictDetection = System.Data.ConflictOption.CompareAllSearchableValues;
        private DbDataAdapter _dataAdapter;
        private DbSchemaRow[] _dbSchemaRows;
        private DataTable _dbSchemaTable;
        private DbCommand _deleteCommand;
        private bool _hasPartialPrimaryKey;
        private DbCommand _insertCommand;
        private MissingMappingAction _missingMappingAction;
        private string _parameterMarkerFormat;
        private int _parameterNameMaxLength;
        private string _parameterNamePattern;
        private ParameterNames _parameterNames;
        private string _quotedBaseTableName;
        private string _quotePrefix = "";
        private string _quoteSuffix = "";
        private string _schemaSeparator = ".";
        private bool _setAllValues;
        private string[] _sourceColumnNames;
        private DbCommand _updateCommand;
        private const string And = " AND ";
        private const string Comma = ", ";
        private const string DefaultValues = " DEFAULT VALUES";
        private const string DeleteFrom = "DELETE FROM ";
        private const string Equal = " = ";
        private const string EqualOne = " = 1";
        private const string InsertInto = "INSERT INTO ";
        private const string IsNull = " IS NULL";
        private const string LeftParenthesis = "(";
        private const string NameSeparator = ".";
        private const string Or = " OR ";
        private const string RightParenthesis = ")";
        private const string Set = " SET ";
        private const string SpaceLeftParenthesis = " (";
        private const string Update = "UPDATE ";
        private const string Values = " VALUES ";
        private const string Where = " WHERE ";

        protected DbCommandBuilder()
        {
        }

        protected abstract void ApplyParameterInfo(DbParameter parameter, DataRow row, StatementType statementType, bool whereClause);
        private void BuildCache(bool closeConnection, DataRow dataRow, bool useColumnsForParameterNames)
        {
            if ((this._dbSchemaTable == null) || (useColumnsForParameterNames && (this._parameterNames == null)))
            {
                DataTable schemaTable = null;
                DbCommand selectCommand = this.GetSelectCommand();
                DbConnection connection = selectCommand.Connection;
                if (connection == null)
                {
                    throw ADP.MissingSourceCommandConnection();
                }
                try
                {
                    if ((ConnectionState.Open & connection.State) == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    else
                    {
                        closeConnection = false;
                    }
                    if (useColumnsForParameterNames)
                    {
                        DataTable schema = connection.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
                        if (schema.Rows.Count == 1)
                        {
                            this._parameterNamePattern = schema.Rows[0][DbMetaDataColumnNames.ParameterNamePattern] as string;
                            this._parameterMarkerFormat = schema.Rows[0][DbMetaDataColumnNames.ParameterMarkerFormat] as string;
                            object obj2 = schema.Rows[0][DbMetaDataColumnNames.ParameterNameMaxLength];
                            this._parameterNameMaxLength = (obj2 is int) ? ((int) obj2) : 0;
                            if (((this._parameterNameMaxLength == 0) || (this._parameterNamePattern == null)) || (this._parameterMarkerFormat == null))
                            {
                                useColumnsForParameterNames = false;
                            }
                        }
                        else
                        {
                            useColumnsForParameterNames = false;
                        }
                    }
                    schemaTable = this.GetSchemaTable(selectCommand);
                }
                finally
                {
                    if (closeConnection)
                    {
                        connection.Close();
                    }
                }
                if (schemaTable == null)
                {
                    throw ADP.DynamicSQLNoTableInfo();
                }
                this.BuildInformation(schemaTable);
                this._dbSchemaTable = schemaTable;
                DbSchemaRow[] schemaRows = this._dbSchemaRows;
                string[] columnNameArray = new string[schemaRows.Length];
                for (int i = 0; i < schemaRows.Length; i++)
                {
                    if (schemaRows[i] != null)
                    {
                        columnNameArray[i] = schemaRows[i].ColumnName;
                    }
                }
                this._sourceColumnNames = columnNameArray;
                if (useColumnsForParameterNames)
                {
                    this._parameterNames = new ParameterNames(this, schemaRows);
                }
                ADP.BuildSchemaTableInfoTableNames(columnNameArray);
            }
        }

        private DbCommand BuildDeleteCommand(DataTableMapping mappings, DataRow dataRow)
        {
            DbCommand command = this.InitializeCommand(this.DeleteCommand);
            StringBuilder builder = new StringBuilder();
            int parameterCount = 0;
            builder.Append("DELETE FROM ");
            builder.Append(this.QuotedBaseTableName);
            parameterCount = this.BuildWhereClause(mappings, dataRow, builder, command, parameterCount, false);
            command.CommandText = builder.ToString();
            RemoveExtraParameters(command, parameterCount);
            this.DeleteCommand = command;
            return command;
        }

        private void BuildInformation(DataTable schemaTable)
        {
            DbSchemaRow[] sortedSchemaRows = DbSchemaRow.GetSortedSchemaRows(schemaTable, false);
            if ((sortedSchemaRows == null) || (sortedSchemaRows.Length == 0))
            {
                throw ADP.DynamicSQLNoTableInfo();
            }
            string strA = "";
            string str5 = "";
            string str6 = "";
            string str = null;
            for (int i = 0; i < sortedSchemaRows.Length; i++)
            {
                DbSchemaRow row2 = sortedSchemaRows[i];
                string baseTableName = row2.BaseTableName;
                if ((baseTableName == null) || (baseTableName.Length == 0))
                {
                    sortedSchemaRows[i] = null;
                }
                else
                {
                    string baseServerName = row2.BaseServerName;
                    string baseCatalogName = row2.BaseCatalogName;
                    string baseSchemaName = row2.BaseSchemaName;
                    if (baseServerName == null)
                    {
                        baseServerName = "";
                    }
                    if (baseCatalogName == null)
                    {
                        baseCatalogName = "";
                    }
                    if (baseSchemaName == null)
                    {
                        baseSchemaName = "";
                    }
                    if (str == null)
                    {
                        strA = baseServerName;
                        str5 = baseCatalogName;
                        str6 = baseSchemaName;
                        str = baseTableName;
                    }
                    else if (((ADP.SrcCompare(str, baseTableName) != 0) || (ADP.SrcCompare(str6, baseSchemaName) != 0)) || ((ADP.SrcCompare(str5, baseCatalogName) != 0) || (ADP.SrcCompare(strA, baseServerName) != 0)))
                    {
                        throw ADP.DynamicSQLJoinUnsupported();
                    }
                }
            }
            if (strA.Length == 0)
            {
                strA = null;
            }
            if (str5.Length == 0)
            {
                strA = null;
                str5 = null;
            }
            if (str6.Length == 0)
            {
                strA = null;
                str5 = null;
                str6 = null;
            }
            if ((str == null) || (str.Length == 0))
            {
                throw ADP.DynamicSQLNoTableInfo();
            }
            System.Data.Common.CatalogLocation catalogLocation = this.CatalogLocation;
            string catalogSeparator = this.CatalogSeparator;
            string schemaSeparator = this.SchemaSeparator;
            string quotePrefix = this.QuotePrefix;
            string quoteSuffix = this.QuoteSuffix;
            if (!ADP.IsEmpty(quotePrefix) && (-1 != str.IndexOf(quotePrefix, StringComparison.Ordinal)))
            {
                throw ADP.DynamicSQLNestedQuote(str, quotePrefix);
            }
            if (!ADP.IsEmpty(quoteSuffix) && (-1 != str.IndexOf(quoteSuffix, StringComparison.Ordinal)))
            {
                throw ADP.DynamicSQLNestedQuote(str, quoteSuffix);
            }
            StringBuilder builder = new StringBuilder();
            if (System.Data.Common.CatalogLocation.Start == catalogLocation)
            {
                if (strA != null)
                {
                    builder.Append(ADP.BuildQuotedString(quotePrefix, quoteSuffix, strA));
                    builder.Append(catalogSeparator);
                }
                if (str5 != null)
                {
                    builder.Append(ADP.BuildQuotedString(quotePrefix, quoteSuffix, str5));
                    builder.Append(catalogSeparator);
                }
            }
            if (str6 != null)
            {
                builder.Append(ADP.BuildQuotedString(quotePrefix, quoteSuffix, str6));
                builder.Append(schemaSeparator);
            }
            builder.Append(ADP.BuildQuotedString(quotePrefix, quoteSuffix, str));
            if (System.Data.Common.CatalogLocation.End == catalogLocation)
            {
                if (strA != null)
                {
                    builder.Append(catalogSeparator);
                    builder.Append(ADP.BuildQuotedString(quotePrefix, quoteSuffix, strA));
                }
                if (str5 != null)
                {
                    builder.Append(catalogSeparator);
                    builder.Append(ADP.BuildQuotedString(quotePrefix, quoteSuffix, str5));
                }
            }
            this._quotedBaseTableName = builder.ToString();
            this._hasPartialPrimaryKey = false;
            foreach (DbSchemaRow row in sortedSchemaRows)
            {
                if (((row != null) && (row.IsKey || row.IsUnique)) && ((!row.IsLong && !row.IsRowVersion) && row.IsHidden))
                {
                    this._hasPartialPrimaryKey = true;
                    break;
                }
            }
            this._dbSchemaRows = sortedSchemaRows;
        }

        private DbCommand BuildInsertCommand(DataTableMapping mappings, DataRow dataRow)
        {
            DbCommand command = this.InitializeCommand(this.InsertCommand);
            StringBuilder builder = new StringBuilder();
            int index = 0;
            string str2 = " (";
            builder.Append("INSERT INTO ");
            builder.Append(this.QuotedBaseTableName);
            DbSchemaRow[] rowArray = this._dbSchemaRows;
            string[] strArray = new string[rowArray.Length];
            for (int i = 0; i < rowArray.Length; i++)
            {
                DbSchemaRow row = rowArray[i];
                if (((row != null) && (row.BaseColumnName.Length != 0)) && this.IncludeInInsertValues(row))
                {
                    object obj2 = null;
                    string columnName = this._sourceColumnNames[i];
                    if ((mappings != null) && (dataRow != null))
                    {
                        DataColumn column = this.GetDataColumn(columnName, mappings, dataRow);
                        if ((column == null) || (row.IsReadOnly && column.ReadOnly))
                        {
                            continue;
                        }
                        obj2 = this.GetColumnValue(dataRow, column, DataRowVersion.Current);
                        if (!row.AllowDBNull && ((obj2 == null) || Convert.IsDBNull(obj2)))
                        {
                            continue;
                        }
                    }
                    builder.Append(str2);
                    str2 = ", ";
                    builder.Append(this.QuotedColumn(row.BaseColumnName));
                    strArray[index] = this.CreateParameterForValue(command, this.GetBaseParameterName(i), columnName, DataRowVersion.Current, index, obj2, row, StatementType.Insert, false);
                    index++;
                }
            }
            if (index == 0)
            {
                builder.Append(" DEFAULT VALUES");
            }
            else
            {
                builder.Append(")");
                builder.Append(" VALUES ");
                builder.Append("(");
                builder.Append(strArray[0]);
                for (int j = 1; j < index; j++)
                {
                    builder.Append(", ");
                    builder.Append(strArray[j]);
                }
                builder.Append(")");
            }
            command.CommandText = builder.ToString();
            RemoveExtraParameters(command, index);
            this.InsertCommand = command;
            return command;
        }

        private DbCommand BuildUpdateCommand(DataTableMapping mappings, DataRow dataRow)
        {
            DbCommand command = this.InitializeCommand(this.UpdateCommand);
            StringBuilder builder = new StringBuilder();
            string str2 = " SET ";
            int parameterCount = 0;
            builder.Append("UPDATE ");
            builder.Append(this.QuotedBaseTableName);
            DbSchemaRow[] rowArray = this._dbSchemaRows;
            for (int i = 0; i < rowArray.Length; i++)
            {
                DbSchemaRow row = rowArray[i];
                if (((row != null) && (row.BaseColumnName.Length != 0)) && this.IncludeInUpdateSet(row))
                {
                    object obj2 = null;
                    string columnName = this._sourceColumnNames[i];
                    if ((mappings != null) && (dataRow != null))
                    {
                        DataColumn column = this.GetDataColumn(columnName, mappings, dataRow);
                        if ((column == null) || (row.IsReadOnly && column.ReadOnly))
                        {
                            continue;
                        }
                        obj2 = this.GetColumnValue(dataRow, column, DataRowVersion.Current);
                        if (!this.SetAllValues)
                        {
                            object obj3 = this.GetColumnValue(dataRow, column, DataRowVersion.Original);
                            if ((obj3 == obj2) || ((obj3 != null) && obj3.Equals(obj2)))
                            {
                                continue;
                            }
                        }
                    }
                    builder.Append(str2);
                    str2 = ", ";
                    builder.Append(this.QuotedColumn(row.BaseColumnName));
                    builder.Append(" = ");
                    builder.Append(this.CreateParameterForValue(command, this.GetBaseParameterName(i), columnName, DataRowVersion.Current, parameterCount, obj2, row, StatementType.Update, false));
                    parameterCount++;
                }
            }
            bool flag = 0 == parameterCount;
            parameterCount = this.BuildWhereClause(mappings, dataRow, builder, command, parameterCount, true);
            command.CommandText = builder.ToString();
            RemoveExtraParameters(command, parameterCount);
            this.UpdateCommand = command;
            if (!flag)
            {
                return command;
            }
            return null;
        }

        private int BuildWhereClause(DataTableMapping mappings, DataRow dataRow, StringBuilder builder, DbCommand command, int parameterCount, bool isUpdate)
        {
            string str3 = string.Empty;
            int num2 = 0;
            builder.Append(" WHERE ");
            builder.Append("(");
            DbSchemaRow[] rowArray = this._dbSchemaRows;
            for (int i = 0; i < rowArray.Length; i++)
            {
                DbSchemaRow row = rowArray[i];
                if (((row != null) && (row.BaseColumnName.Length != 0)) && this.IncludeInWhereClause(row, isUpdate))
                {
                    builder.Append(str3);
                    str3 = " AND ";
                    object obj2 = null;
                    string columnName = this._sourceColumnNames[i];
                    string str2 = this.QuotedColumn(row.BaseColumnName);
                    if ((mappings != null) && (dataRow != null))
                    {
                        obj2 = this.GetColumnValue(dataRow, columnName, mappings, DataRowVersion.Original);
                    }
                    if (!row.AllowDBNull)
                    {
                        builder.Append("(");
                        builder.Append(str2);
                        builder.Append(" = ");
                        builder.Append(this.CreateParameterForValue(command, this.GetOriginalParameterName(i), columnName, DataRowVersion.Original, parameterCount, obj2, row, isUpdate ? StatementType.Update : StatementType.Delete, true));
                        parameterCount++;
                        builder.Append(")");
                    }
                    else
                    {
                        builder.Append("(");
                        builder.Append("(");
                        builder.Append(this.CreateParameterForNullTest(command, this.GetNullParameterName(i), columnName, DataRowVersion.Original, parameterCount, obj2, row, isUpdate ? StatementType.Update : StatementType.Delete, true));
                        parameterCount++;
                        builder.Append(" = 1");
                        builder.Append(" AND ");
                        builder.Append(str2);
                        builder.Append(" IS NULL");
                        builder.Append(")");
                        builder.Append(" OR ");
                        builder.Append("(");
                        builder.Append(str2);
                        builder.Append(" = ");
                        builder.Append(this.CreateParameterForValue(command, this.GetOriginalParameterName(i), columnName, DataRowVersion.Original, parameterCount, obj2, row, isUpdate ? StatementType.Update : StatementType.Delete, true));
                        parameterCount++;
                        builder.Append(")");
                        builder.Append(")");
                    }
                    if (this.IncrementWhereCount(row))
                    {
                        num2++;
                    }
                }
            }
            builder.Append(")");
            if (num2 != 0)
            {
                return parameterCount;
            }
            if (isUpdate)
            {
                if (System.Data.ConflictOption.CompareRowVersion == this.ConflictOption)
                {
                    throw ADP.DynamicSQLNoKeyInfoRowVersionUpdate();
                }
                throw ADP.DynamicSQLNoKeyInfoUpdate();
            }
            if (System.Data.ConflictOption.CompareRowVersion == this.ConflictOption)
            {
                throw ADP.DynamicSQLNoKeyInfoRowVersionDelete();
            }
            throw ADP.DynamicSQLNoKeyInfoDelete();
        }

        private string CreateParameterForNullTest(DbCommand command, string parameterName, string sourceColumn, DataRowVersion version, int parameterCount, object value, DbSchemaRow row, StatementType statementType, bool whereClause)
        {
            DbParameter nextParameter = GetNextParameter(command, parameterCount);
            if (parameterName == null)
            {
                nextParameter.ParameterName = this.GetParameterName((int) (1 + parameterCount));
            }
            else
            {
                nextParameter.ParameterName = parameterName;
            }
            nextParameter.Direction = ParameterDirection.Input;
            nextParameter.SourceColumn = sourceColumn;
            nextParameter.SourceVersion = version;
            nextParameter.SourceColumnNullMapping = true;
            nextParameter.Value = value;
            nextParameter.Size = 0;
            this.ApplyParameterInfo(nextParameter, row.DataRow, statementType, whereClause);
            nextParameter.DbType = DbType.Int32;
            nextParameter.Value = ADP.IsNull(value) ? DbDataAdapter.ParameterValueNullValue : DbDataAdapter.ParameterValueNonNullValue;
            if (!command.Parameters.Contains(nextParameter))
            {
                command.Parameters.Add(nextParameter);
            }
            if (parameterName == null)
            {
                return this.GetParameterPlaceholder(1 + parameterCount);
            }
            return string.Format(CultureInfo.InvariantCulture, this._parameterMarkerFormat, new object[] { parameterName });
        }

        private string CreateParameterForValue(DbCommand command, string parameterName, string sourceColumn, DataRowVersion version, int parameterCount, object value, DbSchemaRow row, StatementType statementType, bool whereClause)
        {
            DbParameter nextParameter = GetNextParameter(command, parameterCount);
            if (parameterName == null)
            {
                nextParameter.ParameterName = this.GetParameterName((int) (1 + parameterCount));
            }
            else
            {
                nextParameter.ParameterName = parameterName;
            }
            nextParameter.Direction = ParameterDirection.Input;
            nextParameter.SourceColumn = sourceColumn;
            nextParameter.SourceVersion = version;
            nextParameter.SourceColumnNullMapping = false;
            nextParameter.Value = value;
            nextParameter.Size = 0;
            this.ApplyParameterInfo(nextParameter, row.DataRow, statementType, whereClause);
            if (!command.Parameters.Contains(nextParameter))
            {
                command.Parameters.Add(nextParameter);
            }
            if (parameterName == null)
            {
                return this.GetParameterPlaceholder(1 + parameterCount);
            }
            return string.Format(CultureInfo.InvariantCulture, this._parameterMarkerFormat, new object[] { parameterName });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DataAdapter = null;
            }
            base.Dispose(disposing);
        }

        private string GetBaseParameterName(int index)
        {
            if (this._parameterNames != null)
            {
                return this._parameterNames.GetBaseParameterName(index);
            }
            return null;
        }

        private object GetColumnValue(DataRow row, DataColumn column, DataRowVersion version)
        {
            object obj2 = null;
            if (column != null)
            {
                obj2 = row[column, version];
            }
            return obj2;
        }

        private object GetColumnValue(DataRow row, string columnName, DataTableMapping mappings, DataRowVersion version)
        {
            return this.GetColumnValue(row, this.GetDataColumn(columnName, mappings, row), version);
        }

        internal DbConnection GetConnection()
        {
            DbDataAdapter dataAdapter = this.DataAdapter;
            if (dataAdapter != null)
            {
                DbCommand selectCommand = dataAdapter.SelectCommand;
                if (selectCommand != null)
                {
                    return selectCommand.Connection;
                }
            }
            return null;
        }

        private DataColumn GetDataColumn(string columnName, DataTableMapping tablemapping, DataRow row)
        {
            DataColumn column = null;
            if (!ADP.IsEmpty(columnName))
            {
                column = tablemapping.GetDataColumn(columnName, null, row.Table, this._missingMappingAction, MissingSchemaAction.Error);
            }
            return column;
        }

        public DbCommand GetDeleteCommand()
        {
            return this.GetDeleteCommand(null, false);
        }

        public DbCommand GetDeleteCommand(bool useColumnsForParameterNames)
        {
            return this.GetDeleteCommand(null, useColumnsForParameterNames);
        }

        internal DbCommand GetDeleteCommand(DataRow dataRow, bool useColumnsForParameterNames)
        {
            this.BuildCache(true, dataRow, useColumnsForParameterNames);
            this.BuildDeleteCommand(this.GetTableMapping(dataRow), dataRow);
            return this.DeleteCommand;
        }

        public DbCommand GetInsertCommand()
        {
            return this.GetInsertCommand(null, false);
        }

        public DbCommand GetInsertCommand(bool useColumnsForParameterNames)
        {
            return this.GetInsertCommand(null, useColumnsForParameterNames);
        }

        internal DbCommand GetInsertCommand(DataRow dataRow, bool useColumnsForParameterNames)
        {
            this.BuildCache(true, dataRow, useColumnsForParameterNames);
            this.BuildInsertCommand(this.GetTableMapping(dataRow), dataRow);
            return this.InsertCommand;
        }

        private static DbParameter GetNextParameter(DbCommand command, int pcount)
        {
            if (pcount < command.Parameters.Count)
            {
                return command.Parameters[pcount];
            }
            return command.CreateParameter();
        }

        private string GetNullParameterName(int index)
        {
            if (this._parameterNames != null)
            {
                return this._parameterNames.GetNullParameterName(index);
            }
            return null;
        }

        private string GetOriginalParameterName(int index)
        {
            if (this._parameterNames != null)
            {
                return this._parameterNames.GetOriginalParameterName(index);
            }
            return null;
        }

        protected abstract string GetParameterName(int parameterOrdinal);
        protected abstract string GetParameterName(string parameterName);
        protected abstract string GetParameterPlaceholder(int parameterOrdinal);
        protected virtual DataTable GetSchemaTable(DbCommand sourceCommand)
        {
            using (IDataReader reader = sourceCommand.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
            {
                return reader.GetSchemaTable();
            }
        }

        private DbCommand GetSelectCommand()
        {
            DbCommand selectCommand = null;
            DbDataAdapter dataAdapter = this.DataAdapter;
            if (dataAdapter != null)
            {
                if (this._missingMappingAction == ((MissingMappingAction) 0))
                {
                    this._missingMappingAction = dataAdapter.MissingMappingAction;
                }
                selectCommand = dataAdapter.SelectCommand;
            }
            if (selectCommand == null)
            {
                throw ADP.MissingSourceCommand();
            }
            return selectCommand;
        }

        private DataTableMapping GetTableMapping(DataRow dataRow)
        {
            DataTableMapping mapping = null;
            if (dataRow == null)
            {
                return mapping;
            }
            DataTable dataTable = dataRow.Table;
            if (dataTable == null)
            {
                return mapping;
            }
            DbDataAdapter dataAdapter = this.DataAdapter;
            if (dataAdapter != null)
            {
                return dataAdapter.GetTableMapping(dataTable);
            }
            string tableName = dataTable.TableName;
            return new DataTableMapping(tableName, tableName);
        }

        public DbCommand GetUpdateCommand()
        {
            return this.GetUpdateCommand(null, false);
        }

        public DbCommand GetUpdateCommand(bool useColumnsForParameterNames)
        {
            return this.GetUpdateCommand(null, useColumnsForParameterNames);
        }

        internal DbCommand GetUpdateCommand(DataRow dataRow, bool useColumnsForParameterNames)
        {
            this.BuildCache(true, dataRow, useColumnsForParameterNames);
            this.BuildUpdateCommand(this.GetTableMapping(dataRow), dataRow);
            return this.UpdateCommand;
        }

        private bool IncludeInInsertValues(DbSchemaRow row)
        {
            return (((!row.IsAutoIncrement && !row.IsHidden) && (!row.IsExpression && !row.IsRowVersion)) && !row.IsReadOnly);
        }

        private bool IncludeInUpdateSet(DbSchemaRow row)
        {
            return (((!row.IsAutoIncrement && !row.IsRowVersion) && !row.IsHidden) && !row.IsReadOnly);
        }

        private bool IncludeInWhereClause(DbSchemaRow row, bool isUpdate)
        {
            bool flag = this.IncrementWhereCount(row);
            if (flag && row.IsHidden)
            {
                if (System.Data.ConflictOption.CompareRowVersion == this.ConflictOption)
                {
                    throw ADP.DynamicSQLNoKeyInfoRowVersionUpdate();
                }
                throw ADP.DynamicSQLNoKeyInfoUpdate();
            }
            if (!flag && (System.Data.ConflictOption.CompareAllSearchableValues == this.ConflictOption))
            {
                flag = (!row.IsLong && !row.IsRowVersion) && !row.IsHidden;
            }
            return flag;
        }

        private bool IncrementWhereCount(DbSchemaRow row)
        {
            System.Data.ConflictOption conflictOption = this.ConflictOption;
            switch (conflictOption)
            {
                case System.Data.ConflictOption.CompareAllSearchableValues:
                case System.Data.ConflictOption.OverwriteChanges:
                    return (((row.IsKey || row.IsUnique) && !row.IsLong) && !row.IsRowVersion);

                case System.Data.ConflictOption.CompareRowVersion:
                    if (((!row.IsKey && !row.IsUnique) || this._hasPartialPrimaryKey) && !row.IsRowVersion)
                    {
                        return false;
                    }
                    return !row.IsLong;
            }
            throw ADP.InvalidConflictOptions(conflictOption);
        }

        protected virtual DbCommand InitializeCommand(DbCommand command)
        {
            if (command == null)
            {
                DbCommand selectCommand = this.GetSelectCommand();
                command = selectCommand.Connection.CreateCommand();
                command.CommandTimeout = selectCommand.CommandTimeout;
                command.Transaction = selectCommand.Transaction;
            }
            command.CommandType = CommandType.Text;
            command.UpdatedRowSource = UpdateRowSource.None;
            return command;
        }

        internal static string[] ParseProcedureName(string name, string quotePrefix, string quoteSuffix)
        {
            string[] strArray = new string[4];
            if (!ADP.IsEmpty(name))
            {
                bool flag = !ADP.IsEmpty(quotePrefix) && !ADP.IsEmpty(quoteSuffix);
                int startIndex = 0;
                int index = 0;
                while ((index < strArray.Length) && (startIndex < name.Length))
                {
                    int num4 = startIndex;
                    if (flag && (name.IndexOf(quotePrefix, startIndex, quotePrefix.Length, StringComparison.Ordinal) == startIndex))
                    {
                        startIndex += quotePrefix.Length;
                        while (startIndex < name.Length)
                        {
                            startIndex = name.IndexOf(quoteSuffix, startIndex, StringComparison.Ordinal);
                            if (startIndex < 0)
                            {
                                startIndex = name.Length;
                                break;
                            }
                            startIndex += quoteSuffix.Length;
                            if ((startIndex >= name.Length) || (name.IndexOf(quoteSuffix, startIndex, quoteSuffix.Length, StringComparison.Ordinal) != startIndex))
                            {
                                break;
                            }
                            startIndex += quoteSuffix.Length;
                        }
                    }
                    if (startIndex < name.Length)
                    {
                        startIndex = name.IndexOf(".", startIndex, StringComparison.Ordinal);
                        if ((startIndex < 0) || (index == (strArray.Length - 1)))
                        {
                            startIndex = name.Length;
                        }
                    }
                    strArray[index] = name.Substring(num4, startIndex - num4);
                    startIndex += ".".Length;
                    index++;
                }
                for (int i = strArray.Length - 1; 0 <= i; i--)
                {
                    strArray[i] = (0 < index) ? strArray[--index] : null;
                }
            }
            return strArray;
        }

        private string QuotedColumn(string column)
        {
            return ADP.BuildQuotedString(this.QuotePrefix, this.QuoteSuffix, column);
        }

        public virtual string QuoteIdentifier(string unquotedIdentifier)
        {
            throw ADP.NotSupported();
        }

        public virtual void RefreshSchema()
        {
            this._dbSchemaTable = null;
            this._dbSchemaRows = null;
            this._sourceColumnNames = null;
            this._quotedBaseTableName = null;
            DbDataAdapter dataAdapter = this.DataAdapter;
            if (dataAdapter != null)
            {
                if (this.InsertCommand == dataAdapter.InsertCommand)
                {
                    dataAdapter.InsertCommand = null;
                }
                if (this.UpdateCommand == dataAdapter.UpdateCommand)
                {
                    dataAdapter.UpdateCommand = null;
                }
                if (this.DeleteCommand == dataAdapter.DeleteCommand)
                {
                    dataAdapter.DeleteCommand = null;
                }
            }
            DbCommand insertCommand = this.InsertCommand;
            if (insertCommand != null)
            {
                insertCommand.Dispose();
            }
            insertCommand = this.UpdateCommand;
            if (insertCommand != null)
            {
                insertCommand.Dispose();
            }
            insertCommand = this.DeleteCommand;
            if (insertCommand != null)
            {
                insertCommand.Dispose();
            }
            this.InsertCommand = null;
            this.UpdateCommand = null;
            this.DeleteCommand = null;
        }

        private static void RemoveExtraParameters(DbCommand command, int usedParameterCount)
        {
            for (int i = command.Parameters.Count - 1; i >= usedParameterCount; i--)
            {
                command.Parameters.RemoveAt(i);
            }
        }

        protected void RowUpdatingHandler(RowUpdatingEventArgs rowUpdatingEvent)
        {
            if (rowUpdatingEvent == null)
            {
                throw ADP.ArgumentNull("rowUpdatingEvent");
            }
            try
            {
                if (rowUpdatingEvent.Status == UpdateStatus.Continue)
                {
                    StatementType statementType = rowUpdatingEvent.StatementType;
                    DbCommand insertCommand = (DbCommand) rowUpdatingEvent.Command;
                    if (insertCommand != null)
                    {
                        switch (statementType)
                        {
                            case StatementType.Select:
                                return;

                            case StatementType.Insert:
                                insertCommand = this.InsertCommand;
                                break;

                            case StatementType.Update:
                                insertCommand = this.UpdateCommand;
                                break;

                            case StatementType.Delete:
                                insertCommand = this.DeleteCommand;
                                break;

                            default:
                                throw ADP.InvalidStatementType(statementType);
                        }
                        if (insertCommand != rowUpdatingEvent.Command)
                        {
                            insertCommand = (DbCommand) rowUpdatingEvent.Command;
                            if ((insertCommand != null) && (insertCommand.Connection == null))
                            {
                                DbDataAdapter dataAdapter = this.DataAdapter;
                                DbCommand command2 = (dataAdapter != null) ? dataAdapter.SelectCommand : null;
                                if (command2 != null)
                                {
                                    insertCommand.Connection = command2.Connection;
                                }
                            }
                        }
                        else
                        {
                            insertCommand = null;
                        }
                    }
                    if (insertCommand == null)
                    {
                        this.RowUpdatingHandlerBuilder(rowUpdatingEvent);
                    }
                }
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                ADP.TraceExceptionForCapture(exception);
                rowUpdatingEvent.Status = UpdateStatus.ErrorsOccurred;
                rowUpdatingEvent.Errors = exception;
            }
        }

        private void RowUpdatingHandlerBuilder(RowUpdatingEventArgs rowUpdatingEvent)
        {
            DbCommand command;
            DataRow dataRow = rowUpdatingEvent.Row;
            this.BuildCache(false, dataRow, false);
            switch (rowUpdatingEvent.StatementType)
            {
                case StatementType.Insert:
                    command = this.BuildInsertCommand(rowUpdatingEvent.TableMapping, dataRow);
                    break;

                case StatementType.Update:
                    command = this.BuildUpdateCommand(rowUpdatingEvent.TableMapping, dataRow);
                    break;

                case StatementType.Delete:
                    command = this.BuildDeleteCommand(rowUpdatingEvent.TableMapping, dataRow);
                    break;

                default:
                    throw ADP.InvalidStatementType(rowUpdatingEvent.StatementType);
            }
            if (command == null)
            {
                if (dataRow != null)
                {
                    dataRow.AcceptChanges();
                }
                rowUpdatingEvent.Status = UpdateStatus.SkipCurrentRow;
            }
            rowUpdatingEvent.Command = command;
        }

        protected abstract void SetRowUpdatingHandler(DbDataAdapter adapter);
        public virtual string UnquoteIdentifier(string quotedIdentifier)
        {
            throw ADP.NotSupported();
        }

        [ResCategory("DataCategory_Schema"), ResDescription("DbCommandBuilder_CatalogLocation"), DefaultValue(1)]
        public virtual System.Data.Common.CatalogLocation CatalogLocation
        {
            get
            {
                return this._catalogLocation;
            }
            set
            {
                if (this._dbSchemaTable != null)
                {
                    throw ADP.NoQuoteChange();
                }
                switch (value)
                {
                    case System.Data.Common.CatalogLocation.Start:
                    case System.Data.Common.CatalogLocation.End:
                        this._catalogLocation = value;
                        return;
                }
                throw ADP.InvalidCatalogLocation(value);
            }
        }

        [ResDescription("DbCommandBuilder_CatalogSeparator"), DefaultValue("."), ResCategory("DataCategory_Schema")]
        public virtual string CatalogSeparator
        {
            get
            {
                string str = this._catalogSeparator;
                if ((str != null) && (0 < str.Length))
                {
                    return str;
                }
                return ".";
            }
            set
            {
                if (this._dbSchemaTable != null)
                {
                    throw ADP.NoQuoteChange();
                }
                this._catalogSeparator = value;
            }
        }

        [ResCategory("DataCategory_Update"), DefaultValue(1), ResDescription("DbCommandBuilder_ConflictOption")]
        public virtual System.Data.ConflictOption ConflictOption
        {
            get
            {
                return this._conflictDetection;
            }
            set
            {
                switch (value)
                {
                    case System.Data.ConflictOption.CompareAllSearchableValues:
                    case System.Data.ConflictOption.CompareRowVersion:
                    case System.Data.ConflictOption.OverwriteChanges:
                        this._conflictDetection = value;
                        return;
                }
                throw ADP.InvalidConflictOptions(value);
            }
        }

        [Browsable(false), ResDescription("DbCommandBuilder_DataAdapter"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DbDataAdapter DataAdapter
        {
            get
            {
                return this._dataAdapter;
            }
            set
            {
                if (this._dataAdapter != value)
                {
                    this.RefreshSchema();
                    if (this._dataAdapter != null)
                    {
                        this.SetRowUpdatingHandler(this._dataAdapter);
                        this._dataAdapter = null;
                    }
                    if (value != null)
                    {
                        this.SetRowUpdatingHandler(value);
                        this._dataAdapter = value;
                    }
                }
            }
        }

        private DbCommand DeleteCommand
        {
            get
            {
                return this._deleteCommand;
            }
            set
            {
                this._deleteCommand = value;
            }
        }

        private DbCommand InsertCommand
        {
            get
            {
                return this._insertCommand;
            }
            set
            {
                this._insertCommand = value;
            }
        }

        internal int ParameterNameMaxLength
        {
            get
            {
                return this._parameterNameMaxLength;
            }
        }

        internal string ParameterNamePattern
        {
            get
            {
                return this._parameterNamePattern;
            }
        }

        private string QuotedBaseTableName
        {
            get
            {
                return this._quotedBaseTableName;
            }
        }

        [DefaultValue(""), ResCategory("DataCategory_Schema"), ResDescription("DbCommandBuilder_QuotePrefix")]
        public virtual string QuotePrefix
        {
            get
            {
                string str = this._quotePrefix;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                if (this._dbSchemaTable != null)
                {
                    throw ADP.NoQuoteChange();
                }
                this._quotePrefix = value;
            }
        }

        [ResDescription("DbCommandBuilder_QuoteSuffix"), ResCategory("DataCategory_Schema"), DefaultValue("")]
        public virtual string QuoteSuffix
        {
            get
            {
                string str = this._quoteSuffix;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                if (this._dbSchemaTable != null)
                {
                    throw ADP.NoQuoteChange();
                }
                this._quoteSuffix = value;
            }
        }

        [ResDescription("DbCommandBuilder_SchemaSeparator"), DefaultValue("."), ResCategory("DataCategory_Schema")]
        public virtual string SchemaSeparator
        {
            get
            {
                string str = this._schemaSeparator;
                if ((str != null) && (0 < str.Length))
                {
                    return str;
                }
                return ".";
            }
            set
            {
                if (this._dbSchemaTable != null)
                {
                    throw ADP.NoQuoteChange();
                }
                this._schemaSeparator = value;
            }
        }

        [ResDescription("DbCommandBuilder_SetAllValues"), DefaultValue(false), ResCategory("DataCategory_Schema")]
        public bool SetAllValues
        {
            get
            {
                return this._setAllValues;
            }
            set
            {
                this._setAllValues = value;
            }
        }

        private DbCommand UpdateCommand
        {
            get
            {
                return this._updateCommand;
            }
            set
            {
                this._updateCommand = value;
            }
        }

        private class ParameterNames
        {
            private int _adjustedParameterNameMaxLength;
            private string[] _baseParameterNames;
            private int _count;
            private DbCommandBuilder _dbCommandBuilder;
            private int _genericParameterCount;
            private bool[] _isMutatedName;
            private string _isNullPrefix;
            private string[] _nullParameterNames;
            private string[] _originalParameterNames;
            private string _originalPrefix;
            private Regex _parameterNameParser;
            private const string AlternativeIsNullPrefix = "isnull";
            private const string AlternativeIsNullPrefix2 = "ISNULL";
            private const string AlternativeOriginalPrefix = "original";
            private const string AlternativeOriginalPrefix2 = "ORIGINAL";
            private const string DefaultIsNullPrefix = "IsNull_";
            private const string DefaultOriginalPrefix = "Original_";

            internal ParameterNames(DbCommandBuilder dbCommandBuilder, DbSchemaRow[] schemaRows)
            {
                this._dbCommandBuilder = dbCommandBuilder;
                this._baseParameterNames = new string[schemaRows.Length];
                this._originalParameterNames = new string[schemaRows.Length];
                this._nullParameterNames = new string[schemaRows.Length];
                this._isMutatedName = new bool[schemaRows.Length];
                this._count = schemaRows.Length;
                this._parameterNameParser = new Regex(this._dbCommandBuilder.ParameterNamePattern, RegexOptions.Singleline | RegexOptions.ExplicitCapture);
                this.SetAndValidateNamePrefixes();
                this._adjustedParameterNameMaxLength = this.GetAdjustedParameterNameMaxLength();
                for (int i = 0; i < schemaRows.Length; i++)
                {
                    if (schemaRows[i] != null)
                    {
                        bool flag = false;
                        string columnName = schemaRows[i].ColumnName;
                        if (((this._originalPrefix == null) || !columnName.StartsWith(this._originalPrefix, StringComparison.OrdinalIgnoreCase)) && ((this._isNullPrefix == null) || !columnName.StartsWith(this._isNullPrefix, StringComparison.OrdinalIgnoreCase)))
                        {
                            if (columnName.IndexOf(' ') >= 0)
                            {
                                columnName = columnName.Replace(' ', '_');
                                flag = true;
                            }
                            if (this._parameterNameParser.IsMatch(columnName) && (columnName.Length <= this._adjustedParameterNameMaxLength))
                            {
                                this._baseParameterNames[i] = columnName;
                                this._isMutatedName[i] = flag;
                            }
                        }
                    }
                }
                this.EliminateConflictingNames();
                for (int j = 0; j < schemaRows.Length; j++)
                {
                    if (this._baseParameterNames[j] != null)
                    {
                        if (this._originalPrefix != null)
                        {
                            this._originalParameterNames[j] = this._originalPrefix + this._baseParameterNames[j];
                        }
                        if ((this._isNullPrefix != null) && schemaRows[j].AllowDBNull)
                        {
                            this._nullParameterNames[j] = this._isNullPrefix + this._baseParameterNames[j];
                        }
                    }
                }
                this.ApplyProviderSpecificFormat();
                this.GenerateMissingNames(schemaRows);
            }

            private void ApplyProviderSpecificFormat()
            {
                for (int i = 0; i < this._baseParameterNames.Length; i++)
                {
                    if (this._baseParameterNames[i] != null)
                    {
                        this._baseParameterNames[i] = this._dbCommandBuilder.GetParameterName(this._baseParameterNames[i]);
                    }
                    if (this._originalParameterNames[i] != null)
                    {
                        this._originalParameterNames[i] = this._dbCommandBuilder.GetParameterName(this._originalParameterNames[i]);
                    }
                    if (this._nullParameterNames[i] != null)
                    {
                        this._nullParameterNames[i] = this._dbCommandBuilder.GetParameterName(this._nullParameterNames[i]);
                    }
                }
            }

            private void EliminateConflictingNames()
            {
                for (int i = 0; i < (this._count - 1); i++)
                {
                    string strvalue = this._baseParameterNames[i];
                    if (strvalue != null)
                    {
                        for (int j = i + 1; j < this._count; j++)
                        {
                            if (ADP.CompareInsensitiveInvariant(strvalue, this._baseParameterNames[j]))
                            {
                                int index = this._isMutatedName[j] ? j : i;
                                this._baseParameterNames[index] = null;
                            }
                        }
                    }
                }
            }

            internal void GenerateMissingNames(DbSchemaRow[] schemaRows)
            {
                for (int i = 0; i < this._baseParameterNames.Length; i++)
                {
                    if (this._baseParameterNames[i] == null)
                    {
                        this._baseParameterNames[i] = this.GetNextGenericParameterName();
                        this._originalParameterNames[i] = this.GetNextGenericParameterName();
                        if ((schemaRows[i] != null) && schemaRows[i].AllowDBNull)
                        {
                            this._nullParameterNames[i] = this.GetNextGenericParameterName();
                        }
                    }
                }
            }

            private int GetAdjustedParameterNameMaxLength()
            {
                int num = Math.Max((this._isNullPrefix != null) ? this._isNullPrefix.Length : 0, (this._originalPrefix != null) ? this._originalPrefix.Length : 0) + this._dbCommandBuilder.GetParameterName("").Length;
                return (this._dbCommandBuilder.ParameterNameMaxLength - num);
            }

            internal string GetBaseParameterName(int index)
            {
                return this._baseParameterNames[index];
            }

            private string GetNextGenericParameterName()
            {
                string parameterName;
                bool flag;
                do
                {
                    flag = false;
                    this._genericParameterCount++;
                    parameterName = this._dbCommandBuilder.GetParameterName(this._genericParameterCount);
                    for (int i = 0; i < this._baseParameterNames.Length; i++)
                    {
                        if (ADP.CompareInsensitiveInvariant(this._baseParameterNames[i], parameterName))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                while (flag);
                return parameterName;
            }

            internal string GetNullParameterName(int index)
            {
                return this._nullParameterNames[index];
            }

            internal string GetOriginalParameterName(int index)
            {
                return this._originalParameterNames[index];
            }

            private void SetAndValidateNamePrefixes()
            {
                if (this._parameterNameParser.IsMatch("IsNull_"))
                {
                    this._isNullPrefix = "IsNull_";
                }
                else if (this._parameterNameParser.IsMatch("isnull"))
                {
                    this._isNullPrefix = "isnull";
                }
                else if (this._parameterNameParser.IsMatch("ISNULL"))
                {
                    this._isNullPrefix = "ISNULL";
                }
                else
                {
                    this._isNullPrefix = null;
                }
                if (this._parameterNameParser.IsMatch("Original_"))
                {
                    this._originalPrefix = "Original_";
                }
                else if (this._parameterNameParser.IsMatch("original"))
                {
                    this._originalPrefix = "original";
                }
                else if (this._parameterNameParser.IsMatch("ORIGINAL"))
                {
                    this._originalPrefix = "ORIGINAL";
                }
                else
                {
                    this._originalPrefix = null;
                }
            }
        }
    }
}

