namespace System.ServiceModel.Configuration
{
    public class WS2007FederationHttpBindingCollectionElement : StandardBindingCollectionElement<WS2007FederationHttpBinding, WS2007FederationHttpBindingElement>
    {
        internal static WS2007FederationHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WS2007FederationHttpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("ws2007FederationHttpBinding");
        }
    }
}

