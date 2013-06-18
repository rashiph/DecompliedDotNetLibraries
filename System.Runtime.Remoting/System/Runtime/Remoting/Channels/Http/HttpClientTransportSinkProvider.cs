namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.Runtime.Remoting.Channels;

    internal class HttpClientTransportSinkProvider : IClientChannelSinkProvider
    {
        private int _timeout;

        internal HttpClientTransportSinkProvider(int timeout)
        {
            this._timeout = timeout;
        }

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            HttpClientTransportSink sink = new HttpClientTransportSink((HttpClientChannel) channel, url);
            sink["timeout"] = this._timeout;
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

