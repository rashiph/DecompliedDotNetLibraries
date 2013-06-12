namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IClientChannelSinkProvider
    {
        [SecurityCritical]
        IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData);

        IClientChannelSinkProvider Next { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

