namespace System.Net
{
    using System;
    using System.Net.Security;
    using System.Security.Authentication.ExtendedProtection;

    internal class SslStreamContext : TransportContext
    {
        private SslStream sslStream;

        internal SslStreamContext(SslStream sslStream)
        {
            this.sslStream = sslStream;
        }

        public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            return this.sslStream.GetChannelBinding(kind);
        }
    }
}

