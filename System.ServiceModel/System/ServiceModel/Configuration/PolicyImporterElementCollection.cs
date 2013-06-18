namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ConfigurationCollection(typeof(PolicyImporterElement), AddItemName="extension")]
    public sealed class PolicyImporterElementCollection : ServiceModelEnhancedConfigurationElementCollection<PolicyImporterElement>
    {
        public PolicyImporterElementCollection() : base("extension")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            PolicyImporterElement element2 = (PolicyImporterElement) element;
            return element2.Type;
        }

        internal void SetDefaults()
        {
            base.Add(new PolicyImporterElement(typeof(PrivacyNoticeBindingElementImporter)));
            base.Add(new PolicyImporterElement(typeof(UseManagedPresentationBindingElementImporter)));
            base.Add(new PolicyImporterElement(typeof(TransactionFlowBindingElementImporter)));
            base.Add(new PolicyImporterElement(typeof(ReliableSessionBindingElementImporter)));
            base.Add(new PolicyImporterElement(typeof(SecurityBindingElementImporter)));
            base.Add(new PolicyImporterElement(typeof(CompositeDuplexBindingElementImporter)));
            base.Add(new PolicyImporterElement(typeof(OneWayBindingElementImporter)));
            base.Add(new PolicyImporterElement(typeof(MessageEncodingBindingElementImporter)));
            base.Add(new PolicyImporterElement(typeof(TransportBindingElementImporter)));
        }
    }
}

