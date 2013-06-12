namespace System.Data.Odbc
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;

    public sealed class OdbcCommandBuilder : DbCommandBuilder
    {
        public OdbcCommandBuilder()
        {
            GC.SuppressFinalize(this);
        }

        public OdbcCommandBuilder(OdbcDataAdapter adapter) : this()
        {
            this.DataAdapter = adapter;
        }

        protected override void ApplyParameterInfo(DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
        {
            OdbcParameter parameter2 = (OdbcParameter) parameter;
            object obj3 = datarow[SchemaTableColumn.ProviderType];
            parameter2.OdbcType = (OdbcType) obj3;
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

        public static void DeriveParameters(OdbcCommand command)
        {
            OdbcConnection.ExecutePermission.Demand();
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
            OdbcConnection connection = command.Connection;
            if (connection == null)
            {
                throw ADP.ConnectionRequired("DeriveParameters");
            }
            ConnectionState state = connection.State;
            if (ConnectionState.Open != state)
            {
                throw ADP.OpenConnectionRequired("DeriveParameters", state);
            }
            OdbcParameter[] parameterArray = DeriveParametersFromStoredProcedure(connection, command);
            OdbcParameterCollection parameters = command.Parameters;
            parameters.Clear();
            int length = parameterArray.Length;
            if (0 < length)
            {
                for (int i = 0; i < parameterArray.Length; i++)
                {
                    parameters.Add(parameterArray[i]);
                }
            }
        }

        private static OdbcParameter[] DeriveParametersFromStoredProcedure(OdbcConnection connection, OdbcCommand command)
        {
            List<OdbcParameter> list = new List<OdbcParameter>();
            CMDWrapper statementHandle = command.GetStatementHandle();
            OdbcStatementHandle hrHandle = statementHandle.StatementHandle;
            string leftQuote = connection.QuoteChar("DeriveParameters");
            string[] strArray = MultipartIdentifier.ParseMultipartIdentifier(command.CommandText, leftQuote, leftQuote, '.', 4, true, "ODBC_ODBCCommandText", false);
            if (strArray[3] == null)
            {
                strArray[3] = command.CommandText;
            }
            ODBC32.RetCode retcode = hrHandle.ProcedureColumns(strArray[1], strArray[2], strArray[3], null);
            if (retcode != ODBC32.RetCode.SUCCESS)
            {
                connection.HandleError(hrHandle, retcode);
            }
            using (OdbcDataReader reader = new OdbcDataReader(command, statementHandle, CommandBehavior.Default))
            {
                reader.FirstResult();
                int fieldCount = reader.FieldCount;
                while (reader.Read())
                {
                    OdbcParameter item = new OdbcParameter {
                        ParameterName = reader.GetString(3)
                    };
                    switch (reader.GetInt16(4))
                    {
                        case 1:
                            item.Direction = ParameterDirection.Input;
                            break;

                        case 2:
                            item.Direction = ParameterDirection.InputOutput;
                            break;

                        case 4:
                            item.Direction = ParameterDirection.Output;
                            break;

                        case 5:
                            item.Direction = ParameterDirection.ReturnValue;
                            break;
                    }
                    item.OdbcType = TypeMap.FromSqlType((ODBC32.SQL_TYPE) reader.GetInt16(5))._odbcType;
                    item.Size = reader.GetInt32(7);
                    switch (item.OdbcType)
                    {
                        case OdbcType.Decimal:
                        case OdbcType.Numeric:
                            item.ScaleInternal = (byte) reader.GetInt16(9);
                            item.PrecisionInternal = (byte) reader.GetInt16(10);
                            break;
                    }
                    list.Add(item);
                }
            }
            retcode = hrHandle.CloseCursor();
            return list.ToArray();
        }

        public OdbcCommand GetDeleteCommand()
        {
            return (OdbcCommand) base.GetDeleteCommand();
        }

        public OdbcCommand GetDeleteCommand(bool useColumnsForParameterNames)
        {
            return (OdbcCommand) base.GetDeleteCommand(useColumnsForParameterNames);
        }

        public OdbcCommand GetInsertCommand()
        {
            return (OdbcCommand) base.GetInsertCommand();
        }

        public OdbcCommand GetInsertCommand(bool useColumnsForParameterNames)
        {
            return (OdbcCommand) base.GetInsertCommand(useColumnsForParameterNames);
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

        public OdbcCommand GetUpdateCommand()
        {
            return (OdbcCommand) base.GetUpdateCommand();
        }

        public OdbcCommand GetUpdateCommand(bool useColumnsForParameterNames)
        {
            return (OdbcCommand) base.GetUpdateCommand(useColumnsForParameterNames);
        }

        private void OdbcRowUpdatingHandler(object sender, OdbcRowUpdatingEventArgs ruevent)
        {
            base.RowUpdatingHandler(ruevent);
        }

        public override string QuoteIdentifier(string unquotedIdentifier)
        {
            return this.QuoteIdentifier(unquotedIdentifier, null);
        }

        public string QuoteIdentifier(string unquotedIdentifier, OdbcConnection connection)
        {
            ADP.CheckArgumentNull(unquotedIdentifier, "unquotedIdentifier");
            string quotePrefix = this.QuotePrefix;
            string quoteSuffix = this.QuoteSuffix;
            if (ADP.IsEmpty(quotePrefix))
            {
                if (connection == null)
                {
                    connection = base.GetConnection() as OdbcConnection;
                    if (connection == null)
                    {
                        throw ADP.QuotePrefixNotSet("QuoteIdentifier");
                    }
                }
                quotePrefix = connection.QuoteChar("QuoteIdentifier");
                quoteSuffix = quotePrefix;
            }
            if (!ADP.IsEmpty(quotePrefix) && (quotePrefix != " "))
            {
                return ADP.BuildQuotedString(quotePrefix, quoteSuffix, unquotedIdentifier);
            }
            return unquotedIdentifier;
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (adapter == base.DataAdapter)
            {
                ((OdbcDataAdapter) adapter).RowUpdating -= new OdbcRowUpdatingEventHandler(this.OdbcRowUpdatingHandler);
            }
            else
            {
                ((OdbcDataAdapter) adapter).RowUpdating += new OdbcRowUpdatingEventHandler(this.OdbcRowUpdatingHandler);
            }
        }

        public override string UnquoteIdentifier(string quotedIdentifier)
        {
            return this.UnquoteIdentifier(quotedIdentifier, null);
        }

        public string UnquoteIdentifier(string quotedIdentifier, OdbcConnection connection)
        {
            ADP.CheckArgumentNull(quotedIdentifier, "quotedIdentifier");
            string quotePrefix = this.QuotePrefix;
            string quoteSuffix = this.QuoteSuffix;
            if (ADP.IsEmpty(quotePrefix))
            {
                if (connection == null)
                {
                    connection = base.GetConnection() as OdbcConnection;
                    if (connection == null)
                    {
                        throw ADP.QuotePrefixNotSet("UnquoteIdentifier");
                    }
                }
                quotePrefix = connection.QuoteChar("UnquoteIdentifier");
                quoteSuffix = quotePrefix;
            }
            if (!ADP.IsEmpty(quotePrefix) || (quotePrefix != " "))
            {
                string str2;
                ADP.RemoveStringQuotes(quotePrefix, quoteSuffix, quotedIdentifier, out str2);
                return str2;
            }
            return quotedIdentifier;
        }

        [DefaultValue((string) null), ResCategory("DataCategory_Update"), ResDescription("OdbcCommandBuilder_DataAdapter")]
        public OdbcDataAdapter DataAdapter
        {
            get
            {
                return (base.DataAdapter as OdbcDataAdapter);
            }
            set
            {
                base.DataAdapter = value;
            }
        }
    }
}

