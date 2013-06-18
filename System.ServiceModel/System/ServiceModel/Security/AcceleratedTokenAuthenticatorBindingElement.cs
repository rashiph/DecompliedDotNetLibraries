namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class AcceleratedTokenAuthenticatorBindingElement : BindingElement
    {
        private AcceleratedTokenAuthenticator authenticator;

        public AcceleratedTokenAuthenticatorBindingElement(AcceleratedTokenAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return this.authenticator.BuildNegotiationChannelListener<TChannel>(context);
        }

        public override BindingElement Clone()
        {
            return new AcceleratedTokenAuthenticatorBindingElement(this.authenticator);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return this.authenticator.BootstrapSecurityBindingElement.GetProperty<ISecurityCapabilities>(context);
            }
            return context.GetInnerProperty<T>();
        }
    }
}

