namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;

    internal delegate IClientChannelSinkStack AsyncMessageDelegate(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream, IClientChannelSinkStack sinkStack);
}

