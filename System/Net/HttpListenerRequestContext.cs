namespace System.Net
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    internal class HttpListenerRequestContext : TransportContext
    {
        private HttpListenerRequest request;

        internal HttpListenerRequestContext(HttpListenerRequest request)
        {
            this.request = request;
        }

        public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            if (kind != ChannelBindingKind.Endpoint)
            {
                throw new NotSupportedException(SR.GetString("net_listener_invalid_cbt_type", new object[] { kind.ToString() }));
            }
            return this.request.GetChannelBinding();
        }
    }
}

