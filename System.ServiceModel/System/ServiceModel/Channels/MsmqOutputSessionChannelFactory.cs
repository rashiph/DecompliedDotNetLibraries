namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal sealed class MsmqOutputSessionChannelFactory : MsmqChannelFactory<IOutputSessionChannel>
    {
        internal MsmqOutputSessionChannelFactory(MsmqTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
        }

        protected override IOutputSessionChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            base.ValidateScheme(via);
            return new MsmqOutputSessionChannel(this, to, via, base.ManualAddressing);
        }
    }
}

