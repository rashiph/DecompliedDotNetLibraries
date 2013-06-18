namespace System.Runtime.Caching
{
    using System;
    using System.Threading;

    internal sealed class CacheExpires
    {
        private readonly ExpiresBucket[] _buckets;
        private readonly System.Runtime.Caching.MemoryCacheStore _cacheStore;
        private int _inFlush;
        private Timer _timer;
        internal static readonly TimeSpan _tsPerBucket = new TimeSpan(0, 0, 20);
        private static readonly TimeSpan _tsPerCycle = new TimeSpan(30L * _tsPerBucket.Ticks);
        private DateTime _utcLastFlush;
        internal static readonly TimeSpan MIN_FLUSH_INTERVAL = new TimeSpan(0, 0, 1);
        internal static readonly TimeSpan MIN_UPDATE_DELTA = new TimeSpan(0, 0, 1);
        private const int NUMBUCKETS = 30;

        internal CacheExpires(System.Runtime.Caching.MemoryCacheStore cacheStore)
        {
            DateTime utcNow = DateTime.UtcNow;
            this._cacheStore = cacheStore;
            this._buckets = new ExpiresBucket[30];
            for (byte i = 0; i < this._buckets.Length; i = (byte) (i + 1))
            {
                this._buckets[i] = new ExpiresBucket(this, i, utcNow);
            }
        }

        internal void Add(MemoryCacheEntry cacheEntry)
        {
            DateTime utcNow = DateTime.UtcNow;
            if (utcNow > cacheEntry.UtcAbsExp)
            {
                cacheEntry.UtcAbsExp = utcNow;
            }
            int index = this.UtcCalcExpiresBucket(cacheEntry.UtcAbsExp);
            this._buckets[index].AddCacheEntry(cacheEntry);
        }

        internal void EnableExpirationTimer(bool enable)
        {
            if (enable)
            {
                if (this._timer == null)
                {
                    DateTime utcNow = DateTime.UtcNow;
                    TimeSpan span = _tsPerBucket - new TimeSpan(utcNow.Ticks % _tsPerBucket.Ticks);
                    this._timer = new Timer(new System.Threading.TimerCallback(this.TimerCallback), null, span.Ticks / 0x2710L, _tsPerBucket.Ticks / 0x2710L);
                }
            }
            else
            {
                Timer comparand = this._timer;
                if ((comparand != null) && (Interlocked.CompareExchange<Timer>(ref this._timer, null, comparand) == comparand))
                {
                    comparand.Dispose();
                    while (this._inFlush != 0)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }

        internal int FlushExpiredItems(bool useInsertBlock)
        {
            return this.FlushExpiredItems(true, useInsertBlock);
        }

        private int FlushExpiredItems(bool checkDelta, bool useInsertBlock)
        {
            int num = 0;
            if (Interlocked.Exchange(ref this._inFlush, 1) == 0)
            {
                try
                {
                    if (this._timer == null)
                    {
                        return 0;
                    }
                    DateTime utcNow = DateTime.UtcNow;
                    if ((checkDelta && ((utcNow - this._utcLastFlush) < MIN_FLUSH_INTERVAL)) && (utcNow >= this._utcLastFlush))
                    {
                        return num;
                    }
                    this._utcLastFlush = utcNow;
                    foreach (ExpiresBucket bucket in this._buckets)
                    {
                        num += bucket.FlushExpiredItems(utcNow, useInsertBlock);
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
            byte expiresBucket = cacheEntry.ExpiresBucket;
            if (expiresBucket != 0xff)
            {
                this._buckets[expiresBucket].RemoveCacheEntry(cacheEntry);
            }
        }

        private void TimerCallback(object state)
        {
            this.FlushExpiredItems(false, false);
        }

        private int UtcCalcExpiresBucket(DateTime utcDate)
        {
            long num = utcDate.Ticks % _tsPerCycle.Ticks;
            return (int) (((num / _tsPerBucket.Ticks) + 1L) % 30L);
        }

        internal void UtcUpdate(MemoryCacheEntry cacheEntry, DateTime utcNewExpires)
        {
            int expiresBucket = cacheEntry.ExpiresBucket;
            int index = this.UtcCalcExpiresBucket(utcNewExpires);
            if (expiresBucket != index)
            {
                if (expiresBucket != 0xff)
                {
                    this._buckets[expiresBucket].RemoveCacheEntry(cacheEntry);
                    cacheEntry.UtcAbsExp = utcNewExpires;
                    this._buckets[index].AddCacheEntry(cacheEntry);
                }
            }
            else if (expiresBucket != 0xff)
            {
                this._buckets[expiresBucket].UtcUpdateCacheEntry(cacheEntry, utcNewExpires);
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

