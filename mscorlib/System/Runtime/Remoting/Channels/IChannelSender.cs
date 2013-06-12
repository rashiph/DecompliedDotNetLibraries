namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [ComVisible(true)]
    public interface IChannelSender : IChannel
    {
        [SecurityCritical]
        IMessageSink CreateMessageSink(string url, object remoteChannelData, out string objectURI);
    }
}

