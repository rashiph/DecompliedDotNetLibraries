namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(TransportConfigurationTypeElement))]
    public sealed class TransportConfigurationTypeElementCollection : ServiceModelConfigurationElementCollection<TransportConfigurationTypeElement>
    {
        public TransportConfigurationTypeElementCollection() : base(ConfigurationElementCollectionType.AddRemoveClearMap, null)
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            TransportConfigurationTypeElement element2 = (TransportConfigurationTypeElement) element;
            return element2.Name;
        }
    }
}

