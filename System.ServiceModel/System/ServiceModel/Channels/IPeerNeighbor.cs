namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal interface IPeerNeighbor : IExtensibleObject<IPeerNeighbor>
    {
        void Abort(PeerCloseReason reason, PeerCloseInitiator initiator);
        IAsyncResult BeginSend(Message message, AsyncCallback callback, object state);
        IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        void EndSend(IAsyncResult result);
        void Ping(Message request);
        Message RequestSecurityToken(Message request);
        void Send(Message message);
        bool TrySetState(PeerNeighborState state);

        bool IsClosing { get; }

        bool IsConnected { get; }

        bool IsInitiator { get; }

        PeerNodeAddress ListenAddress { get; set; }

        ulong NodeId { get; set; }

        PeerNeighborState State { get; set; }

        UtilityExtension Utility { get; }
    }
}

