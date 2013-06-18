namespace System.ServiceModel.Channels
{
    using System;

    internal class TransactionInputDuplexChannelGeneric<TChannel> : TransactionDuplexChannelGeneric<TChannel> where TChannel: class, IDuplexChannel
    {
        public TransactionInputDuplexChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel) : base(channelManager, innerChannel, MessageDirection.Input)
        {
        }
    }
}

