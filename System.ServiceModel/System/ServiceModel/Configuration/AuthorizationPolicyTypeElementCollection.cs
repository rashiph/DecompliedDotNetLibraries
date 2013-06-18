namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(AuthorizationPolicyTypeElement))]
    public sealed class AuthorizationPolicyTypeElementCollection : ServiceModelConfigurationElementCollection<AuthorizationPolicyTypeElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            AuthorizationPolicyTypeElement element2 = (AuthorizationPolicyTypeElement) element;
            return element2.PolicyType;
        }
    }
}

