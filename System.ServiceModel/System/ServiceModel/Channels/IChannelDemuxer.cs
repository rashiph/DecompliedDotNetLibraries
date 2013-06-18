namespace System.ServiceModel.Channels
{
    using System;

    internal interface IChannelDemuxer
    {
        IAsyncResult OnBeginOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult OnBeginOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout, AsyncCallback callback, object state);
        void OnEndOuterListenerClose(IAsyncResult result);
        void OnEndOuterListenerOpen(IAsyncResult result);
        void OnOuterListenerAbort(ChannelDemuxerFilter filter);
        void OnOuterListenerClose(ChannelDemuxerFilter filter, TimeSpan timeout);
        void OnOuterListenerOpen(ChannelDemuxerFilter filter, IChannelListener listener, TimeSpan timeout);
    }
}

