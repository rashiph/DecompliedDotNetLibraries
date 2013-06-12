namespace System.Web.UI
{
    using System;
    using System.Collections.Specialized;
    using System.Web.Caching;

    internal sealed class FileDataSourceCache : DataSourceCache
    {
        private StringCollection _fileDependencies;

        protected override void SaveDataToCacheInternal(string key, object data, CacheDependency dependency)
        {
            string[] array = new string[this.FileDependencies.Count];
            this.FileDependencies.CopyTo(array, 0);
            CacheDependency dependency2 = new CacheDependency(0, array);
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
            base.SaveDataToCacheInternal(key, data, dependency);
        }

        public StringCollection FileDependencies
        {
            get
            {
                if (this._fileDependencies == null)
                {
                    this._fileDependencies = new StringCollection();
                }
                return this._fileDependencies;
            }
        }
    }
}

