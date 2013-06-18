namespace System.Runtime.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal class ObjectCache<TKey, TValue> where TValue: class
    {
        private Dictionary<TKey, Item<TKey, TValue>> cacheItems;
        private bool disposed;
        private bool idleTimeoutEnabled;
        private IOThreadTimer idleTimer;
        private bool leaseTimeoutEnabled;
        private static Action<object> onIdle;
        private ObjectCacheSettings settings;
        private const int timerThreshold = 1;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ObjectCache(ObjectCacheSettings settings) : this(settings, null)
        {
        }

        public ObjectCache(ObjectCacheSettings settings, IEqualityComparer<TKey> comparer)
        {
            this.settings = settings.Clone();
            this.cacheItems = new Dictionary<TKey, Item<TKey, TValue>>(comparer);
            this.idleTimeoutEnabled = settings.IdleTimeout != TimeSpan.MaxValue;
            this.leaseTimeoutEnabled = settings.LeaseTimeout != TimeSpan.MaxValue;
        }

        private static void Add<T>(ref List<T> list, T item)
        {
            if (list == null)
            {
                list = new List<T>();
            }
            list.Add(item);
        }

        public ObjectCacheItem<TValue> Add(TKey key, TValue value)
        {
            lock (this.ThisLock)
            {
                if ((this.Count >= this.settings.CacheLimit) || this.cacheItems.ContainsKey(key))
                {
                    return new Item<TKey, TValue>(key, value, this.DisposeItemCallback);
                }
                return this.InternalAdd(key, value);
            }
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                foreach (Item<TKey, TValue> item in this.cacheItems.Values)
                {
                    if (item != null)
                    {
                        item.Dispose();
                    }
                }
                this.cacheItems.Clear();
                this.settings.CacheLimit = 0;
                this.disposed = true;
                if (this.idleTimer != null)
                {
                    this.idleTimer.Cancel();
                    this.idleTimer = null;
                }
            }
        }

        private void GatherExpiredItems(ref List<KeyValuePair<TKey, Item<TKey, TValue>>> expiredItems, bool calledFromTimer)
        {
            if ((this.Count != 0) && (this.leaseTimeoutEnabled || this.idleTimeoutEnabled))
            {
                DateTime utcNow = DateTime.UtcNow;
                bool flag = false;
                lock (this.ThisLock)
                {
                    foreach (KeyValuePair<TKey, Item<TKey, TValue>> pair in this.cacheItems)
                    {
                        if (this.ShouldPurgeItem(pair.Value, utcNow))
                        {
                            pair.Value.LockedDispose();
                            ObjectCache<TKey, TValue>.Add<KeyValuePair<TKey, Item<TKey, TValue>>>(ref expiredItems, pair);
                        }
                    }
                    if (expiredItems != null)
                    {
                        for (int i = 0; i < expiredItems.Count; i++)
                        {
                            KeyValuePair<TKey, Item<TKey, TValue>> pair2 = expiredItems[i];
                            this.cacheItems.Remove(pair2.Key);
                        }
                    }
                    flag = calledFromTimer && (this.Count > 0);
                }
                if (flag)
                {
                    this.idleTimer.Set(this.settings.IdleTimeout);
                }
            }
        }

        private Item<TKey, TValue> InternalAdd(TKey key, TValue value)
        {
            Item<TKey, TValue> item = new Item<TKey, TValue>(key, value, (ObjectCache<TKey, TValue>) this);
            if (this.leaseTimeoutEnabled)
            {
                item.CreationTime = DateTime.UtcNow;
            }
            this.cacheItems.Add(key, item);
            this.StartTimerIfNecessary();
            return item;
        }

        private static void OnIdle(object state)
        {
            ((ObjectCache<TKey, TValue>) state).PurgeCache(true);
        }

        private void PurgeCache(bool calledFromTimer)
        {
            List<KeyValuePair<TKey, Item<TKey, TValue>>> expiredItems = null;
            lock (this.ThisLock)
            {
                this.GatherExpiredItems(ref expiredItems, calledFromTimer);
            }
            if (expiredItems != null)
            {
                for (int i = 0; i < expiredItems.Count; i++)
                {
                    KeyValuePair<TKey, Item<TKey, TValue>> pair = expiredItems[i];
                    pair.Value.LocalDispose();
                }
            }
        }

        private bool Return(TKey key, Item<TKey, TValue> cacheItem)
        {
            bool flag = false;
            if (this.disposed)
            {
                return true;
            }
            cacheItem.InternalReleaseReference();
            DateTime utcNow = DateTime.UtcNow;
            if (this.idleTimeoutEnabled)
            {
                cacheItem.LastUsage = utcNow;
            }
            if (this.ShouldPurgeItem(cacheItem, utcNow))
            {
                this.cacheItems.Remove(key);
                cacheItem.LockedDispose();
                flag = true;
            }
            return flag;
        }

        private bool ShouldPurgeItem(Item<TKey, TValue> cacheItem, DateTime now)
        {
            if (cacheItem.ReferenceCount > 0)
            {
                return false;
            }
            return ((this.idleTimeoutEnabled && (now >= (cacheItem.LastUsage + this.settings.IdleTimeout))) || (this.leaseTimeoutEnabled && ((now - cacheItem.CreationTime) >= this.settings.LeaseTimeout)));
        }

        private void StartTimerIfNecessary()
        {
            if (this.idleTimeoutEnabled && (this.Count > 1))
            {
                if (this.idleTimer == null)
                {
                    if (ObjectCache<TKey, TValue>.onIdle == null)
                    {
                        ObjectCache<TKey, TValue>.onIdle = new Action<object>(ObjectCache<TKey, TValue>.OnIdle);
                    }
                    this.idleTimer = new IOThreadTimer(ObjectCache<TKey, TValue>.onIdle, this, false);
                }
                this.idleTimer.Set(this.settings.IdleTimeout);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ObjectCacheItem<TValue> Take(TKey key)
        {
            return this.Take(key, null);
        }

        public ObjectCacheItem<TValue> Take(TKey key, Func<TValue> initializerDelegate)
        {
            Item<TKey, TValue> item = null;
            lock (this.ThisLock)
            {
                if (this.cacheItems.TryGetValue(key, out item))
                {
                    item.InternalAddReference();
                    return item;
                }
                if (initializerDelegate == null)
                {
                    return null;
                }
                TValue local = initializerDelegate();
                if (this.Count >= this.settings.CacheLimit)
                {
                    return new Item<TKey, TValue>(key, local, this.DisposeItemCallback);
                }
                return this.InternalAdd(key, local);
            }
        }

        public int Count
        {
            get
            {
                return this.cacheItems.Count;
            }
        }

        public Action<TValue> DisposeItemCallback
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<DisposeItemCallback>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<DisposeItemCallback>k__BackingField = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private class Item : ObjectCacheItem<TValue>
        {
            private readonly Action<TValue> disposeItemCallback;
            private readonly TKey key;
            private readonly ObjectCache<TKey, TValue> parent;
            private int referenceCount;
            private TValue value;

            private Item(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
                this.referenceCount = 1;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public Item(TKey key, TValue value, Action<TValue> disposeItemCallback) : this(key, value)
            {
                this.disposeItemCallback = disposeItemCallback;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public Item(TKey key, TValue value, ObjectCache<TKey, TValue> parent) : this(key, value)
            {
                this.parent = parent;
            }

            public void Dispose()
            {
                if (this.Value != null)
                {
                    Action<TValue> disposeItemCallback = this.disposeItemCallback;
                    if (this.parent != null)
                    {
                        disposeItemCallback = this.parent.DisposeItemCallback;
                    }
                    if (disposeItemCallback != null)
                    {
                        disposeItemCallback(this.Value);
                    }
                    else if (this.Value is IDisposable)
                    {
                        ((IDisposable) this.Value).Dispose();
                    }
                }
                this.value = default(TValue);
                this.referenceCount = -1;
            }

            internal void InternalAddReference()
            {
                this.referenceCount++;
            }

            internal void InternalReleaseReference()
            {
                this.referenceCount--;
            }

            public void LocalDispose()
            {
                this.Dispose();
            }

            public void LockedDispose()
            {
                this.referenceCount = -1;
            }

            public override void ReleaseReference()
            {
                bool flag;
                if (this.parent == null)
                {
                    this.referenceCount = -1;
                    flag = true;
                }
                else
                {
                    lock (this.parent.ThisLock)
                    {
                        if (this.referenceCount > 1)
                        {
                            this.InternalReleaseReference();
                            flag = false;
                        }
                        else
                        {
                            flag = this.parent.Return(this.key, (ObjectCache<TKey, TValue>.Item) this);
                        }
                    }
                }
                if (flag)
                {
                    this.LocalDispose();
                }
            }

            public override bool TryAddReference()
            {
                bool flag;
                if ((this.parent == null) || (this.referenceCount == -1))
                {
                    return false;
                }
                bool flag2 = false;
                lock (this.parent.ThisLock)
                {
                    if (this.referenceCount == -1)
                    {
                        flag = false;
                    }
                    else if ((this.referenceCount == 0) && this.parent.ShouldPurgeItem((ObjectCache<TKey, TValue>.Item) this, DateTime.UtcNow))
                    {
                        this.LockedDispose();
                        flag2 = true;
                        flag = false;
                        this.parent.cacheItems.Remove(this.key);
                    }
                    else
                    {
                        this.referenceCount++;
                        flag = true;
                    }
                }
                if (flag2)
                {
                    this.LocalDispose();
                }
                return flag;
            }

            public DateTime CreationTime
            {
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.<CreationTime>k__BackingField;
                }
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.<CreationTime>k__BackingField = value;
                }
            }

            public DateTime LastUsage
            {
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.<LastUsage>k__BackingField;
                }
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.<LastUsage>k__BackingField = value;
                }
            }

            public int ReferenceCount
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.referenceCount;
                }
            }

            public override TValue Value
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.value;
                }
            }
        }
    }
}

