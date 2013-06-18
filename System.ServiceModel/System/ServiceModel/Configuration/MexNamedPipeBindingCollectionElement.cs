namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class MexNamedPipeBindingCollectionElement : MexBindingBindingCollectionElement<CustomBinding, MexNamedPipeBindingElement>
    {
        internal static MexNamedPipeBindingCollectionElement GetBindingCollectionElement()
        {
            return (MexNamedPipeBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("mexNamedPipeBinding");
        }

        protected internal override Binding GetDefault()
        {
            return MetadataExchangeBindings.GetBindingForScheme(Uri.UriSchemeNetPipe);
        }
    }
}

