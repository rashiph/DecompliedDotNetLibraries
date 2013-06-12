namespace System.Web.UI
{
    using System;
    using System.Web.Caching;

    internal sealed class SqlDataSourceCache : DataSourceCache
    {
        internal const string Sql9CacheDependencyDirective = "CommandNotification";

        protected override void SaveDataToCacheInternal(string key, object data, CacheDependency dependency)
        {
            string sqlCacheDependency = this.SqlCacheDependency;
            if ((sqlCacheDependency.Length > 0) && !string.Equals(sqlCacheDependency, "CommandNotification", StringComparison.OrdinalIgnoreCase))
            {
                CacheDependency dependency2 = System.Web.Caching.SqlCacheDependency.CreateOutputCacheDependency(sqlCacheDependency);
                if (dependency != null)
                {
                    AggregateCacheDependency dependency3 = new AggregateCacheDependency();
                    dependency3.Add(new CacheDependency[] { dependency2, dependency });
                    dependency = dependency3;
                }
                else
                {
                    dependency = dependency2;
                }
            }
            base.SaveDataToCacheInternal(key, data, dependency);
        }

        public string SqlCacheDependency
        {
            get
            {
                object obj2 = base.ViewState["SqlCacheDependency"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                base.ViewState["SqlCacheDependency"] = value;
            }
        }
    }
}

