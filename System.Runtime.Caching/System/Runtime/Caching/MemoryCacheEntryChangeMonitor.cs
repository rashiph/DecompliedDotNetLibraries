namespace System.Runtime.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Text;

    internal sealed class MemoryCacheEntryChangeMonitor : CacheEntryChangeMonitor
    {
        private List<MemoryCacheEntry> _dependencies;
        private ReadOnlyCollection<string> _keys;
        private DateTimeOffset _lastModified;
        private string _regionName;
        private string _uniqueId;
        private static readonly DateTime DATETIME_MINVALUE_UTC = new DateTime(0L, DateTimeKind.Utc);
        private const int MAX_CHAR_COUNT_OF_LONG_CONVERTED_TO_HEXADECIMAL_STRING = 0x10;

        private MemoryCacheEntryChangeMonitor()
        {
        }

        internal MemoryCacheEntryChangeMonitor(ReadOnlyCollection<string> keys, string regionName, MemoryCache cache)
        {
            this._keys = keys;
            this._regionName = regionName;
            this.InitDisposableMembers(cache);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this._dependencies != null))
            {
                foreach (MemoryCacheEntry entry in this._dependencies)
                {
                    if (entry != null)
                    {
                        entry.RemoveDependent(this);
                    }
                }
            }
        }

        private void InitDisposableMembers(MemoryCache cache)
        {
            bool flag = true;
            try
            {
                bool hasChanged = false;
                string str = null;
                this._dependencies = new List<MemoryCacheEntry>(this._keys.Count);
                if (this._keys.Count == 1)
                {
                    string key = this._keys[0];
                    MemoryCacheEntry entry = cache.GetEntry(key);
                    DateTime utcCreated = DATETIME_MINVALUE_UTC;
                    this.StartMonitoring(cache, entry, ref hasChanged, ref utcCreated);
                    str = key + utcCreated.Ticks.ToString("X", CultureInfo.InvariantCulture);
                    this._lastModified = utcCreated;
                }
                else
                {
                    int capacity = 0;
                    foreach (string str3 in this._keys)
                    {
                        capacity += str3.Length + 0x10;
                    }
                    StringBuilder builder = new StringBuilder(capacity);
                    foreach (string str4 in this._keys)
                    {
                        MemoryCacheEntry entry2 = cache.GetEntry(str4);
                        DateTime time2 = DATETIME_MINVALUE_UTC;
                        this.StartMonitoring(cache, entry2, ref hasChanged, ref time2);
                        builder.Append(str4);
                        builder.Append(time2.Ticks.ToString("X", CultureInfo.InvariantCulture));
                        if (time2 > this._lastModified)
                        {
                            this._lastModified = time2;
                        }
                    }
                    str = builder.ToString();
                }
                this._uniqueId = str;
                if (hasChanged)
                {
                    base.OnChanged(null);
                }
                flag = false;
            }
            finally
            {
                base.InitializationComplete();
                if (flag)
                {
                    base.Dispose();
                }
            }
        }

        internal void OnCacheEntryReleased()
        {
            base.OnChanged(null);
        }

        private void StartMonitoring(MemoryCache cache, MemoryCacheEntry entry, ref bool hasChanged, ref DateTime utcCreated)
        {
            if (entry != null)
            {
                entry.AddDependent(cache, this);
                this._dependencies.Add(entry);
                if (entry.State != EntryState.AddedToCache)
                {
                    hasChanged = true;
                }
                utcCreated = entry.UtcCreated;
            }
            else
            {
                hasChanged = true;
            }
        }

        public override ReadOnlyCollection<string> CacheKeys
        {
            get
            {
                return new ReadOnlyCollection<string>(this._keys);
            }
        }

        internal List<MemoryCacheEntry> Dependencies
        {
            get
            {
                return this._dependencies;
            }
        }

        public override DateTimeOffset LastModified
        {
            get
            {
                return this._lastModified;
            }
        }

        public override string RegionName
        {
            get
            {
                return this._regionName;
            }
        }

        public override string UniqueId
        {
            get
            {
                return this._uniqueId;
            }
        }
    }
}

