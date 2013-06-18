namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(AllowedAudienceUriElement), CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class AllowedAudienceUriElementCollection : ServiceModelConfigurationElementCollection<AllowedAudienceUriElement>
    {
        public AllowedAudienceUriElementCollection() : base(ConfigurationElementCollectionType.BasicMap, "add")
        {
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AllowedAudienceUriElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            AllowedAudienceUriElement element2 = (AllowedAudienceUriElement) element;
            return element2.AllowedAudienceUri;
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

