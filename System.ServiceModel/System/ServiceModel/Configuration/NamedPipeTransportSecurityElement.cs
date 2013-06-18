namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class NamedPipeTransportSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(NamedPipeTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.ProtectionLevel = this.ProtectionLevel;
        }

        internal void InitializeFrom(NamedPipeTransportSecurity security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.ProtectionLevel = security.ProtectionLevel;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("protectionLevel", typeof(System.Net.Security.ProtectionLevel), System.Net.Security.ProtectionLevel.EncryptAndSign, null, new ServiceModelEnumValidator(typeof(ProtectionLevelHelper)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ServiceModelEnumValidator(typeof(ProtectionLevelHelper)), ConfigurationProperty("protectionLevel", DefaultValue=2)]
        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return (System.Net.Security.ProtectionLevel) base["protectionLevel"];
            }
            set
            {
                base["protectionLevel"] = value;
            }
        }
    }
}

