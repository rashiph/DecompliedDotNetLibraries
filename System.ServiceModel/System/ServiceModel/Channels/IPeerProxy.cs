namespace System.ServiceModel.Channels
{
    using System.ServiceModel;

    [ServiceContract(Name="PeerService", Namespace="http://schemas.microsoft.com/net/2006/05/peer", SessionMode=SessionMode.Required, CallbackContract=typeof(IPeerService))]
    internal interface IPeerProxy : IPeerServiceContract, IOutputChannel, IChannel, ICommunicationObject
    {
    }
}

