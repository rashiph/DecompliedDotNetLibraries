namespace System.ServiceModel.Channels
{
    using System;

    internal enum PeerCloseReason
    {
        None,
        InvalidNeighbor,
        LeavingMesh,
        NotUsefulNeighbor,
        DuplicateNeighbor,
        DuplicateNodeId,
        NodeBusy,
        ConnectTimedOut,
        Faulted,
        Closed,
        InternalFailure,
        AuthenticationFailure,
        NodeTooSlow
    }
}

