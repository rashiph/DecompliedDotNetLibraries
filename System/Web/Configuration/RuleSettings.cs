namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    public sealed class RuleSettings : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propCustom = new ConfigurationProperty("custom", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propEventName = new ConfigurationProperty("eventName", typeof(string), string.Empty, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propMaxLimit = new ConfigurationProperty("maxLimit", typeof(int), DEFAULT_MAX_LIMIT, new InfiniteIntConverter(), StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinInstances = new ConfigurationProperty("minInstances", typeof(int), DEFAULT_MIN_INSTANCES, null, StdValidatorsAndConverters.NonZeroPositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinInterval = new ConfigurationProperty("minInterval", typeof(TimeSpan), DEFAULT_MIN_INTERVAL, StdValidatorsAndConverters.InfiniteTimeSpanConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propProfile = new ConfigurationProperty("profile", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProvider = new ConfigurationProperty("provider", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        internal static string DEFAULT_CUSTOM_EVAL = null;
        internal static int DEFAULT_MAX_LIMIT = 0x7fffffff;
        internal static int DEFAULT_MIN_INSTANCES = 1;
        internal static TimeSpan DEFAULT_MIN_INTERVAL = TimeSpan.Zero;

        static RuleSettings()
        {
            _properties.Add(_propName);
            _properties.Add(_propEventName);
            _properties.Add(_propProvider);
            _properties.Add(_propProfile);
            _properties.Add(_propMinInstances);
            _properties.Add(_propMaxLimit);
            _properties.Add(_propMinInterval);
            _properties.Add(_propCustom);
        }

        internal RuleSettings()
        {
        }

        public RuleSettings(string name, string eventName, string provider) : this()
        {
            this.Name = name;
            this.EventName = eventName;
            this.Provider = provider;
        }

        public RuleSettings(string name, string eventName, string provider, string profile, int minInstances, int maxLimit, TimeSpan minInterval) : this(name, eventName, provider)
        {
            this.Profile = profile;
            this.MinInstances = minInstances;
            this.MaxLimit = maxLimit;
            this.MinInterval = minInterval;
        }

        public RuleSettings(string name, string eventName, string provider, string profile, int minInstances, int maxLimit, TimeSpan minInterval, string custom) : this(name, eventName, provider)
        {
            this.Profile = profile;
            this.MinInstances = minInstances;
            this.MaxLimit = maxLimit;
            this.MinInterval = minInterval;
            this.Custom = custom;
        }

        [ConfigurationProperty("custom", DefaultValue="")]
        public string Custom
        {
            get
            {
                return (string) base[_propCustom];
            }
            set
            {
                base[_propCustom] = value;
            }
        }

        [ConfigurationProperty("eventName", IsRequired=true, DefaultValue="")]
        public string EventName
        {
            get
            {
                return (string) base[_propEventName];
            }
            set
            {
                base[_propEventName] = value;
            }
        }

        [TypeConverter(typeof(InfiniteIntConverter)), ConfigurationProperty("maxLimit", DefaultValue=0x7fffffff), IntegerValidator(MinValue=0)]
        public int MaxLimit
        {
            get
            {
                return (int) base[_propMaxLimit];
            }
            set
            {
                base[_propMaxLimit] = value;
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("minInstances", DefaultValue=1)]
        public int MinInstances
        {
            get
            {
                return (int) base[_propMinInstances];
            }
            set
            {
                base[_propMinInstances] = value;
            }
        }

        [TypeConverter(typeof(InfiniteTimeSpanConverter)), ConfigurationProperty("minInterval", DefaultValue="00:00:00")]
        public TimeSpan MinInterval
        {
            get
            {
                return (TimeSpan) base[_propMinInterval];
            }
            set
            {
                base[_propMinInterval] = value;
            }
        }

        [ConfigurationProperty("name", IsRequired=true, IsKey=true, DefaultValue=""), StringValidator(MinLength=1)]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("profile", DefaultValue="")]
        public string Profile
        {
            get
            {
                return (string) base[_propProfile];
            }
            set
            {
                base[_propProfile] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("provider", DefaultValue="")]
        public string Provider
        {
            get
            {
                return (string) base[_propProvider];
            }
            set
            {
                base[_propProvider] = value;
            }
        }
    }
}

