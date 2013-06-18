namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ComContractElement), AddItemName="comContract")]
    public sealed class ComContractElementCollection : ServiceModelEnhancedConfigurationElementCollection<ComContractElement>
    {
        public ComContractElementCollection() : base("comContract")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ComContractElement element2 = (ComContractElement) element;
            return element2.Contract;
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

