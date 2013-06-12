namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Management;
    using System.Web.Util;

    public sealed class Cache : IEnumerable
    {
        private CacheInternal _cacheInternal;
        public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
        public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;
        private static CacheItemRemovedCallback s_sentinelRemovedCallback = new CacheItemRemovedCallback(SentinelEntry.OnCacheItemRemovedCallback);

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public Cache()
        {
        }

        internal Cache(int dummy)
        {
        }

        public object Add(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            return this._cacheInternal.DoInsert(true, key, value, dependencies, utcAbsoluteExpiration, slidingExpiration, priority, onRemoveCallback, false);
        }

        public object Get(string key)
        {
            return this._cacheInternal.DoGet(true, key, CacheGetOptions.None);
        }

        internal object Get(string key, CacheGetOptions getOptions)
        {
            return this._cacheInternal.DoGet(true, key, getOptions);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return this._cacheInternal.GetEnumerator();
        }

        public void Insert(string key, object value)
        {
            this._cacheInternal.DoInsert(true, key, value, null, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null, true);
        }

        public void Insert(string key, object value, CacheDependency dependencies)
        {
            this._cacheInternal.DoInsert(true, key, value, dependencies, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.Normal, null, true);
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            this._cacheInternal.DoInsert(true, key, value, dependencies, utcAbsoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null, true);
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemUpdateCallback onUpdateCallback)
        {
            if (((dependencies == null) && (absoluteExpiration == NoAbsoluteExpiration)) && (slidingExpiration == NoSlidingExpiration))
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_Parameters_To_Insert"));
            }
            if (onUpdateCallback == null)
            {
                throw new ArgumentNullException("onUpdateCallback");
            }
            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            this._cacheInternal.DoInsert(true, key, value, null, NoAbsoluteExpiration, NoSlidingExpiration, CacheItemPriority.NotRemovable, null, true);
            string[] cachekeys = new string[] { key };
            CacheDependency expensiveObjectDependency = new CacheDependency(null, cachekeys);
            if (dependencies == null)
            {
                dependencies = expensiveObjectDependency;
            }
            else
            {
                AggregateCacheDependency dependency2 = new AggregateCacheDependency();
                dependency2.Add(new CacheDependency[] { dependencies, expensiveObjectDependency });
                dependencies = dependency2;
            }
            this._cacheInternal.DoInsert(false, "w" + key, new SentinelEntry(key, expensiveObjectDependency, onUpdateCallback), dependencies, utcAbsoluteExpiration, slidingExpiration, CacheItemPriority.NotRemovable, s_sentinelRemovedCallback, true);
        }

        public void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            DateTime utcAbsoluteExpiration = DateTimeUtil.ConvertToUniversalTime(absoluteExpiration);
            this._cacheInternal.DoInsert(true, key, value, dependencies, utcAbsoluteExpiration, slidingExpiration, priority, onRemoveCallback, true);
        }

        public object Remove(string key)
        {
            CacheKey cacheKey = new CacheKey(key, true);
            return this._cacheInternal.DoRemove(cacheKey, CacheItemRemovedReason.Removed);
        }

        internal void SetCacheInternal(CacheInternal cacheInternal)
        {
            this._cacheInternal = cacheInternal;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this._cacheInternal).GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this._cacheInternal.PublicCount;
            }
        }

        public long EffectivePercentagePhysicalMemoryLimit
        {
            get
            {
                return this._cacheInternal.EffectivePercentagePhysicalMemoryLimit;
            }
        }

        public long EffectivePrivateBytesLimit
        {
            get
            {
                return this._cacheInternal.EffectivePrivateBytesLimit;
            }
        }

        public object this[string key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                this.Insert(key, value);
            }
        }

        private class SentinelEntry
        {
            private System.Web.Caching.CacheItemUpdateCallback _cacheItemUpdateCallback;
            private CacheDependency _expensiveObjectDependency;
            private string _key;

            public SentinelEntry(string key, CacheDependency expensiveObjectDependency, System.Web.Caching.CacheItemUpdateCallback callback)
            {
                this._key = key;
                this._expensiveObjectDependency = expensiveObjectDependency;
                this._cacheItemUpdateCallback = callback;
            }

            public static void OnCacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason)
            {
                CacheItemUpdateReason expired;
                System.Web.Caching.CacheItemUpdateCallback callback;
                Cache.SentinelEntry entry = value as Cache.SentinelEntry;
                switch (reason)
                {
                    case CacheItemRemovedReason.Expired:
                        expired = CacheItemUpdateReason.Expired;
                        goto Label_0034;

                    case CacheItemRemovedReason.Underused:
                        break;

                    case CacheItemRemovedReason.DependencyChanged:
                        expired = CacheItemUpdateReason.DependencyChanged;
                        if (!entry.ExpensiveObjectDependency.HasChanged)
                        {
                            goto Label_0034;
                        }
                        break;

                    default:
                        return;
                }
                return;
            Label_0034:
                callback = entry.CacheItemUpdateCallback;
                try
                {
                    CacheDependency dependency;
                    DateTime time;
                    TimeSpan span;
                    object obj2;
                    callback(entry.Key, expired, out obj2, out dependency, out time, out span);
                    if ((obj2 != null) && ((dependency == null) || !dependency.HasChanged))
                    {
                        HttpRuntime.Cache.Insert(entry.Key, obj2, dependency, time, span, entry.CacheItemUpdateCallback);
                    }
                    else
                    {
                        HttpRuntime.Cache.Remove(entry.Key);
                    }
                }
                catch (Exception exception)
                {
                    HttpRuntime.Cache.Remove(entry.Key);
                    try
                    {
                        WebBaseEvent.RaiseRuntimeError(exception, value);
                    }
                    catch
                    {
                    }
                }
            }

            public System.Web.Caching.CacheItemUpdateCallback CacheItemUpdateCallback
            {
                get
                {
                    return this._cacheItemUpdateCallback;
                }
            }

            public CacheDependency ExpensiveObjectDependency
            {
                get
                {
                    return this._expensiveObjectDependency;
                }
            }

            public string Key
            {
                get
                {
                    return this._key;
                }
            }
        }
    }
}

