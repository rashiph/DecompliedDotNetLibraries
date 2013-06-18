namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.Caching;

    internal class DataSourceCache : IStateManager
    {
        private bool _tracking;
        private StateBag _viewState;
        public const int Infinite = 0;

        public void Invalidate(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if (!this.Enabled)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("DataSourceCache_CacheMustBeEnabled"));
            }
            HttpRuntime.CacheInternal.Remove(key);
        }

        public object LoadDataFromCache(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if (!this.Enabled)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("DataSourceCache_CacheMustBeEnabled"));
            }
            return HttpRuntime.CacheInternal.Get(key);
        }

        protected virtual void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                ((IStateManager) this.ViewState).LoadViewState(savedState);
            }
        }

        public void SaveDataToCache(string key, object data)
        {
            this.SaveDataToCache(key, data, null);
        }

        public void SaveDataToCache(string key, object data, CacheDependency dependency)
        {
            this.SaveDataToCacheInternal(key, data, dependency);
        }

        protected virtual void SaveDataToCacheInternal(string key, object data, CacheDependency dependency)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            if (!this.Enabled)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("DataSourceCache_CacheMustBeEnabled"));
            }
            DateTime noAbsoluteExpiration = Cache.NoAbsoluteExpiration;
            TimeSpan noSlidingExpiration = Cache.NoSlidingExpiration;
            switch (this.ExpirationPolicy)
            {
                case DataSourceCacheExpiry.Absolute:
                    noAbsoluteExpiration = DateTime.UtcNow.AddSeconds((this.Duration == 0) ? ((double) 0x7fffffff) : ((double) this.Duration));
                    break;

                case DataSourceCacheExpiry.Sliding:
                    noSlidingExpiration = TimeSpan.FromSeconds((double) this.Duration);
                    break;
            }
            AggregateCacheDependency dependencies = new AggregateCacheDependency();
            string[] cachekeys = null;
            if (this.KeyDependency.Length > 0)
            {
                cachekeys = new string[] { this.KeyDependency };
                dependencies.Add(new CacheDependency[] { new CacheDependency(null, cachekeys) });
            }
            if (dependency != null)
            {
                dependencies.Add(new CacheDependency[] { dependency });
            }
            HttpRuntime.CacheInternal.UtcInsert(key, data, dependencies, noAbsoluteExpiration, noSlidingExpiration);
        }

        protected virtual object SaveViewState()
        {
            if (this._viewState == null)
            {
                return null;
            }
            return ((IStateManager) this._viewState).SaveViewState();
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

        protected void TrackViewState()
        {
            this._tracking = true;
            if (this._viewState != null)
            {
                this._viewState.TrackViewState();
            }
        }

        public virtual int Duration
        {
            get
            {
                object obj2 = this.ViewState["Duration"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("DataSourceCache_InvalidDuration"));
                }
                this.ViewState["Duration"] = value;
            }
        }

        public virtual bool Enabled
        {
            get
            {
                object obj2 = this.ViewState["Enabled"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["Enabled"] = value;
            }
        }

        public virtual DataSourceCacheExpiry ExpirationPolicy
        {
            get
            {
                object obj2 = this.ViewState["ExpirationPolicy"];
                if (obj2 != null)
                {
                    return (DataSourceCacheExpiry) obj2;
                }
                return DataSourceCacheExpiry.Absolute;
            }
            set
            {
                if ((value < DataSourceCacheExpiry.Absolute) || (value > DataSourceCacheExpiry.Sliding))
                {
                    throw new ArgumentOutOfRangeException(System.Web.SR.GetString("DataSourceCache_InvalidExpiryPolicy"));
                }
                this.ViewState["ExpirationPolicy"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("DataSourceCache_KeyDependency"), NotifyParentProperty(true)]
        public virtual string KeyDependency
        {
            get
            {
                object obj2 = this.ViewState["KeyDependency"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["KeyDependency"] = value;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this._tracking;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected StateBag ViewState
        {
            get
            {
                if (this._viewState == null)
                {
                    this._viewState = new StateBag();
                    if (this._tracking)
                    {
                        this._viewState.TrackViewState();
                    }
                }
                return this._viewState;
            }
        }
    }
}

