namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(IssuedTokenClientBehaviorsElement))]
    public sealed class IssuedTokenClientBehaviorsElementCollection : ServiceModelConfigurationElementCollection<IssuedTokenClientBehaviorsElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            IssuedTokenClientBehaviorsElement element2 = (IssuedTokenClientBehaviorsElement) element;
            return element2.IssuerAddress;
        }
    }
}

