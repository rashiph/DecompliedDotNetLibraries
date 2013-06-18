namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Description;

    public sealed class ServiceThrottlingElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            ServiceThrottlingElement element = (ServiceThrottlingElement) from;
            this.MaxConcurrentCalls = element.MaxConcurrentCalls;
            this.MaxConcurrentSessions = element.MaxConcurrentSessions;
            this.MaxConcurrentInstances = element.MaxConcurrentInstances;
        }

        protected internal override object CreateBehavior()
        {
            ServiceThrottlingBehavior behavior = new ServiceThrottlingBehavior();
            PropertyInformationCollection properties = base.ElementInformation.Properties;
            if (properties["maxConcurrentCalls"].ValueOrigin != PropertyValueOrigin.Default)
            {
                behavior.MaxConcurrentCalls = this.MaxConcurrentCalls;
            }
            if (properties["maxConcurrentSessions"].ValueOrigin != PropertyValueOrigin.Default)
            {
                behavior.MaxConcurrentSessions = this.MaxConcurrentSessions;
            }
            if (properties["maxConcurrentInstances"].ValueOrigin != PropertyValueOrigin.Default)
            {
                behavior.MaxConcurrentInstances = this.MaxConcurrentInstances;
            }
            return behavior;
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ServiceThrottlingBehavior);
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("maxConcurrentCalls", DefaultValue=0x10)]
        public int MaxConcurrentCalls
        {
            get
            {
                return (int) base["maxConcurrentCalls"];
            }
            set
            {
                base["maxConcurrentCalls"] = value;
            }
        }

        [ConfigurationProperty("maxConcurrentInstances", DefaultValue=0x74), IntegerValidator(MinValue=1)]
        public int MaxConcurrentInstances
        {
            get
            {
                return (int) base["maxConcurrentInstances"];
            }
            set
            {
                base["maxConcurrentInstances"] = value;
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("maxConcurrentSessions", DefaultValue=100)]
        public int MaxConcurrentSessions
        {
            get
            {
                return (int) base["maxConcurrentSessions"];
            }
            set
            {
                base["maxConcurrentSessions"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("maxConcurrentCalls", typeof(int), 0x10, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxConcurrentSessions", typeof(int), 100, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxConcurrentInstances", typeof(int), 0x74, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

