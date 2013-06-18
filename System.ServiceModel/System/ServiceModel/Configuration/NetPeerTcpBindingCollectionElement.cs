namespace System.ServiceModel.Configuration
{
    public class NetPeerTcpBindingCollectionElement : StandardBindingCollectionElement<NetPeerTcpBinding, NetPeerTcpBindingElement>
    {
        internal static NetPeerTcpBindingCollectionElement GetBindingCollectionElement()
        {
            return (NetPeerTcpBindingCollectionElement) ConfigurationHelpers.GetBindingCollectionElement("netPeerTcpBinding");
        }
    }
}

