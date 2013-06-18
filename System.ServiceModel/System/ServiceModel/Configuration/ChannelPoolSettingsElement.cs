namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public sealed class ChannelPoolSettingsElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(ChannelPoolSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            settings.IdleTimeout = this.IdleTimeout;
            settings.LeaseTimeout = this.LeaseTimeout;
            settings.MaxOutboundChannelsPerEndpoint = this.MaxOutboundChannelsPerEndpoint;
        }

        internal void CopyFrom(ChannelPoolSettingsElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.IdleTimeout = source.IdleTimeout;
            this.LeaseTimeout = source.LeaseTimeout;
            this.MaxOutboundChannelsPerEndpoint = source.MaxOutboundChannelsPerEndpoint;
        }

        internal void InitializeFrom(ChannelPoolSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            this.IdleTimeout = settings.IdleTimeout;
            this.LeaseTimeout = settings.LeaseTimeout;
            this.MaxOutboundChannelsPerEndpoint = settings.MaxOutboundChannelsPerEndpoint;
        }

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("idleTimeout", DefaultValue="00:02:00")]
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

        [ConfigurationProperty("leaseTimeout", DefaultValue="00:10:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
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

        [IntegerValidator(MinValue=1), ConfigurationProperty("maxOutboundChannelsPerEndpoint", DefaultValue=10)]
        public int MaxOutboundChannelsPerEndpoint
        {
            get
            {
                return (int) base["maxOutboundChannelsPerEndpoint"];
            }
            set
            {
                base["maxOutboundChannelsPerEndpoint"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("idleTimeout", typeof(TimeSpan), TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("leaseTimeout", typeof(TimeSpan), TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxOutboundChannelsPerEndpoint", typeof(int), 10, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

