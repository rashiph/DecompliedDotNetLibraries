namespace System.Security.Authentication.ExtendedProtection.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Security.Authentication.ExtendedProtection;

    public sealed class ExtendedProtectionPolicyElement : ConfigurationElement
    {
        private readonly ConfigurationProperty customServiceNames = new ConfigurationProperty("customServiceNames", typeof(ServiceNameElementCollection), null, ConfigurationPropertyOptions.None);
        private readonly ConfigurationProperty policyEnforcement = new ConfigurationProperty("policyEnforcement", typeof(System.Security.Authentication.ExtendedProtection.PolicyEnforcement), DefaultPolicyEnforcement, ConfigurationPropertyOptions.None);
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private readonly ConfigurationProperty protectionScenario = new ConfigurationProperty("protectionScenario", typeof(System.Security.Authentication.ExtendedProtection.ProtectionScenario), System.Security.Authentication.ExtendedProtection.ProtectionScenario.TransportSelected, ConfigurationPropertyOptions.None);

        public ExtendedProtectionPolicyElement()
        {
            this.properties.Add(this.policyEnforcement);
            this.properties.Add(this.protectionScenario);
            this.properties.Add(this.customServiceNames);
        }

        public ExtendedProtectionPolicy BuildPolicy()
        {
            if (this.PolicyEnforcement == System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never)
            {
                return new ExtendedProtectionPolicy(System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never);
            }
            ServiceNameCollection customServiceNames = null;
            ServiceNameElementCollection elements = this.CustomServiceNames;
            if ((elements != null) && (elements.Count > 0))
            {
                List<string> items = new List<string>(elements.Count);
                foreach (ServiceNameElement element in elements)
                {
                    items.Add(element.Name);
                }
                customServiceNames = new ServiceNameCollection(items);
            }
            return new ExtendedProtectionPolicy(this.PolicyEnforcement, this.ProtectionScenario, customServiceNames);
        }

        [ConfigurationProperty("customServiceNames")]
        public ServiceNameElementCollection CustomServiceNames
        {
            get
            {
                return (ServiceNameElementCollection) base[this.customServiceNames];
            }
        }

        private static System.Security.Authentication.ExtendedProtection.PolicyEnforcement DefaultPolicyEnforcement
        {
            get
            {
                return System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never;
            }
        }

        [ConfigurationProperty("policyEnforcement")]
        public System.Security.Authentication.ExtendedProtection.PolicyEnforcement PolicyEnforcement
        {
            get
            {
                return (System.Security.Authentication.ExtendedProtection.PolicyEnforcement) base[this.policyEnforcement];
            }
            set
            {
                base[this.policyEnforcement] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        [ConfigurationProperty("protectionScenario", DefaultValue=0)]
        public System.Security.Authentication.ExtendedProtection.ProtectionScenario ProtectionScenario
        {
            get
            {
                return (System.Security.Authentication.ExtendedProtection.ProtectionScenario) base[this.protectionScenario];
            }
            set
            {
                base[this.protectionScenario] = value;
            }
        }
    }
}

