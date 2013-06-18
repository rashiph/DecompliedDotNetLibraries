namespace System.Runtime.Caching
{
    using System;

    public class CacheEntryRemovedArguments
    {
        private System.Runtime.Caching.CacheItem _cacheItem;
        private CacheEntryRemovedReason _reason;
        private ObjectCache _source;

        public CacheEntryRemovedArguments(ObjectCache source, CacheEntryRemovedReason reason, System.Runtime.Caching.CacheItem cacheItem)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (cacheItem == null)
            {
                throw new ArgumentNullException("cacheItem");
            }
            this._source = source;
            this._reason = reason;
            this._cacheItem = cacheItem;
        }

        public System.Runtime.Caching.CacheItem CacheItem
        {
            get
            {
                return this._cacheItem;
            }
        }

        public CacheEntryRemovedReason RemovedReason
        {
            get
            {
                return this._reason;
            }
        }

        public ObjectCache Source
        {
            get
            {
                return this._source;
            }
        }
    }
}

