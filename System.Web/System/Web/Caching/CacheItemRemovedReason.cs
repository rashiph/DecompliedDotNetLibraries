namespace System.Web.Caching
{
    using System;

    public enum CacheItemRemovedReason
    {
        DependencyChanged = 4,
        Expired = 2,
        Removed = 1,
        Underused = 3
    }
}

