namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class MexHttpsBindingCollectionElement : MexBindingBindingCollectionElement<WSHttpBinding, MexHttpsBindingElement>
    {
        internal static MexHttpsBindingCollectionElement GetBindingCollectionElement()
        {
            return (MexHttpsBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("mexHttpsBinding");
        }

        protected internal override Binding GetDefault()
        {
            return MetadataExchangeBindings.GetBindingForScheme(Uri.UriSchemeHttps);
        }
    }
}

