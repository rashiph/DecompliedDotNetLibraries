namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class MexHttpBindingCollectionElement : MexBindingBindingCollectionElement<WSHttpBinding, MexHttpBindingElement>
    {
        internal static MexHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (MexHttpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("mexHttpBinding");
        }

        protected internal override Binding GetDefault()
        {
            return MetadataExchangeBindings.GetBindingForScheme(Uri.UriSchemeHttp);
        }
    }
}

