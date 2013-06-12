namespace System.Net.Cache
{
    using System;

    internal enum CacheValidationStatus
    {
        DoNotUseCache,
        Fail,
        DoNotTakeFromCache,
        RetryResponseFromCache,
        RetryResponseFromServer,
        ReturnCachedResponse,
        CombineCachedAndServerResponse,
        CacheResponse,
        UpdateResponseInformation,
        RemoveFromCache,
        DoNotUpdateCache,
        Continue
    }
}

