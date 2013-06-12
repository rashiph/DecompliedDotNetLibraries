namespace System.Net.Cache
{
    using System;

    public enum HttpRequestCacheLevel
    {
        Default,
        BypassCache,
        CacheOnly,
        CacheIfAvailable,
        Revalidate,
        Reload,
        NoCacheNoStore,
        CacheOrNextCacheOnly,
        Refresh
    }
}

