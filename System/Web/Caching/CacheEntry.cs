namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Management;

    internal sealed class CacheEntry : CacheKey, ICacheDependencyChanged
    {
        private CacheDependency _dependency;
        private byte _expiresBucket;
        private System.Web.Caching.ExpiresEntryRef _expiresEntryRef;
        private object _onRemovedTargets;
        private TimeSpan _slidingExpiration;
        private byte _usageBucket;
        private System.Web.Caching.UsageEntryRef _usageEntryRef;
        private DateTime _utcCreated;
        private DateTime _utcExpires;
        private DateTime _utcLastUpdate;
        private object _value;
        private const CacheItemPriority CacheItemPriorityMax = CacheItemPriority.NotRemovable;
        private const CacheItemPriority CacheItemPriorityMin = CacheItemPriority.Low;
        private const byte EntryStateMask = 0x1f;
        private static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
        private static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;
        private static readonly TimeSpan OneYear = new TimeSpan(0x16d, 0, 0, 0);

        internal CacheEntry(string key, object value, CacheDependency dependency, CacheItemRemovedCallback onRemovedHandler, DateTime utcAbsoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, bool isPublic) : base(key, isPublic)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((slidingExpiration < TimeSpan.Zero) || (OneYear < slidingExpiration))
            {
                throw new ArgumentOutOfRangeException("slidingExpiration");
            }
            if ((utcAbsoluteExpiration != Cache.NoAbsoluteExpiration) && (slidingExpiration != Cache.NoSlidingExpiration))
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_expiration_combination"));
            }
            if ((priority < CacheItemPriority.Low) || (CacheItemPriority.NotRemovable < priority))
            {
                throw new ArgumentOutOfRangeException("priority");
            }
            this._value = value;
            this._dependency = dependency;
            this._onRemovedTargets = onRemovedHandler;
            this._utcCreated = DateTime.UtcNow;
            this._slidingExpiration = slidingExpiration;
            if (this._slidingExpiration > TimeSpan.Zero)
            {
                this._utcExpires = this._utcCreated + this._slidingExpiration;
            }
            else
            {
                this._utcExpires = utcAbsoluteExpiration;
            }
            this._expiresEntryRef = System.Web.Caching.ExpiresEntryRef.INVALID;
            this._expiresBucket = 0xff;
            this._usageEntryRef = System.Web.Caching.UsageEntryRef.INVALID;
            if (priority == CacheItemPriority.NotRemovable)
            {
                this._usageBucket = 0xff;
            }
            else
            {
                this._usageBucket = (byte) (priority - 1);
            }
        }

        internal void AddCacheDependencyNotify(CacheDependency dependency)
        {
            lock (this)
            {
                if (this._onRemovedTargets == null)
                {
                    this._onRemovedTargets = dependency;
                }
                else if (this._onRemovedTargets is Hashtable)
                {
                    Hashtable hashtable = (Hashtable) this._onRemovedTargets;
                    hashtable[dependency] = dependency;
                }
                else
                {
                    Hashtable hashtable2 = new Hashtable(2);
                    hashtable2[this._onRemovedTargets] = this._onRemovedTargets;
                    hashtable2[dependency] = dependency;
                    this._onRemovedTargets = hashtable2;
                }
            }
        }

        private void CallCacheItemRemovedCallback(CacheItemRemovedCallback callback, CacheItemRemovedReason reason)
        {
            if (base.IsPublic)
            {
                try
                {
                    if (HttpContext.Current == null)
                    {
                        using (new ApplicationImpersonationContext())
                        {
                            callback(base._key, this._value, reason);
                            return;
                        }
                    }
                    callback(base._key, this._value, reason);
                }
                catch (Exception exception)
                {
                    HttpApplicationFactory.RaiseError(exception);
                    try
                    {
                        WebBaseEvent.RaiseRuntimeError(exception, this);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                try
                {
                    using (new ApplicationImpersonationContext())
                    {
                        callback(base._key, this._value, reason);
                    }
                }
                catch
                {
                }
            }
        }

        internal void Close(CacheItemRemovedReason reason)
        {
            this.State = EntryState.Closed;
            object obj2 = null;
            object[] array = null;
            lock (this)
            {
                if (this._onRemovedTargets != null)
                {
                    obj2 = this._onRemovedTargets;
                    if (obj2 is Hashtable)
                    {
                        ICollection keys = ((Hashtable) obj2).Keys;
                        array = new object[keys.Count];
                        keys.CopyTo(array, 0);
                    }
                }
            }
            if (obj2 != null)
            {
                if (array != null)
                {
                    foreach (object obj3 in array)
                    {
                        if (obj3 is CacheDependency)
                        {
                            ((CacheDependency) obj3).ItemRemoved();
                        }
                        else
                        {
                            this.CallCacheItemRemovedCallback((CacheItemRemovedCallback) obj3, reason);
                        }
                    }
                }
                else if (obj2 is CacheItemRemovedCallback)
                {
                    this.CallCacheItemRemovedCallback((CacheItemRemovedCallback) obj2, reason);
                }
                else
                {
                    ((CacheDependency) obj2).ItemRemoved();
                }
            }
            if (this._dependency != null)
            {
                this._dependency.DisposeInternal();
            }
        }

        internal bool HasExpiration()
        {
            return (this._utcExpires < DateTime.MaxValue);
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

        internal void MonitorDependencyChanges()
        {
            CacheDependency dependency = this._dependency;
            if ((dependency != null) && (this.State == EntryState.AddedToCache))
            {
                if (!dependency.Use())
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Cache_dependency_used_more_that_once"));
                }
                dependency.SetCacheDependencyChanged(this);
            }
        }

        internal void RemoveCacheDependencyNotify(CacheDependency dependency)
        {
            lock (this)
            {
                if (this._onRemovedTargets != null)
                {
                    if (this._onRemovedTargets == dependency)
                    {
                        this._onRemovedTargets = null;
                    }
                    else
                    {
                        Hashtable hashtable = (Hashtable) this._onRemovedTargets;
                        hashtable.Remove(dependency);
                        if (hashtable.Count == 0)
                        {
                            this._onRemovedTargets = null;
                        }
                    }
                }
            }
        }

        void ICacheDependencyChanged.DependencyChanged(object sender, EventArgs e)
        {
            if (this.State == EntryState.AddedToCache)
            {
                HttpRuntime.CacheInternal.Remove(this, CacheItemRemovedReason.DependencyChanged);
            }
        }

        internal CacheDependency Dependency
        {
            get
            {
                return this._dependency;
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

        internal System.Web.Caching.ExpiresEntryRef ExpiresEntryRef
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

        internal TimeSpan SlidingExpiration
        {
            get
            {
                return this._slidingExpiration;
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

        internal System.Web.Caching.UsageEntryRef UsageEntryRef
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

        internal DateTime UtcCreated
        {
            get
            {
                return this._utcCreated;
            }
        }

        internal DateTime UtcExpires
        {
            get
            {
                return this._utcExpires;
            }
            set
            {
                this._utcExpires = value;
            }
        }

        internal DateTime UtcLastUsageUpdate
        {
            get
            {
                return this._utcLastUpdate;
            }
            set
            {
                this._utcLastUpdate = value;
            }
        }

        internal object Value
        {
            get
            {
                return this._value;
            }
        }

        internal enum EntryState : byte
        {
            AddedToCache = 2,
            AddingToCache = 1,
            Closed = 0x10,
            NotInCache = 0,
            RemovedFromCache = 8,
            RemovingFromCache = 4
        }
    }
}

