namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    public sealed class ProfileSettings : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propCustom = new ConfigurationProperty("custom", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propMaxLimit = new ConfigurationProperty("maxLimit", typeof(int), RuleSettings.DEFAULT_MAX_LIMIT, new InfiniteIntConverter(), StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinInstances = new ConfigurationProperty("minInstances", typeof(int), RuleSettings.DEFAULT_MIN_INSTANCES, null, StdValidatorsAndConverters.NonZeroPositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMinInterval = new ConfigurationProperty("minInterval", typeof(TimeSpan), RuleSettings.DEFAULT_MIN_INTERVAL, StdValidatorsAndConverters.InfiniteTimeSpanConverter, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);

        static ProfileSettings()
        {
            _properties.Add(_propName);
            _properties.Add(_propMinInstances);
            _properties.Add(_propMaxLimit);
            _properties.Add(_propMinInterval);
            _properties.Add(_propCustom);
        }

        internal ProfileSettings()
        {
        }

        public ProfileSettings(string name) : this()
        {
            this.Name = name;
        }

        public ProfileSettings(string name, int minInstances, int maxLimit, TimeSpan minInterval) : this(name)
        {
            this.MinInstances = minInstances;
            this.MaxLimit = maxLimit;
            this.MinInterval = minInterval;
        }

        public ProfileSettings(string name, int minInstances, int maxLimit, TimeSpan minInterval, string custom) : this(name)
        {
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

        [TypeConverter(typeof(InfiniteIntConverter)), IntegerValidator(MinValue=0), ConfigurationProperty("maxLimit", DefaultValue=0x7fffffff)]
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

        [ConfigurationProperty("minInstances", DefaultValue=1), IntegerValidator(MinValue=1)]
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

        [ConfigurationProperty("minInterval", DefaultValue="00:00:00"), TypeConverter(typeof(InfiniteTimeSpanConverter))]
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

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

