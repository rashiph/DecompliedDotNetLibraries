namespace System.ServiceModel.Configuration
{
    public class WSHttpBindingCollectionElement : StandardBindingCollectionElement<WSHttpBinding, WSHttpBindingElement>
    {
        internal static WSHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WSHttpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("wsHttpBinding");
        }
    }
}

