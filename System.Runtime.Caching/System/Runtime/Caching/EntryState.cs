namespace System.Runtime.Caching
{
    using System;

    internal enum EntryState : byte
    {
        AddedToCache = 2,
        AddingToCache = 1,
        Closed = 0x10,
        NotInCache = 0,
        RemovedFromCache = 8,
        RemovingFromCache = 4
    }
}

