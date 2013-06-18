namespace System.Runtime.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class MemoryCacheEntry : MemoryCacheKey
    {
        private CacheEntryRemovedCallback _callback;
        private byte _expiresBucket;
        private System.Runtime.Caching.ExpiresEntryRef _expiresEntryRef;
        private SeldomUsedFields _fields;
        private TimeSpan _slidingExp;
        private byte _usageBucket;
        private System.Runtime.Caching.UsageEntryRef _usageEntryRef;
        private DateTime _utcAbsExp;
        private DateTime _utcCreated;
        private DateTime _utcLastUpdateUsage;
        private object _value;
        private const byte EntryStateMask = 0x1f;

        internal MemoryCacheEntry(string key, object value, DateTimeOffset absExp, TimeSpan slidingExp, CacheItemPriority priority, Collection<ChangeMonitor> dependencies, CacheEntryRemovedCallback removedCallback, MemoryCache cache) : base(key)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this._utcCreated = DateTime.UtcNow;
            this._value = value;
            this._slidingExp = slidingExp;
            if (this._slidingExp > TimeSpan.Zero)
            {
                this._utcAbsExp = this._utcCreated + this._slidingExp;
            }
            else
            {
                this._utcAbsExp = absExp.UtcDateTime;
            }
            this._expiresEntryRef = System.Runtime.Caching.ExpiresEntryRef.INVALID;
            this._expiresBucket = 0xff;
            this._usageEntryRef = System.Runtime.Caching.UsageEntryRef.INVALID;
            if (priority == CacheItemPriority.NotRemovable)
            {
                this._usageBucket = 0xff;
            }
            else
            {
                this._usageBucket = 0;
            }
            this._callback = removedCallback;
            if (dependencies != null)
            {
                this._fields = new SeldomUsedFields();
                this._fields._dependencies = dependencies;
                this._fields._cache = cache;
            }
        }

        internal void AddDependent(MemoryCache cache, MemoryCacheEntryChangeMonitor dependent)
        {
            lock (this)
            {
                if (this.State <= EntryState.AddedToCache)
                {
                    if (this._fields == null)
                    {
                        this._fields = new SeldomUsedFields();
                    }
                    if (this._fields._cache == null)
                    {
                        this._fields._cache = cache;
                    }
                    if (this._fields._dependents == null)
                    {
                        this._fields._dependents = new Dictionary<MemoryCacheEntryChangeMonitor, MemoryCacheEntryChangeMonitor>();
                    }
                    this._fields._dependents[dependent] = dependent;
                }
            }
        }

        private void CallCacheEntryRemovedCallback(MemoryCache cache, CacheEntryRemovedReason reason)
        {
            if (this._callback != null)
            {
                CacheEntryRemovedArguments arguments = new CacheEntryRemovedArguments(cache, reason, new CacheItem(base.Key, this._value));
                try
                {
                    this._callback(arguments);
                }
                catch
                {
                }
            }
        }

        internal void CallNotifyOnChanged()
        {
            if ((this._fields != null) && (this._fields._dependencies != null))
            {
                foreach (ChangeMonitor monitor in this._fields._dependencies)
                {
                    monitor.NotifyOnChanged(new OnChangedCallback(this.OnDependencyChanged));
                }
            }
        }

        internal bool HasExpiration()
        {
            return (this._utcAbsExp < DateTime.MaxValue);
        }

        internal bool HasUsage()
        {
            return (this._usageBucket != 0xff);
        }

        internal bool InExpires()
        {
            return !this._expiresEntryRef.IsInvalid;
        }

        internal bool InUsage()
        {
            return !this._usageEntryRef.IsInvalid;
        }

        private void OnDependencyChanged(object state)
        {
            if (this.State == EntryState.AddedToCache)
            {
                this._fields._cache.RemoveEntry(base.Key, this, CacheEntryRemovedReason.ChangeMonitorChanged);
            }
        }

        internal void Release(MemoryCache cache, CacheEntryRemovedReason reason)
        {
            this.State = EntryState.Closed;
            Dictionary<MemoryCacheEntryChangeMonitor, MemoryCacheEntryChangeMonitor>.KeyCollection keys = null;
            lock (this)
            {
                if (((this._fields != null) && (this._fields._dependents != null)) && (this._fields._dependents.Count > 0))
                {
                    keys = this._fields._dependents.Keys;
                    this._fields._dependents = null;
                }
            }
            if (keys != null)
            {
                foreach (MemoryCacheEntryChangeMonitor monitor in keys)
                {
                    if (monitor != null)
                    {
                        monitor.OnCacheEntryReleased();
                    }
                }
            }
            this.CallCacheEntryRemovedCallback(cache, reason);
            if ((this._fields != null) && (this._fields._dependencies != null))
            {
                foreach (ChangeMonitor monitor2 in this._fields._dependencies)
                {
                    monitor2.Dispose();
                }
            }
        }

        internal void RemoveDependent(MemoryCacheEntryChangeMonitor dependent)
        {
            lock (this)
            {
                if (this._fields._dependents != null)
                {
                    this._fields._dependents.Remove(dependent);
                }
            }
        }

        internal void UpdateSlidingExp(DateTime utcNow, CacheExpires expires)
        {
            if (this._slidingExp > TimeSpan.Zero)
            {
                DateTime utcNewExpires = utcNow + this._slidingExp;
                if (((utcNewExpires - this._utcAbsExp) >= CacheExpires.MIN_UPDATE_DELTA) || (utcNewExpires < this._utcAbsExp))
                {
                    expires.UtcUpdate(this, utcNewExpires);
                }
            }
        }

        internal void UpdateUsage(DateTime utcNow, CacheUsage usage)
        {
            if (this.InUsage() && (this._utcLastUpdateUsage < (utcNow - CacheUsage.CORRELATED_REQUEST_TIMEOUT)))
            {
                this._utcLastUpdateUsage = utcNow;
                usage.Update(this);
                if ((this._fields != null) && (this._fields._dependencies != null))
                {
                    foreach (ChangeMonitor monitor in this._fields._dependencies)
                    {
                        MemoryCacheEntryChangeMonitor monitor2 = monitor as MemoryCacheEntryChangeMonitor;
                        if (monitor2 != null)
                        {
                            foreach (MemoryCacheEntry entry in monitor2.Dependencies)
                            {
                                MemoryCacheStore store = entry._fields._cache.GetStore(entry);
                                entry.UpdateUsage(utcNow, store.Usage);
                            }
                        }
                    }
                }
            }
        }

        internal byte ExpiresBucket
        {
            get
            {
                return this._expiresBucket;
            }
            set
            {
                this._expiresBucket = value;
            }
        }

        internal System.Runtime.Caching.ExpiresEntryRef ExpiresEntryRef
        {
            get
            {
                return this._expiresEntryRef;
            }
            set
            {
                this._expiresEntryRef = value;
            }
        }

        internal TimeSpan SlidingExp
        {
            get
            {
                return this._slidingExp;
            }
        }

        internal EntryState State
        {
            get
            {
                return (EntryState) ((byte) (base._bits & 0x1f));
            }
            set
            {
                base._bits = (byte) ((base._bits & -32) | ((int) value));
            }
        }

        internal byte UsageBucket
        {
            get
            {
                return this._usageBucket;
            }
        }

        internal System.Runtime.Caching.UsageEntryRef UsageEntryRef
        {
            get
            {
                return this._usageEntryRef;
            }
            set
            {
                this._usageEntryRef = value;
            }
        }

        internal DateTime UtcAbsExp
        {
            get
            {
                return this._utcAbsExp;
            }
            set
            {
                this._utcAbsExp = value;
            }
        }

        internal DateTime UtcCreated
        {
            get
            {
                return this._utcCreated;
            }
        }

        internal DateTime UtcLastUpdateUsage
        {
            get
            {
                return this._utcLastUpdateUsage;
            }
            set
            {
                this._utcLastUpdateUsage = value;
            }
        }

        internal object Value
        {
            get
            {
                return this._value;
            }
        }

        private class SeldomUsedFields
        {
            internal MemoryCache _cache;
            internal Collection<ChangeMonitor> _dependencies;
            internal Dictionary<MemoryCacheEntryChangeMonitor, MemoryCacheEntryChangeMonitor> _dependents;
        }
    }
}

