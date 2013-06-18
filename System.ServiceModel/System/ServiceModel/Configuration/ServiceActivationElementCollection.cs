namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ServiceActivationElement))]
    public sealed class ServiceActivationElementCollection : ServiceModelConfigurationElementCollection<ServiceActivationElement>
    {
        public ServiceActivationElementCollection() : base(ConfigurationElementCollectionType.AddRemoveClearMap, "add")
        {
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceActivationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ServiceActivationElement element2 = (ServiceActivationElement) element;
            return element2.RelativeAddress;
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }
    }
}

