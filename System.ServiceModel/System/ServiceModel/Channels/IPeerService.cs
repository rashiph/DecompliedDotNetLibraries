namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    [ServiceContract(Name="PeerService", Namespace="http://schemas.microsoft.com/net/2006/05/peer", SessionMode=SessionMode.Required, CallbackContract=typeof(IPeerProxy))]
    internal interface IPeerService : IPeerServiceContract
    {
    }
}

