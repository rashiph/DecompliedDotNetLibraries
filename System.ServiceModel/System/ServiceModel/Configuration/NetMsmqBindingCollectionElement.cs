namespace System.ServiceModel.Configuration
{
    public class NetMsmqBindingCollectionElement : StandardBindingCollectionElement<NetMsmqBinding, NetMsmqBindingElement>
    {
        internal static NetMsmqBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetMsmqBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("netMsmqBinding");
        }
    }
}

