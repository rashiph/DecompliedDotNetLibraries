namespace System.Net
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    public abstract class TransportContext
    {
        protected TransportContext()
        {
        }

        public abstract ChannelBinding GetChannelBinding(ChannelBindingKind kind);
    }
}

