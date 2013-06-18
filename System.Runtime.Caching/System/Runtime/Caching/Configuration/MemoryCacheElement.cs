namespace System.Runtime.Caching.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    public sealed class MemoryCacheElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propCacheMemoryLimitMegabytes = new ConfigurationProperty("cacheMemoryLimitMegabytes", typeof(int), 0, null, new IntegerValidator(0, 0x7fffffff), ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), null, new WhiteSpaceTrimStringConverter(), new StringValidator(1), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propPhysicalMemoryLimitPercentage = new ConfigurationProperty("physicalMemoryLimitPercentage", typeof(int), 0, null, new IntegerValidator(0, 100), ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPollingInterval = new ConfigurationProperty("pollingInterval", typeof(TimeSpan), TimeSpan.FromMilliseconds(120000.0), new InfiniteTimeSpanConverter(), new PositiveTimeSpanValidator(), ConfigurationPropertyOptions.None);

        static MemoryCacheElement()
        {
            _properties.Add(_propName);
            _properties.Add(_propPhysicalMemoryLimitPercentage);
            _properties.Add(_propCacheMemoryLimitMegabytes);
            _properties.Add(_propPollingInterval);
        }

        internal MemoryCacheElement()
        {
        }

        public MemoryCacheElement(string name)
        {
            this.Name = name;
        }

        [ConfigurationProperty("cacheMemoryLimitMegabytes", DefaultValue=0), IntegerValidator(MinValue=0)]
        public int CacheMemoryLimitMegabytes
        {
            get
            {
                return (int) base["cacheMemoryLimitMegabytes"];
            }
            set
            {
                base["cacheMemoryLimitMegabytes"] = value;
            }
        }

        [TypeConverter(typeof(WhiteSpaceTrimStringConverter)), StringValidator(MinLength=1), ConfigurationProperty("name", DefaultValue="", IsRequired=true, IsKey=true)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        [IntegerValidator(MinValue=0, MaxValue=100), ConfigurationProperty("physicalMemoryLimitPercentage", DefaultValue=0)]
        public int PhysicalMemoryLimitPercentage
        {
            get
            {
                return (int) base["physicalMemoryLimitPercentage"];
            }
            set
            {
                base["physicalMemoryLimitPercentage"] = value;
            }
        }

        [ConfigurationProperty("pollingInterval", DefaultValue="00:02:00"), TypeConverter(typeof(InfiniteTimeSpanConverter))]
        public TimeSpan PollingInterval
        {
            get
            {
                return (TimeSpan) base["pollingInterval"];
            }
            set
            {
                base["pollingInterval"] = value;
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

