namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.UI;

    [DefaultProperty("SelectQuery"), ParseChildren(true), WebSysDescription("SqlDataSource_Description"), DefaultEvent("Selecting"), WebSysDisplayName("SqlDataSource_DisplayName"), Designer("System.Web.UI.Design.WebControls.SqlDataSourceDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), PersistChildren(false), ToolboxBitmap(typeof(SqlDataSource))]
    public class SqlDataSource : DataSourceControl
    {
        private DataSourceCache _cache;
        private string _cachedSelectCommand;
        private string _connectionString;
        private SqlDataSourceMode _dataSourceMode;
        private DbProviderFactory _providerFactory;
        private string _providerName;
        private SqlDataSourceView _view;
        private ICollection _viewNames;
        private const string DefaultProviderName = "System.Data.SqlClient";
        private const string DefaultViewName = "DefaultView";

        [WebSysDescription("DataSource_Deleted"), WebCategory("Data")]
        public event SqlDataSourceStatusEventHandler Deleted
        {
            add
            {
                this.GetView().Deleted += value;
            }
            remove
            {
                this.GetView().Deleted -= value;
            }
        }

        [WebSysDescription("DataSource_Deleting"), WebCategory("Data")]
        public event SqlDataSourceCommandEventHandler Deleting
        {
            add
            {
                this.GetView().Deleting += value;
            }
            remove
            {
                this.GetView().Deleting -= value;
            }
        }

        [WebCategory("Data"), WebSysDescription("DataSource_Filtering")]
        public event SqlDataSourceFilteringEventHandler Filtering
        {
            add
            {
                this.GetView().Filtering += value;
            }
            remove
            {
                this.GetView().Filtering -= value;
            }
        }

        [WebSysDescription("DataSource_Inserted"), WebCategory("Data")]
        public event SqlDataSourceStatusEventHandler Inserted
        {
            add
            {
                this.GetView().Inserted += value;
            }
            remove
            {
                this.GetView().Inserted -= value;
            }
        }

        [WebCategory("Data"), WebSysDescription("DataSource_Inserting")]
        public event SqlDataSourceCommandEventHandler Inserting
        {
            add
            {
                this.GetView().Inserting += value;
            }
            remove
            {
                this.GetView().Inserting -= value;
            }
        }

        [WebSysDescription("SqlDataSource_Selected"), WebCategory("Data")]
        public event SqlDataSourceStatusEventHandler Selected
        {
            add
            {
                this.GetView().Selected += value;
            }
            remove
            {
                this.GetView().Selected -= value;
            }
        }

        [WebCategory("Data"), WebSysDescription("SqlDataSource_Selecting")]
        public event SqlDataSourceSelectingEventHandler Selecting
        {
            add
            {
                this.GetView().Selecting += value;
            }
            remove
            {
                this.GetView().Selecting -= value;
            }
        }

        [WebCategory("Data"), WebSysDescription("DataSource_Updated")]
        public event SqlDataSourceStatusEventHandler Updated
        {
            add
            {
                this.GetView().Updated += value;
            }
            remove
            {
                this.GetView().Updated -= value;
            }
        }

        [WebCategory("Data"), WebSysDescription("DataSource_Updating")]
        public event SqlDataSourceCommandEventHandler Updating
        {
            add
            {
                this.GetView().Updating += value;
            }
            remove
            {
                this.GetView().Updating -= value;
            }
        }

        public SqlDataSource()
        {
            this._dataSourceMode = SqlDataSourceMode.DataSet;
        }

        public SqlDataSource(string connectionString, string selectCommand)
        {
            this._dataSourceMode = SqlDataSourceMode.DataSet;
            this._connectionString = connectionString;
            this._cachedSelectCommand = selectCommand;
        }

        public SqlDataSource(string providerName, string connectionString, string selectCommand) : this(connectionString, selectCommand)
        {
            this._providerName = providerName;
        }

        internal string CreateCacheKey(int startRowIndex, int maximumRows)
        {
            StringBuilder builder = this.CreateRawCacheKey();
            builder.Append(startRowIndex.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(maximumRows.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        internal DbCommand CreateCommand(string commandText, DbConnection connection)
        {
            DbCommand command = this.GetDbProviderFactorySecure().CreateCommand();
            command.CommandText = commandText;
            command.Connection = connection;
            return command;
        }

        internal DbConnection CreateConnection(string connectionString)
        {
            DbConnection connection = this.GetDbProviderFactorySecure().CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        internal DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            DbDataAdapter adapter = this.GetDbProviderFactorySecure().CreateDataAdapter();
            adapter.SelectCommand = command;
            return adapter;
        }

        protected virtual SqlDataSourceView CreateDataSourceView(string viewName)
        {
            return new SqlDataSourceView(this, viewName, this.Context);
        }

        internal string CreateMasterCacheKey()
        {
            return this.CreateRawCacheKey().ToString();
        }

        internal DbParameter CreateParameter(string parameterName, object parameterValue)
        {
            DbParameter parameter = this.GetDbProviderFactorySecure().CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            return parameter;
        }

        private StringBuilder CreateRawCacheKey()
        {
            StringBuilder builder = new StringBuilder("u", 0x400);
            builder.Append(base.GetType().GetHashCode().ToString(CultureInfo.InvariantCulture));
            builder.Append(this.CacheDuration.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(((int) this.CacheExpirationPolicy).ToString(CultureInfo.InvariantCulture));
            System.Web.UI.SqlDataSourceCache cache = this.Cache as System.Web.UI.SqlDataSourceCache;
            if (cache != null)
            {
                builder.Append(":");
                builder.Append(cache.SqlCacheDependency);
            }
            builder.Append(":");
            builder.Append(this.ConnectionString);
            builder.Append(":");
            builder.Append(this.SelectCommand);
            if (this.SelectParameters.Count > 0)
            {
                builder.Append("?");
                foreach (DictionaryEntry entry in this.SelectParameters.GetValues(this.Context, this))
                {
                    builder.Append(entry.Key.ToString());
                    if ((entry.Value != null) && (entry.Value != DBNull.Value))
                    {
                        builder.Append("=");
                        builder.Append(entry.Value.ToString());
                    }
                    else if (entry.Value == DBNull.Value)
                    {
                        builder.Append("(dbnull)");
                    }
                    else
                    {
                        builder.Append("(null)");
                    }
                    builder.Append("&");
                }
            }
            return builder;
        }

        public int Delete()
        {
            return this.GetView().Delete(null, null);
        }

        protected virtual DbProviderFactory GetDbProviderFactory()
        {
            string providerName = this.ProviderName;
            if (string.IsNullOrEmpty(providerName))
            {
                return SqlClientFactory.Instance;
            }
            return DbProviderFactories.GetFactory(providerName);
        }

        private DbProviderFactory GetDbProviderFactorySecure()
        {
            if (this._providerFactory == null)
            {
                this._providerFactory = this.GetDbProviderFactory();
                if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && !HttpRuntime.ProcessRequestInApplicationTrust) && !HttpRuntime.HasDbPermission(this._providerFactory))
                {
                    throw new HttpException(System.Web.SR.GetString("SqlDataSource_NoDbPermission", new object[] { this._providerFactory.GetType().Name, this.ID }));
                }
            }
            return this._providerFactory;
        }

        private SqlDataSourceView GetView()
        {
            if (this._view == null)
            {
                this._view = this.CreateDataSourceView("DefaultView");
                if (this._cachedSelectCommand != null)
                {
                    this._view.SelectCommand = this._cachedSelectCommand;
                }
                if (base.IsTrackingViewState)
                {
                    ((IStateManager) this._view).TrackViewState();
                }
            }
            return this._view;
        }

        protected override DataSourceView GetView(string viewName)
        {
            if ((viewName == null) || ((viewName.Length != 0) && !string.Equals(viewName, "DefaultView", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(System.Web.SR.GetString("DataSource_InvalidViewName", new object[] { this.ID, "DefaultView" }), "viewName");
            }
            return this.GetView();
        }

        protected override ICollection GetViewNames()
        {
            if (this._viewNames == null)
            {
                this._viewNames = new string[] { "DefaultView" };
            }
            return this._viewNames;
        }

        public int Insert()
        {
            return this.GetView().Insert(null);
        }

        internal void InvalidateCacheEntry()
        {
            string key = this.CreateMasterCacheKey();
            this.Cache.Invalidate(key);
        }

        private void LoadCompleteEventHandler(object sender, EventArgs e)
        {
            this.SelectParameters.UpdateValues(this.Context, this);
            this.FilterParameters.UpdateValues(this.Context, this);
        }

        internal object LoadDataFromCache(int startRowIndex, int maximumRows)
        {
            string key = this.CreateCacheKey(startRowIndex, maximumRows);
            return this.Cache.LoadDataFromCache(key);
        }

        internal int LoadTotalRowCountFromCache()
        {
            string key = this.CreateMasterCacheKey();
            object obj2 = this.Cache.LoadDataFromCache(key);
            if (obj2 is int)
            {
                return (int) obj2;
            }
            return -1;
        }

        protected override void LoadViewState(object savedState)
        {
            Pair pair = (Pair) savedState;
            if (savedState == null)
            {
                base.LoadViewState(null);
            }
            else
            {
                base.LoadViewState(pair.First);
                if (pair.Second != null)
                {
                    ((IStateManager) this.GetView()).LoadViewState(pair.Second);
                }
            }
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.Page != null)
            {
                this.Page.LoadComplete += new EventHandler(this.LoadCompleteEventHandler);
            }
        }

        internal virtual void SaveDataToCache(int startRowIndex, int maximumRows, object data, CacheDependency dependency)
        {
            string key = this.CreateCacheKey(startRowIndex, maximumRows);
            string str2 = this.CreateMasterCacheKey();
            if (this.Cache.LoadDataFromCache(str2) == null)
            {
                this.Cache.SaveDataToCache(str2, -1, dependency);
            }
            CacheDependency dependency2 = new CacheDependency(0, new string[0], new string[] { str2 });
            this.Cache.SaveDataToCache(key, data, dependency2);
        }

        protected override object SaveViewState()
        {
            Pair pair = new Pair {
                First = base.SaveViewState()
            };
            if (this._view != null)
            {
                pair.Second = ((IStateManager) this._view).SaveViewState();
            }
            if ((pair.First == null) && (pair.Second == null))
            {
                return null;
            }
            return pair;
        }

        public IEnumerable Select(DataSourceSelectArguments arguments)
        {
            return this.GetView().Select(arguments);
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._view != null)
            {
                ((IStateManager) this._view).TrackViewState();
            }
        }

        public int Update()
        {
            return this.GetView().Update(null, null, null);
        }

        internal virtual DataSourceCache Cache
        {
            get
            {
                if (this._cache == null)
                {
                    this._cache = new System.Web.UI.SqlDataSourceCache();
                }
                return this._cache;
            }
        }

        [TypeConverter(typeof(DataSourceCacheDurationConverter)), WebSysDescription("DataSourceCache_Duration"), DefaultValue(0), WebCategory("Cache")]
        public virtual int CacheDuration
        {
            get
            {
                return this.Cache.Duration;
            }
            set
            {
                this.Cache.Duration = value;
            }
        }

        [DefaultValue(0), WebSysDescription("DataSourceCache_ExpirationPolicy"), WebCategory("Cache")]
        public virtual DataSourceCacheExpiry CacheExpirationPolicy
        {
            get
            {
                return this.Cache.ExpirationPolicy;
            }
            set
            {
                this.Cache.ExpirationPolicy = value;
            }
        }

        [WebSysDescription("DataSourceCache_KeyDependency"), DefaultValue(""), WebCategory("Cache")]
        public virtual string CacheKeyDependency
        {
            get
            {
                return this.Cache.KeyDependency;
            }
            set
            {
                this.Cache.KeyDependency = value;
            }
        }

        [WebCategory("Data"), DefaultValue(true), WebSysDescription("SqlDataSource_CancelSelectOnNullParameter")]
        public virtual bool CancelSelectOnNullParameter
        {
            get
            {
                return this.GetView().CancelSelectOnNullParameter;
            }
            set
            {
                this.GetView().CancelSelectOnNullParameter = value;
            }
        }

        [WebCategory("Data"), DefaultValue(0), WebSysDescription("SqlDataSource_ConflictDetection")]
        public ConflictOptions ConflictDetection
        {
            get
            {
                return this.GetView().ConflictDetection;
            }
            set
            {
                this.GetView().ConflictDetection = value;
            }
        }

        [DefaultValue(""), WebSysDescription("SqlDataSource_ConnectionString"), Editor("System.Web.UI.Design.WebControls.SqlDataSourceConnectionStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Data"), MergableProperty(false)]
        public virtual string ConnectionString
        {
            get
            {
                if (this._connectionString != null)
                {
                    return this._connectionString;
                }
                return string.Empty;
            }
            set
            {
                if (this.ConnectionString != value)
                {
                    this._connectionString = value;
                    this.RaiseDataSourceChangedEvent(EventArgs.Empty);
                }
            }
        }

        [WebCategory("Behavior"), DefaultValue(1), WebSysDescription("SqlDataSource_DataSourceMode")]
        public SqlDataSourceMode DataSourceMode
        {
            get
            {
                return this._dataSourceMode;
            }
            set
            {
                if ((value < SqlDataSourceMode.DataReader) || (value > SqlDataSourceMode.DataSet))
                {
                    throw new ArgumentOutOfRangeException(System.Web.SR.GetString("SqlDataSource_InvalidMode", new object[] { this.ID }));
                }
                if (this.DataSourceMode != value)
                {
                    this._dataSourceMode = value;
                    this.RaiseDataSourceChangedEvent(EventArgs.Empty);
                }
            }
        }

        [WebCategory("Data"), DefaultValue(""), WebSysDescription("SqlDataSource_DeleteCommand")]
        public string DeleteCommand
        {
            get
            {
                return this.GetView().DeleteCommand;
            }
            set
            {
                this.GetView().DeleteCommand = value;
            }
        }

        [DefaultValue(0), WebCategory("Data"), WebSysDescription("SqlDataSource_DeleteCommandType")]
        public SqlDataSourceCommandType DeleteCommandType
        {
            get
            {
                return this.GetView().DeleteCommandType;
            }
            set
            {
                this.GetView().DeleteCommandType = value;
            }
        }

        [DefaultValue((string) null), WebSysDescription("SqlDataSource_DeleteParameters"), WebCategory("Data"), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), MergableProperty(false)]
        public ParameterCollection DeleteParameters
        {
            get
            {
                return this.GetView().DeleteParameters;
            }
        }

        [WebSysDescription("DataSourceCache_Enabled"), DefaultValue(false), WebCategory("Cache")]
        public virtual bool EnableCaching
        {
            get
            {
                return this.Cache.Enabled;
            }
            set
            {
                this.Cache.Enabled = value;
            }
        }

        [WebSysDescription("SqlDataSource_FilterExpression"), DefaultValue(""), WebCategory("Data")]
        public string FilterExpression
        {
            get
            {
                return this.GetView().FilterExpression;
            }
            set
            {
                this.GetView().FilterExpression = value;
            }
        }

        [WebCategory("Data"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("SqlDataSource_FilterParameters")]
        public ParameterCollection FilterParameters
        {
            get
            {
                return this.GetView().FilterParameters;
            }
        }

        [DefaultValue(""), WebCategory("Data"), WebSysDescription("SqlDataSource_InsertCommand")]
        public string InsertCommand
        {
            get
            {
                return this.GetView().InsertCommand;
            }
            set
            {
                this.GetView().InsertCommand = value;
            }
        }

        [WebCategory("Data"), DefaultValue(0), WebSysDescription("SqlDataSource_InsertCommandType")]
        public SqlDataSourceCommandType InsertCommandType
        {
            get
            {
                return this.GetView().InsertCommandType;
            }
            set
            {
                this.GetView().InsertCommandType = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), DefaultValue((string) null), WebCategory("Data"), WebSysDescription("SqlDataSource_InsertParameters")]
        public ParameterCollection InsertParameters
        {
            get
            {
                return this.GetView().InsertParameters;
            }
        }

        [DefaultValue("{0}"), WebCategory("Data"), WebSysDescription("DataSource_OldValuesParameterFormatString")]
        public string OldValuesParameterFormatString
        {
            get
            {
                return this.GetView().OldValuesParameterFormatString;
            }
            set
            {
                this.GetView().OldValuesParameterFormatString = value;
            }
        }

        [WebCategory("Data"), DefaultValue(""), TypeConverter("System.Web.UI.Design.WebControls.DataProviderNameConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebSysDescription("SqlDataSource_ProviderName")]
        public virtual string ProviderName
        {
            get
            {
                if (this._providerName != null)
                {
                    return this._providerName;
                }
                return string.Empty;
            }
            set
            {
                if (this.ProviderName != value)
                {
                    this._providerFactory = null;
                    this._providerName = value;
                    this.RaiseDataSourceChangedEvent(EventArgs.Empty);
                }
            }
        }

        [WebCategory("Data"), DefaultValue(""), WebSysDescription("SqlDataSource_SelectCommand")]
        public string SelectCommand
        {
            get
            {
                return this.GetView().SelectCommand;
            }
            set
            {
                this.GetView().SelectCommand = value;
            }
        }

        [DefaultValue(0), WebCategory("Data"), WebSysDescription("SqlDataSource_SelectCommandType")]
        public SqlDataSourceCommandType SelectCommandType
        {
            get
            {
                return this.GetView().SelectCommandType;
            }
            set
            {
                this.GetView().SelectCommandType = value;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), WebCategory("Data"), WebSysDescription("SqlDataSource_SelectParameters")]
        public ParameterCollection SelectParameters
        {
            get
            {
                return this.GetView().SelectParameters;
            }
        }

        [WebSysDescription("SqlDataSource_SortParameterName"), DefaultValue(""), WebCategory("Data")]
        public string SortParameterName
        {
            get
            {
                return this.GetView().SortParameterName;
            }
            set
            {
                this.GetView().SortParameterName = value;
            }
        }

        [WebSysDescription("SqlDataSourceCache_SqlCacheDependency"), DefaultValue(""), WebCategory("Cache")]
        public virtual string SqlCacheDependency
        {
            get
            {
                return this.SqlDataSourceCache.SqlCacheDependency;
            }
            set
            {
                this.SqlDataSourceCache.SqlCacheDependency = value;
            }
        }

        private System.Web.UI.SqlDataSourceCache SqlDataSourceCache
        {
            get
            {
                System.Web.UI.SqlDataSourceCache cache = this.Cache as System.Web.UI.SqlDataSourceCache;
                if (cache == null)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("SqlDataSource_SqlCacheDependencyNotSupported", new object[] { this.ID }));
                }
                return cache;
            }
        }

        [DefaultValue(""), WebSysDescription("SqlDataSource_UpdateCommand"), WebCategory("Data")]
        public string UpdateCommand
        {
            get
            {
                return this.GetView().UpdateCommand;
            }
            set
            {
                this.GetView().UpdateCommand = value;
            }
        }

        [WebSysDescription("SqlDataSource_UpdateCommandType"), DefaultValue(0), WebCategory("Data")]
        public SqlDataSourceCommandType UpdateCommandType
        {
            get
            {
                return this.GetView().UpdateCommandType;
            }
            set
            {
                this.GetView().UpdateCommandType = value;
            }
        }

        [WebCategory("Data"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("SqlDataSource_UpdateParameters")]
        public ParameterCollection UpdateParameters
        {
            get
            {
                return this.GetView().UpdateParameters;
            }
        }
    }
}

