namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ProtocolMappingElement), AddItemName="add")]
    public sealed class ProtocolMappingElementCollection : ServiceModelEnhancedConfigurationElementCollection<ProtocolMappingElement>
    {
        public ProtocolMappingElementCollection() : base("add")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ProtocolMappingElement element2 = (ProtocolMappingElement) element;
            return element2.Scheme;
        }
    }
}

