namespace System.ServiceModel.Configuration
{
    public class WSFederationHttpBindingCollectionElement : StandardBindingCollectionElement<WSFederationHttpBinding, WSFederationHttpBindingElement>
    {
        internal static WSFederationHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WSFederationHttpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("wsFederationHttpBinding");
        }
    }
}

