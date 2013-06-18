namespace System.ServiceModel.Channels
{
    using System;

    internal sealed class MsmqInputChannel : MsmqInputChannelBase
    {
        public MsmqInputChannel(MsmqInputChannelListener listener) : base(listener, new MsmqInputMessagePool((listener.ReceiveParameters as MsmqTransportReceiveParameters).MaxPoolSize))
        {
        }

        protected override Message DecodeMsmqMessage(MsmqInputMessage msmqMessage, MsmqMessageProperty messageProperty)
        {
            MsmqInputChannelListener manager = base.Manager as MsmqInputChannelListener;
            return MsmqDecodeHelper.DecodeTransportDatagram(manager, base.MsmqReceiveHelper, msmqMessage, messageProperty);
        }
    }
}

