namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ComPersistableTypeElement), AddItemName="type")]
    public sealed class ComPersistableTypeElementCollection : ServiceModelEnhancedConfigurationElementCollection<ComPersistableTypeElement>
    {
        public ComPersistableTypeElementCollection() : base("type")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ComPersistableTypeElement element2 = (ComPersistableTypeElement) element;
            return element2.ID;
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

