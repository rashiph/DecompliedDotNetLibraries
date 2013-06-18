namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class EndToEndTracingElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        [ConfigurationProperty("activityTracing", DefaultValue=false)]
        public bool ActivityTracing
        {
            get
            {
                return (bool) base["activityTracing"];
            }
            set
            {
                base["activityTracing"] = value;
            }
        }

        [ConfigurationProperty("messageFlowTracing", DefaultValue=false)]
        public bool MessageFlowTracing
        {
            get
            {
                return (bool) base["messageFlowTracing"];
            }
            set
            {
                base["messageFlowTracing"] = value;
            }
        }

        [ConfigurationProperty("propagateActivity", DefaultValue=false)]
        public bool PropagateActivity
        {
            get
            {
                return (bool) base["propagateActivity"];
            }
            set
            {
                base["propagateActivity"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("propagateActivity", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("activityTracing", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("messageFlowTracing", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

