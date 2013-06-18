namespace System.ServiceModel.Configuration
{
    public class WS2007HttpBindingCollectionElement : StandardBindingCollectionElement<WS2007HttpBinding, WS2007HttpBindingElement>
    {
        internal static WS2007HttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (WS2007HttpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("ws2007HttpBinding");
        }
    }
}

