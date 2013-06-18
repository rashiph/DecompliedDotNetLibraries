namespace System.Runtime.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime.Caching.Resources;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public class MemoryCache : ObjectCache, IEnumerable, IDisposable
    {
        private bool _configLess;
        private int _disposed;
        private string _name;
        private EventHandler _onAppDomainUnload;
        private UnhandledExceptionEventHandler _onUnhandledException;
        private PerfCounters _perfCounters;
        private MemoryCacheStatistics _stats;
        private int _storeCount;
        private int _storeMask;
        private MemoryCacheStore[] _stores;
        private const System.Runtime.Caching.DefaultCacheCapabilities CAPABILITIES = (System.Runtime.Caching.DefaultCacheCapabilities.CacheEntryRemovedCallback | System.Runtime.Caching.DefaultCacheCapabilities.CacheEntryUpdateCallback | System.Runtime.Caching.DefaultCacheCapabilities.SlidingExpirations | System.Runtime.Caching.DefaultCacheCapabilities.AbsoluteExpirations | System.Runtime.Caching.DefaultCacheCapabilities.CacheEntryChangeMonitors | System.Runtime.Caching.DefaultCacheCapabilities.InMemoryProvider);
        private static readonly TimeSpan OneYear = new TimeSpan(0x16d, 0, 0, 0);
        private static MemoryCache s_defaultCache;
        private static object s_initLock = new object();
        private static CacheEntryRemovedCallback s_sentinelRemovedCallback = new CacheEntryRemovedCallback(SentinelEntry.OnCacheEntryRemovedCallback);

        private MemoryCache()
        {
            this._name = "Default";
            this.Init(null);
        }

        public MemoryCache(string name, NameValueCollection config = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException(R.Empty_string_invalid, "name");
            }
            if (string.Equals(name, "default", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(R.Default_is_reserved, "name");
            }
            this._name = name;
            this.Init(config);
        }

        internal MemoryCache(string name, NameValueCollection config, bool configLess)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException(R.Empty_string_invalid, "name");
            }
            if (string.Equals(name, "default", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(R.Default_is_reserved, "name");
            }
            this._name = name;
            this._configLess = configLess;
            this.Init(config);
        }

        public override CacheItem AddOrGetExisting(CacheItem item, CacheItemPolicy policy)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            return new CacheItem(item.Key, this.AddOrGetExistingInternal(item.Key, item.Value, policy));
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            CacheItemPolicy policy = new CacheItemPolicy {
                AbsoluteExpiration = absoluteExpiration
            };
            return this.AddOrGetExistingInternal(key, value, policy);
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            return this.AddOrGetExistingInternal(key, value, policy);
        }

        private object AddOrGetExistingInternal(string key, object value, CacheItemPolicy policy)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            DateTimeOffset infiniteAbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
            TimeSpan noSlidingExpiration = ObjectCache.NoSlidingExpiration;
            CacheItemPriority priority = CacheItemPriority.Default;
            Collection<ChangeMonitor> dependencies = null;
            CacheEntryRemovedCallback removedCallback = null;
            if (policy != null)
            {
                this.ValidatePolicy(policy);
                if (policy.UpdateCallback != null)
                {
                    throw new ArgumentException(R.Update_callback_must_be_null, "policy");
                }
                infiniteAbsoluteExpiration = policy.AbsoluteExpiration;
                noSlidingExpiration = policy.SlidingExpiration;
                priority = policy.Priority;
                dependencies = policy.ChangeMonitors;
                removedCallback = policy.RemovedCallback;
            }
            if (this.IsDisposed)
            {
                if (dependencies != null)
                {
                    foreach (ChangeMonitor monitor in dependencies)
                    {
                        if (monitor != null)
                        {
                            monitor.Dispose();
                        }
                    }
                }
                return null;
            }
            MemoryCacheKey key2 = new MemoryCacheKey(key);
            MemoryCacheEntry entry = this.GetStore(key2).AddOrGetExisting(key2, new MemoryCacheEntry(key, value, infiniteAbsoluteExpiration, noSlidingExpiration, priority, dependencies, removedCallback, this));
            if (entry == null)
            {
                return null;
            }
            return entry.Value;
        }

        public override bool Contains(string key, string regionName = null)
        {
            return (this.GetInternal(key, regionName) != null);
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            List<string> list = new List<string>(keys);
            if (list.Count == 0)
            {
                throw new ArgumentException(RH.Format(R.Empty_collection, new object[] { "keys" }));
            }
            using (List<string>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == null)
                    {
                        throw new ArgumentException(RH.Format(R.Collection_contains_null_element, new object[] { "keys" }));
                    }
                }
            }
            return new MemoryCacheEntryChangeMonitor(list.AsReadOnly(), regionName, this);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._disposed, 1) == 0)
            {
                this.DisposeSafeCritical();
                if (this._stats != null)
                {
                    this._stats.Dispose();
                }
                if (this._stores != null)
                {
                    foreach (MemoryCacheStore store in this._stores)
                    {
                        if (store != null)
                        {
                            store.Dispose();
                        }
                    }
                }
                if (this._perfCounters != null)
                {
                    this._perfCounters.Dispose();
                }
                GC.SuppressFinalize(this);
            }
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void DisposeSafeCritical()
        {
            AppDomain domain = Thread.GetDomain();
            if (this._onAppDomainUnload != null)
            {
                domain.DomainUnload -= this._onAppDomainUnload;
            }
            if (this._onUnhandledException != null)
            {
                domain.UnhandledException -= this._onUnhandledException;
            }
        }

        public override object Get(string key, string regionName = null)
        {
            return this.GetInternal(key, regionName);
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            object obj2 = this.GetInternal(key, regionName);
            if (obj2 == null)
            {
                return null;
            }
            return new CacheItem(key, obj2);
        }

        public override long GetCount(string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            long num = 0L;
            if (!this.IsDisposed)
            {
                foreach (MemoryCacheStore store in this._stores)
                {
                    num += store.Count;
                }
            }
            return num;
        }

        internal MemoryCacheEntry GetEntry(string key)
        {
            if (this.IsDisposed)
            {
                return null;
            }
            MemoryCacheKey key2 = new MemoryCacheKey(key);
            return this.GetStore(key2).Get(key2);
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            Dictionary<string, object> h = new Dictionary<string, object>();
            if (!this.IsDisposed)
            {
                foreach (MemoryCacheStore store in this._stores)
                {
                    store.CopyTo(h);
                }
            }
            return h.GetEnumerator();
        }

        private object GetInternal(string key, string regionName)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            MemoryCacheEntry entry = this.GetEntry(key);
            if (entry == null)
            {
                return null;
            }
            return entry.Value;
        }

        internal MemoryCacheStore GetStore(MemoryCacheKey cacheKey)
        {
            int index = Math.Abs(cacheKey.Hash) & this._storeMask;
            return this._stores[index];
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            Dictionary<string, object> dictionary = null;
            if (!this.IsDisposed)
            {
                foreach (string str in keys)
                {
                    if (str == null)
                    {
                        throw new ArgumentException(RH.Format(R.Collection_contains_null_element, new object[] { "keys" }));
                    }
                    object obj2 = this.GetInternal(str, null);
                    if (obj2 != null)
                    {
                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, object>();
                        }
                        dictionary[str] = obj2;
                    }
                }
            }
            return dictionary;
        }

        private void Init(NameValueCollection config)
        {
            this._storeCount = Environment.ProcessorCount;
            this._storeMask = this._storeCount - 1;
            this._stores = new MemoryCacheStore[this._storeCount];
            this.InitDisposableMembers(config);
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void InitDisposableMembers(NameValueCollection config)
        {
            bool flag = true;
            try
            {
                try
                {
                    this._perfCounters = new PerfCounters(this._name);
                }
                catch
                {
                }
                for (int i = 0; i < this._stores.Length; i++)
                {
                    this._stores[i] = new MemoryCacheStore(this, this._perfCounters);
                }
                this._stats = new MemoryCacheStatistics(this, config);
                AppDomain domain = Thread.GetDomain();
                EventHandler handler = new EventHandler(this.OnAppDomainUnload);
                domain.DomainUnload += handler;
                this._onAppDomainUnload = handler;
                UnhandledExceptionEventHandler handler2 = new UnhandledExceptionEventHandler(this.OnUnhandledException);
                domain.UnhandledException += handler2;
                this._onUnhandledException = handler2;
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    this.Dispose();
                }
            }
        }

        private void OnAppDomainUnload(object unusedObject, EventArgs unusedEventArgs)
        {
            this.Dispose();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
        {
            if (eventArgs.IsTerminating)
            {
                this.Dispose();
            }
        }

        public override object Remove(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (this.IsDisposed)
            {
                return null;
            }
            MemoryCacheEntry entry = this.RemoveEntry(key, null, CacheEntryRemovedReason.Removed);
            if (entry == null)
            {
                return null;
            }
            return entry.Value;
        }

        internal MemoryCacheEntry RemoveEntry(string key, MemoryCacheEntry entry, CacheEntryRemovedReason reason)
        {
            MemoryCacheKey key2 = new MemoryCacheKey(key);
            return this.GetStore(key2).Remove(key2, entry, reason);
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            this.Set(item.Key, item.Value, policy, null);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            CacheItemPolicy policy = new CacheItemPolicy {
                AbsoluteExpiration = absoluteExpiration
            };
            this.Set(key, value, policy, null);
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException(R.RegionName_not_supported);
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            DateTimeOffset infiniteAbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
            TimeSpan noSlidingExpiration = ObjectCache.NoSlidingExpiration;
            CacheItemPriority priority = CacheItemPriority.Default;
            Collection<ChangeMonitor> dependencies = null;
            CacheEntryRemovedCallback removedCallback = null;
            if (policy != null)
            {
                this.ValidatePolicy(policy);
                if (policy.UpdateCallback != null)
                {
                    this.Set(key, value, policy.ChangeMonitors, policy.AbsoluteExpiration, policy.SlidingExpiration, policy.UpdateCallback);
                    return;
                }
                infiniteAbsoluteExpiration = policy.AbsoluteExpiration;
                noSlidingExpiration = policy.SlidingExpiration;
                priority = policy.Priority;
                dependencies = policy.ChangeMonitors;
                removedCallback = policy.RemovedCallback;
            }
            if (this.IsDisposed)
            {
                if (dependencies != null)
                {
                    foreach (ChangeMonitor monitor in dependencies)
                    {
                        if (monitor != null)
                        {
                            monitor.Dispose();
                        }
                    }
                }
            }
            else
            {
                MemoryCacheKey key2 = new MemoryCacheKey(key);
                this.GetStore(key2).Set(key2, new MemoryCacheEntry(key, value, infiniteAbsoluteExpiration, noSlidingExpiration, priority, dependencies, removedCallback, this));
            }
        }

        internal void Set(string key, object value, Collection<ChangeMonitor> changeMonitors, DateTimeOffset absoluteExpiration, TimeSpan slidingExpiration, CacheEntryUpdateCallback onUpdateCallback)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (((changeMonitors == null) && (absoluteExpiration == ObjectCache.InfiniteAbsoluteExpiration)) && (slidingExpiration == ObjectCache.NoSlidingExpiration))
            {
                throw new ArgumentException(R.Invalid_argument_combination);
            }
            if (onUpdateCallback == null)
            {
                throw new ArgumentNullException("onUpdateCallback");
            }
            if (this.IsDisposed)
            {
                if (changeMonitors != null)
                {
                    foreach (ChangeMonitor monitor in changeMonitors)
                    {
                        if (monitor != null)
                        {
                            monitor.Dispose();
                        }
                    }
                }
            }
            else
            {
                MemoryCacheKey key2 = new MemoryCacheKey(key);
                this.GetStore(key2).Set(key2, new MemoryCacheEntry(key, value, ObjectCache.InfiniteAbsoluteExpiration, ObjectCache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null, null, this));
                string[] keys = new string[] { key };
                ChangeMonitor item = this.CreateCacheEntryChangeMonitor(keys, null);
                if (changeMonitors == null)
                {
                    changeMonitors = new Collection<ChangeMonitor>();
                }
                changeMonitors.Add(item);
                key2 = new MemoryCacheKey("OnUpdateSentinel" + key);
                this.GetStore(key2).Set(key2, new MemoryCacheEntry(key2.Key, new SentinelEntry(key, item, onUpdateCallback), absoluteExpiration, slidingExpiration, CacheItemPriority.NotRemovable, changeMonitors, s_sentinelRemovedCallback, this));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Hashtable h = new Hashtable();
            if (!this.IsDisposed)
            {
                foreach (MemoryCacheStore store in this._stores)
                {
                    store.CopyTo(h);
                }
            }
            return h.GetEnumerator();
        }

        public long Trim(int percent)
        {
            long num = 0L;
            if (this._disposed == 0)
            {
                foreach (MemoryCacheStore store in this._stores)
                {
                    num += store.TrimInternal(percent);
                }
            }
            return num;
        }

        internal void UpdateConfig(NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (!this.IsDisposed)
            {
                this._stats.UpdateConfig(config);
            }
        }

        private void ValidatePolicy(CacheItemPolicy policy)
        {
            if ((policy.AbsoluteExpiration != ObjectCache.InfiniteAbsoluteExpiration) && (policy.SlidingExpiration != ObjectCache.NoSlidingExpiration))
            {
                throw new ArgumentException(R.Invalid_expiration_combination, "policy");
            }
            if ((policy.SlidingExpiration < ObjectCache.NoSlidingExpiration) || (OneYear < policy.SlidingExpiration))
            {
                throw new ArgumentOutOfRangeException("policy", RH.Format(R.Argument_out_of_range, new object[] { "SlidingExpiration", ObjectCache.NoSlidingExpiration, OneYear }));
            }
            if ((policy.RemovedCallback != null) && (policy.UpdateCallback != null))
            {
                throw new ArgumentException(R.Invalid_callback_combination, "policy");
            }
            if ((policy.Priority != CacheItemPriority.Default) && (policy.Priority != CacheItemPriority.NotRemovable))
            {
                throw new ArgumentOutOfRangeException("policy", RH.Format(R.Argument_out_of_range, new object[] { "Priority", CacheItemPriority.Default, CacheItemPriority.NotRemovable }));
            }
        }

        public long CacheMemoryLimit
        {
            get
            {
                return this._stats.CacheMemoryLimit;
            }
        }

        internal bool ConfigLess
        {
            get
            {
                return this._configLess;
            }
        }

        public static MemoryCache Default
        {
            get
            {
                if (s_defaultCache == null)
                {
                    lock (s_initLock)
                    {
                        if (s_defaultCache == null)
                        {
                            s_defaultCache = new MemoryCache();
                        }
                    }
                }
                return s_defaultCache;
            }
        }

        public override System.Runtime.Caching.DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get
            {
                return (System.Runtime.Caching.DefaultCacheCapabilities.CacheEntryRemovedCallback | System.Runtime.Caching.DefaultCacheCapabilities.CacheEntryUpdateCallback | System.Runtime.Caching.DefaultCacheCapabilities.SlidingExpirations | System.Runtime.Caching.DefaultCacheCapabilities.AbsoluteExpirations | System.Runtime.Caching.DefaultCacheCapabilities.CacheEntryChangeMonitors | System.Runtime.Caching.DefaultCacheCapabilities.InMemoryProvider);
            }
        }

        private bool IsDisposed
        {
            get
            {
                return (this._disposed == 1);
            }
        }

        public override object this[string key]
        {
            get
            {
                return this.GetInternal(key, null);
            }
            set
            {
                this.Set(key, value, ObjectCache.InfiniteAbsoluteExpiration, null);
            }
        }

        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        public long PhysicalMemoryLimit
        {
            get
            {
                return this._stats.PhysicalMemoryLimit;
            }
        }

        public TimeSpan PollingInterval
        {
            get
            {
                return this._stats.PollingInterval;
            }
        }

        private class SentinelEntry
        {
            private ChangeMonitor _expensiveObjectDependency;
            private string _key;
            private System.Runtime.Caching.CacheEntryUpdateCallback _updateCallback;

            internal SentinelEntry(string key, ChangeMonitor expensiveObjectDependency, System.Runtime.Caching.CacheEntryUpdateCallback callback)
            {
                this._key = key;
                this._expensiveObjectDependency = expensiveObjectDependency;
                this._updateCallback = callback;
            }

            private static bool IsPolicyValid(CacheItemPolicy policy)
            {
                if (policy != null)
                {
                    bool flag = false;
                    Collection<ChangeMonitor> changeMonitors = policy.ChangeMonitors;
                    if (changeMonitors != null)
                    {
                        foreach (ChangeMonitor monitor in changeMonitors)
                        {
                            if ((monitor != null) && monitor.HasChanged)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag && (policy.UpdateCallback != null))
                    {
                        return true;
                    }
                    if (flag)
                    {
                        foreach (ChangeMonitor monitor2 in changeMonitors)
                        {
                            if (monitor2 != null)
                            {
                                monitor2.Dispose();
                            }
                        }
                    }
                }
                return false;
            }

            internal static void OnCacheEntryRemovedCallback(CacheEntryRemovedArguments arguments)
            {
                MemoryCache source = arguments.Source as MemoryCache;
                MemoryCache.SentinelEntry entry = arguments.CacheItem.Value as MemoryCache.SentinelEntry;
                CacheEntryRemovedReason removedReason = arguments.RemovedReason;
                switch (removedReason)
                {
                    case CacheEntryRemovedReason.Expired:
                        goto Label_004B;

                    case CacheEntryRemovedReason.Evicted:
                        break;

                    case CacheEntryRemovedReason.ChangeMonitorChanged:
                        if (!entry.ExpensiveObjectDependency.HasChanged)
                        {
                            goto Label_004B;
                        }
                        break;

                    default:
                        return;
                }
                return;
            Label_004B:
                try
                {
                    CacheEntryUpdateArguments arguments2 = new CacheEntryUpdateArguments(source, removedReason, entry.Key, null);
                    entry.CacheEntryUpdateCallback(arguments2);
                    object obj2 = (arguments2.UpdatedCacheItem != null) ? arguments2.UpdatedCacheItem.Value : null;
                    CacheItemPolicy updatedCacheItemPolicy = arguments2.UpdatedCacheItemPolicy;
                    if ((obj2 != null) && IsPolicyValid(updatedCacheItemPolicy))
                    {
                        source.Set(entry.Key, obj2, updatedCacheItemPolicy, null);
                    }
                    else
                    {
                        source.Remove(entry.Key, null);
                    }
                }
                catch
                {
                    source.Remove(entry.Key, null);
                }
            }

            internal System.Runtime.Caching.CacheEntryUpdateCallback CacheEntryUpdateCallback
            {
                get
                {
                    return this._updateCallback;
                }
            }

            internal ChangeMonitor ExpensiveObjectDependency
            {
                get
                {
                    return this._expensiveObjectDependency;
                }
            }

            internal string Key
            {
                get
                {
                    return this._key;
                }
            }
        }
    }
}

