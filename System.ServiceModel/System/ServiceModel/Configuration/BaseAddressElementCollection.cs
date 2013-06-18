namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(BaseAddressElement), CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class BaseAddressElementCollection : ServiceModelConfigurationElementCollection<BaseAddressElement>
    {
        public BaseAddressElementCollection() : base(ConfigurationElementCollectionType.BasicMap, "add")
        {
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BaseAddressElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            BaseAddressElement element2 = (BaseAddressElement) element;
            return element2.BaseAddress;
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return true;
            }
        }
    }
}

