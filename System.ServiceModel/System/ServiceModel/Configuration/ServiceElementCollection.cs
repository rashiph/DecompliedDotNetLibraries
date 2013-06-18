namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ServiceElement), AddItemName="service")]
    public sealed class ServiceElementCollection : ServiceModelEnhancedConfigurationElementCollection<ServiceElement>
    {
        public ServiceElementCollection() : base("service")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ServiceElement element2 = (ServiceElement) element;
            return element2.Name;
        }
    }
}

