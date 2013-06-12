namespace System.Net
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    internal class ConnectStreamContext : TransportContext
    {
        private ConnectStream connectStream;

        internal ConnectStreamContext(ConnectStream connectStream)
        {
            this.connectStream = connectStream;
        }

        public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            return this.connectStream.GetChannelBinding(kind);
        }
    }
}

