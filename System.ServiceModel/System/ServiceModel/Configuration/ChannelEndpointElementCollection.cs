namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(ChannelEndpointElement), AddItemName="endpoint")]
    public sealed class ChannelEndpointElementCollection : ServiceModelEnhancedConfigurationElementCollection<ChannelEndpointElement>
    {
        public ChannelEndpointElementCollection() : base("endpoint")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ChannelEndpointElement element2 = (ChannelEndpointElement) element;
            return string.Format(CultureInfo.InvariantCulture, "contractType:{0};name:{1}", new object[] { element2.Contract, element2.Name });
        }
    }
}

