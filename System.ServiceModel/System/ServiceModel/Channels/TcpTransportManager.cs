namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class TcpTransportManager : ConnectionOrientedTransportManager<TcpChannelListener>
    {
        internal TcpTransportManager()
        {
        }

        protected virtual bool IsCompatible(TcpChannelListener channelListener)
        {
            return base.IsCompatible(channelListener);
        }

        internal override string Scheme
        {
            get
            {
                return Uri.UriSchemeNetTcp;
            }
        }
    }
}

