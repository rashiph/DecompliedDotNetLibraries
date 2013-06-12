namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IServerChannelSinkProvider
    {
        [SecurityCritical]
        IServerChannelSink CreateSink(IChannelReceiver channel);
        [SecurityCritical]
        void GetChannelData(IChannelDataStore channelData);

        IServerChannelSinkProvider Next { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

