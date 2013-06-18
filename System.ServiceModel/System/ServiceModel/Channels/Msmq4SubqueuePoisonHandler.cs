namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal sealed class Msmq4SubqueuePoisonHandler : IPoisonHandlingStrategy, IDisposable
    {
        private MsmqReceiveHelper receiver;

        public Msmq4SubqueuePoisonHandler(MsmqReceiveHelper receiver)
        {
            this.receiver = receiver;
        }

        public bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty)
        {
            if (messageProperty.AbortCount > this.receiver.MsmqReceiveParameters.ReceiveRetryCount)
            {
                this.FinalDisposition(messageProperty);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
        }

        public void FinalDisposition(MsmqMessageProperty messageProperty)
        {
            switch (this.receiver.MsmqReceiveParameters.ReceiveErrorHandling)
            {
                case ReceiveErrorHandling.Fault:
                    MsmqReceiveHelper.TryAbortTransactionCurrent();
                    if (this.receiver.ChannelListener != null)
                    {
                        this.receiver.ChannelListener.FaultListener();
                    }
                    if (this.receiver.Channel == null)
                    {
                        break;
                    }
                    this.receiver.Channel.FaultChannel();
                    return;

                case ReceiveErrorHandling.Drop:
                    this.receiver.DropOrRejectReceivedMessage(messageProperty, false);
                    return;

                case ReceiveErrorHandling.Reject:
                    this.receiver.DropOrRejectReceivedMessage(messageProperty, true);
                    MsmqDiagnostics.PoisonMessageRejected(messageProperty.MessageId, this.receiver.InstanceId);
                    break;

                default:
                    return;
            }
        }

        public void Open()
        {
        }
    }
}

