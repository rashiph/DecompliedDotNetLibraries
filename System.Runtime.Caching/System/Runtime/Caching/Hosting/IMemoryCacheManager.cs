namespace System.Runtime.Caching.Hosting
{
    using System;
    using System.Runtime.Caching;

    public interface IMemoryCacheManager
    {
        void ReleaseCache(MemoryCache cache);
        void UpdateCacheSize(long size, MemoryCache cache);
    }
}

