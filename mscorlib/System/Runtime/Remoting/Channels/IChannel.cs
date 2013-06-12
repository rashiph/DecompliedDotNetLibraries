namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IChannel
    {
        [SecurityCritical]
        string Parse(string url, out string objectURI);

        string ChannelName { [SecurityCritical] get; }

        int ChannelPriority { [SecurityCritical] get; }
    }
}

