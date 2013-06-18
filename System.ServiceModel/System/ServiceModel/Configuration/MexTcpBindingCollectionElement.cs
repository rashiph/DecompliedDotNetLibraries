namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class MexTcpBindingCollectionElement : MexBindingBindingCollectionElement<CustomBinding, MexTcpBindingElement>
    {
        internal static MexTcpBindingCollectionElement GetBindingCollectionElement()
        {
            return (MexTcpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("mexTcpBinding");
        }

        protected internal override Binding GetDefault()
        {
            return MetadataExchangeBindings.GetBindingForScheme(Uri.UriSchemeNetTcp);
        }
    }
}

