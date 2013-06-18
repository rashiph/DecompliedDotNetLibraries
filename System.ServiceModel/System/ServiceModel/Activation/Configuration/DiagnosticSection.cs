namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.Configuration;

    public sealed class DiagnosticSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        internal static DiagnosticSection GetSection()
        {
            DiagnosticSection section = (DiagnosticSection) ConfigurationManager.GetSection(ConfigurationStrings.DiagnosticSectionPath);
            if (section == null)
            {
                section = new DiagnosticSection();
            }
            return section;
        }

        [ConfigurationProperty("performanceCountersEnabled", DefaultValue=true)]
        public bool PerformanceCountersEnabled
        {
            get
            {
                return (bool) base["performanceCountersEnabled"];
            }
            set
            {
                base["performanceCountersEnabled"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("performanceCountersEnabled", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

