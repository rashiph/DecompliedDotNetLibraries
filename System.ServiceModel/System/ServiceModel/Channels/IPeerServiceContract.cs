namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    [ServiceContract(Name="PeerService", Namespace="http://schemas.microsoft.com/net/2006/05/peer", SessionMode=SessionMode.Required, CallbackContract=typeof(IPeerServiceContract))]
    internal interface IPeerServiceContract
    {
        [OperationContract(IsOneWay=true, Action="http://schemas.microsoft.com/net/2006/05/peer/Flood", AsyncPattern=true)]
        IAsyncResult BeginFloodMessage(Message floodedInfo, AsyncCallback callback, object state);
        [OperationContract(IsOneWay=true, Action="http://schemas.microsoft.com/net/2006/05/peer/Connect")]
        void Connect(ConnectInfo connectInfo);
        [OperationContract(IsOneWay=true, Action="http://schemas.microsoft.com/net/2006/05/peer/Disconnect")]
        void Disconnect(DisconnectInfo disconnectInfo);
        void EndFloodMessage(IAsyncResult result);
        [OperationContract(IsOneWay=true, Action="http://www.w3.org/2005/08/addressing/fault")]
        void Fault(Message message);
        [OperationContract(IsOneWay=true, Action="http://schemas.microsoft.com/net/2006/05/peer/LinkUtility")]
        void LinkUtility(UtilityInfo utilityInfo);
        [OperationContract(IsOneWay=true, Action="http://schemas.microsoft.com/net/2006/05/peer/Ping")]
        void Ping(Message message);
        [OperationContract(Action="RequestSecurityToken", ReplyAction="RequestSecurityTokenResponse")]
        Message ProcessRequestSecurityToken(Message message);
        [OperationContract(IsOneWay=true, Action="http://schemas.microsoft.com/net/2006/05/peer/Refuse")]
        void Refuse(RefuseInfo refuseInfo);
        [OperationContract(IsOneWay=true, Action="http://schemas.microsoft.com/net/2006/05/peer/Welcome")]
        void Welcome(WelcomeInfo welcomeInfo);
    }
}

