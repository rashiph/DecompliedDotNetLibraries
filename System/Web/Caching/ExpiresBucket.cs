namespace System.Web.Caching
{
    using System;

    internal sealed class ExpiresBucket
    {
        private bool _blockReduce;
        private readonly byte _bucket;
        private readonly CacheExpires _cacheExpires;
        private int _cEntriesInFlush;
        private int _cEntriesInUse;
        private int[] _counts;
        private int _cPagesInUse;
        private ExpiresPageList _freeEntryList;
        private ExpiresPageList _freePageList;
        private int _minEntriesInUse;
        private ExpiresPage[] _pages;
        private DateTime _utcLastCountReset;
        private DateTime _utcMinExpires;
        private static readonly TimeSpan COUNT_INTERVAL = new TimeSpan(CacheExpires._tsPerBucket.Ticks / 4L);
        private const int COUNTS_LENGTH = 4;
        private const int LENGTH_ENTRIES = 0x80;
        private const int MAX_PAGES_INCREMENT = 340;
        private const double MIN_LOAD_FACTOR = 0.5;
        private const int MIN_PAGES_INCREMENT = 10;
        internal const int NUM_ENTRIES = 0x7f;

        internal ExpiresBucket(CacheExpires cacheExpires, byte bucket, DateTime utcNow)
        {
            this._cacheExpires = cacheExpires;
            this._bucket = bucket;
            this._counts = new int[4];
            this.ResetCounts(utcNow);
            this.InitZeroPages();
        }

        internal void AddCacheEntry(CacheEntry cacheEntry)
        {
            lock (this)
            {
                if (((byte) (cacheEntry.State & (CacheEntry.EntryState.AddedToCache | CacheEntry.EntryState.AddingToCache))) != 0)
                {
                    ExpiresEntryRef expiresEntryRef = cacheEntry.ExpiresEntryRef;
                    if ((cacheEntry.ExpiresBucket == 0xff) && expiresEntryRef.IsInvalid)
                    {
                        if (this._freeEntryList._head == -1)
                        {
                            this.Expand();
                        }
                        ExpiresEntryRef freeExpiresEntry = this.GetFreeExpiresEntry();
                        cacheEntry.ExpiresBucket = this._bucket;
                        cacheEntry.ExpiresEntryRef = freeExpiresEntry;
                        ExpiresEntry[] entryArray = this._pages[freeExpiresEntry.PageIndex]._entries;
                        int index = freeExpiresEntry.Index;
                        entryArray[index]._cacheEntry = cacheEntry;
                        entryArray[index]._utcExpires = cacheEntry.UtcExpires;
                        this.AddCount(cacheEntry.UtcExpires);
                        this._cEntriesInUse++;
                        if (((byte) (cacheEntry.State & (CacheEntry.EntryState.AddedToCache | CacheEntry.EntryState.AddingToCache))) == 0)
                        {
                            this.RemoveCacheEntryNoLock(cacheEntry);
                        }
                    }
                }
            }
        }

        private void AddCount(DateTime utcExpires)
        {
            int countIndex = this.GetCountIndex(utcExpires);
            for (int i = this._counts.Length - 1; i >= countIndex; i--)
            {
                this._counts[i]++;
            }
            if (utcExpires < this._utcMinExpires)
            {
                this._utcMinExpires = utcExpires;
            }
        }

        private void AddExpiresEntryToFreeList(ExpiresEntryRef entryRef)
        {
            ExpiresEntry[] entryArray = this._pages[entryRef.PageIndex]._entries;
            int index = entryRef.Index;
            entryArray[index]._cFree = 0;
            entryArray[index]._next = entryArray[0]._next;
            entryArray[0]._next = entryRef;
            this._cEntriesInUse--;
            int pageIndex = entryRef.PageIndex;
            entryArray[0]._cFree++;
            if (entryArray[0]._cFree == 1)
            {
                this.AddToListHead(pageIndex, ref this._freeEntryList);
            }
            else if (entryArray[0]._cFree == 0x7f)
            {
                this.RemovePage(pageIndex);
            }
        }

        private void AddToListHead(int pageIndex, ref ExpiresPageList list)
        {
            this._pages[pageIndex]._pagePrev = -1;
            this._pages[pageIndex]._pageNext = list._head;
            if (list._head != -1)
            {
                this._pages[list._head]._pagePrev = pageIndex;
            }
            else
            {
                list._tail = pageIndex;
            }
            list._head = pageIndex;
        }

        private void AddToListTail(int pageIndex, ref ExpiresPageList list)
        {
            this._pages[pageIndex]._pageNext = -1;
            this._pages[pageIndex]._pagePrev = list._tail;
            if (list._tail != -1)
            {
                this._pages[list._tail]._pageNext = pageIndex;
            }
            else
            {
                list._head = pageIndex;
            }
            list._tail = pageIndex;
        }

        private void Expand()
        {
            if (this._freePageList._head == -1)
            {
                int length;
                if (this._pages == null)
                {
                    length = 0;
                }
                else
                {
                    length = this._pages.Length;
                }
                int num2 = length * 2;
                ExpiresPage[] pageArray = new ExpiresPage[Math.Min(Math.Max(length + 10, num2), length + 340)];
                for (int j = 0; j < length; j++)
                {
                    pageArray[j] = this._pages[j];
                }
                for (int k = length; k < pageArray.Length; k++)
                {
                    pageArray[k]._pagePrev = k - 1;
                    pageArray[k]._pageNext = k + 1;
                }
                pageArray[length]._pagePrev = -1;
                pageArray[pageArray.Length - 1]._pageNext = -1;
                this._freePageList._head = length;
                this._freePageList._tail = pageArray.Length - 1;
                this._pages = pageArray;
            }
            int pageIndex = this.RemoveFromListHead(ref this._freePageList);
            this.AddToListHead(pageIndex, ref this._freeEntryList);
            ExpiresEntry[] entryArray = new ExpiresEntry[0x80];
            entryArray[0]._cFree = 0x7f;
            for (int i = 0; i < (entryArray.Length - 1); i++)
            {
                entryArray[i]._next = new ExpiresEntryRef(pageIndex, i + 1);
            }
            entryArray[entryArray.Length - 1]._next = ExpiresEntryRef.INVALID;
            this._pages[pageIndex]._entries = entryArray;
            this._cPagesInUse++;
            this.UpdateMinEntries();
        }

        internal int FlushExpiredItems(DateTime utcNow, bool useInsertBlock)
        {
            ExpiresEntry[] entryArray;
            int index;
            CacheEntry entry;
            ExpiresEntryRef ref3;
            ExpiresEntryRef ref4;
            if ((this._cEntriesInUse == 0) || (this.GetExpiresCount(utcNow) == 0))
            {
                return 0;
            }
            ExpiresEntryRef iNVALID = ExpiresEntryRef.INVALID;
            int num2 = 0;
            try
            {
                if (useInsertBlock)
                {
                    this._cacheExpires.CacheSingle.BlockInsertIfNeeded();
                }
                lock (this)
                {
                    if ((this._cEntriesInUse == 0) || (this.GetExpiresCount(utcNow) == 0))
                    {
                        return 0;
                    }
                    this.ResetCounts(utcNow);
                    int num3 = this._cPagesInUse;
                    for (int i = 0; i < this._pages.Length; i++)
                    {
                        entryArray = this._pages[i]._entries;
                        if (entryArray != null)
                        {
                            int num5 = 0x7f - entryArray[0]._cFree;
                            for (int j = 1; j < entryArray.Length; j++)
                            {
                                entry = entryArray[j]._cacheEntry;
                                if (entry != null)
                                {
                                    if (entryArray[j]._utcExpires > utcNow)
                                    {
                                        this.AddCount(entryArray[j]._utcExpires);
                                    }
                                    else
                                    {
                                        entry.ExpiresBucket = 0xff;
                                        entry.ExpiresEntryRef = ExpiresEntryRef.INVALID;
                                        entryArray[j]._cFree = 1;
                                        entryArray[j]._next = iNVALID;
                                        iNVALID = new ExpiresEntryRef(i, j);
                                        num2++;
                                        this._cEntriesInFlush++;
                                    }
                                    num5--;
                                    if (num5 == 0)
                                    {
                                        break;
                                    }
                                }
                            }
                            num3--;
                            if (num3 == 0)
                            {
                                break;
                            }
                        }
                    }
                    if (num2 == 0)
                    {
                        return 0;
                    }
                    this._blockReduce = true;
                }
            }
            finally
            {
                if (useInsertBlock)
                {
                    this._cacheExpires.CacheSingle.UnblockInsert();
                }
            }
            CacheSingle cacheSingle = this._cacheExpires.CacheSingle;
            for (ref3 = iNVALID; !ref3.IsInvalid; ref3 = ref4)
            {
                entryArray = this._pages[ref3.PageIndex]._entries;
                index = ref3.Index;
                ref4 = entryArray[index]._next;
                entry = entryArray[index]._cacheEntry;
                entryArray[index]._cacheEntry = null;
                cacheSingle.Remove(entry, CacheItemRemovedReason.Expired);
            }
            try
            {
                if (useInsertBlock)
                {
                    this._cacheExpires.CacheSingle.BlockInsertIfNeeded();
                }
                lock (this)
                {
                    for (ref3 = iNVALID; !ref3.IsInvalid; ref3 = ref4)
                    {
                        entryArray = this._pages[ref3.PageIndex]._entries;
                        index = ref3.Index;
                        ref4 = entryArray[index]._next;
                        this._cEntriesInFlush--;
                        this.AddExpiresEntryToFreeList(ref3);
                    }
                    this._blockReduce = false;
                    this.Reduce();
                }
            }
            finally
            {
                if (useInsertBlock)
                {
                    this._cacheExpires.CacheSingle.UnblockInsert();
                }
            }
            return num2;
        }

        private int GetCountIndex(DateTime utcExpires)
        {
            TimeSpan span = (TimeSpan) (utcExpires - this._utcLastCountReset);
            return Math.Max(0, (int) (span.Ticks / COUNT_INTERVAL.Ticks));
        }

        private int GetExpiresCount(DateTime utcExpires)
        {
            if (utcExpires < this._utcMinExpires)
            {
                return 0;
            }
            int countIndex = this.GetCountIndex(utcExpires);
            if (countIndex >= this._counts.Length)
            {
                return this._cEntriesInUse;
            }
            return this._counts[countIndex];
        }

        private ExpiresEntryRef GetFreeExpiresEntry()
        {
            int index = this._freeEntryList._head;
            ExpiresEntry[] entryArray = this._pages[index]._entries;
            int num2 = entryArray[0]._next.Index;
            entryArray[0]._next = entryArray[num2]._next;
            entryArray[0]._cFree--;
            if (entryArray[0]._cFree == 0)
            {
                this.RemoveFromList(index, ref this._freeEntryList);
            }
            return new ExpiresEntryRef(index, num2);
        }

        private void InitZeroPages()
        {
            this._pages = null;
            this._minEntriesInUse = -1;
            this._freePageList._head = -1;
            this._freePageList._tail = -1;
            this._freeEntryList._head = -1;
            this._freeEntryList._tail = -1;
        }

        private void MoveToListHead(int pageIndex, ref ExpiresPageList list)
        {
            if (list._head != pageIndex)
            {
                this.RemoveFromList(pageIndex, ref list);
                this.AddToListHead(pageIndex, ref list);
            }
        }

        private void MoveToListTail(int pageIndex, ref ExpiresPageList list)
        {
            if (list._tail != pageIndex)
            {
                this.RemoveFromList(pageIndex, ref list);
                this.AddToListTail(pageIndex, ref list);
            }
        }

        private void Reduce()
        {
            int num4;
            if ((this._cEntriesInUse >= this._minEntriesInUse) || this._blockReduce)
            {
                return;
            }
            int num = 0x3f;
            int num2 = this._freeEntryList._tail;
            int index = this._freeEntryList._head;
        Label_0032:
            num4 = this._pages[index]._pageNext;
            if (this._pages[index]._entries[0]._cFree > num)
            {
                this.MoveToListTail(index, ref this._freeEntryList);
            }
            else
            {
                this.MoveToListHead(index, ref this._freeEntryList);
            }
            if (index != num2)
            {
                index = num4;
                goto Label_0032;
            }
            while (this._freeEntryList._tail != -1)
            {
                ExpiresEntry[] entryArray = this._pages[this._freeEntryList._tail]._entries;
                int num5 = ((this._cPagesInUse * 0x7f) - entryArray[0]._cFree) - this._cEntriesInUse;
                if (num5 < (0x7f - entryArray[0]._cFree))
                {
                    return;
                }
                for (int i = 1; i < entryArray.Length; i++)
                {
                    if (entryArray[i]._cacheEntry != null)
                    {
                        ExpiresEntryRef freeExpiresEntry = this.GetFreeExpiresEntry();
                        entryArray[i]._cacheEntry.ExpiresEntryRef = freeExpiresEntry;
                        this._pages[freeExpiresEntry.PageIndex]._entries[freeExpiresEntry.Index] = entryArray[i];
                        entryArray[0]._cFree++;
                    }
                }
                this.RemovePage(this._freeEntryList._tail);
            }
            return;
        }

        internal void RemoveCacheEntry(CacheEntry cacheEntry)
        {
            lock (this)
            {
                this.RemoveCacheEntryNoLock(cacheEntry);
            }
        }

        private void RemoveCacheEntryNoLock(CacheEntry cacheEntry)
        {
            ExpiresEntryRef expiresEntryRef = cacheEntry.ExpiresEntryRef;
            if ((cacheEntry.ExpiresBucket == this._bucket) && !expiresEntryRef.IsInvalid)
            {
                ExpiresEntry[] entryArray = this._pages[expiresEntryRef.PageIndex]._entries;
                int index = expiresEntryRef.Index;
                this.RemoveCount(entryArray[index]._utcExpires);
                cacheEntry.ExpiresBucket = 0xff;
                cacheEntry.ExpiresEntryRef = ExpiresEntryRef.INVALID;
                entryArray[index]._cacheEntry = null;
                this.AddExpiresEntryToFreeList(expiresEntryRef);
                if (this._cEntriesInUse == 0)
                {
                    this.ResetCounts(DateTime.UtcNow);
                }
                this.Reduce();
            }
        }

        private void RemoveCount(DateTime utcExpires)
        {
            int countIndex = this.GetCountIndex(utcExpires);
            for (int i = this._counts.Length - 1; i >= countIndex; i--)
            {
                this._counts[i]--;
            }
        }

        private void RemoveFromList(int pageIndex, ref ExpiresPageList list)
        {
            if (this._pages[pageIndex]._pagePrev != -1)
            {
                this._pages[this._pages[pageIndex]._pagePrev]._pageNext = this._pages[pageIndex]._pageNext;
            }
            else
            {
                list._head = this._pages[pageIndex]._pageNext;
            }
            if (this._pages[pageIndex]._pageNext != -1)
            {
                this._pages[this._pages[pageIndex]._pageNext]._pagePrev = this._pages[pageIndex]._pagePrev;
            }
            else
            {
                list._tail = this._pages[pageIndex]._pagePrev;
            }
            this._pages[pageIndex]._pagePrev = -1;
            this._pages[pageIndex]._pageNext = -1;
        }

        private int RemoveFromListHead(ref ExpiresPageList list)
        {
            int pageIndex = list._head;
            this.RemoveFromList(pageIndex, ref list);
            return pageIndex;
        }

        private void RemovePage(int pageIndex)
        {
            this.RemoveFromList(pageIndex, ref this._freeEntryList);
            this.AddToListHead(pageIndex, ref this._freePageList);
            this._pages[pageIndex]._entries = null;
            this._cPagesInUse--;
            if (this._cPagesInUse == 0)
            {
                this.InitZeroPages();
            }
            else
            {
                this.UpdateMinEntries();
            }
        }

        private void ResetCounts(DateTime utcNow)
        {
            this._utcLastCountReset = utcNow;
            this._utcMinExpires = DateTime.MaxValue;
            for (int i = 0; i < this._counts.Length; i++)
            {
                this._counts[i] = 0;
            }
        }

        private void UpdateMinEntries()
        {
            if (this._cPagesInUse <= 1)
            {
                this._minEntriesInUse = -1;
            }
            else
            {
                int num = this._cPagesInUse * 0x7f;
                this._minEntriesInUse = (int) (num * 0.5);
                if ((this._minEntriesInUse - 1) > ((this._cPagesInUse - 1) * 0x7f))
                {
                    this._minEntriesInUse = -1;
                }
            }
        }

        internal void UtcUpdateCacheEntry(CacheEntry cacheEntry, DateTime utcExpires)
        {
            lock (this)
            {
                ExpiresEntryRef expiresEntryRef = cacheEntry.ExpiresEntryRef;
                if ((cacheEntry.ExpiresBucket == this._bucket) && !expiresEntryRef.IsInvalid)
                {
                    ExpiresEntry[] entryArray = this._pages[expiresEntryRef.PageIndex]._entries;
                    int index = expiresEntryRef.Index;
                    this.RemoveCount(entryArray[index]._utcExpires);
                    this.AddCount(utcExpires);
                    entryArray[index]._utcExpires = utcExpires;
                    cacheEntry.UtcExpires = utcExpires;
                }
            }
        }
    }
}

