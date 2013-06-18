namespace System.Web.UI
{
    using System;
    using System.Collections;

    [Serializable]
    internal class PartialCachingCacheEntry
    {
        internal Guid _cachedVaryId;
        internal string[] _dependencies;
        internal string _dependenciesKey;
        internal string CssStyleString;
        internal string OutputString;
        internal ArrayList RegisteredClientCalls;
    }
}

