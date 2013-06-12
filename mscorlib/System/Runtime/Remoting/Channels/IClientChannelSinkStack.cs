namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IClientChannelSinkStack : IClientResponseChannelSinkStack
    {
        [SecurityCritical]
        object Pop(IClientChannelSink sink);
        [SecurityCritical]
        void Push(IClientChannelSink sink, object state);
    }
}

