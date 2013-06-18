namespace System.ServiceModel.Configuration
{
    public class NetNamedPipeBindingCollectionElement : StandardBindingCollectionElement<NetNamedPipeBinding, NetNamedPipeBindingElement>
    {
        internal static NetNamedPipeBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetNamedPipeBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("netNamedPipeBinding");
        }
    }
}

