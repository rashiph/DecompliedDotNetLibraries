namespace System.Web.Caching
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void CacheItemRemovedCallback(string key, object value, CacheItemRemovedReason reason);
}

