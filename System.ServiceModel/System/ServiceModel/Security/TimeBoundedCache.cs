namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class TimeBoundedCache
    {
        private ReaderWriterLock cacheLock;
        private bool doRemoveNotification;
        private Hashtable entries;
        private int lowWaterMark;
        private int maxCacheItems;
        private DateTime nextPurgeTimeUtc;
        private static Action<object> purgeCallback;
        private TimeSpan purgeInterval;
        private PurgingMode purgingMode;
        private IOThreadTimer purgingTimer;

        protected TimeBoundedCache(int lowWaterMark, int maxCacheItems, IEqualityComparer keyComparer, PurgingMode purgingMode, TimeSpan purgeInterval, bool doRemoveNotification)
        {
            this.entries = new Hashtable(keyComparer);
            this.cacheLock = new ReaderWriterLock();
            this.lowWaterMark = lowWaterMark;
            this.maxCacheItems = maxCacheItems;
            this.purgingMode = purgingMode;
            this.purgeInterval = purgeInterval;
            this.doRemoveNotification = doRemoveNotification;
            this.nextPurgeTimeUtc = DateTime.UtcNow.Add(this.purgeInterval);
        }

        private void CancelTimerIfNeeded()
        {
            if ((this.Count == 0) && (this.purgingTimer != null))
            {
                this.purgingTimer.Cancel();
                this.purgingTimer = null;
            }
        }

        protected void ClearItems()
        {
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    this.cacheLock.AcquireWriterLock(-1);
                    flag = true;
                }
                int count = this.entries.Count;
                if (this.doRemoveNotification)
                {
                    foreach (IExpirableItem item in this.entries.Values)
                    {
                        this.OnRemove(this.ExtractItem(item));
                    }
                }
                this.entries.Clear();
                this.CancelTimerIfNeeded();
            }
            finally
            {
                if (flag)
                {
                    this.cacheLock.ReleaseWriterLock();
                }
            }
        }

        private void EnforceQuota()
        {
            if (!this.cacheLock.IsWriterLockHeld)
            {
                DiagnosticUtility.FailFast("Cache write lock is not held.");
            }
            if (this.Count >= this.maxCacheItems)
            {
                ArrayList list = this.OnQuotaReached(this.entries);
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        this.entries.Remove(list[i]);
                    }
                }
                this.CancelTimerIfNeeded();
                if (this.Count >= this.maxCacheItems)
                {
                    this.ThrowQuotaReachedException();
                }
            }
        }

        protected object ExtractItem(IExpirableItem val)
        {
            ExpirableItem item = val as ExpirableItem;
            if (item != null)
            {
                return item.Item;
            }
            return val;
        }

        protected object GetItem(object key)
        {
            object obj2;
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    this.cacheLock.AcquireReaderLock(-1);
                    flag = true;
                }
                IExpirableItem item = this.entries[key] as IExpirableItem;
                if (item == null)
                {
                    return null;
                }
                if (this.IsExpired(item))
                {
                    return null;
                }
                obj2 = this.ExtractItem(item);
            }
            finally
            {
                if (flag)
                {
                    this.cacheLock.ReleaseReaderLock();
                }
            }
            return obj2;
        }

        private bool IsExpired(IExpirableItem item)
        {
            return (item.ExpirationTime <= DateTime.UtcNow);
        }

        protected virtual ArrayList OnQuotaReached(Hashtable cacheTable)
        {
            this.ThrowQuotaReachedException();
            return null;
        }

        protected virtual void OnRemove(object item)
        {
        }

        private static void PurgeCallbackStatic(object state)
        {
            TimeBoundedCache cache = (TimeBoundedCache) state;
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    cache.cacheLock.AcquireWriterLock(-1);
                    flag = true;
                }
                if (cache.purgingTimer != null)
                {
                    cache.PurgeStaleItems();
                    if ((cache.Count > 0) && (cache.purgingTimer != null))
                    {
                        cache.purgingTimer.Set(cache.purgeInterval);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    cache.cacheLock.ReleaseWriterLock();
                }
            }
        }

        private void PurgeIfNeeded()
        {
            if (!this.cacheLock.IsWriterLockHeld)
            {
                DiagnosticUtility.FailFast("Cache write lock is not held.");
            }
            if (this.ShouldPurge())
            {
                this.PurgeStaleItems();
            }
        }

        private void PurgeStaleItems()
        {
            if (!this.cacheLock.IsWriterLockHeld)
            {
                DiagnosticUtility.FailFast("Cache write lock is not held.");
            }
            ArrayList list = new ArrayList();
            foreach (object obj2 in this.entries.Keys)
            {
                IExpirableItem item = this.entries[obj2] as IExpirableItem;
                if (this.IsExpired(item))
                {
                    this.OnRemove(this.ExtractItem(item));
                    list.Add(obj2);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                this.entries.Remove(list[i]);
            }
            this.CancelTimerIfNeeded();
            this.nextPurgeTimeUtc = DateTime.UtcNow.Add(this.purgeInterval);
        }

        private bool ShouldPurge()
        {
            return ((this.Count >= this.maxCacheItems) || (((this.purgingMode == PurgingMode.AccessBasedPurge) && (DateTime.UtcNow > this.nextPurgeTimeUtc)) && (this.Count > this.lowWaterMark)));
        }

        private void StartTimerIfNeeded()
        {
            if ((this.purgingMode == PurgingMode.TimerBasedPurge) && (this.purgingTimer == null))
            {
                this.purgingTimer = new IOThreadTimer(PurgeCallback, this, false);
                this.purgingTimer.Set(this.purgeInterval);
            }
        }

        private void ThrowQuotaReachedException()
        {
            string message = System.ServiceModel.SR.GetString("CacheQuotaReached", new object[] { this.maxCacheItems });
            Exception innerException = new QuotaExceededException(message);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(message, innerException));
        }

        protected bool TryAddItem(object key, IExpirableItem item, bool replaceExistingEntry)
        {
            bool flag2;
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    this.cacheLock.AcquireWriterLock(-1);
                    flag = true;
                }
                this.PurgeIfNeeded();
                this.EnforceQuota();
                IExpirableItem item2 = this.entries[key] as IExpirableItem;
                if ((item2 == null) || this.IsExpired(item2))
                {
                    this.entries[key] = item;
                }
                else
                {
                    if (!replaceExistingEntry)
                    {
                        return false;
                    }
                    this.entries[key] = item;
                }
                if ((item2 != null) && this.doRemoveNotification)
                {
                    this.OnRemove(this.ExtractItem(item2));
                }
                this.StartTimerIfNeeded();
                flag2 = true;
            }
            finally
            {
                if (flag)
                {
                    this.cacheLock.ReleaseWriterLock();
                }
            }
            return flag2;
        }

        protected bool TryAddItem(object key, object item, DateTime expirationTime, bool replaceExistingEntry)
        {
            return this.TryAddItem(key, new ExpirableItem(item, expirationTime), replaceExistingEntry);
        }

        protected bool TryRemoveItem(object key)
        {
            bool flag3;
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    this.cacheLock.AcquireWriterLock(-1);
                    flag = true;
                }
                this.PurgeIfNeeded();
                IExpirableItem item = this.entries[key] as IExpirableItem;
                bool flag2 = (item != null) && !this.IsExpired(item);
                if (item != null)
                {
                    this.entries.Remove(key);
                    if (this.doRemoveNotification)
                    {
                        this.OnRemove(this.ExtractItem(item));
                    }
                    this.CancelTimerIfNeeded();
                }
                flag3 = flag2;
            }
            finally
            {
                if (flag)
                {
                    this.cacheLock.ReleaseWriterLock();
                }
            }
            return flag3;
        }

        protected bool TryReplaceItem(object key, object item, DateTime expirationTime)
        {
            bool flag2;
            bool flag = false;
            try
            {
                try
                {
                }
                finally
                {
                    this.cacheLock.AcquireWriterLock(-1);
                    flag = true;
                }
                this.PurgeIfNeeded();
                this.EnforceQuota();
                IExpirableItem item2 = this.entries[key] as IExpirableItem;
                if ((item2 == null) || this.IsExpired(item2))
                {
                    return false;
                }
                this.entries[key] = new ExpirableItem(item, expirationTime);
                if ((item2 != null) && this.doRemoveNotification)
                {
                    this.OnRemove(this.ExtractItem(item2));
                }
                this.StartTimerIfNeeded();
                flag2 = true;
            }
            finally
            {
                if (flag)
                {
                    this.cacheLock.ReleaseWriterLock();
                }
            }
            return flag2;
        }

        protected ReaderWriterLock CacheLock
        {
            get
            {
                return this.cacheLock;
            }
        }

        protected int Capacity
        {
            get
            {
                return this.maxCacheItems;
            }
        }

        public int Count
        {
            get
            {
                return this.entries.Count;
            }
        }

        protected Hashtable Entries
        {
            get
            {
                return this.entries;
            }
        }

        private static Action<object> PurgeCallback
        {
            get
            {
                if (purgeCallback == null)
                {
                    purgeCallback = new Action<object>(TimeBoundedCache.PurgeCallbackStatic);
                }
                return purgeCallback;
            }
        }

        internal sealed class ExpirableItem : TimeBoundedCache.IExpirableItem
        {
            private DateTime expirationTime;
            private object item;

            public ExpirableItem(object item, DateTime expirationTime)
            {
                this.item = item;
                this.expirationTime = expirationTime;
            }

            public DateTime ExpirationTime
            {
                get
                {
                    return this.expirationTime;
                }
            }

            public object Item
            {
                get
                {
                    return this.item;
                }
            }
        }

        internal class ExpirableItemComparer : IComparer<TimeBoundedCache.IExpirableItem>
        {
            private static TimeBoundedCache.ExpirableItemComparer instance;

            public int Compare(TimeBoundedCache.IExpirableItem item1, TimeBoundedCache.IExpirableItem item2)
            {
                if (!object.ReferenceEquals(item1, item2))
                {
                    if (item1.ExpirationTime < item2.ExpirationTime)
                    {
                        return 1;
                    }
                    if (item1.ExpirationTime > item2.ExpirationTime)
                    {
                        return -1;
                    }
                }
                return 0;
            }

            public static TimeBoundedCache.ExpirableItemComparer Default
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new TimeBoundedCache.ExpirableItemComparer();
                    }
                    return instance;
                }
            }
        }

        internal interface IExpirableItem
        {
            DateTime ExpirationTime { get; }
        }
    }
}

