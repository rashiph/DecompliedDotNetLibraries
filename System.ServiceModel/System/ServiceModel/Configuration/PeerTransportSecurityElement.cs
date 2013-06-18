namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class PeerTransportSecurityElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(PeerTransportSecuritySettings security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.CredentialType = this.CredentialType;
        }

        internal void CopyFrom(PeerTransportSecurityElement security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.CredentialType = security.CredentialType;
        }

        internal void InitializeFrom(PeerTransportSecuritySettings security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.CredentialType = security.CredentialType;
        }

        [ConfigurationProperty("credentialType", DefaultValue=0), ServiceModelEnumValidator(typeof(PeerTransportCredentialTypeHelper))]
        public PeerTransportCredentialType CredentialType
        {
            get
            {
                return (PeerTransportCredentialType) base["credentialType"];
            }
            set
            {
                base["credentialType"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("credentialType", typeof(PeerTransportCredentialType), PeerTransportCredentialType.Password, null, new ServiceModelEnumValidator(typeof(PeerTransportCredentialTypeHelper)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

