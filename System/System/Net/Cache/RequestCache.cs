namespace System.Net.Cache
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime.InteropServices;

    internal abstract class RequestCache
    {
        private bool _CanWrite;
        private bool _IsPrivateCache;
        internal static readonly char[] LineSplits = new char[] { '\r', '\n' };

        protected RequestCache(bool isPrivateCache, bool canWrite)
        {
            this._IsPrivateCache = isPrivateCache;
            this._CanWrite = canWrite;
        }

        internal abstract void Remove(string key);
        internal abstract Stream Retrieve(string key, out RequestCacheEntry cacheEntry);
        internal abstract Stream Store(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata);
        internal abstract bool TryRemove(string key);
        internal abstract bool TryRetrieve(string key, out RequestCacheEntry cacheEntry, out Stream readStream);
        internal abstract bool TryStore(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, out Stream writeStream);
        internal abstract bool TryUpdate(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata);
        internal abstract void UnlockEntry(Stream retrieveStream);
        internal abstract void Update(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata);

        internal bool CanWrite
        {
            get
            {
                return this._CanWrite;
            }
        }

        internal bool IsPrivateCache
        {
            get
            {
                return this._IsPrivateCache;
            }
        }
    }
}

