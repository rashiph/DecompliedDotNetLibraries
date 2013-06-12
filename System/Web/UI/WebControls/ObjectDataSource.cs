namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.UI;

    [PersistChildren(false), ParseChildren(true), Designer("System.Web.UI.Design.WebControls.ObjectDataSourceDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebSysDescription("ObjectDataSource_Description"), WebSysDisplayName("ObjectDataSource_DisplayName"), ToolboxBitmap(typeof(ObjectDataSource)), DefaultEvent("Selecting"), DefaultProperty("TypeName")]
    public class ObjectDataSource : DataSourceControl
    {
        private SqlDataSourceCache _cache;
        private ObjectDataSourceView _view;
        private ICollection _viewNames;
        private const string DefaultViewName = "DefaultView";

        [WebCategory("Data"), WebSysDescription("DataSource_Deleted")]
        public event ObjectDataSourceStatusEventHandler Deleted
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
        public event ObjectDataSourceMethodEventHandler Deleting
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

        [WebSysDescription("DataSource_Filtering"), WebCategory("Data")]
        public event ObjectDataSourceFilteringEventHandler Filtering
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
        public event ObjectDataSourceStatusEventHandler Inserted
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
        public event ObjectDataSourceMethodEventHandler Inserting
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

        [WebSysDescription("ObjectDataSource_ObjectCreated"), WebCategory("Data")]
        public event ObjectDataSourceObjectEventHandler ObjectCreated
        {
            add
            {
                this.GetView().ObjectCreated += value;
            }
            remove
            {
                this.GetView().ObjectCreated -= value;
            }
        }

        [WebSysDescription("ObjectDataSource_ObjectCreating"), WebCategory("Data")]
        public event ObjectDataSourceObjectEventHandler ObjectCreating
        {
            add
            {
                this.GetView().ObjectCreating += value;
            }
            remove
            {
                this.GetView().ObjectCreating -= value;
            }
        }

        [WebCategory("Data"), WebSysDescription("ObjectDataSource_ObjectDisposing")]
        public event ObjectDataSourceDisposingEventHandler ObjectDisposing
        {
            add
            {
                this.GetView().ObjectDisposing += value;
            }
            remove
            {
                this.GetView().ObjectDisposing -= value;
            }
        }

        [WebCategory("Data"), WebSysDescription("ObjectDataSource_Selected")]
        public event ObjectDataSourceStatusEventHandler Selected
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

        [WebCategory("Data"), WebSysDescription("ObjectDataSource_Selecting")]
        public event ObjectDataSourceSelectingEventHandler Selecting
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

        [WebSysDescription("DataSource_Updated"), WebCategory("Data")]
        public event ObjectDataSourceStatusEventHandler Updated
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

        [WebSysDescription("DataSource_Updating"), WebCategory("Data")]
        public event ObjectDataSourceMethodEventHandler Updating
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

        public ObjectDataSource()
        {
        }

        public ObjectDataSource(string typeName, string selectMethod)
        {
            this.TypeName = typeName;
            this.SelectMethod = selectMethod;
        }

        internal string CreateCacheKey(int startRowIndex, int maximumRows)
        {
            StringBuilder builder = this.CreateRawCacheKey();
            builder.Append(':');
            builder.Append(startRowIndex.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(maximumRows.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        internal string CreateMasterCacheKey()
        {
            return this.CreateRawCacheKey().ToString();
        }

        private StringBuilder CreateRawCacheKey()
        {
            StringBuilder builder = new StringBuilder("u", 0x400);
            builder.Append(base.GetType().GetHashCode().ToString(CultureInfo.InvariantCulture));
            builder.Append(":");
            builder.Append(this.CacheDuration.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(((int) this.CacheExpirationPolicy).ToString(CultureInfo.InvariantCulture));
            builder.Append(":");
            builder.Append(this.SqlCacheDependency);
            builder.Append(":");
            builder.Append(this.TypeName);
            builder.Append(":");
            builder.Append(this.SelectMethod);
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

        private ObjectDataSourceView GetView()
        {
            if (this._view == null)
            {
                this._view = new ObjectDataSourceView(this, "DefaultView", this.Context);
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

        internal void SaveDataToCache(int startRowIndex, int maximumRows, object data)
        {
            string key = this.CreateCacheKey(startRowIndex, maximumRows);
            string str2 = this.CreateMasterCacheKey();
            if (this.Cache.LoadDataFromCache(str2) == null)
            {
                this.Cache.SaveDataToCache(str2, -1);
            }
            CacheDependency dependency = new CacheDependency(0, new string[0], new string[] { str2 });
            this.Cache.SaveDataToCache(key, data, dependency);
        }

        internal void SaveTotalRowCountToCache(int totalRowCount)
        {
            string key = this.CreateMasterCacheKey();
            this.Cache.SaveDataToCache(key, totalRowCount);
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

        public IEnumerable Select()
        {
            return this.GetView().Select(DataSourceSelectArguments.Empty);
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

        internal SqlDataSourceCache Cache
        {
            get
            {
                if (this._cache == null)
                {
                    this._cache = new SqlDataSourceCache();
                }
                return this._cache;
            }
        }

        [TypeConverter(typeof(DataSourceCacheDurationConverter)), DefaultValue(0), WebCategory("Cache"), WebSysDescription("DataSourceCache_Duration")]
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

        [DefaultValue(0), WebCategory("Cache"), WebSysDescription("DataSourceCache_ExpirationPolicy")]
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

        [WebCategory("Cache"), DefaultValue(""), WebSysDescription("DataSourceCache_KeyDependency")]
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

        [WebSysDescription("ObjectDataSource_ConflictDetection"), DefaultValue(0), WebCategory("Data")]
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

        [DefaultValue(false), WebSysDescription("ObjectDataSource_ConvertNullToDBNull"), WebCategory("Data")]
        public bool ConvertNullToDBNull
        {
            get
            {
                return this.GetView().ConvertNullToDBNull;
            }
            set
            {
                this.GetView().ConvertNullToDBNull = value;
            }
        }

        [WebSysDescription("ObjectDataSource_DataObjectTypeName"), DefaultValue(""), WebCategory("Data")]
        public string DataObjectTypeName
        {
            get
            {
                return this.GetView().DataObjectTypeName;
            }
            set
            {
                this.GetView().DataObjectTypeName = value;
            }
        }

        [DefaultValue(""), WebSysDescription("ObjectDataSource_DeleteMethod"), WebCategory("Data")]
        public string DeleteMethod
        {
            get
            {
                return this.GetView().DeleteMethod;
            }
            set
            {
                this.GetView().DeleteMethod = value;
            }
        }

        [MergableProperty(false), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Data"), PersistenceMode(PersistenceMode.InnerProperty), WebSysDescription("ObjectDataSource_DeleteParameters")]
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

        [DefaultValue(false), WebSysDescription("ObjectDataSource_EnablePaging"), WebCategory("Paging")]
        public bool EnablePaging
        {
            get
            {
                return this.GetView().EnablePaging;
            }
            set
            {
                this.GetView().EnablePaging = value;
            }
        }

        [WebSysDescription("ObjectDataSource_FilterExpression"), DefaultValue(""), WebCategory("Data")]
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

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), WebSysDescription("ObjectDataSource_FilterParameters"), WebCategory("Data")]
        public ParameterCollection FilterParameters
        {
            get
            {
                return this.GetView().FilterParameters;
            }
        }

        [WebSysDescription("ObjectDataSource_InsertMethod"), DefaultValue(""), WebCategory("Data")]
        public string InsertMethod
        {
            get
            {
                return this.GetView().InsertMethod;
            }
            set
            {
                this.GetView().InsertMethod = value;
            }
        }

        [MergableProperty(false), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Data"), WebSysDescription("ObjectDataSource_InsertParameters")]
        public ParameterCollection InsertParameters
        {
            get
            {
                return this.GetView().InsertParameters;
            }
        }

        [WebCategory("Paging"), DefaultValue("maximumRows"), WebSysDescription("ObjectDataSource_MaximumRowsParameterName")]
        public string MaximumRowsParameterName
        {
            get
            {
                return this.GetView().MaximumRowsParameterName;
            }
            set
            {
                this.GetView().MaximumRowsParameterName = value;
            }
        }

        [WebCategory("Data"), DefaultValue("{0}"), WebSysDescription("DataSource_OldValuesParameterFormatString")]
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

        [DefaultValue(""), WebCategory("Paging"), WebSysDescription("ObjectDataSource_SelectCountMethod")]
        public string SelectCountMethod
        {
            get
            {
                return this.GetView().SelectCountMethod;
            }
            set
            {
                this.GetView().SelectCountMethod = value;
            }
        }

        [DefaultValue(""), WebSysDescription("ObjectDataSource_SelectMethod"), WebCategory("Data")]
        public string SelectMethod
        {
            get
            {
                return this.GetView().SelectMethod;
            }
            set
            {
                this.GetView().SelectMethod = value;
            }
        }

        [WebSysDescription("ObjectDataSource_SelectParameters"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Data")]
        public ParameterCollection SelectParameters
        {
            get
            {
                return this.GetView().SelectParameters;
            }
        }

        [DefaultValue(""), WebSysDescription("ObjectDataSource_SortParameterName"), WebCategory("Data")]
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

        [DefaultValue(""), WebSysDescription("SqlDataSourceCache_SqlCacheDependency"), WebCategory("Cache")]
        public virtual string SqlCacheDependency
        {
            get
            {
                return this.Cache.SqlCacheDependency;
            }
            set
            {
                this.Cache.SqlCacheDependency = value;
            }
        }

        [WebSysDescription("ObjectDataSource_StartRowIndexParameterName"), DefaultValue("startRowIndex"), WebCategory("Paging")]
        public string StartRowIndexParameterName
        {
            get
            {
                return this.GetView().StartRowIndexParameterName;
            }
            set
            {
                this.GetView().StartRowIndexParameterName = value;
            }
        }

        [WebCategory("Data"), WebSysDescription("ObjectDataSource_TypeName"), DefaultValue("")]
        public string TypeName
        {
            get
            {
                return this.GetView().TypeName;
            }
            set
            {
                this.GetView().TypeName = value;
            }
        }

        [WebSysDescription("ObjectDataSource_UpdateMethod"), DefaultValue(""), WebCategory("Data")]
        public string UpdateMethod
        {
            get
            {
                return this.GetView().UpdateMethod;
            }
            set
            {
                this.GetView().UpdateMethod = value;
            }
        }

        [WebSysDescription("ObjectDataSource_UpdateParameters"), WebCategory("Data"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), PersistenceMode(PersistenceMode.InnerProperty)]
        public ParameterCollection UpdateParameters
        {
            get
            {
                return this.GetView().UpdateParameters;
            }
        }
    }
}

