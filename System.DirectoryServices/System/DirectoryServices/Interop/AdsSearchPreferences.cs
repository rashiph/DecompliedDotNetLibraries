namespace System.DirectoryServices.Interop
{
    using System;

    internal enum AdsSearchPreferences
    {
        ASYNCHRONOUS,
        DEREF_ALIASES,
        SIZE_LIMIT,
        TIME_LIMIT,
        ATTRIBTYPES_ONLY,
        SEARCH_SCOPE,
        TIMEOUT,
        PAGESIZE,
        PAGED_TIME_LIMIT,
        CHASE_REFERRALS,
        SORT_ON,
        CACHE_RESULTS,
        DIRSYNC,
        TOMBSTONE,
        VLV,
        ATTRIBUTE_QUERY,
        SECURITY_MASK,
        DIRSYNC_FLAG,
        EXTENDED_DN
    }
}

