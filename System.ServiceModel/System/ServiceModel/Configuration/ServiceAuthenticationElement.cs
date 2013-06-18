namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public sealed class ServiceAuthenticationElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        protected internal override object CreateBehavior()
        {
            ServiceAuthenticationBehavior behavior = new ServiceAuthenticationBehavior();
            string serviceAuthenticationManagerType = this.ServiceAuthenticationManagerType;
            if (!string.IsNullOrEmpty(serviceAuthenticationManagerType))
            {
                Type c = Type.GetType(serviceAuthenticationManagerType, true);
                if (!typeof(ServiceAuthenticationManager).IsAssignableFrom(c))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidServiceAuthenticationManagerType", new object[] { serviceAuthenticationManagerType, typeof(ServiceAuthenticationManager) })));
                }
                behavior.ServiceAuthenticationManager = (ServiceAuthenticationManager) Activator.CreateInstance(c);
            }
            return behavior;
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ServiceAuthenticationBehavior);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("serviceAuthenticationManagerType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("serviceAuthenticationManagerType", DefaultValue="")]
        public string ServiceAuthenticationManagerType
        {
            get
            {
                return (string) base["serviceAuthenticationManagerType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["serviceAuthenticationManagerType"] = value;
            }
        }
    }
}

