namespace System.ServiceModel.Channels
{
    using System;

    internal interface IPeerFlooderContract<TFloodContract, TLinkContract>
    {
        void EndFloodMessage(IAsyncResult result);
        IAsyncResult OnFloodedMessage(IPeerNeighbor neighbor, TFloodContract floodedInfo, AsyncCallback callback, object state);
        void ProcessLinkUtility(IPeerNeighbor neighbor, TLinkContract utilityInfo);
    }
}

