namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ComUdtElement), AddItemName="userDefinedType")]
    public sealed class ComUdtElementCollection : ServiceModelEnhancedConfigurationElementCollection<ComUdtElement>
    {
        public ComUdtElementCollection() : base("userDefinedType")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ComUdtElement element2 = (ComUdtElement) element;
            return element2.TypeDefID;
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }
    }
}

