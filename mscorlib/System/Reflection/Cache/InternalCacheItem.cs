namespace System.Reflection.Cache
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct InternalCacheItem
    {
        internal CacheObjType Key;
        internal object Value;
    }
}

