namespace System.Runtime.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal struct ExpiresEntry
    {
        [FieldOffset(8)]
        internal MemoryCacheEntry _cacheEntry;
        [FieldOffset(4)]
        internal int _cFree;
        [FieldOffset(0)]
        internal ExpiresEntryRef _next;
        [FieldOffset(0)]
        internal DateTime _utcExpires;
    }
}

