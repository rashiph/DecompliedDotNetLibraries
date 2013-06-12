namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IChannelReceiverHook
    {
        [SecurityCritical]
        void AddHookChannelUri(string channelUri);

        string ChannelScheme { [SecurityCritical] get; }

        IServerChannelSink ChannelSinkChain { [SecurityCritical] get; }

        bool WantsToListen { [SecurityCritical] get; }
    }
}

