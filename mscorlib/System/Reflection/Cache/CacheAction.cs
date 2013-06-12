namespace System.Reflection.Cache
{
    using System;

    [Serializable]
    internal enum CacheAction
    {
        AddItem = 2,
        AllocateCache = 1,
        ClearCache = 3,
        GrowCache = 6,
        LookupItemHit = 4,
        LookupItemMiss = 5,
        ReplaceFailed = 8,
        SetItemReplace = 7
    }
}

