namespace System.ServiceModel.Configuration
{
    public class BasicHttpBindingCollectionElement : StandardBindingCollectionElement<BasicHttpBinding, BasicHttpBindingElement>
    {
        internal static BasicHttpBindingCollectionElement GetBindingCollectionElement()
        {
            return (BasicHttpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("basicHttpBinding");
        }
    }
}

