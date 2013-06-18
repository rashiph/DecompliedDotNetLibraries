namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ClaimTypeElement))]
    public sealed class ClaimTypeElementCollection : ServiceModelConfigurationElementCollection<ClaimTypeElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ClaimTypeElement element2 = (ClaimTypeElement) element;
            return element2.ClaimType;
        }
    }
}

