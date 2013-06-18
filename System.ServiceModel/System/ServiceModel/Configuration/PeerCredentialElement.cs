namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class PeerCredentialElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(PeerCredential creds)
        {
            if (creds == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("creds");
            }
            PropertyInformationCollection properties = base.ElementInformation.Properties;
            if (properties["certificate"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Certificate.ApplyConfiguration(creds);
            }
            if (properties["peerAuthentication"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.PeerAuthentication.ApplyConfiguration(creds.PeerAuthentication);
            }
            if (properties["messageSenderAuthentication"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.MessageSenderAuthentication.ApplyConfiguration(creds.MessageSenderAuthentication);
            }
        }

        public void Copy(PeerCredentialElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.Certificate.Copy(from.Certificate);
            this.PeerAuthentication.Copy(from.PeerAuthentication);
            this.MessageSenderAuthentication.Copy(from.MessageSenderAuthentication);
        }

        [ConfigurationProperty("certificate")]
        public X509PeerCertificateElement Certificate
        {
            get
            {
                return (X509PeerCertificateElement) base["certificate"];
            }
        }

        [ConfigurationProperty("messageSenderAuthentication")]
        public X509PeerCertificateAuthenticationElement MessageSenderAuthentication
        {
            get
            {
                return (X509PeerCertificateAuthenticationElement) base["messageSenderAuthentication"];
            }
        }

        [ConfigurationProperty("peerAuthentication")]
        public X509PeerCertificateAuthenticationElement PeerAuthentication
        {
            get
            {
                return (X509PeerCertificateAuthenticationElement) base["peerAuthentication"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("certificate", typeof(X509PeerCertificateElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("peerAuthentication", typeof(X509PeerCertificateAuthenticationElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("messageSenderAuthentication", typeof(X509PeerCertificateAuthenticationElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

