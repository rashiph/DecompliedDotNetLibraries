namespace System.Runtime.Caching
{
    using System;

    public enum CacheEntryRemovedReason
    {
        Removed,
        Expired,
        Evicted,
        ChangeMonitorChanged,
        CacheSpecificEviction
    }
}

