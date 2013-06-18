namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    public sealed class StandardBindingElementCollection<TBindingConfiguration> : ServiceModelEnhancedConfigurationElementCollection<TBindingConfiguration> where TBindingConfiguration: StandardBindingElement, new()
    {
        public StandardBindingElementCollection() : base("binding")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            TBindingConfiguration local = (TBindingConfiguration) element;
            return local.Name;
        }
    }
}

