namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IChannelReceiver : IChannel
    {
        [SecurityCritical]
        string[] GetUrlsForUri(string objectURI);
        [SecurityCritical]
        void StartListening(object data);
        [SecurityCritical]
        void StopListening(object data);

        object ChannelData { [SecurityCritical] get; }
    }
}

