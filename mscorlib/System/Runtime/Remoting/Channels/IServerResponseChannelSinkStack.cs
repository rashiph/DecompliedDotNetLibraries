namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [ComVisible(true)]
    public interface IServerResponseChannelSinkStack
    {
        [SecurityCritical]
        void AsyncProcessResponse(IMessage msg, ITransportHeaders headers, Stream stream);
        [SecurityCritical]
        Stream GetResponseStream(IMessage msg, ITransportHeaders headers);
    }
}

