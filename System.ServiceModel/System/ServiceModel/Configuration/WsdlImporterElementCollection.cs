namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    [ConfigurationCollection(typeof(WsdlImporterElement), AddItemName="extension")]
    public sealed class WsdlImporterElementCollection : ServiceModelEnhancedConfigurationElementCollection<WsdlImporterElement>
    {
        public WsdlImporterElementCollection() : base("extension")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            WsdlImporterElement element2 = (WsdlImporterElement) element;
            return element2.Type;
        }

        internal void SetDefaults()
        {
            base.Add(new WsdlImporterElement(typeof(DataContractSerializerMessageContractImporter)));
            base.Add(new WsdlImporterElement(typeof(XmlSerializerMessageContractImporter)));
            base.Add(new WsdlImporterElement(typeof(MessageEncodingBindingElementImporter)));
            base.Add(new WsdlImporterElement(typeof(TransportBindingElementImporter)));
            base.Add(new WsdlImporterElement(typeof(StandardBindingImporter)));
        }
    }
}

