namespace System.ServiceModel.Channels
{
    using System;

    internal class TransactionOutputDuplexChannelGeneric<TChannel> : TransactionDuplexChannelGeneric<TChannel> where TChannel: class, IDuplexChannel
    {
        public TransactionOutputDuplexChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel) : base(channelManager, innerChannel, MessageDirection.Output)
        {
        }
    }
}

