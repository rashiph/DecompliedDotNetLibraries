namespace System.Web.Caching
{
    using System;

    internal sealed class UsageBucket
    {
        private UsageEntryRef _addRef2Head;
        private bool _blockReduce;
        private byte _bucket;
        private CacheUsage _cacheUsage;
        private int _cEntriesInFlush;
        private int _cEntriesInUse;
        private int _cPagesInUse;
        private UsagePageList _freeEntryList;
        private UsagePageList _freePageList;
        private UsageEntryRef _lastRefHead;
        private UsageEntryRef _lastRefTail;
        private int _minEntriesInUse;
        private UsagePage[] _pages;
        private const int LENGTH_ENTRIES = 0x80;
        private const int MAX_PAGES_INCREMENT = 340;
        private const double MIN_LOAD_FACTOR = 0.5;
        private const int MIN_PAGES_INCREMENT = 10;
        internal const int NUM_ENTRIES = 0x7f;

        internal UsageBucket(CacheUsage cacheUsage, byte bucket)
        {
            this._cacheUsage = cacheUsage;
            this._bucket = bucket;
            this.InitZeroPages();
        }

        internal void AddCacheEntry(CacheEntry cacheEntry)
        {
            lock (this)
            {
                if (this._freeEntryList._head == -1)
                {
                    this.Expand();
                }
                UsageEntryRef freeUsageEntry = this.GetFreeUsageEntry();
                UsageEntryRef ref3 = new UsageEntryRef(freeUsageEntry.PageIndex, -freeUsageEntry.Ref1Index);
                cacheEntry.UsageEntryRef = freeUsageEntry;
                UsageEntry[] entryArray = this._pages[freeUsageEntry.PageIndex]._entries;
                int index = freeUsageEntry.Ref1Index;
                entryArray[index]._cacheEntry = cacheEntry;
                entryArray[index]._utcDate = DateTime.UtcNow;
                entryArray[index]._ref1._prev = UsageEntryRef.INVALID;
                entryArray[index]._ref2._next = this._addRef2Head;
                if (this._lastRefHead.IsInvalid)
                {
                    entryArray[index]._ref1._next = ref3;
                    entryArray[index]._ref2._prev = freeUsageEntry;
                    this._lastRefTail = ref3;
                }
                else
                {
                    UsageEntryRef iNVALID;
                    UsageEntryRef ref5;
                    entryArray[index]._ref1._next = this._lastRefHead;
                    if (this._lastRefHead.IsRef1)
                    {
                        this._pages[this._lastRefHead.PageIndex]._entries[this._lastRefHead.Ref1Index]._ref1._prev = freeUsageEntry;
                    }
                    else if (this._lastRefHead.IsRef2)
                    {
                        this._pages[this._lastRefHead.PageIndex]._entries[this._lastRefHead.Ref2Index]._ref2._prev = freeUsageEntry;
                    }
                    else
                    {
                        this._lastRefTail = freeUsageEntry;
                    }
                    if (this._addRef2Head.IsInvalid)
                    {
                        ref5 = this._lastRefTail;
                        iNVALID = UsageEntryRef.INVALID;
                    }
                    else
                    {
                        ref5 = this._pages[this._addRef2Head.PageIndex]._entries[this._addRef2Head.Ref2Index]._ref2._prev;
                        iNVALID = this._addRef2Head;
                    }
                    entryArray[index]._ref2._prev = ref5;
                    if (ref5.IsRef1)
                    {
                        this._pages[ref5.PageIndex]._entries[ref5.Ref1Index]._ref1._next = ref3;
                    }
                    else if (ref5.IsRef2)
                    {
                        this._pages[ref5.PageIndex]._entries[ref5.Ref2Index]._ref2._next = ref3;
                    }
                    else
                    {
                        this._lastRefHead = ref3;
                    }
                    if (iNVALID.IsRef1)
                    {
                        this._pages[iNVALID.PageIndex]._entries[iNVALID.Ref1Index]._ref1._prev = ref3;
                    }
                    else if (iNVALID.IsRef2)
                    {
                        this._pages[iNVALID.PageIndex]._entries[iNVALID.Ref2Index]._ref2._prev = ref3;
                    }
                    else
                    {
                        this._lastRefTail = ref3;
                    }
                }
                this._lastRefHead = freeUsageEntry;
                this._addRef2Head = ref3;
                this._cEntriesInUse++;
            }
        }

        private void AddToListHead(int pageIndex, ref UsagePageList list)
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

        private void AddToListTail(int pageIndex, ref UsagePageList list)
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

        private void AddUsageEntryToFreeList(UsageEntryRef entryRef)
        {
            UsageEntry[] entryArray = this._pages[entryRef.PageIndex]._entries;
            int index = entryRef.Ref1Index;
            entryArray[index]._utcDate = DateTime.MinValue;
            entryArray[index]._ref1._prev = UsageEntryRef.INVALID;
            entryArray[index]._ref2._next = UsageEntryRef.INVALID;
            entryArray[index]._ref2._prev = UsageEntryRef.INVALID;
            entryArray[index]._ref1._next = entryArray[0]._ref1._next;
            entryArray[0]._ref1._next = entryRef;
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
                UsagePage[] pageArray = new UsagePage[Math.Min(Math.Max(length + 10, num2), length + 340)];
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
            UsageEntry[] entryArray = new UsageEntry[0x80];
            entryArray[0]._cFree = 0x7f;
            for (int i = 0; i < (entryArray.Length - 1); i++)
            {
                entryArray[i]._ref1._next = new UsageEntryRef(pageIndex, i + 1);
            }
            entryArray[entryArray.Length - 1]._ref1._next = UsageEntryRef.INVALID;
            this._pages[pageIndex]._entries = entryArray;
            this._cPagesInUse++;
            this.UpdateMinEntries();
        }

        internal int FlushUnderUsedItems(int maxFlush, bool force, ref int publicEntriesFlushed, ref int ocEntriesFlushed)
        {
            UsageEntry[] entryArray;
            int num;
            CacheEntry entry;
            UsageEntryRef ref6;
            UsageEntryRef ref7;
            if (this._cEntriesInUse == 0)
            {
                return 0;
            }
            UsageEntryRef iNVALID = UsageEntryRef.INVALID;
            int num2 = 0;
            try
            {
                this._cacheUsage.CacheSingle.BlockInsertIfNeeded();
                lock (this)
                {
                    UsageEntryRef ref4;
                    if (this._cEntriesInUse == 0)
                    {
                        return 0;
                    }
                    DateTime utcNow = DateTime.UtcNow;
                    for (UsageEntryRef ref3 = this._lastRefTail; (this._cEntriesInFlush < maxFlush) && !ref3.IsInvalid; ref3 = ref4)
                    {
                        for (ref4 = this._pages[ref3.PageIndex]._entries[ref3.Ref2Index]._ref2._prev; ref4.IsRef1; ref4 = this._pages[ref4.PageIndex]._entries[ref4.Ref1Index]._ref1._prev)
                        {
                        }
                        entryArray = this._pages[ref3.PageIndex]._entries;
                        num = ref3.Ref2Index;
                        if (!force)
                        {
                            DateTime time = entryArray[num]._utcDate;
                            if (((utcNow - time) <= CacheUsage.NEWADD_INTERVAL) && (utcNow >= time))
                            {
                                continue;
                            }
                        }
                        UsageEntryRef entryRef = new UsageEntryRef(ref3.PageIndex, ref3.Ref2Index);
                        entry = entryArray[num]._cacheEntry;
                        entry.UsageEntryRef = UsageEntryRef.INVALID;
                        if (entry.IsPublic)
                        {
                            publicEntriesFlushed++;
                        }
                        else if (entry.IsOutputCache)
                        {
                            ocEntriesFlushed++;
                        }
                        this.RemoveEntryFromLastRefList(entryRef);
                        entryArray[num]._ref1._next = iNVALID;
                        iNVALID = entryRef;
                        num2++;
                        this._cEntriesInFlush++;
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
                this._cacheUsage.CacheSingle.UnblockInsert();
            }
            CacheSingle cacheSingle = this._cacheUsage.CacheSingle;
            for (ref6 = iNVALID; !ref6.IsInvalid; ref6 = ref7)
            {
                entryArray = this._pages[ref6.PageIndex]._entries;
                num = ref6.Ref1Index;
                ref7 = entryArray[num]._ref1._next;
                entry = entryArray[num]._cacheEntry;
                entryArray[num]._cacheEntry = null;
                cacheSingle.Remove(entry, CacheItemRemovedReason.Underused);
            }
            try
            {
                this._cacheUsage.CacheSingle.BlockInsertIfNeeded();
                lock (this)
                {
                    for (ref6 = iNVALID; !ref6.IsInvalid; ref6 = ref7)
                    {
                        entryArray = this._pages[ref6.PageIndex]._entries;
                        num = ref6.Ref1Index;
                        ref7 = entryArray[num]._ref1._next;
                        this._cEntriesInFlush--;
                        this.AddUsageEntryToFreeList(ref6);
                    }
                    this._blockReduce = false;
                    this.Reduce();
                }
            }
            finally
            {
                this._cacheUsage.CacheSingle.UnblockInsert();
            }
            return num2;
        }

        private UsageEntryRef GetFreeUsageEntry()
        {
            int index = this._freeEntryList._head;
            UsageEntry[] entryArray = this._pages[index]._entries;
            int num2 = entryArray[0]._ref1._next.Ref1Index;
            entryArray[0]._ref1._next = entryArray[num2]._ref1._next;
            entryArray[0]._cFree--;
            if (entryArray[0]._cFree == 0)
            {
                this.RemoveFromList(index, ref this._freeEntryList);
            }
            return new UsageEntryRef(index, num2);
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

        private void MoveToListHead(int pageIndex, ref UsagePageList list)
        {
            if (list._head != pageIndex)
            {
                this.RemoveFromList(pageIndex, ref list);
                this.AddToListHead(pageIndex, ref list);
            }
        }

        private void MoveToListTail(int pageIndex, ref UsagePageList list)
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
                UsageEntry[] entryArray = this._pages[this._freeEntryList._tail]._entries;
                int num5 = ((this._cPagesInUse * 0x7f) - entryArray[0]._cFree) - this._cEntriesInUse;
                if (num5 < (0x7f - entryArray[0]._cFree))
                {
                    return;
                }
                for (int i = 1; i < entryArray.Length; i++)
                {
                    if (entryArray[i]._cacheEntry != null)
                    {
                        UsageEntryRef freeUsageEntry = this.GetFreeUsageEntry();
                        UsageEntryRef ref3 = new UsageEntryRef(freeUsageEntry.PageIndex, -freeUsageEntry.Ref1Index);
                        UsageEntryRef ref4 = new UsageEntryRef(this._freeEntryList._tail, i);
                        UsageEntryRef ref5 = new UsageEntryRef(ref4.PageIndex, -ref4.Ref1Index);
                        entryArray[i]._cacheEntry.UsageEntryRef = freeUsageEntry;
                        UsageEntry[] entryArray2 = this._pages[freeUsageEntry.PageIndex]._entries;
                        entryArray2[freeUsageEntry.Ref1Index] = entryArray[i];
                        entryArray[0]._cFree++;
                        UsageEntryRef ref6 = entryArray2[freeUsageEntry.Ref1Index]._ref1._prev;
                        UsageEntryRef ref7 = entryArray2[freeUsageEntry.Ref1Index]._ref1._next;
                        if (ref7 == ref5)
                        {
                            ref7 = ref3;
                        }
                        if (ref6.IsRef1)
                        {
                            this._pages[ref6.PageIndex]._entries[ref6.Ref1Index]._ref1._next = freeUsageEntry;
                        }
                        else if (ref6.IsRef2)
                        {
                            this._pages[ref6.PageIndex]._entries[ref6.Ref2Index]._ref2._next = freeUsageEntry;
                        }
                        else
                        {
                            this._lastRefHead = freeUsageEntry;
                        }
                        if (ref7.IsRef1)
                        {
                            this._pages[ref7.PageIndex]._entries[ref7.Ref1Index]._ref1._prev = freeUsageEntry;
                        }
                        else if (ref7.IsRef2)
                        {
                            this._pages[ref7.PageIndex]._entries[ref7.Ref2Index]._ref2._prev = freeUsageEntry;
                        }
                        else
                        {
                            this._lastRefTail = freeUsageEntry;
                        }
                        ref6 = entryArray2[freeUsageEntry.Ref1Index]._ref2._prev;
                        if (ref6 == ref4)
                        {
                            ref6 = freeUsageEntry;
                        }
                        ref7 = entryArray2[freeUsageEntry.Ref1Index]._ref2._next;
                        if (ref6.IsRef1)
                        {
                            this._pages[ref6.PageIndex]._entries[ref6.Ref1Index]._ref1._next = ref3;
                        }
                        else if (ref6.IsRef2)
                        {
                            this._pages[ref6.PageIndex]._entries[ref6.Ref2Index]._ref2._next = ref3;
                        }
                        else
                        {
                            this._lastRefHead = ref3;
                        }
                        if (ref7.IsRef1)
                        {
                            this._pages[ref7.PageIndex]._entries[ref7.Ref1Index]._ref1._prev = ref3;
                        }
                        else if (ref7.IsRef2)
                        {
                            this._pages[ref7.PageIndex]._entries[ref7.Ref2Index]._ref2._prev = ref3;
                        }
                        else
                        {
                            this._lastRefTail = ref3;
                        }
                        if (this._addRef2Head == ref5)
                        {
                            this._addRef2Head = ref3;
                        }
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
                UsageEntryRef usageEntryRef = cacheEntry.UsageEntryRef;
                if (!usageEntryRef.IsInvalid)
                {
                    UsageEntry[] entryArray = this._pages[usageEntryRef.PageIndex]._entries;
                    int index = usageEntryRef.Ref1Index;
                    cacheEntry.UsageEntryRef = UsageEntryRef.INVALID;
                    entryArray[index]._cacheEntry = null;
                    this.RemoveEntryFromLastRefList(usageEntryRef);
                    this.AddUsageEntryToFreeList(usageEntryRef);
                    this.Reduce();
                }
            }
        }

        private void RemoveEntryFromLastRefList(UsageEntryRef entryRef)
        {
            UsageEntry[] entryArray = this._pages[entryRef.PageIndex]._entries;
            int index = entryRef.Ref1Index;
            UsageEntryRef ref2 = entryArray[index]._ref1._prev;
            UsageEntryRef ref3 = entryArray[index]._ref1._next;
            if (ref2.IsRef1)
            {
                this._pages[ref2.PageIndex]._entries[ref2.Ref1Index]._ref1._next = ref3;
            }
            else if (ref2.IsRef2)
            {
                this._pages[ref2.PageIndex]._entries[ref2.Ref2Index]._ref2._next = ref3;
            }
            else
            {
                this._lastRefHead = ref3;
            }
            if (ref3.IsRef1)
            {
                this._pages[ref3.PageIndex]._entries[ref3.Ref1Index]._ref1._prev = ref2;
            }
            else if (ref3.IsRef2)
            {
                this._pages[ref3.PageIndex]._entries[ref3.Ref2Index]._ref2._prev = ref2;
            }
            else
            {
                this._lastRefTail = ref2;
            }
            ref2 = entryArray[index]._ref2._prev;
            ref3 = entryArray[index]._ref2._next;
            UsageEntryRef ref4 = new UsageEntryRef(entryRef.PageIndex, -entryRef.Ref1Index);
            if (ref2.IsRef1)
            {
                this._pages[ref2.PageIndex]._entries[ref2.Ref1Index]._ref1._next = ref3;
            }
            else if (ref2.IsRef2)
            {
                this._pages[ref2.PageIndex]._entries[ref2.Ref2Index]._ref2._next = ref3;
            }
            else
            {
                this._lastRefHead = ref3;
            }
            if (ref3.IsRef1)
            {
                this._pages[ref3.PageIndex]._entries[ref3.Ref1Index]._ref1._prev = ref2;
            }
            else if (ref3.IsRef2)
            {
                this._pages[ref3.PageIndex]._entries[ref3.Ref2Index]._ref2._prev = ref2;
            }
            else
            {
                this._lastRefTail = ref2;
            }
            if (this._addRef2Head == ref4)
            {
                this._addRef2Head = ref3;
            }
        }

        private void RemoveFromList(int pageIndex, ref UsagePageList list)
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

        private int RemoveFromListHead(ref UsagePageList list)
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

        internal void UpdateCacheEntry(CacheEntry cacheEntry)
        {
            lock (this)
            {
                UsageEntryRef usageEntryRef = cacheEntry.UsageEntryRef;
                if (!usageEntryRef.IsInvalid)
                {
                    UsageEntry[] entryArray = this._pages[usageEntryRef.PageIndex]._entries;
                    int index = usageEntryRef.Ref1Index;
                    UsageEntryRef ref3 = new UsageEntryRef(usageEntryRef.PageIndex, -usageEntryRef.Ref1Index);
                    UsageEntryRef ref4 = entryArray[index]._ref2._prev;
                    UsageEntryRef ref5 = entryArray[index]._ref2._next;
                    if (ref4.IsRef1)
                    {
                        this._pages[ref4.PageIndex]._entries[ref4.Ref1Index]._ref1._next = ref5;
                    }
                    else if (ref4.IsRef2)
                    {
                        this._pages[ref4.PageIndex]._entries[ref4.Ref2Index]._ref2._next = ref5;
                    }
                    else
                    {
                        this._lastRefHead = ref5;
                    }
                    if (ref5.IsRef1)
                    {
                        this._pages[ref5.PageIndex]._entries[ref5.Ref1Index]._ref1._prev = ref4;
                    }
                    else if (ref5.IsRef2)
                    {
                        this._pages[ref5.PageIndex]._entries[ref5.Ref2Index]._ref2._prev = ref4;
                    }
                    else
                    {
                        this._lastRefTail = ref4;
                    }
                    if (this._addRef2Head == ref3)
                    {
                        this._addRef2Head = ref5;
                    }
                    entryArray[index]._ref2 = entryArray[index]._ref1;
                    ref4 = entryArray[index]._ref2._prev;
                    ref5 = entryArray[index]._ref2._next;
                    if (ref4.IsRef1)
                    {
                        this._pages[ref4.PageIndex]._entries[ref4.Ref1Index]._ref1._next = ref3;
                    }
                    else if (ref4.IsRef2)
                    {
                        this._pages[ref4.PageIndex]._entries[ref4.Ref2Index]._ref2._next = ref3;
                    }
                    else
                    {
                        this._lastRefHead = ref3;
                    }
                    if (ref5.IsRef1)
                    {
                        this._pages[ref5.PageIndex]._entries[ref5.Ref1Index]._ref1._prev = ref3;
                    }
                    else if (ref5.IsRef2)
                    {
                        this._pages[ref5.PageIndex]._entries[ref5.Ref2Index]._ref2._prev = ref3;
                    }
                    else
                    {
                        this._lastRefTail = ref3;
                    }
                    entryArray[index]._ref1._prev = UsageEntryRef.INVALID;
                    entryArray[index]._ref1._next = this._lastRefHead;
                    if (this._lastRefHead.IsRef1)
                    {
                        this._pages[this._lastRefHead.PageIndex]._entries[this._lastRefHead.Ref1Index]._ref1._prev = usageEntryRef;
                    }
                    else if (this._lastRefHead.IsRef2)
                    {
                        this._pages[this._lastRefHead.PageIndex]._entries[this._lastRefHead.Ref2Index]._ref2._prev = usageEntryRef;
                    }
                    else
                    {
                        this._lastRefTail = usageEntryRef;
                    }
                    this._lastRefHead = usageEntryRef;
                }
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
    }
}

