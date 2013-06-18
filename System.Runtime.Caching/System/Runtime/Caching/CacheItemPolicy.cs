namespace System.Runtime.Caching
{
    using System;
    using System.Collections.ObjectModel;

    public class CacheItemPolicy
    {
        private DateTimeOffset _absExpiry = ObjectCache.InfiniteAbsoluteExpiration;
        private Collection<ChangeMonitor> _changeMonitors;
        private CacheItemPriority _priority = CacheItemPriority.Default;
        private CacheEntryRemovedCallback _removedCallback;
        private TimeSpan _sldExpiry = ObjectCache.NoSlidingExpiration;
        private CacheEntryUpdateCallback _updateCallback;

        public DateTimeOffset AbsoluteExpiration
        {
            get
            {
                return this._absExpiry;
            }
            set
            {
                this._absExpiry = value;
            }
        }

        public Collection<ChangeMonitor> ChangeMonitors
        {
            get
            {
                if (this._changeMonitors == null)
                {
                    this._changeMonitors = new Collection<ChangeMonitor>();
                }
                return this._changeMonitors;
            }
        }

        public CacheItemPriority Priority
        {
            get
            {
                return this._priority;
            }
            set
            {
                this._priority = value;
            }
        }

        public CacheEntryRemovedCallback RemovedCallback
        {
            get
            {
                return this._removedCallback;
            }
            set
            {
                this._removedCallback = value;
            }
        }

        public TimeSpan SlidingExpiration
        {
            get
            {
                return this._sldExpiry;
            }
            set
            {
                this._sldExpiry = value;
            }
        }

        public CacheEntryUpdateCallback UpdateCallback
        {
            get
            {
                return this._updateCallback;
            }
            set
            {
                this._updateCallback = value;
            }
        }
    }
}

