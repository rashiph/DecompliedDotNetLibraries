namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ComMethodElement))]
    public sealed class ComMethodElementCollection : ServiceModelEnhancedConfigurationElementCollection<ComMethodElement>
    {
        public ComMethodElementCollection() : base("add")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ComMethodElement element2 = (ComMethodElement) element;
            return element2.ExposedMethod;
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

