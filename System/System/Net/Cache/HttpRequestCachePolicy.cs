namespace System.Net.Cache
{
    using System;
    using System.Globalization;

    public class HttpRequestCachePolicy : RequestCachePolicy
    {
        internal static readonly HttpRequestCachePolicy BypassCache = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);
        private DateTime m_LastSyncDateUtc;
        private HttpRequestCacheLevel m_Level;
        private TimeSpan m_MaxAge;
        private TimeSpan m_MaxStale;
        private TimeSpan m_MinFresh;

        public HttpRequestCachePolicy() : this(HttpRequestCacheLevel.Default)
        {
        }

        public HttpRequestCachePolicy(DateTime cacheSyncDate) : this(HttpRequestCacheLevel.Default)
        {
            this.m_LastSyncDateUtc = cacheSyncDate.ToUniversalTime();
        }

        public HttpRequestCachePolicy(HttpRequestCacheLevel level) : base(MapLevel(level))
        {
            this.m_LastSyncDateUtc = DateTime.MinValue;
            this.m_MaxAge = TimeSpan.MaxValue;
            this.m_MinFresh = TimeSpan.MinValue;
            this.m_MaxStale = TimeSpan.MinValue;
            this.m_Level = level;
        }

        public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan ageOrFreshOrStale) : this(HttpRequestCacheLevel.Default)
        {
            switch (cacheAgeControl)
            {
                case HttpCacheAgeControl.MinFresh:
                    this.m_MinFresh = ageOrFreshOrStale;
                    return;

                case HttpCacheAgeControl.MaxAge:
                    this.m_MaxAge = ageOrFreshOrStale;
                    return;

                case HttpCacheAgeControl.MaxStale:
                    this.m_MaxStale = ageOrFreshOrStale;
                    return;
            }
            throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "HttpCacheAgeControl" }), "cacheAgeControl");
        }

        public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan maxAge, TimeSpan freshOrStale) : this(HttpRequestCacheLevel.Default)
        {
            switch (cacheAgeControl)
            {
                case HttpCacheAgeControl.MinFresh:
                    this.m_MinFresh = freshOrStale;
                    return;

                case HttpCacheAgeControl.MaxAge:
                    this.m_MaxAge = maxAge;
                    return;

                case HttpCacheAgeControl.MaxAgeAndMinFresh:
                    this.m_MaxAge = maxAge;
                    this.m_MinFresh = freshOrStale;
                    return;

                case HttpCacheAgeControl.MaxStale:
                    this.m_MaxStale = freshOrStale;
                    return;

                case HttpCacheAgeControl.MaxAgeAndMaxStale:
                    this.m_MaxAge = maxAge;
                    this.m_MaxStale = freshOrStale;
                    return;
            }
            throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "HttpCacheAgeControl" }), "cacheAgeControl");
        }

        public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan maxAge, TimeSpan freshOrStale, DateTime cacheSyncDate) : this(cacheAgeControl, maxAge, freshOrStale)
        {
            this.m_LastSyncDateUtc = cacheSyncDate.ToUniversalTime();
        }

        private static RequestCacheLevel MapLevel(HttpRequestCacheLevel level)
        {
            if (level <= HttpRequestCacheLevel.NoCacheNoStore)
            {
                return (RequestCacheLevel) level;
            }
            if (level == HttpRequestCacheLevel.CacheOrNextCacheOnly)
            {
                return RequestCacheLevel.CacheOnly;
            }
            if (level != HttpRequestCacheLevel.Refresh)
            {
                throw new ArgumentOutOfRangeException("level");
            }
            return RequestCacheLevel.Reload;
        }

        public override string ToString()
        {
            return ("Level:" + this.m_Level.ToString() + ((this.m_MaxAge == TimeSpan.MaxValue) ? string.Empty : (" MaxAge:" + this.m_MaxAge.ToString())) + ((this.m_MinFresh == TimeSpan.MinValue) ? string.Empty : (" MinFresh:" + this.m_MinFresh.ToString())) + ((this.m_MaxStale == TimeSpan.MinValue) ? string.Empty : (" MaxStale:" + this.m_MaxStale.ToString())) + ((this.CacheSyncDate == DateTime.MinValue) ? string.Empty : (" CacheSyncDate:" + this.CacheSyncDate.ToString(CultureInfo.CurrentCulture))));
        }

        public DateTime CacheSyncDate
        {
            get
            {
                if (!(this.m_LastSyncDateUtc == DateTime.MinValue) && !(this.m_LastSyncDateUtc == DateTime.MaxValue))
                {
                    return this.m_LastSyncDateUtc.ToLocalTime();
                }
                return this.m_LastSyncDateUtc;
            }
        }

        internal DateTime InternalCacheSyncDateUtc
        {
            get
            {
                return this.m_LastSyncDateUtc;
            }
        }

        public HttpRequestCacheLevel Level
        {
            get
            {
                return this.m_Level;
            }
        }

        public TimeSpan MaxAge
        {
            get
            {
                return this.m_MaxAge;
            }
        }

        public TimeSpan MaxStale
        {
            get
            {
                return this.m_MaxStale;
            }
        }

        public TimeSpan MinFresh
        {
            get
            {
                return this.m_MinFresh;
            }
        }
    }
}

