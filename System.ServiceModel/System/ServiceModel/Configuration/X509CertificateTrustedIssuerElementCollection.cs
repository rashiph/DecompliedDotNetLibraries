namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    [ConfigurationCollection(typeof(X509CertificateTrustedIssuerElement))]
    public sealed class X509CertificateTrustedIssuerElementCollection : ServiceModelConfigurationElementCollection<X509CertificateTrustedIssuerElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }
    }
}

