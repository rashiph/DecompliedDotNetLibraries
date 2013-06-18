namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(CustomBindingElement), AddItemName="binding")]
    public sealed class CustomBindingElementCollection : ServiceModelEnhancedConfigurationElementCollection<CustomBindingElement>
    {
        public CustomBindingElementCollection() : base("binding")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            CustomBindingElement element2 = (CustomBindingElement) element;
            return element2.Name;
        }
    }
}

