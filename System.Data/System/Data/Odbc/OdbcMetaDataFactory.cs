namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class OdbcMetaDataFactory : DbMetaDataFactory
    {
        private const string _collectionName = "CollectionName";
        private const string _populationMechanism = "PopulationMechanism";
        private const string _prepareCollection = "PrepareCollection";
        private readonly SchemaFunctionName[] _schemaMapping;
        internal static readonly char[] KeywordSeparatorChar = new char[] { ',' };

        internal OdbcMetaDataFactory(Stream XMLStream, string serverVersion, string serverVersionNormalized, OdbcConnection connection) : base(XMLStream, serverVersion, serverVersionNormalized)
        {
            this._schemaMapping = new SchemaFunctionName[] { new SchemaFunctionName(DbMetaDataCollectionNames.DataTypes, ODBC32.SQL_API.SQLGETTYPEINFO), new SchemaFunctionName(OdbcMetaDataCollectionNames.Columns, ODBC32.SQL_API.SQLCOLUMNS), new SchemaFunctionName(OdbcMetaDataCollectionNames.Indexes, ODBC32.SQL_API.SQLSTATISTICS), new SchemaFunctionName(OdbcMetaDataCollectionNames.Procedures, ODBC32.SQL_API.SQLPROCEDURES), new SchemaFunctionName(OdbcMetaDataCollectionNames.ProcedureColumns, ODBC32.SQL_API.SQLPROCEDURECOLUMNS), new SchemaFunctionName(OdbcMetaDataCollectionNames.ProcedureParameters, ODBC32.SQL_API.SQLPROCEDURECOLUMNS), new SchemaFunctionName(OdbcMetaDataCollectionNames.Tables, ODBC32.SQL_API.SQLTABLES), new SchemaFunctionName(OdbcMetaDataCollectionNames.Views, ODBC32.SQL_API.SQLTABLES) };
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.MetaDataCollections] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.MetaDataCollections);
            }
            DataTable table2 = base.CloneAndFilterCollection(DbMetaDataCollectionNames.MetaDataCollections, null);
            DataTable table = base.CollectionDataSet.Tables[DbMetaDataCollectionNames.Restrictions];
            if (table != null)
            {
                table = base.CloneAndFilterCollection(DbMetaDataCollectionNames.Restrictions, null);
            }
            DataColumn column3 = table2.Columns["PopulationMechanism"];
            DataColumn column2 = table2.Columns["CollectionName"];
            DataColumn column = null;
            if (table != null)
            {
                column = table.Columns["CollectionName"];
            }
            foreach (DataRow row in table2.Rows)
            {
                if (!(((string) row[column3]) == "PrepareCollection"))
                {
                    continue;
                }
                int index = -1;
                for (int i = 0; i < this._schemaMapping.Length; i++)
                {
                    if (this._schemaMapping[i]._schemaName == ((string) row[column2]))
                    {
                        index = i;
                        break;
                    }
                }
                if ((index != -1) && !connection.SQLGetFunctions(this._schemaMapping[index]._odbcFunction))
                {
                    if (table != null)
                    {
                        foreach (DataRow row2 in table.Rows)
                        {
                            if (((string) row[column2]) == ((string) row2[column]))
                            {
                                row2.Delete();
                            }
                        }
                        table.AcceptChanges();
                    }
                    row.Delete();
                }
            }
            table2.AcceptChanges();
            base.CollectionDataSet.Tables.Remove(base.CollectionDataSet.Tables[DbMetaDataCollectionNames.MetaDataCollections]);
            base.CollectionDataSet.Tables.Add(table2);
            if (table != null)
            {
                base.CollectionDataSet.Tables.Remove(base.CollectionDataSet.Tables[DbMetaDataCollectionNames.Restrictions]);
                base.CollectionDataSet.Tables.Add(table);
            }
        }

        private object BooleanFromODBC(object odbcSource)
        {
            if (odbcSource == DBNull.Value)
            {
                return DBNull.Value;
            }
            if (Convert.ToInt32(odbcSource, null) == 0)
            {
                return false;
            }
            return true;
        }

        private DataTable DataTableFromDataReader(IDataReader reader, string tableName)
        {
            object[] objArray;
            DataTable table = this.NewDataTableFromReader(reader, out objArray, tableName);
            while (reader.Read())
            {
                reader.GetValues(objArray);
                table.Rows.Add(objArray);
            }
            return table;
        }

        private void DataTableFromDataReaderDataTypes(DataTable dataTypesTable, OdbcDataReader dataReader, OdbcConnection connection)
        {
            DataTable schemaTable = null;
            schemaTable = dataReader.GetSchemaTable();
            if (schemaTable == null)
            {
                throw ADP.OdbcNoTypesFromProvider();
            }
            object[] values = new object[schemaTable.Rows.Count];
            DataColumn column19 = dataTypesTable.Columns[DbMetaDataColumnNames.TypeName];
            DataColumn column18 = dataTypesTable.Columns[DbMetaDataColumnNames.ProviderDbType];
            DataColumn column17 = dataTypesTable.Columns[DbMetaDataColumnNames.ColumnSize];
            DataColumn column16 = dataTypesTable.Columns[DbMetaDataColumnNames.CreateParameters];
            DataColumn column15 = dataTypesTable.Columns[DbMetaDataColumnNames.DataType];
            DataColumn column6 = dataTypesTable.Columns[DbMetaDataColumnNames.IsAutoIncrementable];
            DataColumn column14 = dataTypesTable.Columns[DbMetaDataColumnNames.IsCaseSensitive];
            DataColumn column5 = dataTypesTable.Columns[DbMetaDataColumnNames.IsFixedLength];
            DataColumn column13 = dataTypesTable.Columns[DbMetaDataColumnNames.IsFixedPrecisionScale];
            DataColumn column4 = dataTypesTable.Columns[DbMetaDataColumnNames.IsLong];
            DataColumn column3 = dataTypesTable.Columns[DbMetaDataColumnNames.IsNullable];
            DataColumn column2 = dataTypesTable.Columns[DbMetaDataColumnNames.IsSearchable];
            DataColumn column = dataTypesTable.Columns[DbMetaDataColumnNames.IsSearchableWithLike];
            DataColumn column12 = dataTypesTable.Columns[DbMetaDataColumnNames.IsUnsigned];
            DataColumn column11 = dataTypesTable.Columns[DbMetaDataColumnNames.MaximumScale];
            DataColumn column10 = dataTypesTable.Columns[DbMetaDataColumnNames.MinimumScale];
            DataColumn column9 = dataTypesTable.Columns[DbMetaDataColumnNames.LiteralPrefix];
            DataColumn column8 = dataTypesTable.Columns[DbMetaDataColumnNames.LiteralSuffix];
            DataColumn column7 = dataTypesTable.Columns[OdbcMetaDataColumnNames.SQLType];
            while (dataReader.Read())
            {
                TypeMap map;
                dataReader.GetValues(values);
                DataRow row = dataTypesTable.NewRow();
                row[column19] = values[0];
                row[column7] = values[1];
                ODBC32.SQL_TYPE sqltype = (ODBC32.SQL_TYPE) ((short) ((int) Convert.ChangeType(values[1], typeof(int), null)));
                if (!connection.IsV3Driver)
                {
                    if (sqltype == ~ODBC32.SQL_TYPE.WLONGVARCHAR)
                    {
                        sqltype = ODBC32.SQL_TYPE.TYPE_DATE;
                    }
                    else if (sqltype == ~ODBC32.SQL_TYPE.GUID)
                    {
                        sqltype = ODBC32.SQL_TYPE.TYPE_TIME;
                    }
                }
                try
                {
                    map = TypeMap.FromSqlType(sqltype);
                }
                catch (ArgumentException)
                {
                    map = null;
                }
                if (map != null)
                {
                    row[column18] = map._odbcType;
                    row[column15] = map._type.FullName;
                    switch (sqltype)
                    {
                        case ODBC32.SQL_TYPE.SS_TIME_EX:
                        case ODBC32.SQL_TYPE.SS_UTCDATETIME:
                        case ODBC32.SQL_TYPE.SS_VARIANT:
                        case ODBC32.SQL_TYPE.GUID:
                        case ODBC32.SQL_TYPE.WCHAR:
                        case ODBC32.SQL_TYPE.BIT:
                        case ODBC32.SQL_TYPE.TINYINT:
                        case ODBC32.SQL_TYPE.BIGINT:
                        case ODBC32.SQL_TYPE.BINARY:
                        case ODBC32.SQL_TYPE.CHAR:
                        case ODBC32.SQL_TYPE.NUMERIC:
                        case ODBC32.SQL_TYPE.DECIMAL:
                        case ODBC32.SQL_TYPE.INTEGER:
                        case ODBC32.SQL_TYPE.SMALLINT:
                        case ODBC32.SQL_TYPE.FLOAT:
                        case ODBC32.SQL_TYPE.REAL:
                        case ODBC32.SQL_TYPE.DOUBLE:
                        case ODBC32.SQL_TYPE.TIMESTAMP:
                        case ODBC32.SQL_TYPE.TYPE_DATE:
                        case ODBC32.SQL_TYPE.TYPE_TIME:
                        case ODBC32.SQL_TYPE.TYPE_TIMESTAMP:
                            goto Label_02F8;

                        case ODBC32.SQL_TYPE.SS_XML:
                        case ODBC32.SQL_TYPE.WLONGVARCHAR:
                        case ODBC32.SQL_TYPE.LONGVARBINARY:
                        case ODBC32.SQL_TYPE.LONGVARCHAR:
                            goto Label_02BC;

                        case ODBC32.SQL_TYPE.WVARCHAR:
                        case ODBC32.SQL_TYPE.VARBINARY:
                        case ODBC32.SQL_TYPE.VARCHAR:
                            goto Label_02DA;
                    }
                }
                goto Label_0314;
            Label_02BC:
                row[column4] = true;
                row[column5] = false;
                goto Label_0314;
            Label_02DA:
                row[column4] = false;
                row[column5] = false;
                goto Label_0314;
            Label_02F8:
                row[column4] = false;
                row[column5] = true;
            Label_0314:
                row[column17] = values[2];
                row[column16] = values[5];
                if ((values[11] == DBNull.Value) || (Convert.ToInt16(values[11], null) == 0))
                {
                    row[column6] = false;
                }
                else
                {
                    row[column6] = true;
                }
                row[column14] = this.BooleanFromODBC(values[7]);
                row[column13] = this.BooleanFromODBC(values[10]);
                if (values[6] != DBNull.Value)
                {
                    switch (((ODBC32.SQL_NULLABILITY) ((ushort) Convert.ToInt16(values[6], null))))
                    {
                        case ODBC32.SQL_NULLABILITY.NO_NULLS:
                            row[column3] = false;
                            break;

                        case ODBC32.SQL_NULLABILITY.NULLABLE:
                            row[column3] = true;
                            break;

                        case ODBC32.SQL_NULLABILITY.UNKNOWN:
                            row[column3] = DBNull.Value;
                            break;
                    }
                }
                if (DBNull.Value != values[8])
                {
                    switch (Convert.ToInt16(values[8], null))
                    {
                        case 0:
                            row[column2] = false;
                            row[column] = false;
                            break;

                        case 1:
                            row[column2] = false;
                            row[column] = true;
                            break;

                        case 2:
                            row[column2] = true;
                            row[column] = false;
                            break;

                        case 3:
                            row[column2] = true;
                            row[column] = true;
                            break;
                    }
                }
                row[column12] = this.BooleanFromODBC(values[9]);
                if (values[14] != DBNull.Value)
                {
                    row[column11] = values[14];
                }
                if (values[13] != DBNull.Value)
                {
                    row[column10] = values[13];
                }
                if (values[3] != DBNull.Value)
                {
                    row[column9] = values[3];
                }
                if (values[4] != DBNull.Value)
                {
                    row[column8] = values[4];
                }
                dataTypesTable.Rows.Add(row);
            }
        }

        private DataTable DataTableFromDataReaderIndex(IDataReader reader, string tableName, string restrictionIndexName)
        {
            object[] objArray;
            DataTable table = this.NewDataTableFromReader(reader, out objArray, tableName);
            int index = 6;
            int num = 5;
            while (reader.Read())
            {
                reader.GetValues(objArray);
                if (this.IncludeIndexRow(objArray[num], restrictionIndexName, Convert.ToInt16(objArray[index], null)))
                {
                    table.Rows.Add(objArray);
                }
            }
            return table;
        }

        private DataTable DataTableFromDataReaderProcedureColumns(IDataReader reader, string tableName, bool isColumn)
        {
            object[] objArray;
            DataTable table = this.NewDataTableFromReader(reader, out objArray, tableName);
            int index = 4;
            while (reader.Read())
            {
                reader.GetValues(objArray);
                if ((objArray[index].GetType() == typeof(short)) && (((((short) objArray[index]) == 3) && isColumn) || ((((short) objArray[index]) != 3) && !isColumn)))
                {
                    table.Rows.Add(objArray);
                }
            }
            return table;
        }

        private DataTable DataTableFromDataReaderProcedures(IDataReader reader, string tableName, short procedureType)
        {
            object[] objArray;
            DataTable table = this.NewDataTableFromReader(reader, out objArray, tableName);
            int index = 7;
            while (reader.Read())
            {
                reader.GetValues(objArray);
                if ((objArray[index].GetType() == typeof(short)) && (((short) objArray[index]) == procedureType))
                {
                    table.Rows.Add(objArray);
                }
            }
            return table;
        }

        private void FillOutRestrictions(int restrictionsCount, string[] restrictions, object[] allRestrictions, string collectionName)
        {
            int index = 0;
            if (restrictions != null)
            {
                if (restrictions.Length > restrictionsCount)
                {
                    throw ADP.TooManyRestrictions(collectionName);
                }
                index = 0;
                while (index < restrictions.Length)
                {
                    if (restrictions[index] != null)
                    {
                        allRestrictions[index] = restrictions[index];
                    }
                    index++;
                }
            }
            while (index < restrictionsCount)
            {
                allRestrictions[index] = null;
                index++;
            }
        }

        private DataTable GetColumnsCollection(string[] restrictions, OdbcConnection connection)
        {
            OdbcDataReader reader = null;
            OdbcCommand command = null;
            DataTable table = null;
            try
            {
                command = this.GetCommand(connection);
                string[] allRestrictions = new string[4];
                this.FillOutRestrictions(4, restrictions, allRestrictions, OdbcMetaDataCollectionNames.Columns);
                reader = command.ExecuteReaderFromSQLMethod(allRestrictions, ODBC32.SQL_API.SQLCOLUMNS);
                table = this.DataTableFromDataReader(reader, OdbcMetaDataCollectionNames.Columns);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
            }
            return table;
        }

        private OdbcCommand GetCommand(OdbcConnection connection)
        {
            OdbcCommand command = connection.CreateCommand();
            command.Transaction = connection.LocalTransaction;
            return command;
        }

        private DataTable GetDataSourceInformationCollection(string[] restrictions, OdbcConnection connection)
        {
            ODBC32.RetCode code;
            int num;
            short num2;
            if (!ADP.IsEmptyArray(restrictions))
            {
                throw ADP.TooManyRestrictions(DbMetaDataCollectionNames.DataSourceInformation);
            }
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.DataSourceInformation] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.DataSourceInformation);
            }
            DataTable table = base.CloneAndFilterCollection(DbMetaDataCollectionNames.DataSourceInformation, null);
            if (table.Rows.Count != 1)
            {
                throw ADP.IncorrectNumberOfDataSourceInformationRows();
            }
            DataRow row = table.Rows[0];
            string infoStringUnhandled = connection.GetInfoStringUnhandled(ODBC32.SQL_INFO.CATALOG_NAME_SEPARATOR);
            if (!ADP.IsEmpty(infoStringUnhandled))
            {
                StringBuilder escapedString = new StringBuilder();
                ADP.EscapeSpecialCharacters(infoStringUnhandled, escapedString);
                row[DbMetaDataColumnNames.CompositeIdentifierSeparatorPattern] = escapedString.ToString();
            }
            infoStringUnhandled = connection.GetInfoStringUnhandled(ODBC32.SQL_INFO.DBMS_NAME);
            if (infoStringUnhandled != null)
            {
                row[DbMetaDataColumnNames.DataSourceProductName] = infoStringUnhandled;
            }
            row[DbMetaDataColumnNames.DataSourceProductVersion] = base.ServerVersion;
            row[DbMetaDataColumnNames.DataSourceProductVersionNormalized] = base.ServerVersionNormalized;
            row[DbMetaDataColumnNames.ParameterMarkerFormat] = "?";
            row[DbMetaDataColumnNames.ParameterMarkerPattern] = @"\?";
            row[DbMetaDataColumnNames.ParameterNameMaxLength] = 0;
            if (connection.IsV3Driver)
            {
                code = connection.GetInfoInt32Unhandled(ODBC32.SQL_INFO.SQL_OJ_CAPABILITIES_30, out num);
            }
            else
            {
                code = connection.GetInfoInt32Unhandled(ODBC32.SQL_INFO.SQL_OJ_CAPABILITIES_20, out num);
            }
            if ((code == ODBC32.RetCode.SUCCESS) || (code == ODBC32.RetCode.SUCCESS_WITH_INFO))
            {
                SupportedJoinOperators none = SupportedJoinOperators.None;
                if ((num & 1) != 0)
                {
                    none |= SupportedJoinOperators.LeftOuter;
                }
                if ((num & 2) != 0)
                {
                    none |= SupportedJoinOperators.RightOuter;
                }
                if ((num & 4) != 0)
                {
                    none |= SupportedJoinOperators.FullOuter;
                }
                if ((num & 0x20) != 0)
                {
                    none |= SupportedJoinOperators.Inner;
                }
                row[DbMetaDataColumnNames.SupportedJoinOperators] = none;
            }
            code = connection.GetInfoInt16Unhandled(ODBC32.SQL_INFO.GROUP_BY, out num2);
            GroupByBehavior unknown = GroupByBehavior.Unknown;
            if ((code == ODBC32.RetCode.SUCCESS) || (code == ODBC32.RetCode.SUCCESS_WITH_INFO))
            {
                switch (num2)
                {
                    case 0:
                        unknown = GroupByBehavior.NotSupported;
                        break;

                    case 1:
                        unknown = GroupByBehavior.ExactMatch;
                        break;

                    case 2:
                        unknown = GroupByBehavior.MustContainAll;
                        break;

                    case 3:
                        unknown = GroupByBehavior.Unrelated;
                        break;
                }
            }
            row[DbMetaDataColumnNames.GroupByBehavior] = unknown;
            code = connection.GetInfoInt16Unhandled(ODBC32.SQL_INFO.IDENTIFIER_CASE, out num2);
            IdentifierCase insensitive = IdentifierCase.Unknown;
            if ((code == ODBC32.RetCode.SUCCESS) || (code == ODBC32.RetCode.SUCCESS_WITH_INFO))
            {
                switch (num2)
                {
                    case 1:
                    case 2:
                    case 4:
                        insensitive = IdentifierCase.Insensitive;
                        break;

                    case 3:
                        insensitive = IdentifierCase.Sensitive;
                        break;
                }
            }
            row[DbMetaDataColumnNames.IdentifierCase] = insensitive;
            switch (connection.GetInfoStringUnhandled(ODBC32.SQL_INFO.ORDER_BY_COLUMNS_IN_SELECT))
            {
                case "Y":
                    row[DbMetaDataColumnNames.OrderByColumnsInSelect] = true;
                    break;

                case "N":
                    row[DbMetaDataColumnNames.OrderByColumnsInSelect] = false;
                    break;
            }
            infoStringUnhandled = connection.QuoteChar("GetSchema");
            if (((infoStringUnhandled != null) && (infoStringUnhandled != " ")) && (infoStringUnhandled.Length == 1))
            {
                StringBuilder builder = new StringBuilder();
                ADP.EscapeSpecialCharacters(infoStringUnhandled, builder);
                string str2 = builder.ToString();
                builder.Length = 0;
                ADP.EscapeSpecialCharacters(infoStringUnhandled, builder);
                builder.Append("(([^");
                builder.Append(str2);
                builder.Append("]|");
                builder.Append(str2);
                builder.Append(str2);
                builder.Append(")*)");
                builder.Append(str2);
                row[DbMetaDataColumnNames.QuotedIdentifierPattern] = builder.ToString();
            }
            code = connection.GetInfoInt16Unhandled(ODBC32.SQL_INFO.QUOTED_IDENTIFIER_CASE, out num2);
            IdentifierCase sensitive = IdentifierCase.Unknown;
            if ((code == ODBC32.RetCode.SUCCESS) || (code == ODBC32.RetCode.SUCCESS_WITH_INFO))
            {
                switch (num2)
                {
                    case 1:
                    case 2:
                    case 4:
                        sensitive = IdentifierCase.Insensitive;
                        break;

                    case 3:
                        sensitive = IdentifierCase.Sensitive;
                        break;
                }
            }
            row[DbMetaDataColumnNames.QuotedIdentifierCase] = sensitive;
            table.AcceptChanges();
            return table;
        }

        private DataTable GetDataTypesCollection(string[] restrictions, OdbcConnection connection)
        {
            if (!ADP.IsEmptyArray(restrictions))
            {
                throw ADP.TooManyRestrictions(DbMetaDataCollectionNames.DataTypes);
            }
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.DataTypes] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.DataTypes);
            }
            DataTable dataTypesTable = base.CloneAndFilterCollection(DbMetaDataCollectionNames.DataTypes, null);
            OdbcCommand command = null;
            OdbcDataReader dataReader = null;
            object[] methodArguments = new object[] { (short) 0 };
            try
            {
                command = this.GetCommand(connection);
                dataReader = command.ExecuteReaderFromSQLMethod(methodArguments, ODBC32.SQL_API.SQLGETTYPEINFO);
                this.DataTableFromDataReaderDataTypes(dataTypesTable, dataReader, connection);
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
            }
            dataTypesTable.AcceptChanges();
            return dataTypesTable;
        }

        private DataTable GetIndexCollection(string[] restrictions, OdbcConnection connection)
        {
            OdbcDataReader reader = null;
            OdbcCommand command = null;
            DataTable table = null;
            try
            {
                command = this.GetCommand(connection);
                object[] allRestrictions = new object[5];
                this.FillOutRestrictions(4, restrictions, allRestrictions, OdbcMetaDataCollectionNames.Indexes);
                if (allRestrictions[2] == null)
                {
                    throw ODBC.GetSchemaRestrictionRequired();
                }
                allRestrictions[3] = (short) 1;
                allRestrictions[4] = (short) 1;
                reader = command.ExecuteReaderFromSQLMethod(allRestrictions, ODBC32.SQL_API.SQLSTATISTICS);
                string restrictionIndexName = null;
                if ((restrictions != null) && (restrictions.Length >= 4))
                {
                    restrictionIndexName = restrictions[3];
                }
                table = this.DataTableFromDataReaderIndex(reader, OdbcMetaDataCollectionNames.Indexes, restrictionIndexName);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
            }
            return table;
        }

        private DataTable GetProcedureColumnsCollection(string[] restrictions, OdbcConnection connection, bool isColumns)
        {
            OdbcDataReader reader = null;
            OdbcCommand command = null;
            DataTable table = null;
            try
            {
                string procedureColumns;
                command = this.GetCommand(connection);
                string[] allRestrictions = new string[4];
                this.FillOutRestrictions(4, restrictions, allRestrictions, OdbcMetaDataCollectionNames.Columns);
                reader = command.ExecuteReaderFromSQLMethod(allRestrictions, ODBC32.SQL_API.SQLPROCEDURECOLUMNS);
                if (isColumns)
                {
                    procedureColumns = OdbcMetaDataCollectionNames.ProcedureColumns;
                }
                else
                {
                    procedureColumns = OdbcMetaDataCollectionNames.ProcedureParameters;
                }
                table = this.DataTableFromDataReaderProcedureColumns(reader, procedureColumns, isColumns);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
            }
            return table;
        }

        private DataTable GetProceduresCollection(string[] restrictions, OdbcConnection connection)
        {
            OdbcDataReader reader = null;
            OdbcCommand command = null;
            DataTable table = null;
            try
            {
                short num;
                command = this.GetCommand(connection);
                string[] allRestrictions = new string[4];
                this.FillOutRestrictions(4, restrictions, allRestrictions, OdbcMetaDataCollectionNames.Procedures);
                reader = command.ExecuteReaderFromSQLMethod(allRestrictions, ODBC32.SQL_API.SQLPROCEDURES);
                if (allRestrictions[3] == null)
                {
                    return this.DataTableFromDataReader(reader, OdbcMetaDataCollectionNames.Procedures);
                }
                if ((restrictions[3] == "SQL_PT_UNKNOWN") || (restrictions[3] == "0"))
                {
                    num = 0;
                }
                else if ((restrictions[3] == "SQL_PT_PROCEDURE") || (restrictions[3] == "1"))
                {
                    num = 1;
                }
                else
                {
                    if (!(restrictions[3] == "SQL_PT_FUNCTION") && !(restrictions[3] == "2"))
                    {
                        throw ADP.InvalidRestrictionValue(OdbcMetaDataCollectionNames.Procedures, "PROCEDURE_TYPE", restrictions[3]);
                    }
                    num = 2;
                }
                table = this.DataTableFromDataReaderProcedures(reader, OdbcMetaDataCollectionNames.Procedures, num);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
            }
            return table;
        }

        private DataTable GetReservedWordsCollection(string[] restrictions, OdbcConnection connection)
        {
            if (!ADP.IsEmptyArray(restrictions))
            {
                throw ADP.TooManyRestrictions(DbMetaDataCollectionNames.ReservedWords);
            }
            if (base.CollectionDataSet.Tables[DbMetaDataCollectionNames.ReservedWords] == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.ReservedWords);
            }
            DataTable table = base.CloneAndFilterCollection(DbMetaDataCollectionNames.ReservedWords, null);
            DataColumn column = table.Columns[DbMetaDataColumnNames.ReservedWord];
            if (column == null)
            {
                throw ADP.UnableToBuildCollection(DbMetaDataCollectionNames.ReservedWords);
            }
            string infoStringUnhandled = connection.GetInfoStringUnhandled(ODBC32.SQL_INFO.KEYWORDS);
            if (infoStringUnhandled != null)
            {
                string[] strArray = infoStringUnhandled.Split(KeywordSeparatorChar);
                for (int i = 0; i < strArray.Length; i++)
                {
                    DataRow row = table.NewRow();
                    row[column] = strArray[i];
                    table.Rows.Add(row);
                    row.AcceptChanges();
                }
            }
            return table;
        }

        private DataTable GetTablesCollection(string[] restrictions, OdbcConnection connection, bool isTables)
        {
            OdbcDataReader reader = null;
            OdbcCommand command = null;
            DataTable table = null;
            try
            {
                string tables;
                string str2;
                command = this.GetCommand(connection);
                string[] allRestrictions = new string[4];
                if (isTables)
                {
                    str2 = "TABLE,SYSTEM TABLE";
                    tables = OdbcMetaDataCollectionNames.Tables;
                }
                else
                {
                    str2 = "VIEW";
                    tables = OdbcMetaDataCollectionNames.Views;
                }
                this.FillOutRestrictions(3, restrictions, allRestrictions, tables);
                allRestrictions[3] = str2;
                reader = command.ExecuteReaderFromSQLMethod(allRestrictions, ODBC32.SQL_API.SQLTABLES);
                table = this.DataTableFromDataReader(reader, tables);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
            }
            return table;
        }

        private bool IncludeIndexRow(object rowIndexName, string restrictionIndexName, short rowIndexType)
        {
            if (rowIndexType == 0)
            {
                return false;
            }
            if ((restrictionIndexName != null) && (restrictionIndexName != ((string) rowIndexName)))
            {
                return false;
            }
            return true;
        }

        private DataTable NewDataTableFromReader(IDataReader reader, out object[] values, string tableName)
        {
            DataTable table = new DataTable(tableName) {
                Locale = CultureInfo.InvariantCulture
            };
            foreach (DataRow row in reader.GetSchemaTable().Rows)
            {
                table.Columns.Add(row["ColumnName"] as string, (Type) row["DataType"]);
            }
            values = new object[table.Columns.Count];
            return table;
        }

        protected override DataTable PrepareCollection(string collectionName, string[] restrictions, DbConnection connection)
        {
            DataTable columnsCollection = null;
            OdbcConnection connection2 = (OdbcConnection) connection;
            if (collectionName == OdbcMetaDataCollectionNames.Tables)
            {
                columnsCollection = this.GetTablesCollection(restrictions, connection2, true);
            }
            else if (collectionName == OdbcMetaDataCollectionNames.Views)
            {
                columnsCollection = this.GetTablesCollection(restrictions, connection2, false);
            }
            else if (collectionName == OdbcMetaDataCollectionNames.Columns)
            {
                columnsCollection = this.GetColumnsCollection(restrictions, connection2);
            }
            else if (collectionName == OdbcMetaDataCollectionNames.Procedures)
            {
                columnsCollection = this.GetProceduresCollection(restrictions, connection2);
            }
            else if (collectionName == OdbcMetaDataCollectionNames.ProcedureColumns)
            {
                columnsCollection = this.GetProcedureColumnsCollection(restrictions, connection2, true);
            }
            else if (collectionName == OdbcMetaDataCollectionNames.ProcedureParameters)
            {
                columnsCollection = this.GetProcedureColumnsCollection(restrictions, connection2, false);
            }
            else if (collectionName == OdbcMetaDataCollectionNames.Indexes)
            {
                columnsCollection = this.GetIndexCollection(restrictions, connection2);
            }
            else if (collectionName == DbMetaDataCollectionNames.DataTypes)
            {
                columnsCollection = this.GetDataTypesCollection(restrictions, connection2);
            }
            else if (collectionName == DbMetaDataCollectionNames.DataSourceInformation)
            {
                columnsCollection = this.GetDataSourceInformationCollection(restrictions, connection2);
            }
            else if (collectionName == DbMetaDataCollectionNames.ReservedWords)
            {
                columnsCollection = this.GetReservedWordsCollection(restrictions, connection2);
            }
            if (columnsCollection == null)
            {
                throw ADP.UnableToBuildCollection(collectionName);
            }
            return columnsCollection;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SchemaFunctionName
        {
            internal readonly string _schemaName;
            internal readonly ODBC32.SQL_API _odbcFunction;
            internal SchemaFunctionName(string schemaName, ODBC32.SQL_API odbcFunction)
            {
                this._schemaName = schemaName;
                this._odbcFunction = odbcFunction;
            }
        }
    }
}

