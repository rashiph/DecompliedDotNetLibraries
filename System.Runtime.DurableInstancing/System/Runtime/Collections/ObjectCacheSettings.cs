namespace System.Runtime.Collections
{
    using System;
    using System.Runtime;

    internal class ObjectCacheSettings
    {
        private int cacheLimit;
        private const int DefaultCacheLimit = 0x40;
        private static TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(2.0);
        private static TimeSpan DefaultLeaseTimeout = TimeSpan.FromMinutes(5.0);
        private const int DefaultPurgeFrequency = 0x20;
        private TimeSpan idleTimeout;
        private TimeSpan leaseTimeout;
        private int purgeFrequency;

        public ObjectCacheSettings()
        {
            this.CacheLimit = 0x40;
            this.IdleTimeout = DefaultIdleTimeout;
            this.LeaseTimeout = DefaultLeaseTimeout;
            this.PurgeFrequency = 0x20;
        }

        private ObjectCacheSettings(ObjectCacheSettings other)
        {
            this.CacheLimit = other.CacheLimit;
            this.IdleTimeout = other.IdleTimeout;
            this.LeaseTimeout = other.LeaseTimeout;
            this.PurgeFrequency = other.PurgeFrequency;
        }

        internal ObjectCacheSettings Clone()
        {
            return new ObjectCacheSettings(this);
        }

        public int CacheLimit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cacheLimit;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.cacheLimit = value;
            }
        }

        public TimeSpan IdleTimeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.idleTimeout;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.idleTimeout = value;
            }
        }

        public TimeSpan LeaseTimeout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.leaseTimeout;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.leaseTimeout = value;
            }
        }

        public int PurgeFrequency
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.purgeFrequency;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.purgeFrequency = value;
            }
        }
    }
}

