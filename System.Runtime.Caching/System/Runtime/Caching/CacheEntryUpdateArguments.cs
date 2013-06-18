namespace System.Runtime.Caching
{
    using System;

    public class CacheEntryUpdateArguments
    {
        private string _key;
        private CacheEntryRemovedReason _reason;
        private string _regionName;
        private ObjectCache _source;
        private CacheItem _updatedCacheItem;
        private CacheItemPolicy _updatedCacheItemPolicy;

        public CacheEntryUpdateArguments(ObjectCache source, CacheEntryRemovedReason reason, string key, string regionName)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this._source = source;
            this._reason = reason;
            this._key = key;
            this._regionName = regionName;
        }

        public string Key
        {
            get
            {
                return this._key;
            }
        }

        public string RegionName
        {
            get
            {
                return this._regionName;
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

        public CacheItem UpdatedCacheItem
        {
            get
            {
                return this._updatedCacheItem;
            }
            set
            {
                this._updatedCacheItem = value;
            }
        }

        public CacheItemPolicy UpdatedCacheItemPolicy
        {
            get
            {
                return this._updatedCacheItemPolicy;
            }
            set
            {
                this._updatedCacheItemPolicy = value;
            }
        }
    }
}

