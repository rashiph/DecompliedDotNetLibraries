namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [ComVisible(true)]
    public interface IClientChannelSink : IChannelSinkBase
    {
        [SecurityCritical]
        void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream stream);
        [SecurityCritical]
        void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream);
        [SecurityCritical]
        Stream GetRequestStream(IMessage msg, ITransportHeaders headers);
        [SecurityCritical]
        void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream);

        IClientChannelSink NextChannelSink { [SecurityCritical] get; }
    }
}

