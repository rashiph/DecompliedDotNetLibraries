namespace System.ServiceModel.Channels
{
    using System;

    internal sealed class MsmqInputChannelListener : MsmqInputChannelListenerBase
    {
        internal MsmqInputChannelListener(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters) : base(bindingElement, context, receiveParameters)
        {
            base.SetSecurityTokenAuthenticator(MsmqUri.NetMsmqAddressTranslator.Scheme, context);
        }

        protected override IInputChannel CreateInputChannel(MsmqInputChannelListenerBase listener)
        {
            return new MsmqInputChannel(listener as MsmqInputChannelListener);
        }
    }
}

