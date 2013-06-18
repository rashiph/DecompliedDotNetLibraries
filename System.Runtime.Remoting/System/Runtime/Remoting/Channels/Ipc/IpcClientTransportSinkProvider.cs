namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting.Channels;

    internal class IpcClientTransportSinkProvider : IClientChannelSinkProvider
    {
        private IDictionary _prop;

        internal IpcClientTransportSinkProvider(IDictionary properties)
        {
            this._prop = properties;
        }

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            IpcClientTransportSink sink = new IpcClientTransportSink(url, (IpcClientChannel) channel);
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

