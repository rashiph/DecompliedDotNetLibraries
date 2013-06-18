namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime;

    public class ChannelCacheSettings
    {
        internal static ChannelCacheSettings EmptyCacheSettings;
        private TimeSpan idleTimeout = ChannelCacheDefaults.DefaultIdleTimeout;
        private TimeSpan leaseTimeout = ChannelCacheDefaults.DefaultLeaseTimeout;
        private int maxItemsInCache = ChannelCacheDefaults.DefaultMaxItemsPerCache;

        static ChannelCacheSettings()
        {
            ChannelCacheSettings settings = new ChannelCacheSettings {
                MaxItemsInCache = 0
            };
            EmptyCacheSettings = settings;
        }

        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
            set
            {
                TimeoutHelper.ThrowIfNegativeArgument(value);
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("IdleTimeout", value, System.ServiceModel.Activities.SR.ValueTooLarge("IdleTimeout"));
                }
                this.idleTimeout = value;
            }
        }

        public TimeSpan LeaseTimeout
        {
            get
            {
                return this.leaseTimeout;
            }
            set
            {
                TimeoutHelper.ThrowIfNegativeArgument(value);
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("LeaseTimeout", value, System.ServiceModel.Activities.SR.ValueTooLarge("LeaseTimeout"));
                }
                this.leaseTimeout = value;
            }
        }

        public int MaxItemsInCache
        {
            get
            {
                return this.maxItemsInCache;
            }
            set
            {
                if (value < 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("MaxItemsInCache", value, System.ServiceModel.Activities.SR.ValueCannotBeNegative("MaxItemsInCache"));
                }
                this.maxItemsInCache = value;
            }
        }
    }
}

