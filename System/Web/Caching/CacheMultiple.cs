namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class CacheMultiple : CacheInternal
    {
        private int _cacheIndexMask;
        private CacheSingle[] _caches;
        private int _disposed;

        internal CacheMultiple(CacheCommon cacheCommon, int numSingleCaches) : base(cacheCommon)
        {
            this._cacheIndexMask = numSingleCaches - 1;
            this._caches = new CacheSingle[numSingleCaches];
            for (int i = 0; i < numSingleCaches; i++)
            {
                this._caches[i] = new CacheSingle(cacheCommon, this, i);
            }
        }

        internal override IDictionaryEnumerator CreateEnumerator()
        {
            IDictionaryEnumerator[] enumerators = new IDictionaryEnumerator[this._caches.Length];
            int index = 0;
            int length = this._caches.Length;
            while (index < length)
            {
                enumerators[index] = this._caches[index].CreateEnumerator();
                index++;
            }
            return new AggregateEnumerator(enumerators);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (Interlocked.Exchange(ref this._disposed, 1) == 0))
            {
                foreach (CacheSingle num in this._caches)
                {
                    num.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        internal override void EnableExpirationTimer(bool enable)
        {
            foreach (CacheSingle num in this._caches)
            {
                num.EnableExpirationTimer(enable);
            }
        }

        internal CacheSingle GetCacheSingle(int hashCode)
        {
            hashCode = Math.Abs(hashCode);
            int index = hashCode & this._cacheIndexMask;
            return this._caches[index];
        }

        internal override long TrimIfNecessary(int percent)
        {
            long num = 0L;
            foreach (CacheSingle num2 in this._caches)
            {
                num += num2.TrimIfNecessary(percent);
            }
            return num;
        }

        internal override CacheEntry UpdateCache(CacheKey cacheKey, CacheEntry newEntry, bool replace, CacheItemRemovedReason removedReason, out object valueOld)
        {
            int hashCode = cacheKey.Key.GetHashCode();
            return this.GetCacheSingle(hashCode).UpdateCache(cacheKey, newEntry, replace, removedReason, out valueOld);
        }

        internal override int PublicCount
        {
            get
            {
                int num = 0;
                foreach (CacheSingle num2 in this._caches)
                {
                    num += num2.PublicCount;
                }
                return num;
            }
        }

        internal override long TotalCount
        {
            get
            {
                long num = 0L;
                foreach (CacheSingle num2 in this._caches)
                {
                    num += num2.TotalCount;
                }
                return num;
            }
        }
    }
}

