namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    [Obsolete("OracleCommandBuilder has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260", false)]
    public sealed class OracleCommandBuilder : DbCommandBuilder
    {
        private const char _doubleQuoteChar = '"';
        private const string _doubleQuoteEscapeString = "\"\"";
        private const string _doubleQuoteString = "\"";
        private const char _singleQuoteChar = '\'';
        private const string _singleQuoteEscapeString = "''";
        private const string _singleQuoteString = "'";
        private static readonly string DeriveParameterCommand_Part1;
        private const string DeriveParameterCommand_Part2 = " and package_name";
        private const string DeriveParameterCommand_Part3 = " and object_name = ";
        private const string DeriveParameterCommand_Part4 = "  order by overload, position";
        private const string ResolveNameCommand_Part1 = "begin dbms_utility.name_resolve(";
        private const string ResolveNameCommand_Part2 = ",1,:schema,:part1,:part2,:dblink,:part1type,:objectnum); end;";

        static OracleCommandBuilder()
        {
            object[] objArray = new object[] { 
                "select overload, decode(position,0,'RETURN_VALUE',nvl(argument_name,chr(0))) name, decode(in_out,'IN',1,'IN/OUT',3,'OUT',decode(argument_name,null,6,2),1) direction, decode(data_type, 'BFILE',", 1.ToString(CultureInfo.CurrentCulture), ", 'BLOB',", 2.ToString(CultureInfo.CurrentCulture), ", 'CHAR',", 3.ToString(CultureInfo.CurrentCulture), ", 'CLOB',", 4.ToString(CultureInfo.CurrentCulture), ", 'DATE',", 6.ToString(CultureInfo.CurrentCulture), ", 'FLOAT',", 13.ToString(CultureInfo.CurrentCulture), ", 'INTERVAL YEAR TO MONTH',", 8.ToString(CultureInfo.CurrentCulture), ", 'INTERVAL DAY TO SECOND',", 7.ToString(CultureInfo.CurrentCulture), 
                ", 'LONG',", 10.ToString(CultureInfo.CurrentCulture), ", 'LONG RAW',", 9.ToString(CultureInfo.CurrentCulture), ", 'NCHAR',", 11.ToString(CultureInfo.CurrentCulture), ", 'NCLOB',", 12.ToString(CultureInfo.CurrentCulture), ", 'NUMBER',", 13.ToString(CultureInfo.CurrentCulture), ", 'NVARCHAR2',", 14.ToString(CultureInfo.CurrentCulture), ", 'RAW',", 15.ToString(CultureInfo.CurrentCulture), ", 'REF CURSOR',", 5.ToString(CultureInfo.CurrentCulture), 
                ", 'ROWID',", 0x10.ToString(CultureInfo.CurrentCulture), ", 'TIMESTAMP',", 0x12.ToString(CultureInfo.CurrentCulture), ", 'TIMESTAMP WITH LOCAL TIME ZONE',", 0x13.ToString(CultureInfo.CurrentCulture), ", 'TIMESTAMP WITH TIME ZONE',", 20.ToString(CultureInfo.CurrentCulture), ", 'VARCHAR2',", 0x16.ToString(CultureInfo.CurrentCulture), ",", 0x16.ToString(CultureInfo.CurrentCulture), ") oracletype, decode(data_type, 'CHAR',", 0x7d0, ", 'LONG',", 0x7fffffff, 
                ", 'LONG RAW',", 0x7fffffff, ", 'NCHAR',", 0xfa0, ", 'NVARCHAR2',", 0xfa0, ", 'RAW',", 0x7d0, ", 'VARCHAR2',", 0x7d0, ",0) length, nvl(data_precision, 255) precision, nvl(data_scale, 255) scale from all_arguments where data_level = 0 and data_type is not null and owner = "
             };
            DeriveParameterCommand_Part1 = string.Concat(objArray);
        }

        public OracleCommandBuilder()
        {
            GC.SuppressFinalize(this);
        }

        public OracleCommandBuilder(OracleDataAdapter adapter) : this()
        {
            this.DataAdapter = adapter;
        }

        protected override void ApplyParameterInfo(DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
        {
            OracleParameter parameter2 = (OracleParameter) parameter;
            object obj2 = datarow["ProviderType", DataRowVersion.Default];
            OracleType varChar = (OracleType) obj2;
            switch (varChar)
            {
                case OracleType.LongVarChar:
                {
                    varChar = OracleType.VarChar;
                }
            }
            parameter2.OracleType = varChar;
            parameter2.Offset = 0;
        }

        public static void DeriveParameters(OracleCommand command)
        {
            OracleConnection.ExecutePermission.Demand();
            if (command == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("command");
            }
            CommandType commandType = command.CommandType;
            if (commandType != CommandType.Text)
            {
                if (commandType == CommandType.StoredProcedure)
                {
                    if (System.Data.Common.ADP.IsEmpty(command.CommandText))
                    {
                        throw System.Data.Common.ADP.CommandTextRequired("DeriveParameters");
                    }
                    OracleConnection connection = command.Connection;
                    if (connection == null)
                    {
                        throw System.Data.Common.ADP.ConnectionRequired("DeriveParameters");
                    }
                    ConnectionState state = connection.State;
                    if (ConnectionState.Open != state)
                    {
                        throw System.Data.Common.ADP.OpenConnectionRequired("DeriveParameters", state);
                    }
                    ArrayList list = DeriveParametersFromStoredProcedure(connection, command);
                    OracleParameterCollection parameters = command.Parameters;
                    parameters.Clear();
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                    {
                        parameters.Add((OracleParameter) list[i]);
                    }
                    return;
                }
                if (commandType != CommandType.TableDirect)
                {
                    throw System.Data.Common.ADP.InvalidCommandType(command.CommandType);
                }
            }
            throw System.Data.Common.ADP.DeriveParametersNotSupported(command);
        }

        private static ArrayList DeriveParametersFromStoredProcedure(OracleConnection connection, OracleCommand command)
        {
            string str;
            string str3;
            string str4;
            string str5;
            ArrayList list = new ArrayList();
            OracleCommand command2 = connection.CreateCommand();
            command2.Transaction = command.Transaction;
            if (ResolveName(command2, command.CommandText, out str4, out str, out str3, out str5) != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(DeriveParameterCommand_Part1);
                builder.Append(QuoteIdentifier(str4, "'", "''"));
                builder.Append(" and package_name");
                if (!System.Data.Common.ADP.IsNull(str))
                {
                    builder.Append(" = ");
                    builder.Append(QuoteIdentifier(str, "'", "''"));
                }
                else
                {
                    builder.Append(" is null");
                }
                builder.Append(" and object_name = ");
                builder.Append(QuoteIdentifier(str3, "'", "''"));
                builder.Append("  order by overload, position");
                command2.Parameters.Clear();
                command2.CommandText = builder.ToString();
                using (OracleDataReader reader = command2.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        byte num;
                        if (!System.Data.Common.ADP.IsNull(reader.GetValue(0)))
                        {
                            throw System.Data.Common.ADP.CannotDeriveOverloaded();
                        }
                        string name = reader.GetString(1);
                        ParameterDirection @decimal = (ParameterDirection) ((int) reader.GetDecimal(2));
                        OracleType oracleType = (OracleType) ((int) reader.GetDecimal(3));
                        int size = (int) reader.GetDecimal(4);
                        byte precision = (byte) reader.GetDecimal(5);
                        int num2 = (int) reader.GetDecimal(6);
                        if (num2 < 0)
                        {
                            num = 0;
                        }
                        else
                        {
                            num = (byte) num2;
                        }
                        OracleParameter parameter = new OracleParameter(name, oracleType, size, @decimal, true, precision, num, "", DataRowVersion.Current, null);
                        list.Add(parameter);
                    }
                }
            }
            return list;
        }

        public OracleCommand GetDeleteCommand()
        {
            return (OracleCommand) base.GetDeleteCommand();
        }

        public OracleCommand GetDeleteCommand(bool useColumnsForParameterNames)
        {
            return (OracleCommand) base.GetDeleteCommand(useColumnsForParameterNames);
        }

        public OracleCommand GetInsertCommand()
        {
            return (OracleCommand) base.GetInsertCommand();
        }

        public OracleCommand GetInsertCommand(bool useColumnsForParameterNames)
        {
            return (OracleCommand) base.GetInsertCommand(useColumnsForParameterNames);
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            return ("p" + parameterOrdinal.ToString(CultureInfo.CurrentCulture));
        }

        protected override string GetParameterName(string parameterName)
        {
            return parameterName;
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            return (":" + this.GetParameterName(parameterOrdinal));
        }

        public OracleCommand GetUpdateCommand()
        {
            return (OracleCommand) base.GetUpdateCommand();
        }

        public OracleCommand GetUpdateCommand(bool useColumnsForParameterNames)
        {
            return (OracleCommand) base.GetUpdateCommand(useColumnsForParameterNames);
        }

        public override string QuoteIdentifier(string unquotedIdentifier)
        {
            return QuoteIdentifier(unquotedIdentifier, "\"", "\"\"");
        }

        private static string QuoteIdentifier(string unquotedIdentifier, string quoteString, string quoteEscapeString)
        {
            System.Data.Common.ADP.CheckArgumentNull(unquotedIdentifier, "unquotedIdentifier");
            StringBuilder builder = new StringBuilder();
            builder.Append(quoteString);
            builder.Append(unquotedIdentifier.Replace(quoteString, quoteEscapeString));
            builder.Append(quoteString);
            return builder.ToString();
        }

        private static uint ResolveName(OracleCommand command, string nameToResolve, out string schema, out string packageName, out string objectName, out string dblink)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("begin dbms_utility.name_resolve(");
            builder.Append(QuoteIdentifier(nameToResolve, "'", "''"));
            builder.Append(",1,:schema,:part1,:part2,:dblink,:part1type,:objectnum); end;");
            command.CommandText = builder.ToString();
            command.Parameters.Add(new OracleParameter("schema", OracleType.VarChar, 30)).Direction = ParameterDirection.Output;
            command.Parameters.Add(new OracleParameter("part1", OracleType.VarChar, 30)).Direction = ParameterDirection.Output;
            command.Parameters.Add(new OracleParameter("part2", OracleType.VarChar, 30)).Direction = ParameterDirection.Output;
            command.Parameters.Add(new OracleParameter("dblink", OracleType.VarChar, 0x80)).Direction = ParameterDirection.Output;
            command.Parameters.Add(new OracleParameter("part1type", OracleType.UInt32)).Direction = ParameterDirection.Output;
            command.Parameters.Add(new OracleParameter("objectnum", OracleType.UInt32)).Direction = ParameterDirection.Output;
            command.ExecuteNonQuery();
            if (System.Data.Common.ADP.IsNull(command.Parameters["objectnum"].Value))
            {
                schema = string.Empty;
                packageName = string.Empty;
                objectName = string.Empty;
                dblink = string.Empty;
                return 0;
            }
            schema = System.Data.Common.ADP.IsNull(command.Parameters["schema"].Value) ? null : ((string) command.Parameters["schema"].Value);
            packageName = System.Data.Common.ADP.IsNull(command.Parameters["part1"].Value) ? null : ((string) command.Parameters["part1"].Value);
            objectName = System.Data.Common.ADP.IsNull(command.Parameters["part2"].Value) ? null : ((string) command.Parameters["part2"].Value);
            dblink = System.Data.Common.ADP.IsNull(command.Parameters["dblink"].Value) ? null : ((string) command.Parameters["dblink"].Value);
            return (uint) command.Parameters["part1type"].Value;
        }

        private void RowUpdatingHandler(object sender, OracleRowUpdatingEventArgs ruevent)
        {
            base.RowUpdatingHandler(ruevent);
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (adapter == base.DataAdapter)
            {
                ((OracleDataAdapter) adapter).RowUpdating -= new OracleRowUpdatingEventHandler(this.RowUpdatingHandler);
            }
            else
            {
                ((OracleDataAdapter) adapter).RowUpdating += new OracleRowUpdatingEventHandler(this.RowUpdatingHandler);
            }
        }

        public override string UnquoteIdentifier(string quotedIdentifier)
        {
            System.Data.Common.ADP.CheckArgumentNull(quotedIdentifier, "quotedIdentifier");
            if (((quotedIdentifier.Length < 2) || (quotedIdentifier[0] != '"')) || (quotedIdentifier[quotedIdentifier.Length - 1] != '"'))
            {
                throw System.Data.Common.ADP.IdentifierIsNotQuoted();
            }
            return quotedIdentifier.Substring(1, quotedIdentifier.Length - 2).Replace("\"\"", "\"");
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override System.Data.Common.CatalogLocation CatalogLocation
        {
            get
            {
                return System.Data.Common.CatalogLocation.End;
            }
            set
            {
                if (System.Data.Common.CatalogLocation.End != value)
                {
                    throw System.Data.Common.ADP.NotSupported();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string CatalogSeparator
        {
            get
            {
                return "@";
            }
            set
            {
                if ("@" != value)
                {
                    throw System.Data.Common.ADP.NotSupported();
                }
            }
        }

        [System.Data.OracleClient.ResCategory("OracleCategory_Update"), System.Data.OracleClient.ResDescription("OracleCommandBuilder_DataAdapter"), DefaultValue((string) null)]
        public OracleDataAdapter DataAdapter
        {
            get
            {
                return (OracleDataAdapter) base.DataAdapter;
            }
            set
            {
                base.DataAdapter = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string SchemaSeparator
        {
            get
            {
                return ".";
            }
            set
            {
                if ("." != value)
                {
                    throw System.Data.Common.ADP.NotSupported();
                }
            }
        }
    }
}

