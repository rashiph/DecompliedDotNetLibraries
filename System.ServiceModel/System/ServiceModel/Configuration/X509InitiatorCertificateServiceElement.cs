namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class X509InitiatorCertificateServiceElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(X509CertificateInitiatorServiceCredential cert)
        {
            if (cert == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cert");
            }
            PropertyInformationCollection properties = base.ElementInformation.Properties;
            if (properties["authentication"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Authentication.ApplyConfiguration(cert.Authentication);
            }
            if (properties["certificate"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Certificate.ApplyConfiguration(cert);
            }
        }

        public void Copy(X509InitiatorCertificateServiceElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.Authentication.Copy(from.Authentication);
            this.Certificate.Copy(from.Certificate);
        }

        [ConfigurationProperty("authentication")]
        public X509ClientCertificateAuthenticationElement Authentication
        {
            get
            {
                return (X509ClientCertificateAuthenticationElement) base["authentication"];
            }
        }

        [ConfigurationProperty("certificate")]
        public X509ClientCertificateCredentialsElement Certificate
        {
            get
            {
                return (X509ClientCertificateCredentialsElement) base["certificate"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("certificate", typeof(X509ClientCertificateCredentialsElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("authentication", typeof(X509ClientCertificateAuthenticationElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

