namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;

    public sealed class ServiceHostingEnvironmentSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        internal static ServiceHostingEnvironmentSection GetSection()
        {
            return (ServiceHostingEnvironmentSection) ConfigurationHelpers.GetSection(ConfigurationStrings.ServiceHostingEnvironmentSectionPath);
        }

        protected override void PostDeserialize()
        {
            if (!base.EvaluationContext.IsMachineLevel && (PropertyValueOrigin.SetHere == base.ElementInformation.Properties["minFreeMemoryPercentageToActivateService"].ValueOrigin))
            {
                try
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                }
                catch (SecurityException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("Hosting_MemoryGatesCheckFailedUnderPartialTrust")));
                }
            }
        }

        [SecurityCritical]
        internal static ServiceHostingEnvironmentSection UnsafeGetSection()
        {
            return (ServiceHostingEnvironmentSection) ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ServiceHostingEnvironmentSectionPath);
        }

        [ConfigurationProperty("aspNetCompatibilityEnabled", DefaultValue=false)]
        public bool AspNetCompatibilityEnabled
        {
            get
            {
                return (bool) base["aspNetCompatibilityEnabled"];
            }
            set
            {
                base["aspNetCompatibilityEnabled"] = value;
            }
        }

        [ConfigurationProperty("baseAddressPrefixFilters", Options=ConfigurationPropertyOptions.None)]
        public BaseAddressPrefixFilterElementCollection BaseAddressPrefixFilters
        {
            get
            {
                return (BaseAddressPrefixFilterElementCollection) base["baseAddressPrefixFilters"];
            }
        }

        [ConfigurationProperty("minFreeMemoryPercentageToActivateService", DefaultValue=5), IntegerValidator(MinValue=0, MaxValue=0x63)]
        public int MinFreeMemoryPercentageToActivateService
        {
            get
            {
                return (int) base["minFreeMemoryPercentageToActivateService"];
            }
            set
            {
                base["minFreeMemoryPercentageToActivateService"] = value;
            }
        }

        [ConfigurationProperty("multipleSiteBindingsEnabled", DefaultValue=false)]
        public bool MultipleSiteBindingsEnabled
        {
            get
            {
                return (bool) base["multipleSiteBindingsEnabled"];
            }
            set
            {
                base["multipleSiteBindingsEnabled"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("", typeof(TransportConfigurationTypeElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    propertys.Add(new ConfigurationProperty("baseAddressPrefixFilters", typeof(BaseAddressPrefixFilterElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("serviceActivations", typeof(ServiceActivationElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("aspNetCompatibilityEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("minFreeMemoryPercentageToActivateService", typeof(int), 5, null, new IntegerValidator(0, 0x63, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("multipleSiteBindingsEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("serviceActivations", Options=ConfigurationPropertyOptions.None)]
        public ServiceActivationElementCollection ServiceActivations
        {
            get
            {
                return (ServiceActivationElementCollection) base["serviceActivations"];
            }
        }

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public TransportConfigurationTypeElementCollection TransportConfigurationTypes
        {
            get
            {
                return (TransportConfigurationTypeElementCollection) base[""];
            }
        }
    }
}

