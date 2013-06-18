namespace System.Runtime.Caching
{
    using System;
    using System.Collections.ObjectModel;

    public abstract class CacheEntryChangeMonitor : ChangeMonitor
    {
        protected CacheEntryChangeMonitor()
        {
        }

        public abstract ReadOnlyCollection<string> CacheKeys { get; }

        public abstract DateTimeOffset LastModified { get; }

        public abstract string RegionName { get; }
    }
}

