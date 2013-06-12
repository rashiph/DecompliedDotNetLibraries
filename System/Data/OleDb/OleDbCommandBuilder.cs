namespace System.Data.OleDb
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;

    public sealed class OleDbCommandBuilder : DbCommandBuilder
    {
        public OleDbCommandBuilder()
        {
            GC.SuppressFinalize(this);
        }

        public OleDbCommandBuilder(OleDbDataAdapter adapter) : this()
        {
            this.DataAdapter = adapter;
        }

        protected override void ApplyParameterInfo(DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
        {
            OleDbParameter parameter2 = (OleDbParameter) parameter;
            object obj3 = datarow[SchemaTableColumn.ProviderType];
            parameter2.OleDbType = (OleDbType) obj3;
            object obj2 = datarow[SchemaTableColumn.NumericPrecision];
            if (DBNull.Value != obj2)
            {
                byte num2 = (byte) ((short) obj2);
                parameter2.PrecisionInternal = (0xff != num2) ? num2 : ((byte) 0);
            }
            obj2 = datarow[SchemaTableColumn.NumericScale];
            if (DBNull.Value != obj2)
            {
                byte num = (byte) ((short) obj2);
                parameter2.ScaleInternal = (0xff != num) ? num : ((byte) 0);
            }
        }

        private static ParameterDirection ConvertToParameterDirection(int value)
        {
            switch (value)
            {
                case 1:
                    return ParameterDirection.Input;

                case 2:
                    return ParameterDirection.InputOutput;

                case 3:
                    return ParameterDirection.Output;

                case 4:
                    return ParameterDirection.ReturnValue;
            }
            return ParameterDirection.Input;
        }

        public static void DeriveParameters(OleDbCommand command)
        {
            OleDbConnection.ExecutePermission.Demand();
            if (command == null)
            {
                throw ADP.ArgumentNull("command");
            }
            CommandType commandType = command.CommandType;
            if (commandType == CommandType.Text)
            {
                throw ADP.DeriveParametersNotSupported(command);
            }
            if (commandType != CommandType.StoredProcedure)
            {
                if (commandType == CommandType.TableDirect)
                {
                    throw ADP.DeriveParametersNotSupported(command);
                }
                throw ADP.InvalidCommandType(command.CommandType);
            }
            if (ADP.IsEmpty(command.CommandText))
            {
                throw ADP.CommandTextRequired("DeriveParameters");
            }
            OleDbConnection connection = command.Connection;
            if (connection == null)
            {
                throw ADP.ConnectionRequired("DeriveParameters");
            }
            ConnectionState state = connection.State;
            if (ConnectionState.Open != state)
            {
                throw ADP.OpenConnectionRequired("DeriveParameters", state);
            }
            OleDbParameter[] parameterArray = DeriveParametersFromStoredProcedure(connection, command);
            OleDbParameterCollection parameters = command.Parameters;
            parameters.Clear();
            for (int i = 0; i < parameterArray.Length; i++)
            {
                parameters.Add(parameterArray[i]);
            }
        }

        private static OleDbParameter[] DeriveParametersFromStoredProcedure(OleDbConnection connection, OleDbCommand command)
        {
            OleDbParameter[] parameterArray = new OleDbParameter[0];
            if (connection.SupportSchemaRowset(OleDbSchemaGuid.Procedure_Parameters))
            {
                string str3;
                string str4;
                connection.GetLiteralQuotes("DeriveParameters", out str4, out str3);
                object[] sourceArray = MultipartIdentifier.ParseMultipartIdentifier(command.CommandText, str4, str3, '.', 4, true, "OLEDB_OLEDBCommandText", false);
                if (sourceArray[3] == null)
                {
                    throw ADP.NoStoredProcedureExists(command.CommandText);
                }
                object[] destinationArray = new object[4];
                Array.Copy(sourceArray, 1, destinationArray, 0, 3);
                DataTable schemaRowset = connection.GetSchemaRowset(OleDbSchemaGuid.Procedure_Parameters, destinationArray);
                if (schemaRowset != null)
                {
                    DataColumnCollection columns = schemaRowset.Columns;
                    DataColumn column6 = null;
                    DataColumn column5 = null;
                    DataColumn column4 = null;
                    DataColumn column3 = null;
                    DataColumn column2 = null;
                    DataColumn column = null;
                    DataColumn column7 = null;
                    int index = columns.IndexOf("PARAMETER_NAME");
                    if (-1 != index)
                    {
                        column6 = columns[index];
                    }
                    index = columns.IndexOf("PARAMETER_TYPE");
                    if (-1 != index)
                    {
                        column5 = columns[index];
                    }
                    index = columns.IndexOf("DATA_TYPE");
                    if (-1 != index)
                    {
                        column4 = columns[index];
                    }
                    index = columns.IndexOf("CHARACTER_MAXIMUM_LENGTH");
                    if (-1 != index)
                    {
                        column3 = columns[index];
                    }
                    index = columns.IndexOf("NUMERIC_PRECISION");
                    if (-1 != index)
                    {
                        column2 = columns[index];
                    }
                    index = columns.IndexOf("NUMERIC_SCALE");
                    if (-1 != index)
                    {
                        column = columns[index];
                    }
                    index = columns.IndexOf("TYPE_NAME");
                    if (-1 != index)
                    {
                        column7 = columns[index];
                    }
                    DataRow[] rowArray = schemaRowset.Select(null, "ORDINAL_POSITION ASC", DataViewRowState.CurrentRows);
                    parameterArray = new OleDbParameter[rowArray.Length];
                    for (index = 0; index < rowArray.Length; index++)
                    {
                        DataRow row = rowArray[index];
                        OleDbParameter parameter = new OleDbParameter();
                        if ((column6 != null) && !row.IsNull(column6, DataRowVersion.Default))
                        {
                            parameter.ParameterName = Convert.ToString(row[column6, DataRowVersion.Default], CultureInfo.InvariantCulture).TrimStart(new char[] { '@', ' ', ':' });
                        }
                        if ((column5 != null) && !row.IsNull(column5, DataRowVersion.Default))
                        {
                            short num3 = Convert.ToInt16(row[column5, DataRowVersion.Default], CultureInfo.InvariantCulture);
                            parameter.Direction = ConvertToParameterDirection(num3);
                        }
                        if ((column4 != null) && !row.IsNull(column4, DataRowVersion.Default))
                        {
                            short dbType = Convert.ToInt16(row[column4, DataRowVersion.Default], CultureInfo.InvariantCulture);
                            parameter.OleDbType = NativeDBType.FromDBType(dbType, false, false).enumOleDbType;
                        }
                        if ((column3 != null) && !row.IsNull(column3, DataRowVersion.Default))
                        {
                            parameter.Size = Convert.ToInt32(row[column3, DataRowVersion.Default], CultureInfo.InvariantCulture);
                        }
                        switch (parameter.OleDbType)
                        {
                            case OleDbType.VarChar:
                            case OleDbType.VarWChar:
                            case OleDbType.VarBinary:
                            {
                                string str;
                                object obj2 = row[column7, DataRowVersion.Default];
                                if ((obj2 is string) && ((str = ((string) obj2).ToLower(CultureInfo.InvariantCulture)) != null))
                                {
                                    if (str == "binary")
                                    {
                                        break;
                                    }
                                    if (str == "image")
                                    {
                                        goto Label_03B9;
                                    }
                                    if (str == "char")
                                    {
                                        goto Label_03C6;
                                    }
                                    if (str == "text")
                                    {
                                        goto Label_03D3;
                                    }
                                    if (str == "nchar")
                                    {
                                        goto Label_03E0;
                                    }
                                    if (str == "ntext")
                                    {
                                        goto Label_03ED;
                                    }
                                }
                                goto Label_03F8;
                            }
                            case OleDbType.VarNumeric:
                            case OleDbType.Decimal:
                            case OleDbType.Numeric:
                                if ((column2 != null) && !row.IsNull(column2, DataRowVersion.Default))
                                {
                                    parameter.PrecisionInternal = (byte) Convert.ToInt16(row[column2], CultureInfo.InvariantCulture);
                                }
                                if ((column != null) && !row.IsNull(column, DataRowVersion.Default))
                                {
                                    parameter.ScaleInternal = (byte) Convert.ToInt16(row[column], CultureInfo.InvariantCulture);
                                }
                                goto Label_03F8;

                            default:
                                goto Label_03F8;
                        }
                        parameter.OleDbType = OleDbType.Binary;
                        goto Label_03F8;
                    Label_03B9:
                        parameter.OleDbType = OleDbType.LongVarBinary;
                        goto Label_03F8;
                    Label_03C6:
                        parameter.OleDbType = OleDbType.Char;
                        goto Label_03F8;
                    Label_03D3:
                        parameter.OleDbType = OleDbType.LongVarChar;
                        goto Label_03F8;
                    Label_03E0:
                        parameter.OleDbType = OleDbType.WChar;
                        goto Label_03F8;
                    Label_03ED:
                        parameter.OleDbType = OleDbType.LongVarWChar;
                    Label_03F8:
                        parameterArray[index] = parameter;
                    }
                }
                if ((parameterArray.Length == 0) && connection.SupportSchemaRowset(OleDbSchemaGuid.Procedures))
                {
                    object[] objArray3 = new object[4];
                    objArray3[2] = command.CommandText;
                    destinationArray = objArray3;
                    if (connection.GetSchemaRowset(OleDbSchemaGuid.Procedures, destinationArray).Rows.Count == 0)
                    {
                        throw ADP.NoStoredProcedureExists(command.CommandText);
                    }
                }
                return parameterArray;
            }
            if (!connection.SupportSchemaRowset(OleDbSchemaGuid.Procedures))
            {
                throw ODB.NoProviderSupportForSProcResetParameters(connection.Provider);
            }
            object[] objArray2 = new object[4];
            objArray2[2] = command.CommandText;
            object[] restrictions = objArray2;
            if (connection.GetSchemaRowset(OleDbSchemaGuid.Procedures, restrictions).Rows.Count == 0)
            {
                throw ADP.NoStoredProcedureExists(command.CommandText);
            }
            throw ODB.NoProviderSupportForSProcResetParameters(connection.Provider);
        }

        public OleDbCommand GetDeleteCommand()
        {
            return (OleDbCommand) base.GetDeleteCommand();
        }

        public OleDbCommand GetDeleteCommand(bool useColumnsForParameterNames)
        {
            return (OleDbCommand) base.GetDeleteCommand(useColumnsForParameterNames);
        }

        public OleDbCommand GetInsertCommand()
        {
            return (OleDbCommand) base.GetInsertCommand();
        }

        public OleDbCommand GetInsertCommand(bool useColumnsForParameterNames)
        {
            return (OleDbCommand) base.GetInsertCommand(useColumnsForParameterNames);
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            return ("p" + parameterOrdinal.ToString(CultureInfo.InvariantCulture));
        }

        protected override string GetParameterName(string parameterName)
        {
            return parameterName;
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            return "?";
        }

        public OleDbCommand GetUpdateCommand()
        {
            return (OleDbCommand) base.GetUpdateCommand();
        }

        public OleDbCommand GetUpdateCommand(bool useColumnsForParameterNames)
        {
            return (OleDbCommand) base.GetUpdateCommand(useColumnsForParameterNames);
        }

        private void OleDbRowUpdatingHandler(object sender, OleDbRowUpdatingEventArgs ruevent)
        {
            base.RowUpdatingHandler(ruevent);
        }

        public override string QuoteIdentifier(string unquotedIdentifier)
        {
            return this.QuoteIdentifier(unquotedIdentifier, null);
        }

        public string QuoteIdentifier(string unquotedIdentifier, OleDbConnection connection)
        {
            ADP.CheckArgumentNull(unquotedIdentifier, "unquotedIdentifier");
            string quotePrefix = this.QuotePrefix;
            string quoteSuffix = this.QuoteSuffix;
            if (ADP.IsEmpty(quotePrefix))
            {
                if (connection == null)
                {
                    connection = base.GetConnection() as OleDbConnection;
                    if (connection == null)
                    {
                        throw ADP.QuotePrefixNotSet("QuoteIdentifier");
                    }
                }
                connection.GetLiteralQuotes("QuoteIdentifier", out quotePrefix, out quoteSuffix);
                if (quoteSuffix == null)
                {
                    quoteSuffix = quotePrefix;
                }
            }
            return ADP.BuildQuotedString(quotePrefix, quoteSuffix, unquotedIdentifier);
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (adapter == base.DataAdapter)
            {
                ((OleDbDataAdapter) adapter).RowUpdating -= new OleDbRowUpdatingEventHandler(this.OleDbRowUpdatingHandler);
            }
            else
            {
                ((OleDbDataAdapter) adapter).RowUpdating += new OleDbRowUpdatingEventHandler(this.OleDbRowUpdatingHandler);
            }
        }

        public override string UnquoteIdentifier(string quotedIdentifier)
        {
            return this.UnquoteIdentifier(quotedIdentifier, null);
        }

        public string UnquoteIdentifier(string quotedIdentifier, OleDbConnection connection)
        {
            string str3;
            ADP.CheckArgumentNull(quotedIdentifier, "quotedIdentifier");
            string quotePrefix = this.QuotePrefix;
            string quoteSuffix = this.QuoteSuffix;
            if (ADP.IsEmpty(quotePrefix))
            {
                if (connection == null)
                {
                    connection = base.GetConnection() as OleDbConnection;
                    if (connection == null)
                    {
                        throw ADP.QuotePrefixNotSet("UnquoteIdentifier");
                    }
                }
                connection.GetLiteralQuotes("UnquoteIdentifier", out quotePrefix, out quoteSuffix);
                if (quoteSuffix == null)
                {
                    quoteSuffix = quotePrefix;
                }
            }
            ADP.RemoveStringQuotes(quotePrefix, quoteSuffix, quotedIdentifier, out str3);
            return str3;
        }

        [DefaultValue((string) null), ResDescription("OleDbCommandBuilder_DataAdapter"), ResCategory("DataCategory_Update")]
        public OleDbDataAdapter DataAdapter
        {
            get
            {
                return (base.DataAdapter as OleDbDataAdapter);
            }
            set
            {
                base.DataAdapter = value;
            }
        }
    }
}

