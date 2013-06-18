namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(DefaultPortElement), AddItemName="add")]
    public sealed class DefaultPortElementCollection : ServiceModelEnhancedConfigurationElementCollection<DefaultPortElement>
    {
        public DefaultPortElementCollection() : base("add")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            DefaultPortElement element2 = (DefaultPortElement) element;
            return element2.Scheme;
        }
    }
}

