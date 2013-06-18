namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class X509RecipientCertificateClientElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(X509CertificateRecipientClientCredential cert)
        {
            if (cert == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cert");
            }
            if (base.ElementInformation.Properties["authentication"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Authentication.ApplyConfiguration(cert.Authentication);
            }
            this.DefaultCertificate.ApplyConfiguration(cert);
            X509ScopedServiceCertificateElementCollection scopedCertificates = this.ScopedCertificates;
            for (int i = 0; i < scopedCertificates.Count; i++)
            {
                scopedCertificates[i].ApplyConfiguration(cert);
            }
        }

        public void Copy(X509RecipientCertificateClientElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.DefaultCertificate.Copy(from.DefaultCertificate);
            X509ScopedServiceCertificateElementCollection scopedCertificates = from.ScopedCertificates;
            X509ScopedServiceCertificateElementCollection elements2 = this.ScopedCertificates;
            elements2.Clear();
            for (int i = 0; i < scopedCertificates.Count; i++)
            {
                elements2.Add(scopedCertificates[i]);
            }
            this.Authentication.Copy(from.Authentication);
        }

        [ConfigurationProperty("authentication")]
        public X509ServiceCertificateAuthenticationElement Authentication
        {
            get
            {
                return (X509ServiceCertificateAuthenticationElement) base["authentication"];
            }
        }

        [ConfigurationProperty("defaultCertificate")]
        public X509DefaultServiceCertificateElement DefaultCertificate
        {
            get
            {
                return (X509DefaultServiceCertificateElement) base["defaultCertificate"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("defaultCertificate", typeof(X509DefaultServiceCertificateElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("scopedCertificates", typeof(X509ScopedServiceCertificateElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("authentication", typeof(X509ServiceCertificateAuthenticationElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("scopedCertificates")]
        public X509ScopedServiceCertificateElementCollection ScopedCertificates
        {
            get
            {
                return (X509ScopedServiceCertificateElementCollection) base["scopedCertificates"];
            }
        }
    }
}

