namespace System.Runtime.Remoting.Channels
{
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;

    [ComVisible(true)]
    public interface IClientFormatterSink : IMessageSink, IClientChannelSink, IChannelSinkBase
    {
    }
}

