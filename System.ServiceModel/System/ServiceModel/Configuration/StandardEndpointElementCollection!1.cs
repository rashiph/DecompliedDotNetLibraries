namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class StandardEndpointElementCollection<TEndpointConfiguration> : ServiceModelEnhancedConfigurationElementCollection<TEndpointConfiguration> where TEndpointConfiguration: StandardEndpointElement, new()
    {
        public StandardEndpointElementCollection() : base("standardEndpoint")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            TEndpointConfiguration local = (TEndpointConfiguration) element;
            return local.Name;
        }
    }
}

