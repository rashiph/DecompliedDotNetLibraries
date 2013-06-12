namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [ComVisible(true)]
    public interface IClientResponseChannelSinkStack
    {
        [SecurityCritical]
        void AsyncProcessResponse(ITransportHeaders headers, Stream stream);
        [SecurityCritical]
        void DispatchException(Exception e);
        [SecurityCritical]
        void DispatchReplyMessage(IMessage msg);
    }
}

