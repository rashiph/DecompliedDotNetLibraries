namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;
    using System.ServiceModel.Diagnostics;

    public sealed class DiagnosticSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        internal static DiagnosticSection GetSection()
        {
            return (DiagnosticSection) ConfigurationHelpers.GetSection(ConfigurationStrings.DiagnosticSectionPath);
        }

        internal bool IsEtwProviderIdFromConfigFile()
        {
            return (PropertyValueOrigin.Default != base.ElementInformation.Properties["etwProviderId"].ValueOrigin);
        }

        [SecurityCritical]
        internal static DiagnosticSection UnsafeGetSection()
        {
            return (DiagnosticSection) ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.DiagnosticSectionPath);
        }

        [SecurityCritical]
        internal static DiagnosticSection UnsafeGetSectionNoTrace()
        {
            return (DiagnosticSection) ConfigurationHelpers.UnsafeGetSectionNoTrace(ConfigurationStrings.DiagnosticSectionPath);
        }

        [ConfigurationProperty("endToEndTracing", Options=ConfigurationPropertyOptions.None)]
        public EndToEndTracingElement EndToEndTracing
        {
            get
            {
                return (EndToEndTracingElement) base["endToEndTracing"];
            }
        }

        [ConfigurationProperty("etwProviderId", DefaultValue="{c651f5f6-1c0d-492e-8ae1-b4efd7c9d503}"), StringValidator(MinLength=0x20)]
        public string EtwProviderId
        {
            get
            {
                return (string) base["etwProviderId"];
            }
            set
            {
                base["etwProviderId"] = value;
            }
        }

        [ConfigurationProperty("messageLogging", Options=ConfigurationPropertyOptions.None)]
        public MessageLoggingElement MessageLogging
        {
            get
            {
                return (MessageLoggingElement) base["messageLogging"];
            }
        }

        [ServiceModelEnumValidator(typeof(PerformanceCounterScopeHelper)), ConfigurationProperty("performanceCounters", DefaultValue=3)]
        public PerformanceCounterScope PerformanceCounters
        {
            get
            {
                return (PerformanceCounterScope) base["performanceCounters"];
            }
            set
            {
                base["performanceCounters"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("wmiProviderEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("messageLogging", typeof(MessageLoggingElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("endToEndTracing", typeof(EndToEndTracingElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("performanceCounters", typeof(PerformanceCounterScope), PerformanceCounterScope.Default, null, new ServiceModelEnumValidator(typeof(PerformanceCounterScopeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("etwProviderId", typeof(string), "{c651f5f6-1c0d-492e-8ae1-b4efd7c9d503}", null, new StringValidator(0x20, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("wmiProviderEnabled", DefaultValue=false)]
        public bool WmiProviderEnabled
        {
            get
            {
                return (bool) base["wmiProviderEnabled"];
            }
            set
            {
                base["wmiProviderEnabled"] = value;
            }
        }
    }
}

