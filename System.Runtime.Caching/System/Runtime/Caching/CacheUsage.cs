namespace System.Runtime.Caching
{
    using System;
    using System.Threading;

    internal class CacheUsage
    {
        internal readonly UsageBucket[] _buckets;
        private readonly System.Runtime.Caching.MemoryCacheStore _cacheStore;
        private int _inFlush;
        internal static readonly TimeSpan CORRELATED_REQUEST_TIMEOUT = new TimeSpan(0, 0, 1);
        private const int MAX_REMOVE = 0x400;
        internal static readonly TimeSpan MIN_LIFETIME_FOR_USAGE = NEWADD_INTERVAL;
        internal static readonly TimeSpan NEWADD_INTERVAL = new TimeSpan(0, 0, 10);
        private const byte NUMBUCKETS = 1;

        internal CacheUsage(System.Runtime.Caching.MemoryCacheStore cacheStore)
        {
            this._cacheStore = cacheStore;
            this._buckets = new UsageBucket[1];
            for (byte i = 0; i < this._buckets.Length; i = (byte) (i + 1))
            {
                this._buckets[i] = new UsageBucket(this, i);
            }
        }

        internal void Add(MemoryCacheEntry cacheEntry)
        {
            byte usageBucket = cacheEntry.UsageBucket;
            this._buckets[usageBucket].AddCacheEntry(cacheEntry);
        }

        internal int FlushUnderUsedItems(int toFlush)
        {
            int num = 0;
            if (Interlocked.Exchange(ref this._inFlush, 1) == 0)
            {
                try
                {
                    foreach (UsageBucket bucket in this._buckets)
                    {
                        int num2 = bucket.FlushUnderUsedItems(toFlush - num, false);
                        num += num2;
                        if (num >= toFlush)
                        {
                            break;
                        }
                    }
                    if (num >= toFlush)
                    {
                        return num;
                    }
                    foreach (UsageBucket bucket2 in this._buckets)
                    {
                        int num3 = bucket2.FlushUnderUsedItems(toFlush - num, true);
                        num += num3;
                        if (num >= toFlush)
                        {
                            return num;
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref this._inFlush, 0);
                }
            }
            return num;
        }

        internal void Remove(MemoryCacheEntry cacheEntry)
        {
            byte usageBucket = cacheEntry.UsageBucket;
            if (usageBucket != 0xff)
            {
                this._buckets[usageBucket].RemoveCacheEntry(cacheEntry);
            }
        }

        internal void Update(MemoryCacheEntry cacheEntry)
        {
            byte usageBucket = cacheEntry.UsageBucket;
            if (usageBucket != 0xff)
            {
                this._buckets[usageBucket].UpdateCacheEntry(cacheEntry);
            }
        }

        internal System.Runtime.Caching.MemoryCacheStore MemoryCacheStore
        {
            get
            {
                return this._cacheStore;
            }
        }
    }
}

