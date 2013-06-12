namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class TraceSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propEnabled = new ConfigurationProperty("enabled", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propLocalOnly = new ConfigurationProperty("localOnly", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMode = new ConfigurationProperty("traceMode", typeof(TraceDisplayMode), TraceDisplayMode.SortByTime, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMostRecent = new ConfigurationProperty("mostRecent", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPageOutput = new ConfigurationProperty("pageOutput", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRequestLimit = new ConfigurationProperty("requestLimit", typeof(int), 10, null, StdValidatorsAndConverters.PositiveIntegerValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _writeToDiagnosticTrace = new ConfigurationProperty("writeToDiagnosticsTrace", typeof(bool), false, ConfigurationPropertyOptions.None);

        static TraceSection()
        {
            _properties.Add(_propEnabled);
            _properties.Add(_propLocalOnly);
            _properties.Add(_propMostRecent);
            _properties.Add(_propPageOutput);
            _properties.Add(_propRequestLimit);
            _properties.Add(_propMode);
            _properties.Add(_writeToDiagnosticTrace);
        }

        [ConfigurationProperty("enabled", DefaultValue=false)]
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

        [ConfigurationProperty("localOnly", DefaultValue=true)]
        public bool LocalOnly
        {
            get
            {
                return (bool) base[_propLocalOnly];
            }
            set
            {
                base[_propLocalOnly] = value;
            }
        }

        [ConfigurationProperty("mostRecent", DefaultValue=false)]
        public bool MostRecent
        {
            get
            {
                return (bool) base[_propMostRecent];
            }
            set
            {
                base[_propMostRecent] = value;
            }
        }

        [ConfigurationProperty("pageOutput", DefaultValue=false)]
        public bool PageOutput
        {
            get
            {
                return (bool) base[_propPageOutput];
            }
            set
            {
                base[_propPageOutput] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("requestLimit", DefaultValue=10), IntegerValidator(MinValue=0)]
        public int RequestLimit
        {
            get
            {
                return (int) base[_propRequestLimit];
            }
            set
            {
                base[_propRequestLimit] = value;
            }
        }

        [ConfigurationProperty("traceMode", DefaultValue=1)]
        public TraceDisplayMode TraceMode
        {
            get
            {
                return (TraceDisplayMode) base[_propMode];
            }
            set
            {
                base[_propMode] = value;
            }
        }

        [ConfigurationProperty("writeToDiagnosticsTrace", DefaultValue=false)]
        public bool WriteToDiagnosticsTrace
        {
            get
            {
                return (bool) base[_writeToDiagnosticTrace];
            }
            set
            {
                base[_writeToDiagnosticTrace] = value;
            }
        }
    }
}

