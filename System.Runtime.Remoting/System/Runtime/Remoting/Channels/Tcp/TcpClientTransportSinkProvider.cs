namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting.Channels;

    internal class TcpClientTransportSinkProvider : IClientChannelSinkProvider
    {
        private IDictionary _prop;

        internal TcpClientTransportSinkProvider(IDictionary properties)
        {
            this._prop = properties;
        }

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            TcpClientTransportSink sink = new TcpClientTransportSink(url, (TcpClientChannel) channel);
            if (this._prop != null)
            {
                foreach (object obj2 in this._prop.Keys)
                {
                    sink[obj2] = this._prop[obj2];
                }
            }
            return sink;
        }

        public IClientChannelSinkProvider Next
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

