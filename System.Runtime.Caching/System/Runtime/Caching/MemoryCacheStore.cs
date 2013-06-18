namespace System.Runtime.Caching
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class MemoryCacheStore : IDisposable
    {
        private MemoryCache _cache;
        private int _disposed;
        private Hashtable _entries;
        private object _entriesLock;
        private CacheExpires _expires;
        private ManualResetEvent _insertBlock;
        private PerfCounters _perfCounters;
        private CacheUsage _usage;
        private volatile bool _useInsertBlock;
        private const int INSERT_BLOCK_WAIT = 0x2710;
        private const int MAX_COUNT = 0x3fffffff;
        private const int MIN_COUNT = 10;

        internal MemoryCacheStore(MemoryCache cache, PerfCounters perfCounters)
        {
            this._cache = cache;
            this._perfCounters = perfCounters;
            this._entries = new Hashtable(new MemoryCacheEqualityComparer());
            this._entriesLock = new object();
            this._expires = new CacheExpires(this);
            this._usage = new CacheUsage(this);
            this.InitDisposableMembers();
        }

        internal MemoryCacheEntry AddOrGetExisting(MemoryCacheKey key, MemoryCacheEntry entry)
        {
            if (this._useInsertBlock && entry.HasUsage())
            {
                this.WaitInsertBlock();
            }
            MemoryCacheEntry entry2 = null;
            MemoryCacheEntry entry3 = null;
            bool flag = false;
            lock (this._entriesLock)
            {
                if (this._disposed == 0)
                {
                    entry2 = this._entries[key] as MemoryCacheEntry;
                    if ((entry2 != null) && (entry.UtcAbsExp <= DateTime.UtcNow))
                    {
                        entry3 = entry2;
                        entry3.State = EntryState.RemovingFromCache;
                        entry2 = null;
                    }
                    if (entry2 == null)
                    {
                        entry.State = EntryState.AddingToCache;
                        flag = true;
                        this._entries[key] = entry;
                    }
                }
            }
            bool delayRelease = true;
            this.RemoveFromCache(entry3, CacheEntryRemovedReason.Expired, delayRelease);
            if (flag)
            {
                this.AddToCache(entry);
            }
            this.UpdateExpAndUsage(entry2);
            if (entry3 != null)
            {
                entry3.Release(this._cache, CacheEntryRemovedReason.Expired);
            }
            return entry2;
        }

        private void AddToCache(MemoryCacheEntry entry)
        {
            if (entry != null)
            {
                if (entry.HasExpiration())
                {
                    this._expires.Add(entry);
                }
                if (entry.HasUsage() && (!entry.HasExpiration() || ((entry.UtcAbsExp - DateTime.UtcNow) >= CacheUsage.MIN_LIFETIME_FOR_USAGE)))
                {
                    this._usage.Add(entry);
                }
                entry.State = EntryState.AddedToCache;
                entry.CallNotifyOnChanged();
                if (this._perfCounters != null)
                {
                    this._perfCounters.Increment(PerfCounterName.Entries);
                    this._perfCounters.Increment(PerfCounterName.Turnover);
                }
            }
        }

        internal void BlockInsert()
        {
            this._insertBlock.Reset();
            this._useInsertBlock = true;
        }

        internal void CopyTo(IDictionary h)
        {
            lock (this._entriesLock)
            {
                if (this._disposed == 0)
                {
                    foreach (DictionaryEntry entry in this._entries)
                    {
                        MemoryCacheKey key = entry.Key as MemoryCacheKey;
                        MemoryCacheEntry entry2 = entry.Value as MemoryCacheEntry;
                        if (entry2.UtcAbsExp > DateTime.UtcNow)
                        {
                            h[key.Key] = entry2.Value;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._disposed, 1) == 0)
            {
                this._expires.EnableExpirationTimer(false);
                ArrayList list = new ArrayList(this._entries.Count);
                lock (this._entriesLock)
                {
                    foreach (DictionaryEntry entry in this._entries)
                    {
                        MemoryCacheEntry entry2 = entry.Value as MemoryCacheEntry;
                        list.Add(entry2);
                    }
                    foreach (MemoryCacheEntry entry3 in list)
                    {
                        MemoryCacheKey key = entry3;
                        entry3.State = EntryState.RemovingFromCache;
                        this._entries.Remove(key);
                    }
                }
                foreach (MemoryCacheEntry entry4 in list)
                {
                    this.RemoveFromCache(entry4, CacheEntryRemovedReason.CacheSpecificEviction, false);
                }
                this._insertBlock.Close();
            }
        }

        internal MemoryCacheEntry Get(MemoryCacheKey key)
        {
            MemoryCacheEntry entryToRemove = this._entries[key] as MemoryCacheEntry;
            if ((entryToRemove != null) && (entryToRemove.UtcAbsExp <= DateTime.UtcNow))
            {
                this.Remove(key, entryToRemove, CacheEntryRemovedReason.Expired);
                entryToRemove = null;
            }
            this.UpdateExpAndUsage(entryToRemove);
            return entryToRemove;
        }

        private void InitDisposableMembers()
        {
            this._insertBlock = new ManualResetEvent(true);
            this._expires.EnableExpirationTimer(true);
        }

        internal MemoryCacheEntry Remove(MemoryCacheKey key, MemoryCacheEntry entryToRemove, CacheEntryRemovedReason reason)
        {
            MemoryCacheEntry objA = null;
            lock (this._entriesLock)
            {
                if (this._disposed == 0)
                {
                    objA = this._entries[key] as MemoryCacheEntry;
                    if ((entryToRemove == null) || object.ReferenceEquals(objA, entryToRemove))
                    {
                        if (objA != null)
                        {
                            objA.State = EntryState.RemovingFromCache;
                        }
                        this._entries.Remove(key);
                    }
                    else
                    {
                        objA = null;
                    }
                }
            }
            this.RemoveFromCache(objA, reason, false);
            return objA;
        }

        private void RemoveFromCache(MemoryCacheEntry entry, CacheEntryRemovedReason reason, bool delayRelease = false)
        {
            if (entry != null)
            {
                if (entry.InExpires())
                {
                    this._expires.Remove(entry);
                }
                if (entry.InUsage())
                {
                    this._usage.Remove(entry);
                }
                entry.State = EntryState.RemovedFromCache;
                if (!delayRelease)
                {
                    entry.Release(this._cache, reason);
                }
                if (this._perfCounters != null)
                {
                    this._perfCounters.Decrement(PerfCounterName.Entries);
                    this._perfCounters.Increment(PerfCounterName.Turnover);
                }
            }
        }

        internal void Set(MemoryCacheKey key, MemoryCacheEntry entry)
        {
            if (this._useInsertBlock && entry.HasUsage())
            {
                this.WaitInsertBlock();
            }
            MemoryCacheEntry entry2 = null;
            bool flag = false;
            lock (this._entriesLock)
            {
                if (this._disposed == 0)
                {
                    entry2 = this._entries[key] as MemoryCacheEntry;
                    if (entry2 != null)
                    {
                        entry2.State = EntryState.RemovingFromCache;
                    }
                    entry.State = EntryState.AddingToCache;
                    flag = true;
                    this._entries[key] = entry;
                }
            }
            CacheEntryRemovedReason removed = CacheEntryRemovedReason.Removed;
            if (entry2 != null)
            {
                if (entry2.UtcAbsExp <= DateTime.UtcNow)
                {
                    removed = CacheEntryRemovedReason.Expired;
                }
                bool delayRelease = true;
                this.RemoveFromCache(entry2, removed, delayRelease);
            }
            if (flag)
            {
                this.AddToCache(entry);
            }
            if (entry2 != null)
            {
                entry2.Release(this._cache, removed);
            }
        }

        internal long TrimInternal(int percent)
        {
            int count = this.Count;
            int num2 = 0;
            if (percent > 0)
            {
                num2 = (int) ((count * percent) / 100L);
                int num3 = count - 0x3fffffff;
                if (num2 < num3)
                {
                    num2 = num3;
                }
                int num4 = count - 10;
                if (num2 > num4)
                {
                    num2 = num4;
                }
            }
            if ((num2 <= 0) || (this._disposed == 1))
            {
                return 0L;
            }
            int num5 = 0;
            int num6 = 0;
            num6 = this._expires.FlushExpiredItems(true);
            if (num6 < num2)
            {
                num5 = this._usage.FlushUnderUsedItems(num2 - num6);
                num6 += num5;
            }
            if ((num5 > 0) && (this._perfCounters != null))
            {
                this._perfCounters.IncrementBy(PerfCounterName.Trims, (long) num5);
            }
            return (long) num6;
        }

        internal void UnblockInsert()
        {
            this._useInsertBlock = false;
            this._insertBlock.Set();
        }

        private void UpdateExpAndUsage(MemoryCacheEntry entry)
        {
            if (entry != null)
            {
                if (entry.InUsage() || (entry.SlidingExp > TimeSpan.Zero))
                {
                    DateTime utcNow = DateTime.UtcNow;
                    entry.UpdateSlidingExp(utcNow, this._expires);
                    entry.UpdateUsage(utcNow, this._usage);
                }
                if (this._perfCounters != null)
                {
                    this._perfCounters.Increment(PerfCounterName.Hits);
                    this._perfCounters.Increment(PerfCounterName.HitRatio);
                    this._perfCounters.Increment(PerfCounterName.HitRatioBase);
                }
            }
            else if (this._perfCounters != null)
            {
                this._perfCounters.Increment(PerfCounterName.Misses);
                this._perfCounters.Increment(PerfCounterName.HitRatioBase);
            }
        }

        private void WaitInsertBlock()
        {
            this._insertBlock.WaitOne(0x2710, false);
        }

        internal int Count
        {
            get
            {
                return this._entries.Count;
            }
        }

        internal CacheUsage Usage
        {
            get
            {
                return this._usage;
            }
        }
    }
}

