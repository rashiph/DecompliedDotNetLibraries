namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IServerChannelSinkStack : IServerResponseChannelSinkStack
    {
        [SecurityCritical]
        object Pop(IServerChannelSink sink);
        [SecurityCritical]
        void Push(IServerChannelSink sink, object state);
        [SecurityCritical]
        void ServerCallback(IAsyncResult ar);
        [SecurityCritical]
        void Store(IServerChannelSink sink, object state);
        [SecurityCritical]
        void StoreAndDispatch(IServerChannelSink sink, object state);
    }
}

