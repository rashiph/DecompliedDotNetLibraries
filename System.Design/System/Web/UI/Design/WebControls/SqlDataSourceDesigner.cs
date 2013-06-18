namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Data;
    using System.Data;
    using System.Data.Common;
    using System.Data.Odbc;
    using System.Data.OleDb;
    using System.Data.OracleClient;
    using System.Data.SqlClient;
    using System.Design;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SqlDataSourceDesigner : DataSourceDesigner
    {
        private bool _forceSchemaRetrieval;
        private static readonly string[] _hiddenProperties = new string[] { "DeleteCommand", "DeleteParameters", "InsertCommand", "InsertParameters", "SelectParameters", "UpdateCommand", "UpdateParameters" };
        private DesignerDataSourceView _view;
        internal const string AspNetDatabaseObjectPrefix = "AspNet_";
        internal const string DefaultProviderName = "System.Data.SqlClient";
        internal const string DefaultViewName = "DefaultView";
        private const string DesignerStateDataSourceSchemaConnectionStringHashKey = "DataSourceSchemaConnectionStringHash";
        private const string DesignerStateDataSourceSchemaKey = "DataSourceSchema";
        private const string DesignerStateDataSourceSchemaProviderNameKey = "DataSourceSchemaProviderName";
        private const string DesignerStateDataSourceSchemaSelectCommandKey = "DataSourceSchemaSelectMethod";
        private const string DesignerStateSaveConfiguredConnectionStateKey = "SaveConfiguredConnectionState";
        private const string DesignerStateTableQueryStateKey = "TableQueryState";

        internal DbCommand BuildSelectCommand(DbProviderFactory factory, DbConnection connection, string commandText, ParameterCollection parameters, SqlDataSourceCommandType commandType)
        {
            DbCommand command = CreateCommand(factory, commandText, connection);
            if ((parameters != null) && (parameters.Count > 0))
            {
                IOrderedDictionary values = parameters.GetValues(null, null);
                string parameterPrefix = GetParameterPrefix(factory);
                for (int i = 0; i < parameters.Count; i++)
                {
                    Parameter parameter = parameters[i];
                    DbParameter parameter2 = CreateParameter(factory);
                    parameter2.ParameterName = parameterPrefix + parameter.Name;
                    if (parameter.DbType != DbType.Object)
                    {
                        SqlParameter parameter3 = parameter2 as SqlParameter;
                        if (parameter3 == null)
                        {
                            parameter2.DbType = parameter.DbType;
                        }
                        else if (parameter.DbType == DbType.Date)
                        {
                            parameter3.SqlDbType = SqlDbType.Date;
                        }
                        else if (parameter.DbType == DbType.Time)
                        {
                            parameter3.SqlDbType = SqlDbType.Time;
                        }
                        else
                        {
                            parameter2.DbType = parameter.DbType;
                        }
                    }
                    else
                    {
                        if ((parameter.Type != TypeCode.Empty) && (parameter.Type != TypeCode.DBNull))
                        {
                            parameter2.DbType = parameter.GetDatabaseType();
                        }
                        if ((parameter.Type == TypeCode.Empty) && ProviderRequiresDbTypeSet(factory))
                        {
                            parameter2.DbType = DbType.Object;
                        }
                    }
                    parameter2.Value = values[i];
                    if (parameter2.Value == null)
                    {
                        parameter2.Value = DBNull.Value;
                    }
                    if (Parameter.ConvertDbTypeToTypeCode(parameter2.DbType) == TypeCode.String)
                    {
                        if ((parameter2.Value is string) && (parameter2.Value != null))
                        {
                            parameter2.Size = ((string) parameter2.Value).Length;
                        }
                        else
                        {
                            parameter2.Size = 1;
                        }
                    }
                    command.Parameters.Add(parameter2);
                }
            }
            command.CommandType = GetCommandType(commandType);
            return command;
        }

        public override void Configure()
        {
            try
            {
                this.SuppressDataSourceEvents();
                ControlDesigner.InvokeTransactedChange(base.Component, new TransactedChangeCallback(this.ConfigureDataSourceChangeCallback), null, System.Design.SR.GetString("DataSource_ConfigureTransactionDescription"));
            }
            finally
            {
                this.ResumeDataSourceEvents();
            }
        }

        private bool ConfigureDataSourceChangeCallback(object context)
        {
            IServiceProvider site = base.Component.Site;
            IDataEnvironment service = (IDataEnvironment) site.GetService(typeof(IDataEnvironment));
            if (service == null)
            {
                return false;
            }
            IDataSourceViewSchema schema = this.GetView("DefaultView").Schema;
            bool flag = false;
            if (schema == null)
            {
                this._forceSchemaRetrieval = true;
                schema = this.GetView("DefaultView").Schema;
                this._forceSchemaRetrieval = false;
                if (schema != null)
                {
                    flag = true;
                }
            }
            SqlDataSourceWizardForm form = this.CreateConfigureDataSourceWizardForm(site, service);
            if (UIServiceHelper.ShowDialog(site, form) != DialogResult.OK)
            {
                return false;
            }
            this.OnComponentChanged(this, new ComponentChangedEventArgs(base.Component, null, null, null));
            IDataSourceViewSchema schema2 = null;
            try
            {
                this._forceSchemaRetrieval = true;
                schema2 = this.GetView("DefaultView").Schema;
            }
            finally
            {
                this._forceSchemaRetrieval = false;
            }
            if (!flag && !DataSourceDesigner.ViewSchemasEquivalent(schema, schema2))
            {
                this.OnSchemaRefreshed(EventArgs.Empty);
            }
            this.OnDataSourceChanged(EventArgs.Empty);
            return true;
        }

        internal static bool ConnectionsEqual(DesignerDataConnection connection1, DesignerDataConnection connection2)
        {
            if ((connection1 == null) || (connection2 == null))
            {
                return false;
            }
            if (connection1.ConnectionString != connection2.ConnectionString)
            {
                return false;
            }
            string str = (connection1.ProviderName.Trim().Length == 0) ? "System.Data.SqlClient" : connection1.ProviderName;
            string str2 = (connection2.ProviderName.Trim().Length == 0) ? "System.Data.SqlClient" : connection2.ProviderName;
            return (str == str2);
        }

        internal static TypeCode ConvertDbTypeToTypeCode(DbType dbType)
        {
            return Parameter.ConvertDbTypeToTypeCode(dbType);
        }

        internal static DbType ConvertTypeCodeToDbType(TypeCode typeCode)
        {
            return Parameter.ConvertTypeCodeToDbType(typeCode);
        }

        internal void CopyList(ICollection source, IList dest)
        {
            dest.Clear();
            foreach (ICloneable cloneable in source)
            {
                object clone = cloneable.Clone();
                base.RegisterClone(cloneable, clone);
                dest.Add(clone);
            }
        }

        internal static DbCommand CreateCommand(DbProviderFactory factory, string commandText, DbConnection connection)
        {
            DbCommand command = factory.CreateCommand();
            command.CommandText = commandText;
            command.Connection = connection;
            return command;
        }

        internal virtual SqlDataSourceWizardForm CreateConfigureDataSourceWizardForm(IServiceProvider serviceProvider, IDataEnvironment dataEnvironment)
        {
            return new SqlDataSourceWizardForm(serviceProvider, this, dataEnvironment);
        }

        internal static DbDataAdapter CreateDataAdapter(DbProviderFactory factory, DbCommand command)
        {
            DbDataAdapter adapter = factory.CreateDataAdapter();
            ((IDbDataAdapter) adapter).SelectCommand = command;
            return adapter;
        }

        internal static DbParameter CreateParameter(DbProviderFactory factory)
        {
            return factory.CreateParameter();
        }

        internal static Parameter CreateParameter(DbProviderFactory factory, string name, DbType dbType)
        {
            if (IsNewSqlServer2008Type(factory, dbType))
            {
                return new Parameter(name, dbType);
            }
            return new Parameter(name, ConvertDbTypeToTypeCode(dbType));
        }

        protected virtual SqlDesignerDataSourceView CreateView(string viewName)
        {
            return new SqlDesignerDataSourceView(this, viewName);
        }

        protected virtual void DeriveParameters(string providerName, DbCommand command)
        {
            if (string.Equals(providerName, "System.Data.Odbc", StringComparison.OrdinalIgnoreCase))
            {
                OdbcCommandBuilder.DeriveParameters((OdbcCommand) command);
            }
            else if (string.Equals(providerName, "System.Data.OleDb", StringComparison.OrdinalIgnoreCase))
            {
                OleDbCommandBuilder.DeriveParameters((OleDbCommand) command);
            }
            else if (string.Equals(providerName, "System.Data.SqlClient", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(providerName))
            {
                SqlCommandBuilder.DeriveParameters((SqlCommand) command);
            }
            else
            {
                UIServiceHelper.ShowError(this.SqlDataSource.Site, System.Design.SR.GetString("SqlDataSourceDesigner_InferStoredProcedureNotSupported", new object[] { providerName }));
            }
        }

        private static CommandType GetCommandType(SqlDataSourceCommandType commandType)
        {
            if (commandType == SqlDataSourceCommandType.Text)
            {
                return CommandType.Text;
            }
            return CommandType.StoredProcedure;
        }

        protected virtual string GetConnectionString()
        {
            return this.SqlDataSource.ConnectionString;
        }

        internal static DbProviderFactory GetDbProviderFactory(string providerName)
        {
            if (providerName.Length == 0)
            {
                providerName = "System.Data.SqlClient";
            }
            return DbProviderFactories.GetFactory(providerName);
        }

        internal static DbConnection GetDesignTimeConnection(IServiceProvider serviceProvider, DesignerDataConnection connection)
        {
            if (serviceProvider != null)
            {
                IDataEnvironment service = (IDataEnvironment) serviceProvider.GetService(typeof(IDataEnvironment));
                if (service != null)
                {
                    if (string.IsNullOrEmpty(connection.ProviderName))
                    {
                        connection = new DesignerDataConnection(connection.Name, "System.Data.SqlClient", connection.ConnectionString);
                    }
                    return service.GetDesignTimeConnection(connection);
                }
            }
            return null;
        }

        internal static string GetParameterPlaceholderPrefix(DbProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if ((factory == SqlClientFactory.Instance) || IsSqlCeClientFactory(factory))
            {
                return "@";
            }
            if (factory == OracleClientFactory.Instance)
            {
                return ":";
            }
            return "?";
        }

        internal static string GetParameterPrefix(DbProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if ((factory != SqlClientFactory.Instance) && !IsSqlCeClientFactory(factory))
            {
                return string.Empty;
            }
            return "@";
        }

        private static string[] GetParameterPrefixes()
        {
            return new string[] { "@", "?", ":" };
        }

        public override DesignerDataSourceView GetView(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = "DefaultView";
            }
            if (!string.Equals(viewName, "DefaultView", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (this._view == null)
            {
                this._view = this.CreateView(viewName);
            }
            return this._view;
        }

        public override string[] GetViewNames()
        {
            return new string[] { "DefaultView" };
        }

        protected internal virtual Parameter[] InferParameterNames(DesignerDataConnection connection, string commandText, SqlDataSourceCommandType commandType)
        {
            Parameter[] parameterArray2;
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (commandText.Length == 0)
                {
                    UIServiceHelper.ShowError(this.SqlDataSource.Site, System.Design.SR.GetString("SqlDataSourceDesigner_NoCommand"));
                    return null;
                }
                if (commandType == SqlDataSourceCommandType.Text)
                {
                    return SqlDataSourceParameterParser.ParseCommandText(connection.ProviderName, commandText);
                }
                DbProviderFactory dbProviderFactory = GetDbProviderFactory(connection.ProviderName);
                DbConnection designTimeConnection = null;
                try
                {
                    designTimeConnection = GetDesignTimeConnection(base.Component.Site, connection);
                }
                catch (Exception exception)
                {
                    if (designTimeConnection == null)
                    {
                        UIServiceHelper.ShowError(this.SqlDataSource.Site, exception, System.Design.SR.GetString("SqlDataSourceDesigner_CouldNotCreateConnection"));
                        return null;
                    }
                }
                if (designTimeConnection == null)
                {
                    UIServiceHelper.ShowError(this.SqlDataSource.Site, System.Design.SR.GetString("SqlDataSourceDesigner_CouldNotCreateConnection"));
                    return null;
                }
                DbCommand command = this.BuildSelectCommand(dbProviderFactory, designTimeConnection, commandText, null, commandType);
                command.CommandType = CommandType.StoredProcedure;
                try
                {
                    this.DeriveParameters(connection.ProviderName, command);
                }
                catch (Exception exception2)
                {
                    UIServiceHelper.ShowError(this.SqlDataSource.Site, System.Design.SR.GetString("SqlDataSourceDesigner_InferStoredProcedureError", new object[] { exception2.Message }));
                    return null;
                }
                finally
                {
                    if (command.Connection.State == ConnectionState.Open)
                    {
                        designTimeConnection.Close();
                    }
                }
                int count = command.Parameters.Count;
                Parameter[] parameterArray = new Parameter[count];
                for (int i = 0; i < count; i++)
                {
                    IDataParameter parameter = command.Parameters[i];
                    if (parameter != null)
                    {
                        string name = StripParameterPrefix(parameter.ParameterName);
                        parameterArray[i] = CreateParameter(dbProviderFactory, name, parameter.DbType);
                        parameterArray[i].Direction = parameter.Direction;
                    }
                }
                parameterArray2 = parameterArray;
            }
            finally
            {
                Cursor.Current = current;
            }
            return parameterArray2;
        }

        internal static bool IsNewSqlServer2008Type(DbProviderFactory factory, DbType type)
        {
            if (!(factory is SqlClientFactory))
            {
                return false;
            }
            if (((type != DbType.Date) && (type != DbType.DateTime2)) && (type != DbType.DateTimeOffset))
            {
                return (type == DbType.Time);
            }
            return true;
        }

        private static bool IsSqlCeClientFactory(DbProviderFactory factory)
        {
            return (factory.GetType().FullName == "System.Data.SqlServerCe.SqlCeProviderFactory");
        }

        internal DataTable LoadSchema()
        {
            if (!this._forceSchemaRetrieval)
            {
                object obj2 = base.DesignerState["DataSourceSchemaConnectionStringHash"];
                string str = base.DesignerState["DataSourceSchemaProviderName"] as string;
                string a = base.DesignerState["DataSourceSchemaSelectMethod"] as string;
                if (string.IsNullOrEmpty(str))
                {
                    str = "System.Data.SqlClient";
                }
                if (string.IsNullOrEmpty(this.ConnectionString))
                {
                    return null;
                }
                DesignerDataConnection connection = new DesignerDataConnection(string.Empty, this.ProviderName, this.ConnectionString);
                int hashCode = connection.ConnectionString.GetHashCode();
                string providerName = connection.ProviderName;
                string selectCommand = this.SelectCommand;
                if (string.IsNullOrEmpty(providerName))
                {
                    providerName = "System.Data.SqlClient";
                }
                if (((obj2 == null) || (((int) obj2) != hashCode)) || (!string.Equals(str, providerName, StringComparison.OrdinalIgnoreCase) || !string.Equals(a, selectCommand, StringComparison.Ordinal)))
                {
                    return null;
                }
            }
            DataTable table = base.DesignerState["DataSourceSchema"] as DataTable;
            if (table != null)
            {
                table.TableName = "DefaultView";
                return table;
            }
            return null;
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            PropertyDescriptor descriptor;
            base.PreFilterProperties(properties);
            foreach (string str in _hiddenProperties)
            {
                descriptor = (PropertyDescriptor) properties[str];
                if (descriptor != null)
                {
                    properties[str] = TypeDescriptor.CreateProperty(descriptor.ComponentType, descriptor, new Attribute[] { BrowsableAttribute.No });
                }
            }
            properties["DeleteQuery"] = TypeDescriptor.CreateProperty(base.GetType(), "DeleteQuery", typeof(DataSourceOperation), new Attribute[0]);
            properties["InsertQuery"] = TypeDescriptor.CreateProperty(base.GetType(), "InsertQuery", typeof(DataSourceOperation), new Attribute[0]);
            properties["SelectQuery"] = TypeDescriptor.CreateProperty(base.GetType(), "SelectQuery", typeof(DataSourceOperation), new Attribute[0]);
            properties["UpdateQuery"] = TypeDescriptor.CreateProperty(base.GetType(), "UpdateQuery", typeof(DataSourceOperation), new Attribute[0]);
            descriptor = (PropertyDescriptor) properties["ConnectionString"];
            properties["ConnectionString"] = TypeDescriptor.CreateProperty(base.GetType(), descriptor, new Attribute[0]);
            descriptor = (PropertyDescriptor) properties["ProviderName"];
            properties["ProviderName"] = TypeDescriptor.CreateProperty(base.GetType(), descriptor, new Attribute[0]);
            descriptor = (PropertyDescriptor) properties["SelectCommand"];
            properties["SelectCommand"] = TypeDescriptor.CreateProperty(base.GetType(), descriptor, new Attribute[] { BrowsableAttribute.No });
        }

        private static bool ProviderRequiresDbTypeSet(DbProviderFactory factory)
        {
            if (factory != OleDbFactory.Instance)
            {
                return (factory == OdbcFactory.Instance);
            }
            return true;
        }

        public override void RefreshSchema(bool preferSilent)
        {
            try
            {
                this.SuppressDataSourceEvents();
                bool flag = false;
                IServiceProvider site = this.SqlDataSource.Site;
                if (!this.CanRefreshSchema)
                {
                    if (!preferSilent)
                    {
                        UIServiceHelper.ShowError(site, System.Design.SR.GetString("SqlDataSourceDesigner_RefreshSchemaRequiresSettings"));
                    }
                }
                else
                {
                    IDataSourceViewSchema schema = this.GetView("DefaultView").Schema;
                    bool flag2 = false;
                    if (schema == null)
                    {
                        this._forceSchemaRetrieval = true;
                        schema = this.GetView("DefaultView").Schema;
                        this._forceSchemaRetrieval = false;
                        flag2 = true;
                    }
                    DesignerDataConnection connection = new DesignerDataConnection(string.Empty, this.ProviderName, this.ConnectionString);
                    if (preferSilent)
                    {
                        flag = this.RefreshSchema(connection, this.SelectCommand, this.SqlDataSource.SelectCommandType, this.SqlDataSource.SelectParameters, true);
                    }
                    else
                    {
                        Parameter[] parameterArray = this.InferParameterNames(connection, this.SelectCommand, this.SqlDataSource.SelectCommandType);
                        if (parameterArray == null)
                        {
                            return;
                        }
                        ParameterCollection parameters = new ParameterCollection();
                        ParameterCollection parameters2 = new ParameterCollection();
                        foreach (ICloneable cloneable in this.SqlDataSource.SelectParameters)
                        {
                            parameters2.Add((Parameter) cloneable.Clone());
                        }
                        foreach (Parameter parameter in parameterArray)
                        {
                            if ((parameter.Direction == ParameterDirection.Input) || (parameter.Direction == ParameterDirection.InputOutput))
                            {
                                Parameter parameter2 = parameters2[parameter.Name];
                                if (parameter2 != null)
                                {
                                    parameter.DefaultValue = parameter2.DefaultValue;
                                    if ((parameter.DbType == DbType.Object) && (parameter.Type == TypeCode.Empty))
                                    {
                                        parameter.DbType = parameter2.DbType;
                                        parameter.Type = parameter2.Type;
                                    }
                                    parameters2.Remove(parameter2);
                                }
                                parameters.Add(parameter);
                            }
                        }
                        if (parameters.Count > 0)
                        {
                            SqlDataSourceRefreshSchemaForm form = new SqlDataSourceRefreshSchemaForm(site, this, parameters);
                            flag = UIServiceHelper.ShowDialog(site, form) == DialogResult.OK;
                        }
                        else
                        {
                            flag = this.RefreshSchema(connection, this.SelectCommand, this.SqlDataSource.SelectCommandType, parameters, false);
                        }
                    }
                    if (flag)
                    {
                        IDataSourceViewSchema schema2 = this.GetView("DefaultView").Schema;
                        if (flag2 && DataSourceDesigner.ViewSchemasEquivalent(schema, schema2))
                        {
                            this.OnDataSourceChanged(EventArgs.Empty);
                        }
                        else if (!DataSourceDesigner.ViewSchemasEquivalent(schema, schema2))
                        {
                            this.OnSchemaRefreshed(EventArgs.Empty);
                        }
                    }
                }
            }
            finally
            {
                this.ResumeDataSourceEvents();
            }
        }

        internal bool RefreshSchema(DesignerDataConnection connection, string commandText, SqlDataSourceCommandType commandType, ParameterCollection parameters, bool preferSilent)
        {
            IServiceProvider site = this.SqlDataSource.Site;
            DbCommand command = null;
            try
            {
                DbProviderFactory dbProviderFactory = GetDbProviderFactory(connection.ProviderName);
                DbConnection designTimeConnection = GetDesignTimeConnection(base.Component.Site, connection);
                if (designTimeConnection == null)
                {
                    if (!preferSilent)
                    {
                        UIServiceHelper.ShowError(this.SqlDataSource.Site, System.Design.SR.GetString("SqlDataSourceDesigner_CouldNotCreateConnection"));
                    }
                    return false;
                }
                command = this.BuildSelectCommand(dbProviderFactory, designTimeConnection, commandText, parameters, commandType);
                DbDataAdapter adapter = CreateDataAdapter(dbProviderFactory, command);
                adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                DataSet dataSet = new DataSet();
                adapter.FillSchema(dataSet, SchemaType.Source, "DefaultView");
                DataTable schemaTable = dataSet.Tables["DefaultView"];
                if (schemaTable == null)
                {
                    if (!preferSilent)
                    {
                        UIServiceHelper.ShowError(site, System.Design.SR.GetString("SqlDataSourceDesigner_CannotGetSchema"));
                    }
                    return false;
                }
                this.SaveSchema(connection, commandText, schemaTable);
                return true;
            }
            catch (Exception exception)
            {
                if (!preferSilent)
                {
                    UIServiceHelper.ShowError(site, exception, System.Design.SR.GetString("SqlDataSourceDesigner_CannotGetSchema"));
                }
            }
            finally
            {
                if ((command != null) && (command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Close();
                }
            }
            return false;
        }

        private void SaveSchema(DesignerDataConnection connection, string selectCommand, DataTable schemaTable)
        {
            base.DesignerState["DataSourceSchema"] = schemaTable;
            base.DesignerState["DataSourceSchemaConnectionStringHash"] = connection.ConnectionString.GetHashCode();
            base.DesignerState["DataSourceSchemaProviderName"] = connection.ProviderName;
            base.DesignerState["DataSourceSchemaSelectMethod"] = selectCommand;
        }

        internal static string StripParameterPrefix(string parameterName)
        {
            foreach (string str in GetParameterPrefixes())
            {
                if (parameterName.StartsWith(str, StringComparison.OrdinalIgnoreCase))
                {
                    return parameterName.Substring(str.Length);
                }
            }
            return parameterName;
        }

        internal static bool SupportsNamedParameters(DbProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (((factory != SqlClientFactory.Instance) && (factory != OracleClientFactory.Instance)) && !IsSqlCeClientFactory(factory))
            {
                return false;
            }
            return true;
        }

        public override bool CanConfigure
        {
            get
            {
                IDataEnvironment service = (IDataEnvironment) base.Component.Site.GetService(typeof(IDataEnvironment));
                return (service != null);
            }
        }

        public override bool CanRefreshSchema
        {
            get
            {
                string connectionString = this.ConnectionString;
                return (((connectionString != null) && (connectionString.Trim().Length != 0)) && (this.SelectCommand.Trim().Length != 0));
            }
        }

        public string ConnectionString
        {
            get
            {
                return this.GetConnectionString();
            }
            set
            {
                if (value != this.ConnectionString)
                {
                    this.SqlDataSource.ConnectionString = value;
                    this.UpdateDesignTimeHtml();
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), TypeConverter(typeof(SqlDataSourceQueryConverter)), DefaultValue(0), System.Design.SRDescription("SqlDataSourceDesigner_DeleteQuery"), Category("Data"), Editor(typeof(SqlDataSourceQueryEditor), typeof(UITypeEditor)), MergableProperty(false)]
        public DataSourceOperation DeleteQuery
        {
            get
            {
                return DataSourceOperation.Delete;
            }
            set
            {
            }
        }

        [System.Design.SRDescription("SqlDataSourceDesigner_InsertQuery"), MergableProperty(false), TypeConverter(typeof(SqlDataSourceQueryConverter)), DefaultValue(1), Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Editor(typeof(SqlDataSourceQueryEditor), typeof(UITypeEditor))]
        public DataSourceOperation InsertQuery
        {
            get
            {
                return DataSourceOperation.Insert;
            }
            set
            {
            }
        }

        public string ProviderName
        {
            get
            {
                return this.SqlDataSource.ProviderName;
            }
            set
            {
                if (value != this.ProviderName)
                {
                    this.SqlDataSource.ProviderName = value;
                    this.UpdateDesignTimeHtml();
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        internal bool SaveConfiguredConnectionState
        {
            get
            {
                object obj2 = base.DesignerState["SaveConfiguredConnectionState"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                base.DesignerState["SaveConfiguredConnectionState"] = value;
            }
        }

        public string SelectCommand
        {
            get
            {
                return this.SqlDataSource.SelectCommand;
            }
            set
            {
                if (value != this.SelectCommand)
                {
                    this.SqlDataSource.SelectCommand = value;
                    this.UpdateDesignTimeHtml();
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [System.Design.SRDescription("SqlDataSourceDesigner_SelectQuery"), MergableProperty(false), TypeConverter(typeof(SqlDataSourceQueryConverter)), DefaultValue(2), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Editor(typeof(SqlDataSourceQueryEditor), typeof(UITypeEditor)), Category("Data")]
        public DataSourceOperation SelectQuery
        {
            get
            {
                return DataSourceOperation.Select;
            }
            set
            {
            }
        }

        internal System.Web.UI.WebControls.SqlDataSource SqlDataSource
        {
            get
            {
                return (System.Web.UI.WebControls.SqlDataSource) base.Component;
            }
        }

        internal Hashtable TableQueryState
        {
            get
            {
                return (base.DesignerState["TableQueryState"] as Hashtable);
            }
            set
            {
                base.DesignerState["TableQueryState"] = value;
            }
        }

        [Editor(typeof(SqlDataSourceQueryEditor), typeof(UITypeEditor)), TypeConverter(typeof(SqlDataSourceQueryConverter)), MergableProperty(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Category("Data"), DefaultValue(3), System.Design.SRDescription("SqlDataSourceDesigner_UpdateQuery")]
        public DataSourceOperation UpdateQuery
        {
            get
            {
                return DataSourceOperation.Update;
            }
            set
            {
            }
        }
    }
}

