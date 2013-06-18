namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.ServiceModel.Channels;

    internal sealed class MsmqIntegrationInputChannel : MsmqInputChannelBase
    {
        public MsmqIntegrationInputChannel(MsmqIntegrationChannelListener listener) : base(listener, new MsmqIntegrationMessagePool(8))
        {
        }

        protected override Message DecodeMsmqMessage(MsmqInputMessage msmqMessage, MsmqMessageProperty property)
        {
            MsmqIntegrationChannelListener manager = base.Manager as MsmqIntegrationChannelListener;
            return MsmqDecodeHelper.DecodeIntegrationDatagram(manager, base.MsmqReceiveHelper, msmqMessage as MsmqIntegrationInputMessage, property);
        }
    }
}

