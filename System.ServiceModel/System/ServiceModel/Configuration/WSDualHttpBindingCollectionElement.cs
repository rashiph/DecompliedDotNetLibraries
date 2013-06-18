namespace System.ServiceModel.Configuration
{
    public class WSDualHttpBindingCollectionElement : StandardBindingCollectionElement<WSDualHttpBinding, WSDualHttpBindingElement>
    {
        internal static WSDualHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WSDualHttpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("wsDualHttpBinding");
        }
    }
}

