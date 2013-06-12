namespace System.Net
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    internal class CachedTransportContext : TransportContext
    {
        private ChannelBinding binding;

        internal CachedTransportContext(ChannelBinding binding)
        {
            this.binding = binding;
        }

        public override ChannelBinding GetChannelBinding(ChannelBindingKind kind)
        {
            if (kind != ChannelBindingKind.Endpoint)
            {
                return null;
            }
            return this.binding;
        }
    }
}

