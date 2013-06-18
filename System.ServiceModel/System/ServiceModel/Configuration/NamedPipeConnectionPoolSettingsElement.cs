namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public sealed class NamedPipeConnectionPoolSettingsElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(NamedPipeConnectionPoolSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            settings.GroupName = this.GroupName;
            settings.IdleTimeout = this.IdleTimeout;
            settings.MaxOutboundConnectionsPerEndpoint = this.MaxOutboundConnectionsPerEndpoint;
        }

        internal void CopyFrom(NamedPipeConnectionPoolSettingsElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.GroupName = source.GroupName;
            this.IdleTimeout = source.IdleTimeout;
            this.MaxOutboundConnectionsPerEndpoint = source.MaxOutboundConnectionsPerEndpoint;
        }

        internal void InitializeFrom(NamedPipeConnectionPoolSettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            this.GroupName = settings.GroupName;
            this.IdleTimeout = settings.IdleTimeout;
            this.MaxOutboundConnectionsPerEndpoint = settings.MaxOutboundConnectionsPerEndpoint;
        }

        [ConfigurationProperty("groupName", DefaultValue="default"), StringValidator(MinLength=0)]
        public string GroupName
        {
            get
            {
                return (string) base["groupName"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["groupName"] = value;
            }
        }

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

        [IntegerValidator(MinValue=0), ConfigurationProperty("maxOutboundConnectionsPerEndpoint", DefaultValue=10)]
        public int MaxOutboundConnectionsPerEndpoint
        {
            get
            {
                return (int) base["maxOutboundConnectionsPerEndpoint"];
            }
            set
            {
                base["maxOutboundConnectionsPerEndpoint"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("groupName", typeof(string), "default", null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("idleTimeout", typeof(TimeSpan), TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxOutboundConnectionsPerEndpoint", typeof(int), 10, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

