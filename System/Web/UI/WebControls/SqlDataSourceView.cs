namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.UI;

    public class SqlDataSourceView : DataSourceView, IStateManager
    {
        private bool _cancelSelectOnNullParameter;
        private ConflictOptions _conflictDetection;
        private HttpContext _context;
        private string _deleteCommand;
        private SqlDataSourceCommandType _deleteCommandType;
        private ParameterCollection _deleteParameters;
        private string _filterExpression;
        private ParameterCollection _filterParameters;
        private string _insertCommand;
        private SqlDataSourceCommandType _insertCommandType;
        private ParameterCollection _insertParameters;
        private string _oldValuesParameterFormatString;
        private SqlDataSource _owner;
        private string _selectCommand;
        private SqlDataSourceCommandType _selectCommandType;
        private ParameterCollection _selectParameters;
        private string _sortParameterName;
        private bool _tracking;
        private string _updateCommand;
        private SqlDataSourceCommandType _updateCommandType;
        private ParameterCollection _updateParameters;
        private static readonly object EventDeleted = new object();
        private static readonly object EventDeleting = new object();
        private static readonly object EventFiltering = new object();
        private static readonly object EventInserted = new object();
        private static readonly object EventInserting = new object();
        private static readonly object EventSelected = new object();
        private static readonly object EventSelecting = new object();
        private static readonly object EventUpdated = new object();
        private static readonly object EventUpdating = new object();
        private const int MustDeclareVariableSqlExceptionNumber = 0x89;
        private const int ProcedureExpectsParameterSqlExceptionNumber = 0xc9;

        public event SqlDataSourceStatusEventHandler Deleted
        {
            add
            {
                base.Events.AddHandler(EventDeleted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDeleted, value);
            }
        }

        public event SqlDataSourceCommandEventHandler Deleting
        {
            add
            {
                base.Events.AddHandler(EventDeleting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDeleting, value);
            }
        }

        public event SqlDataSourceFilteringEventHandler Filtering
        {
            add
            {
                base.Events.AddHandler(EventFiltering, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFiltering, value);
            }
        }

        public event SqlDataSourceStatusEventHandler Inserted
        {
            add
            {
                base.Events.AddHandler(EventInserted, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInserted, value);
            }
        }

        public event SqlDataSourceCommandEventHandler Inserting
        {
            add
            {
                base.Events.AddHandler(EventInserting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInserting, value);
            }
        }

        public event SqlDataSourceStatusEventHandler Selected
        {
            add
            {
                base.Events.AddHandler(EventSelected, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelected, value);
            }
        }

        public event SqlDataSourceSelectingEventHandler Selecting
        {
            add
            {
                base.Events.AddHandler(EventSelecting, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelecting, value);
            }
        }

        public event SqlDataSourceStatusEventHandler Updated
        {
            add
            {
                base.Events.AddHandler(EventUpdated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventUpdated, value);
            }
        }

        public event SqlDataSourceCommandEventHandler Updating
        {
            add
            {
                base.Events.AddHandler(EventUpdating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventUpdating, value);
            }
        }

        public SqlDataSourceView(SqlDataSource owner, string name, HttpContext context) : base(owner, name)
        {
            this._cancelSelectOnNullParameter = true;
            this._owner = owner;
            this._context = context;
        }

        private void AddParameters(DbCommand command, ParameterCollection reference, IDictionary parameters, IDictionary exclusionList, string oldValuesParameterFormatString)
        {
            IDictionary dictionary = null;
            if (exclusionList != null)
            {
                dictionary = new ListDictionary(StringComparer.OrdinalIgnoreCase);
                foreach (DictionaryEntry entry in exclusionList)
                {
                    dictionary.Add(entry.Key, entry.Value);
                }
            }
            if (parameters != null)
            {
                string parameterPrefix = this.ParameterPrefix;
                foreach (DictionaryEntry entry2 in parameters)
                {
                    string key = (string) entry2.Key;
                    if ((dictionary == null) || !dictionary.Contains(key))
                    {
                        string str3;
                        if (oldValuesParameterFormatString == null)
                        {
                            str3 = key;
                        }
                        else
                        {
                            str3 = string.Format(CultureInfo.InvariantCulture, oldValuesParameterFormatString, new object[] { key });
                        }
                        object parameterValue = entry2.Value;
                        Parameter parameter = reference[str3];
                        if (parameter != null)
                        {
                            parameterValue = parameter.GetValue(entry2.Value, false);
                        }
                        str3 = parameterPrefix + str3;
                        if (command.Parameters.Contains(str3))
                        {
                            if (parameterValue != null)
                            {
                                command.Parameters[str3].Value = parameterValue;
                            }
                        }
                        else
                        {
                            DbParameter parameter2 = this._owner.CreateParameter(str3, parameterValue);
                            command.Parameters.Add(parameter2);
                        }
                    }
                }
            }
        }

        private Exception BuildCustomException(Exception ex, DataSourceOperation operation, DbCommand command, out bool isCustomException)
        {
            SqlException exception = ex as SqlException;
            if ((exception != null) && ((exception.Number == 0x89) || (exception.Number == 0xc9)))
            {
                string str;
                if (command.Parameters.Count > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    bool flag = true;
                    foreach (DbParameter parameter in command.Parameters)
                    {
                        if (!flag)
                        {
                            builder.Append(", ");
                        }
                        builder.Append(parameter.ParameterName);
                        flag = false;
                    }
                    str = builder.ToString();
                }
                else
                {
                    str = System.Web.SR.GetString("SqlDataSourceView_NoParameters");
                }
                isCustomException = true;
                return new InvalidOperationException(System.Web.SR.GetString("SqlDataSourceView_MissingParameters", new object[] { operation, this._owner.ID, str }));
            }
            isCustomException = false;
            return ex;
        }

        public int Delete(IDictionary keys, IDictionary oldValues)
        {
            return this.ExecuteDelete(keys, oldValues);
        }

        private int ExecuteDbCommand(DbCommand command, DataSourceOperation operation)
        {
            int affectedRows = 0;
            bool flag = false;
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }
                affectedRows = command.ExecuteNonQuery();
                if (affectedRows > 0)
                {
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                    DataSourceCache cache = this._owner.Cache;
                    if ((cache != null) && cache.Enabled)
                    {
                        this._owner.InvalidateCacheEntry();
                    }
                }
                flag = true;
                SqlDataSourceStatusEventArgs e = new SqlDataSourceStatusEventArgs(command, affectedRows, null);
                switch (operation)
                {
                    case DataSourceOperation.Delete:
                        this.OnDeleted(e);
                        return affectedRows;

                    case DataSourceOperation.Insert:
                        this.OnInserted(e);
                        return affectedRows;

                    case DataSourceOperation.Select:
                        return affectedRows;

                    case DataSourceOperation.Update:
                        this.OnUpdated(e);
                        return affectedRows;
                }
                return affectedRows;
            }
            catch (Exception exception)
            {
                bool flag2;
                if (!flag)
                {
                    SqlDataSourceStatusEventArgs args2 = new SqlDataSourceStatusEventArgs(command, affectedRows, exception);
                    switch (operation)
                    {
                        case DataSourceOperation.Delete:
                            this.OnDeleted(args2);
                            break;

                        case DataSourceOperation.Insert:
                            this.OnInserted(args2);
                            break;

                        case DataSourceOperation.Update:
                            this.OnUpdated(args2);
                            break;
                    }
                    if (!args2.ExceptionHandled)
                    {
                        throw;
                    }
                    return affectedRows;
                }
                exception = this.BuildCustomException(exception, operation, command, out flag2);
                if (flag2)
                {
                    throw exception;
                }
                throw;
            }
            finally
            {
                if (command.Connection.State == ConnectionState.Open)
                {
                    command.Connection.Close();
                }
            }
            return affectedRows;
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
        {
            if (!this.CanDelete)
            {
                throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_DeleteNotSupported", new object[] { this._owner.ID }));
            }
            DbConnection connection = this._owner.CreateConnection(this._owner.ConnectionString);
            if (connection == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("SqlDataSourceView_CouldNotCreateConnection", new object[] { this._owner.ID }));
            }
            string oldValuesParameterFormatString = this.OldValuesParameterFormatString;
            DbCommand command = this._owner.CreateCommand(this.DeleteCommand, connection);
            this.InitializeParameters(command, this.DeleteParameters, oldValues);
            this.AddParameters(command, this.DeleteParameters, keys, null, oldValuesParameterFormatString);
            if (this.ConflictDetection == ConflictOptions.CompareAllValues)
            {
                if ((oldValues == null) || (oldValues.Count == 0))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SqlDataSourceView_Pessimistic", new object[] { System.Web.SR.GetString("DataSourceView_delete"), this._owner.ID, "values" }));
                }
                this.AddParameters(command, this.DeleteParameters, oldValues, null, oldValuesParameterFormatString);
            }
            command.CommandType = GetCommandType(this.DeleteCommandType);
            SqlDataSourceCommandEventArgs e = new SqlDataSourceCommandEventArgs(command);
            this.OnDeleting(e);
            if (e.Cancel)
            {
                return 0;
            }
            this.ReplaceNullValues(command);
            return this.ExecuteDbCommand(command, DataSourceOperation.Delete);
        }

        protected override int ExecuteInsert(IDictionary values)
        {
            if (!this.CanInsert)
            {
                throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_InsertNotSupported", new object[] { this._owner.ID }));
            }
            DbConnection connection = this._owner.CreateConnection(this._owner.ConnectionString);
            if (connection == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("SqlDataSourceView_CouldNotCreateConnection", new object[] { this._owner.ID }));
            }
            DbCommand command = this._owner.CreateCommand(this.InsertCommand, connection);
            this.InitializeParameters(command, this.InsertParameters, null);
            this.AddParameters(command, this.InsertParameters, values, null, null);
            command.CommandType = GetCommandType(this.InsertCommandType);
            SqlDataSourceCommandEventArgs e = new SqlDataSourceCommandEventArgs(command);
            this.OnInserting(e);
            if (e.Cancel)
            {
                return 0;
            }
            this.ReplaceNullValues(command);
            return this.ExecuteDbCommand(command, DataSourceOperation.Insert);
        }

        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            SqlCacheDependency dependency;
            if (this.SelectCommand.Length == 0)
            {
                return null;
            }
            DbConnection connection = this._owner.CreateConnection(this._owner.ConnectionString);
            if (connection == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("SqlDataSourceView_CouldNotCreateConnection", new object[] { this._owner.ID }));
            }
            DataSourceCache cache = this._owner.Cache;
            bool flag = (cache != null) && cache.Enabled;
            string sortExpression = arguments.SortExpression;
            if (this.CanPage)
            {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Page);
            }
            if (this.CanSort)
            {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Sort);
            }
            if (this.CanRetrieveTotalRowCount)
            {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.RetrieveTotalRowCount);
            }
            if (flag)
            {
                if (this._owner.DataSourceMode != SqlDataSourceMode.DataSet)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_CacheNotSupported", new object[] { this._owner.ID }));
                }
                arguments.RaiseUnsupportedCapabilitiesError(this);
                DataSet set = this._owner.LoadDataFromCache(0, -1) as DataSet;
                if (set != null)
                {
                    IOrderedDictionary parameterValues = this.FilterParameters.GetValues(this._context, this._owner);
                    if (this.FilterExpression.Length > 0)
                    {
                        SqlDataSourceFilteringEventArgs args = new SqlDataSourceFilteringEventArgs(parameterValues);
                        this.OnFiltering(args);
                        if (args.Cancel)
                        {
                            return null;
                        }
                    }
                    return FilteredDataSetHelper.CreateFilteredDataView(set.Tables[0], sortExpression, this.FilterExpression, parameterValues);
                }
            }
            DbCommand command = this._owner.CreateCommand(this.SelectCommand, connection);
            this.InitializeParameters(command, this.SelectParameters, null);
            command.CommandType = GetCommandType(this.SelectCommandType);
            SqlDataSourceSelectingEventArgs e = new SqlDataSourceSelectingEventArgs(command, arguments);
            this.OnSelecting(e);
            if (e.Cancel)
            {
                return null;
            }
            string sortParameterName = this.SortParameterName;
            if (sortParameterName.Length > 0)
            {
                if (command.CommandType != CommandType.StoredProcedure)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_SortParameterRequiresStoredProcedure", new object[] { this._owner.ID }));
                }
                command.Parameters.Add(this._owner.CreateParameter(this.ParameterPrefix + sortParameterName, sortExpression));
                arguments.SortExpression = string.Empty;
            }
            arguments.RaiseUnsupportedCapabilitiesError(this);
            sortExpression = arguments.SortExpression;
            if (this.CancelSelectOnNullParameter)
            {
                int count = command.Parameters.Count;
                for (int i = 0; i < count; i++)
                {
                    DbParameter parameter = command.Parameters[i];
                    if (((parameter != null) && (parameter.Value == null)) && ((parameter.Direction == ParameterDirection.Input) || (parameter.Direction == ParameterDirection.InputOutput)))
                    {
                        return null;
                    }
                }
            }
            this.ReplaceNullValues(command);
            IEnumerable enumerable = null;
            switch (this._owner.DataSourceMode)
            {
                case SqlDataSourceMode.DataReader:
                {
                    if (this.FilterExpression.Length > 0)
                    {
                        throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_FilterNotSupported", new object[] { this._owner.ID }));
                    }
                    if (sortExpression.Length > 0)
                    {
                        throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_SortNotSupported", new object[] { this._owner.ID }));
                    }
                    bool flag4 = false;
                    try
                    {
                        if (connection.State != ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        enumerable = command.ExecuteReader(CommandBehavior.CloseConnection);
                        flag4 = true;
                        SqlDataSourceStatusEventArgs args6 = new SqlDataSourceStatusEventArgs(command, 0, null);
                        this.OnSelected(args6);
                    }
                    catch (Exception exception2)
                    {
                        bool flag5;
                        if (!flag4)
                        {
                            SqlDataSourceStatusEventArgs args7 = new SqlDataSourceStatusEventArgs(command, 0, exception2);
                            this.OnSelected(args7);
                            if (!args7.ExceptionHandled)
                            {
                                throw;
                            }
                            return enumerable;
                        }
                        exception2 = this.BuildCustomException(exception2, DataSourceOperation.Select, command, out flag5);
                        if (flag5)
                        {
                            throw exception2;
                        }
                        throw;
                    }
                    return enumerable;
                }
                case SqlDataSourceMode.DataSet:
                    dependency = null;
                    if (flag && (cache is SqlDataSourceCache))
                    {
                        SqlDataSourceCache cache2 = (SqlDataSourceCache) cache;
                        if (string.Equals(cache2.SqlCacheDependency, "CommandNotification", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!(command is SqlCommand))
                            {
                                throw new InvalidOperationException(System.Web.SR.GetString("SqlDataSourceView_CommandNotificationNotSupported", new object[] { this._owner.ID }));
                            }
                            dependency = new SqlCacheDependency((SqlCommand) command);
                            break;
                        }
                    }
                    break;

                default:
                    return enumerable;
            }
            DbDataAdapter adapter = this._owner.CreateDataAdapter(command);
            DataSet dataSet = new DataSet();
            int affectedRows = 0;
            bool flag2 = false;
            try
            {
                affectedRows = adapter.Fill(dataSet, base.Name);
                flag2 = true;
                SqlDataSourceStatusEventArgs args3 = new SqlDataSourceStatusEventArgs(command, affectedRows, null);
                this.OnSelected(args3);
            }
            catch (Exception exception)
            {
                if (flag2)
                {
                    bool flag3;
                    exception = this.BuildCustomException(exception, DataSourceOperation.Select, command, out flag3);
                    if (flag3)
                    {
                        throw exception;
                    }
                    throw;
                }
                SqlDataSourceStatusEventArgs args4 = new SqlDataSourceStatusEventArgs(command, affectedRows, exception);
                this.OnSelected(args4);
                if (!args4.ExceptionHandled)
                {
                    throw;
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            DataTable table = (dataSet.Tables.Count > 0) ? dataSet.Tables[0] : null;
            if (flag && (table != null))
            {
                this._owner.SaveDataToCache(0, -1, dataSet, dependency);
            }
            if (table == null)
            {
                return enumerable;
            }
            IOrderedDictionary values = this.FilterParameters.GetValues(this._context, this._owner);
            if (this.FilterExpression.Length > 0)
            {
                SqlDataSourceFilteringEventArgs args5 = new SqlDataSourceFilteringEventArgs(values);
                this.OnFiltering(args5);
                if (args5.Cancel)
                {
                    return null;
                }
            }
            return FilteredDataSetHelper.CreateFilteredDataView(table, sortExpression, this.FilterExpression, values);
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            if (!this.CanUpdate)
            {
                throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_UpdateNotSupported", new object[] { this._owner.ID }));
            }
            DbConnection connection = this._owner.CreateConnection(this._owner.ConnectionString);
            if (connection == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("SqlDataSourceView_CouldNotCreateConnection", new object[] { this._owner.ID }));
            }
            string oldValuesParameterFormatString = this.OldValuesParameterFormatString;
            DbCommand command = this._owner.CreateCommand(this.UpdateCommand, connection);
            this.InitializeParameters(command, this.UpdateParameters, keys);
            this.AddParameters(command, this.UpdateParameters, values, null, null);
            this.AddParameters(command, this.UpdateParameters, keys, null, oldValuesParameterFormatString);
            if (this.ConflictDetection == ConflictOptions.CompareAllValues)
            {
                if ((oldValues == null) || (oldValues.Count == 0))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SqlDataSourceView_Pessimistic", new object[] { System.Web.SR.GetString("DataSourceView_update"), this._owner.ID, "oldValues" }));
                }
                this.AddParameters(command, this.UpdateParameters, oldValues, null, oldValuesParameterFormatString);
            }
            command.CommandType = GetCommandType(this.UpdateCommandType);
            SqlDataSourceCommandEventArgs e = new SqlDataSourceCommandEventArgs(command);
            this.OnUpdating(e);
            if (e.Cancel)
            {
                return 0;
            }
            this.ReplaceNullValues(command);
            return this.ExecuteDbCommand(command, DataSourceOperation.Update);
        }

        private static CommandType GetCommandType(SqlDataSourceCommandType commandType)
        {
            if (commandType == SqlDataSourceCommandType.Text)
            {
                return CommandType.Text;
            }
            return CommandType.StoredProcedure;
        }

        private void InitializeParameters(DbCommand command, ParameterCollection parameters, IDictionary exclusionList)
        {
            string parameterPrefix = this.ParameterPrefix;
            IDictionary dictionary = null;
            if (exclusionList != null)
            {
                dictionary = new ListDictionary(StringComparer.OrdinalIgnoreCase);
                foreach (DictionaryEntry entry in exclusionList)
                {
                    dictionary.Add(entry.Key, entry.Value);
                }
            }
            IOrderedDictionary values = parameters.GetValues(this._context, this._owner);
            for (int i = 0; i < parameters.Count; i++)
            {
                Parameter parameter = parameters[i];
                if ((dictionary != null) && dictionary.Contains(parameter.Name))
                {
                    continue;
                }
                DbParameter parameter2 = this._owner.CreateParameter(parameterPrefix + parameter.Name, values[i]);
                parameter2.Direction = parameter.Direction;
                parameter2.Size = parameter.Size;
                if ((parameter.DbType != DbType.Object) || ((parameter.Type != TypeCode.Empty) && (parameter.Type != TypeCode.DBNull)))
                {
                    SqlParameter parameter3 = parameter2 as SqlParameter;
                    if (parameter3 == null)
                    {
                        parameter2.DbType = parameter.GetDatabaseType();
                    }
                    else
                    {
                        DbType databaseType = parameter.GetDatabaseType();
                        if (databaseType != DbType.Date)
                        {
                            if (databaseType != DbType.Time)
                            {
                                goto Label_0143;
                            }
                            parameter3.SqlDbType = SqlDbType.Time;
                        }
                        else
                        {
                            parameter3.SqlDbType = SqlDbType.Date;
                        }
                    }
                }
                goto Label_0151;
            Label_0143:
                parameter2.DbType = parameter.GetDatabaseType();
            Label_0151:
                command.Parameters.Add(parameter2);
            }
        }

        public int Insert(IDictionary values)
        {
            return this.ExecuteInsert(values);
        }

        protected virtual void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Pair pair = (Pair) savedState;
                if (pair.First != null)
                {
                    ((IStateManager) this.SelectParameters).LoadViewState(pair.First);
                }
                if (pair.Second != null)
                {
                    ((IStateManager) this.FilterParameters).LoadViewState(pair.Second);
                }
            }
        }

        protected virtual void OnDeleted(SqlDataSourceStatusEventArgs e)
        {
            SqlDataSourceStatusEventHandler handler = base.Events[EventDeleted] as SqlDataSourceStatusEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDeleting(SqlDataSourceCommandEventArgs e)
        {
            SqlDataSourceCommandEventHandler handler = base.Events[EventDeleting] as SqlDataSourceCommandEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFiltering(SqlDataSourceFilteringEventArgs e)
        {
            SqlDataSourceFilteringEventHandler handler = base.Events[EventFiltering] as SqlDataSourceFilteringEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnInserted(SqlDataSourceStatusEventArgs e)
        {
            SqlDataSourceStatusEventHandler handler = base.Events[EventInserted] as SqlDataSourceStatusEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnInserting(SqlDataSourceCommandEventArgs e)
        {
            SqlDataSourceCommandEventHandler handler = base.Events[EventInserting] as SqlDataSourceCommandEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelected(SqlDataSourceStatusEventArgs e)
        {
            SqlDataSourceStatusEventHandler handler = base.Events[EventSelected] as SqlDataSourceStatusEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelecting(SqlDataSourceSelectingEventArgs e)
        {
            SqlDataSourceSelectingEventHandler handler = base.Events[EventSelecting] as SqlDataSourceSelectingEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnUpdated(SqlDataSourceStatusEventArgs e)
        {
            SqlDataSourceStatusEventHandler handler = base.Events[EventUpdated] as SqlDataSourceStatusEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnUpdating(SqlDataSourceCommandEventArgs e)
        {
            SqlDataSourceCommandEventHandler handler = base.Events[EventUpdating] as SqlDataSourceCommandEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void RaiseUnsupportedCapabilityError(DataSourceCapabilities capability)
        {
            if (!this.CanPage && ((capability & DataSourceCapabilities.Page) != DataSourceCapabilities.None))
            {
                throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_NoPaging", new object[] { this._owner.ID }));
            }
            if (!this.CanSort && ((capability & DataSourceCapabilities.Sort) != DataSourceCapabilities.None))
            {
                throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_NoSorting", new object[] { this._owner.ID }));
            }
            if (!this.CanRetrieveTotalRowCount && ((capability & DataSourceCapabilities.RetrieveTotalRowCount) != DataSourceCapabilities.None))
            {
                throw new NotSupportedException(System.Web.SR.GetString("SqlDataSourceView_NoRowCount", new object[] { this._owner.ID }));
            }
            base.RaiseUnsupportedCapabilityError(capability);
        }

        private void ReplaceNullValues(DbCommand command)
        {
            int count = command.Parameters.Count;
            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Value == null)
                {
                    parameter.Value = DBNull.Value;
                }
            }
        }

        protected virtual object SaveViewState()
        {
            Pair pair = new Pair {
                First = (this._selectParameters != null) ? ((IStateManager) this._selectParameters).SaveViewState() : null,
                Second = (this._filterParameters != null) ? ((IStateManager) this._filterParameters).SaveViewState() : null
            };
            if ((pair.First == null) && (pair.Second == null))
            {
                return null;
            }
            return pair;
        }

        public IEnumerable Select(DataSourceSelectArguments arguments)
        {
            return this.ExecuteSelect(arguments);
        }

        private void SelectParametersChangedEventHandler(object o, EventArgs e)
        {
            this.OnDataSourceViewChanged(EventArgs.Empty);
        }

        void IStateManager.LoadViewState(object savedState)
        {
            this.LoadViewState(savedState);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        protected virtual void TrackViewState()
        {
            this._tracking = true;
            if (this._selectParameters != null)
            {
                ((IStateManager) this._selectParameters).TrackViewState();
            }
            if (this._filterParameters != null)
            {
                ((IStateManager) this._filterParameters).TrackViewState();
            }
        }

        public int Update(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            return this.ExecuteUpdate(keys, values, oldValues);
        }

        public bool CancelSelectOnNullParameter
        {
            get
            {
                return this._cancelSelectOnNullParameter;
            }
            set
            {
                if (this.CancelSelectOnNullParameter != value)
                {
                    this._cancelSelectOnNullParameter = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public override bool CanDelete
        {
            get
            {
                return (this.DeleteCommand.Length != 0);
            }
        }

        public override bool CanInsert
        {
            get
            {
                return (this.InsertCommand.Length != 0);
            }
        }

        public override bool CanPage
        {
            get
            {
                return false;
            }
        }

        public override bool CanRetrieveTotalRowCount
        {
            get
            {
                return false;
            }
        }

        public override bool CanSort
        {
            get
            {
                if (this._owner.DataSourceMode != SqlDataSourceMode.DataSet)
                {
                    return (this.SortParameterName.Length > 0);
                }
                return true;
            }
        }

        public override bool CanUpdate
        {
            get
            {
                return (this.UpdateCommand.Length != 0);
            }
        }

        public ConflictOptions ConflictDetection
        {
            get
            {
                return this._conflictDetection;
            }
            set
            {
                if ((value < ConflictOptions.OverwriteChanges) || (value > ConflictOptions.CompareAllValues))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._conflictDetection = value;
                this.OnDataSourceViewChanged(EventArgs.Empty);
            }
        }

        public string DeleteCommand
        {
            get
            {
                if (this._deleteCommand == null)
                {
                    return string.Empty;
                }
                return this._deleteCommand;
            }
            set
            {
                this._deleteCommand = value;
            }
        }

        public SqlDataSourceCommandType DeleteCommandType
        {
            get
            {
                return this._deleteCommandType;
            }
            set
            {
                if ((value < SqlDataSourceCommandType.Text) || (value > SqlDataSourceCommandType.StoredProcedure))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._deleteCommandType = value;
            }
        }

        [DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("SqlDataSource_DeleteParameters")]
        public ParameterCollection DeleteParameters
        {
            get
            {
                if (this._deleteParameters == null)
                {
                    this._deleteParameters = new ParameterCollection();
                }
                return this._deleteParameters;
            }
        }

        public string FilterExpression
        {
            get
            {
                if (this._filterExpression == null)
                {
                    return string.Empty;
                }
                return this._filterExpression;
            }
            set
            {
                if (this.FilterExpression != value)
                {
                    this._filterExpression = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue((string) null), WebSysDescription("SqlDataSource_FilterParameters")]
        public ParameterCollection FilterParameters
        {
            get
            {
                if (this._filterParameters == null)
                {
                    this._filterParameters = new ParameterCollection();
                    this._filterParameters.ParametersChanged += new EventHandler(this.SelectParametersChangedEventHandler);
                    if (this._tracking)
                    {
                        ((IStateManager) this._filterParameters).TrackViewState();
                    }
                }
                return this._filterParameters;
            }
        }

        public string InsertCommand
        {
            get
            {
                if (this._insertCommand == null)
                {
                    return string.Empty;
                }
                return this._insertCommand;
            }
            set
            {
                this._insertCommand = value;
            }
        }

        public SqlDataSourceCommandType InsertCommandType
        {
            get
            {
                return this._insertCommandType;
            }
            set
            {
                if ((value < SqlDataSourceCommandType.Text) || (value > SqlDataSourceCommandType.StoredProcedure))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._insertCommandType = value;
            }
        }

        [Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("SqlDataSource_InsertParameters")]
        public ParameterCollection InsertParameters
        {
            get
            {
                if (this._insertParameters == null)
                {
                    this._insertParameters = new ParameterCollection();
                }
                return this._insertParameters;
            }
        }

        protected bool IsTrackingViewState
        {
            get
            {
                return this._tracking;
            }
        }

        [DefaultValue("{0}"), WebCategory("Data"), WebSysDescription("DataSource_OldValuesParameterFormatString")]
        public string OldValuesParameterFormatString
        {
            get
            {
                if (this._oldValuesParameterFormatString == null)
                {
                    return "{0}";
                }
                return this._oldValuesParameterFormatString;
            }
            set
            {
                this._oldValuesParameterFormatString = value;
                this.OnDataSourceViewChanged(EventArgs.Empty);
            }
        }

        protected virtual string ParameterPrefix
        {
            get
            {
                if (!string.IsNullOrEmpty(this._owner.ProviderName) && !string.Equals(this._owner.ProviderName, "System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Empty;
                }
                return "@";
            }
        }

        public string SelectCommand
        {
            get
            {
                if (this._selectCommand == null)
                {
                    return string.Empty;
                }
                return this._selectCommand;
            }
            set
            {
                if (this.SelectCommand != value)
                {
                    this._selectCommand = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public SqlDataSourceCommandType SelectCommandType
        {
            get
            {
                return this._selectCommandType;
            }
            set
            {
                if ((value < SqlDataSourceCommandType.Text) || (value > SqlDataSourceCommandType.StoredProcedure))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._selectCommandType = value;
            }
        }

        public ParameterCollection SelectParameters
        {
            get
            {
                if (this._selectParameters == null)
                {
                    this._selectParameters = new ParameterCollection();
                    this._selectParameters.ParametersChanged += new EventHandler(this.SelectParametersChangedEventHandler);
                    if (this._tracking)
                    {
                        ((IStateManager) this._selectParameters).TrackViewState();
                    }
                }
                return this._selectParameters;
            }
        }

        public string SortParameterName
        {
            get
            {
                if (this._sortParameterName == null)
                {
                    return string.Empty;
                }
                return this._sortParameterName;
            }
            set
            {
                if (this.SortParameterName != value)
                {
                    this._sortParameterName = value;
                    this.OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        public string UpdateCommand
        {
            get
            {
                if (this._updateCommand == null)
                {
                    return string.Empty;
                }
                return this._updateCommand;
            }
            set
            {
                this._updateCommand = value;
            }
        }

        public SqlDataSourceCommandType UpdateCommandType
        {
            get
            {
                return this._updateCommandType;
            }
            set
            {
                if ((value < SqlDataSourceCommandType.Text) || (value > SqlDataSourceCommandType.StoredProcedure))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._updateCommandType = value;
            }
        }

        [Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("SqlDataSource_UpdateParameters")]
        public ParameterCollection UpdateParameters
        {
            get
            {
                if (this._updateParameters == null)
                {
                    this._updateParameters = new ParameterCollection();
                }
                return this._updateParameters;
            }
        }
    }
}

