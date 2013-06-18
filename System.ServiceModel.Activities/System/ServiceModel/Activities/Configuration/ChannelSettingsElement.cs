namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Configuration;

    public sealed class ChannelSettingsElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        [ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("idleTimeout", DefaultValue="00:02:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan IdleTimeout
        {
            get
            {
                return (TimeSpan) base["idleTimeout"];
            }
            set
            {
                base["idleTimeout"] = value;
            }
        }

        [ConfigurationProperty("leaseTimeout", DefaultValue="00:05:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan LeaseTimeout
        {
            get
            {
                return (TimeSpan) base["leaseTimeout"];
            }
            set
            {
                base["leaseTimeout"] = value;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("maxItemsInCache", DefaultValue="16")]
        public int MaxItemsInCache
        {
            get
            {
                return (int) base["maxItemsInCache"];
            }
            set
            {
                base["maxItemsInCache"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("maxItemsInCache", typeof(int), ChannelCacheDefaults.DefaultMaxItemsPerCache, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("idleTimeout", typeof(TimeSpan), ChannelCacheDefaults.DefaultIdleTimeout, new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("leaseTimeout", typeof(TimeSpan), "Infinite", new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

