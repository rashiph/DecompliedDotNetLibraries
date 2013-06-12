namespace System.Net.Cache
{
    using System;

    public enum RequestCacheLevel
    {
        Default,
        BypassCache,
        CacheOnly,
        CacheIfAvailable,
        Revalidate,
        Reload,
        NoCacheNoStore
    }
}

