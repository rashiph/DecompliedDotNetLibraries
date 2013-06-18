namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal sealed class MsmqOutputChannelFactory : MsmqChannelFactory<IOutputChannel>
    {
        internal MsmqOutputChannelFactory(MsmqTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            base.ValidateScheme(via);
            return new MsmqOutputChannel(this, to, via, base.ManualAddressing);
        }
    }
}

