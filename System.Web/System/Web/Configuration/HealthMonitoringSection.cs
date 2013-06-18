namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    public sealed class HealthMonitoringSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propBufferModes = new ConfigurationProperty("bufferModes", typeof(BufferModesCollection), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnabled = new ConfigurationProperty("enabled", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propEventMappingSettingsCollection = new ConfigurationProperty("eventMappings", typeof(EventMappingSettingsCollection), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propHeartbeatInterval = new ConfigurationProperty("heartbeatInterval", typeof(TimeSpan), TimeSpan.FromSeconds(0.0), StdValidatorsAndConverters.TimeSpanSecondsConverter, new TimeSpanValidator(TimeSpan.Zero, TimeSpan.FromSeconds(2147483.0)), ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProfileSettingsCollection = new ConfigurationProperty("profiles", typeof(ProfileSettingsCollection), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProviders = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRuleSettingsCollection = new ConfigurationProperty("rules", typeof(RuleSettingsCollection), null, ConfigurationPropertyOptions.None);
        private const bool DEFAULT_HEALTH_MONITORING_ENABLED = true;
        private const int DEFAULT_HEARTBEATINTERVAL = 0;
        private const int MAX_HEARTBEAT_VALUE = 0x20c49b;

        static HealthMonitoringSection()
        {
            _properties.Add(_propHeartbeatInterval);
            _properties.Add(_propEnabled);
            _properties.Add(_propBufferModes);
            _properties.Add(_propProviders);
            _properties.Add(_propProfileSettingsCollection);
            _properties.Add(_propRuleSettingsCollection);
            _properties.Add(_propEventMappingSettingsCollection);
        }

        [ConfigurationProperty("bufferModes")]
        public BufferModesCollection BufferModes
        {
            get
            {
                return (BufferModesCollection) base[_propBufferModes];
            }
        }

        [ConfigurationProperty("enabled", DefaultValue=true)]
        public bool Enabled
        {
            get
            {
                return (bool) base[_propEnabled];
            }
            set
            {
                base[_propEnabled] = value;
            }
        }

        [ConfigurationProperty("eventMappings")]
        public EventMappingSettingsCollection EventMappings
        {
            get
            {
                return (EventMappingSettingsCollection) base[_propEventMappingSettingsCollection];
            }
        }

        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString="24.20:31:23"), ConfigurationProperty("heartbeatInterval", DefaultValue="00:00:00"), TypeConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan HeartbeatInterval
        {
            get
            {
                return (TimeSpan) base[_propHeartbeatInterval];
            }
            set
            {
                base[_propHeartbeatInterval] = value;
            }
        }

        [ConfigurationProperty("profiles")]
        public ProfileSettingsCollection Profiles
        {
            get
            {
                return (ProfileSettingsCollection) base[_propProfileSettingsCollection];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection) base[_propProviders];
            }
        }

        [ConfigurationProperty("rules")]
        public RuleSettingsCollection Rules
        {
            get
            {
                return (RuleSettingsCollection) base[_propRuleSettingsCollection];
            }
        }
    }
}

