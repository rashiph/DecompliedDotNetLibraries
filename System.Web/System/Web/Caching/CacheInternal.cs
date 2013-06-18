namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web.Configuration;
    using System.Web.Util;

    internal abstract class CacheInternal : IEnumerable, IDisposable
    {
        protected CacheCommon _cacheCommon;
        private int _disposed;
        internal const string PrefixAdRotator = "n";
        internal const string PrefixAspCompatThreading = "s";
        internal const string PrefixAssemblyPath = "y";
        internal const string PrefixBrowserCapsHash = "z";
        internal const string PrefixDataSourceControl = "u";
        internal const string PrefixFileSecurity = "h";
        internal const string PrefixFIRST = "A";
        internal const string PrefixHttpCapabilities = "e";
        internal const string PrefixHttpSys = "g";
        internal const string PrefixInProcSessionState = "j";
        internal const string PrefixLAST = "z";
        internal const string PrefixLoadTransform = "r";
        internal const string PrefixLoadXml = "q";
        internal const string PrefixLoadXPath = "p";
        internal const string PrefixMapPath = "f";
        internal const string PrefixMapPathVPPDir = "Bd";
        internal const string PrefixMapPathVPPFile = "Bf";
        internal const string PrefixMemoryBuildResult = "c";
        internal const string PrefixOutputCache = "a";
        internal const string PrefixPartialCachingControl = "l";
        internal const string PrefixPathData = "d";
        internal const string PrefixResourceProvider = "A";
        internal const string PrefixSqlCacheDependency = "b";
        internal const string PrefixStateApplication = "k";
        internal const string PrefixValidationSentinel = "w";
        internal const string PrefixWebEventResource = "x";
        internal const string PrefixWebServiceDataSource = "o";
        internal const string UNUSED = "m";

        protected CacheInternal(CacheCommon cacheCommon)
        {
            this._cacheCommon = cacheCommon;
        }

        internal static CacheInternal Create()
        {
            CacheInternal internal2;
            CacheCommon cacheCommon = new CacheCommon();
            int numSingleCaches = 0;
            uint numProcessCPUs = (uint) SystemInfo.GetNumProcessCPUs();
            numSingleCaches = 1;
            numProcessCPUs--;
            while (numProcessCPUs > 0)
            {
                numSingleCaches = numSingleCaches << 1;
                numProcessCPUs = numProcessCPUs >> 1;
            }
            if (numSingleCaches == 1)
            {
                internal2 = new CacheSingle(cacheCommon, null, 0);
            }
            else
            {
                internal2 = new CacheMultiple(cacheCommon, numSingleCaches);
            }
            cacheCommon.SetCacheInternal(internal2);
            cacheCommon.ResetFromConfigSettings();
            return internal2;
        }

        internal abstract IDictionaryEnumerator CreateEnumerator();
        public void Dispose()
        {
            this._disposed = 1;
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._cacheCommon.Dispose(disposing);
        }

        internal object DoGet(bool isPublic, string key, CacheGetOptions getOptions)
        {
            object obj2;
            CacheKey cacheKey = new CacheKey(key, isPublic);
            CacheEntry entry = this.UpdateCache(cacheKey, null, false, CacheItemRemovedReason.Removed, out obj2);
            if (entry == null)
            {
                return null;
            }
            if ((getOptions & CacheGetOptions.ReturnCacheEntry) != CacheGetOptions.None)
            {
                return entry;
            }
            return entry.Value;
        }

        internal object DoInsert(bool isPublic, string key, object value, CacheDependency dependencies, DateTime utcAbsoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback, bool replace)
        {
            using (dependencies)
            {
                object obj2;
                CacheEntry cacheKey = new CacheEntry(key, value, dependencies, onRemoveCallback, utcAbsoluteExpiration, slidingExpiration, priority, isPublic);
                cacheKey = this.UpdateCache(cacheKey, cacheKey, replace, CacheItemRemovedReason.Removed, out obj2);
                if (cacheKey != null)
                {
                    return cacheKey.Value;
                }
                return null;
            }
        }

        internal object DoRemove(CacheKey cacheKey, CacheItemRemovedReason reason)
        {
            object obj2;
            this.UpdateCache(cacheKey, null, true, reason, out obj2);
            return obj2;
        }

        internal abstract void EnableExpirationTimer(bool enable);
        internal object Get(string key)
        {
            return this.DoGet(false, key, CacheGetOptions.None);
        }

        internal object Get(string key, CacheGetOptions getOptions)
        {
            return this.DoGet(false, key, getOptions);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return this.CreateEnumerator();
        }

        internal void ReadCacheInternalConfig(CacheSection cacheSection)
        {
            this._cacheCommon.ReadCacheInternalConfig(cacheSection);
        }

        internal object Remove(string key)
        {
            CacheKey cacheKey = new CacheKey(key, false);
            return this.DoRemove(cacheKey, CacheItemRemovedReason.Removed);
        }

        internal object Remove(CacheKey cacheKey, CacheItemRemovedReason reason)
        {
            return this.DoRemove(cacheKey, reason);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.CreateEnumerator();
        }

        internal long TrimCache(int percent)
        {
            return this._cacheCommon.CacheManagerThread(percent);
        }

        internal abstract long TrimIfNecessary(int percent);
        internal abstract CacheEntry UpdateCache(CacheKey cacheKey, CacheEntry newEntry, bool replace, CacheItemRemovedReason removedReason, out object valueOld);
        internal object UtcAdd(string key, object value, CacheDependency dependencies, DateTime utcAbsoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            return this.DoInsert(false, key, value, dependencies, utcAbsoluteExpiration, slidingExpiration, priority, onRemoveCallback, false);
        }

        internal void UtcInsert(string key, object value)
        {
            this.DoInsert(false, key, value, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null, true);
        }

        internal void UtcInsert(string key, object value, CacheDependency dependencies)
        {
            this.DoInsert(false, key, value, dependencies, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null, true);
        }

        internal void UtcInsert(string key, object value, CacheDependency dependencies, DateTime utcAbsoluteExpiration, TimeSpan slidingExpiration)
        {
            this.DoInsert(false, key, value, dependencies, utcAbsoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null, true);
        }

        internal void UtcInsert(string key, object value, CacheDependency dependencies, DateTime utcAbsoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            this.DoInsert(false, key, value, dependencies, utcAbsoluteExpiration, slidingExpiration, priority, onRemoveCallback, true);
        }

        internal Cache CachePublic
        {
            get
            {
                return this._cacheCommon._cachePublic;
            }
        }

        internal long EffectivePercentagePhysicalMemoryLimit
        {
            get
            {
                return this._cacheCommon._cacheMemoryStats.TotalMemoryPressure.MemoryLimit;
            }
        }

        internal long EffectivePrivateBytesLimit
        {
            get
            {
                return this._cacheCommon._cacheMemoryStats.CacheSizePressure.MemoryLimit;
            }
        }

        internal bool IsDisposed
        {
            get
            {
                return (this._disposed == 1);
            }
        }

        internal object this[string key]
        {
            get
            {
                return this.Get(key);
            }
        }

        internal abstract int PublicCount { get; }

        internal abstract long TotalCount { get; }
    }
}

