namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;

    public class MsmqIntegrationBinding : MsmqBindingBase
    {
        private MsmqIntegrationSecurity security;

        public MsmqIntegrationBinding()
        {
            this.security = new MsmqIntegrationSecurity();
            this.Initialize();
        }

        public MsmqIntegrationBinding(MsmqIntegrationSecurityMode securityMode)
        {
            this.security = new MsmqIntegrationSecurity();
            if (!MsmqIntegrationSecurityModeHelper.IsDefined(securityMode))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("securityMode", (int) securityMode, typeof(MsmqIntegrationSecurityMode)));
            }
            this.Initialize();
            this.security.Mode = securityMode;
        }

        public MsmqIntegrationBinding(string configurationName)
        {
            this.security = new MsmqIntegrationSecurity();
            this.Initialize();
            this.ApplyConfiguration(configurationName);
        }

        private void ApplyConfiguration(string configurationName)
        {
            System.ServiceModel.Configuration.MsmqIntegrationBindingElement element2 = MsmqIntegrationBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "msmqIntegrationBinding" })));
            }
            element2.ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection();
            this.security.ConfigureTransportSecurity(base.transport);
            elements.Add(base.transport);
            return elements.Clone();
        }

        private void Initialize()
        {
            base.transport = new System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            if ((this.security.Mode == MsmqIntegrationSecurityMode.Transport) && (((this.security.Transport.MsmqAuthenticationMode == MsmqAuthenticationMode.WindowsDomain) && (this.security.Transport.MsmqEncryptionAlgorithm == MsmqEncryptionAlgorithm.RC4Stream)) && ((this.security.Transport.MsmqSecureHashAlgorithm == MsmqSecureHashAlgorithm.Sha1) && (this.security.Transport.MsmqProtectionLevel == ProtectionLevel.Sign))))
            {
                return false;
            }
            return true;
        }

        public MsmqIntegrationSecurity Security
        {
            get
            {
                return this.security;
            }
            set
            {
                this.security = value;
            }
        }

        [DefaultValue(0)]
        public MsmqMessageSerializationFormat SerializationFormat
        {
            get
            {
                return (base.transport as System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement).SerializationFormat;
            }
            set
            {
                (base.transport as System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement).SerializationFormat = value;
            }
        }

        internal System.Type[] TargetSerializationTypes
        {
            get
            {
                return (base.transport as System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement).TargetSerializationTypes;
            }
            set
            {
                (base.transport as System.ServiceModel.MsmqIntegration.MsmqIntegrationBindingElement).TargetSerializationTypes = value;
            }
        }
    }
}

