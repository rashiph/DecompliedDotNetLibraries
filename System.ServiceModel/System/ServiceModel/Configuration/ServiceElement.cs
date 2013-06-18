namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;

    public sealed class ServiceElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        [SecurityCritical]
        private EvaluationContextHelper contextHelper;
        private ConfigurationPropertyCollection properties;

        public ServiceElement()
        {
        }

        public ServiceElement(string serviceName) : this()
        {
            this.Name = serviceName;
        }

        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);
            base.Reset(parentElement);
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }

        [StringValidator(MinLength=0), ConfigurationProperty("behaviorConfiguration", DefaultValue="")]
        public string BehaviorConfiguration
        {
            get
            {
                return (string) base["behaviorConfiguration"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["behaviorConfiguration"] = value;
            }
        }

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public ServiceEndpointElementCollection Endpoints
        {
            get
            {
                return (ServiceEndpointElementCollection) base[""];
            }
        }

        [ConfigurationProperty("host", Options=ConfigurationPropertyOptions.None)]
        public HostElement Host
        {
            get
            {
                return (HostElement) base["host"];
            }
        }

        [ConfigurationProperty("name", Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired), StringValidator(MinLength=1)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["name"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("behaviorConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("", typeof(ServiceEndpointElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    propertys.Add(new ConfigurationProperty("host", typeof(HostElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("name", typeof(string), null, null, new StringValidator(1, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

