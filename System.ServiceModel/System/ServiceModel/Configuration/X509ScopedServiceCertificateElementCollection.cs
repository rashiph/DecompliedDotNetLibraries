namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(X509ScopedServiceCertificateElement))]
    public sealed class X509ScopedServiceCertificateElementCollection : ServiceModelConfigurationElementCollection<X509ScopedServiceCertificateElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            X509ScopedServiceCertificateElement element2 = (X509ScopedServiceCertificateElement) element;
            return element2.TargetUri;
        }
    }
}

