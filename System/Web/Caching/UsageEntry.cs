namespace System.Web.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal struct UsageEntry
    {
        [FieldOffset(0x18)]
        internal CacheEntry _cacheEntry;
        [FieldOffset(4)]
        internal int _cFree;
        [FieldOffset(0)]
        internal UsageEntryLink _ref1;
        [FieldOffset(8)]
        internal UsageEntryLink _ref2;
        [FieldOffset(0x10)]
        internal DateTime _utcDate;
    }
}

