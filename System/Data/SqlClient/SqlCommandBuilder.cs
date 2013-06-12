namespace System.Data.SqlClient
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Sql;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed class SqlCommandBuilder : DbCommandBuilder
    {
        public SqlCommandBuilder()
        {
            GC.SuppressFinalize(this);
            base.QuotePrefix = "[";
            base.QuoteSuffix = "]";
        }

        public SqlCommandBuilder(SqlDataAdapter adapter) : this()
        {
            this.DataAdapter = adapter;
        }

        protected override void ApplyParameterInfo(DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
        {
            SqlParameter parameter2 = (SqlParameter) parameter;
            object obj3 = datarow[SchemaTableColumn.ProviderType];
            parameter2.SqlDbType = (SqlDbType) obj3;
            parameter2.Offset = 0;
            if ((parameter2.SqlDbType == SqlDbType.Udt) && !parameter2.SourceColumnNullMapping)
            {
                parameter2.UdtTypeName = datarow["DataTypeName"] as string;
            }
            else
            {
                parameter2.UdtTypeName = string.Empty;
            }
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

        private void ConsistentQuoteDelimiters(string quotePrefix, string quoteSuffix)
        {
            if ((("\"" == quotePrefix) && ("\"" != quoteSuffix)) || (("[" == quotePrefix) && ("]" != quoteSuffix)))
            {
                throw ADP.InvalidPrefixSuffix();
            }
        }

        public static void DeriveParameters(SqlCommand command)
        {
            SqlConnection.ExecutePermission.Demand();
            if (command == null)
            {
                throw ADP.ArgumentNull("command");
            }
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(command.Connection);
                command.DeriveParameters();
            }
            catch (OutOfMemoryException exception3)
            {
                if ((command != null) && (command.Connection != null))
                {
                    command.Connection.Abort(exception3);
                }
                throw;
            }
            catch (StackOverflowException exception2)
            {
                if ((command != null) && (command.Connection != null))
                {
                    command.Connection.Abort(exception2);
                }
                throw;
            }
            catch (ThreadAbortException exception)
            {
                if ((command != null) && (command.Connection != null))
                {
                    command.Connection.Abort(exception);
                }
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
        }

        public SqlCommand GetDeleteCommand()
        {
            return (SqlCommand) base.GetDeleteCommand();
        }

        public SqlCommand GetDeleteCommand(bool useColumnsForParameterNames)
        {
            return (SqlCommand) base.GetDeleteCommand(useColumnsForParameterNames);
        }

        public SqlCommand GetInsertCommand()
        {
            return (SqlCommand) base.GetInsertCommand();
        }

        public SqlCommand GetInsertCommand(bool useColumnsForParameterNames)
        {
            return (SqlCommand) base.GetInsertCommand(useColumnsForParameterNames);
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            return ("@p" + parameterOrdinal.ToString(CultureInfo.InvariantCulture));
        }

        protected override string GetParameterName(string parameterName)
        {
            return ("@" + parameterName);
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            return ("@p" + parameterOrdinal.ToString(CultureInfo.InvariantCulture));
        }

        protected override DataTable GetSchemaTable(DbCommand srcCommand)
        {
            DataTable table;
            SqlCommand command = srcCommand as SqlCommand;
            SqlNotificationRequest notification = command.Notification;
            bool notificationAutoEnlist = command.NotificationAutoEnlist;
            command.Notification = null;
            command.NotificationAutoEnlist = false;
            try
            {
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly))
                {
                    return reader.GetSchemaTable();
                }
            }
            finally
            {
                command.Notification = notification;
                command.NotificationAutoEnlist = notificationAutoEnlist;
            }
            return table;
        }

        public SqlCommand GetUpdateCommand()
        {
            return (SqlCommand) base.GetUpdateCommand();
        }

        public SqlCommand GetUpdateCommand(bool useColumnsForParameterNames)
        {
            return (SqlCommand) base.GetUpdateCommand(useColumnsForParameterNames);
        }

        protected override DbCommand InitializeCommand(DbCommand command)
        {
            SqlCommand command2 = (SqlCommand) base.InitializeCommand(command);
            command2.NotificationAutoEnlist = false;
            return command2;
        }

        public override string QuoteIdentifier(string unquotedIdentifier)
        {
            ADP.CheckArgumentNull(unquotedIdentifier, "unquotedIdentifier");
            string quoteSuffix = this.QuoteSuffix;
            string quotePrefix = this.QuotePrefix;
            this.ConsistentQuoteDelimiters(quotePrefix, quoteSuffix);
            return ADP.BuildQuotedString(quotePrefix, quoteSuffix, unquotedIdentifier);
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (adapter == base.DataAdapter)
            {
                ((SqlDataAdapter) adapter).RowUpdating -= new SqlRowUpdatingEventHandler(this.SqlRowUpdatingHandler);
            }
            else
            {
                ((SqlDataAdapter) adapter).RowUpdating += new SqlRowUpdatingEventHandler(this.SqlRowUpdatingHandler);
            }
        }

        private void SqlRowUpdatingHandler(object sender, SqlRowUpdatingEventArgs ruevent)
        {
            base.RowUpdatingHandler(ruevent);
        }

        public override string UnquoteIdentifier(string quotedIdentifier)
        {
            string str3;
            ADP.CheckArgumentNull(quotedIdentifier, "quotedIdentifier");
            string quoteSuffix = this.QuoteSuffix;
            string quotePrefix = this.QuotePrefix;
            this.ConsistentQuoteDelimiters(quotePrefix, quoteSuffix);
            ADP.RemoveStringQuotes(quotePrefix, quoteSuffix, quotedIdentifier, out str3);
            return str3;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Data.Common.CatalogLocation CatalogLocation
        {
            get
            {
                return System.Data.Common.CatalogLocation.Start;
            }
            set
            {
                if (System.Data.Common.CatalogLocation.Start != value)
                {
                    throw ADP.SingleValuedProperty("CatalogLocation", "Start");
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override string CatalogSeparator
        {
            get
            {
                return ".";
            }
            set
            {
                if ("." != value)
                {
                    throw ADP.SingleValuedProperty("CatalogSeparator", ".");
                }
            }
        }

        [ResDescription("SqlCommandBuilder_DataAdapter"), ResCategory("DataCategory_Update"), DefaultValue((string) null)]
        public SqlDataAdapter DataAdapter
        {
            get
            {
                return (SqlDataAdapter) base.DataAdapter;
            }
            set
            {
                base.DataAdapter = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string QuotePrefix
        {
            get
            {
                return base.QuotePrefix;
            }
            set
            {
                if (("[" != value) && ("\"" != value))
                {
                    throw ADP.DoubleValuedProperty("QuotePrefix", "[", "\"");
                }
                base.QuotePrefix = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string QuoteSuffix
        {
            get
            {
                return base.QuoteSuffix;
            }
            set
            {
                if (("]" != value) && ("\"" != value))
                {
                    throw ADP.DoubleValuedProperty("QuoteSuffix", "]", "\"");
                }
                base.QuoteSuffix = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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
                    throw ADP.SingleValuedProperty("SchemaSeparator", ".");
                }
            }
        }
    }
}

