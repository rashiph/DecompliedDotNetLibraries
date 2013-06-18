namespace System.Runtime.Caching
{
    using System;

    [Flags]
    public enum DefaultCacheCapabilities
    {
        AbsoluteExpirations = 8,
        CacheEntryChangeMonitors = 4,
        CacheEntryRemovedCallback = 0x40,
        CacheEntryUpdateCallback = 0x20,
        CacheRegions = 0x80,
        InMemoryProvider = 1,
        None = 0,
        OutOfProcessProvider = 2,
        SlidingExpirations = 0x10
    }
}

