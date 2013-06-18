namespace System.ServiceModel.Channels
{
    using System;

    internal sealed class MsmqNonTransactedPoisonHandler : IPoisonHandlingStrategy, IDisposable
    {
        private MsmqReceiveHelper receiver;

        internal MsmqNonTransactedPoisonHandler(MsmqReceiveHelper receiver)
        {
            this.receiver = receiver;
        }

        public bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty)
        {
            return false;
        }

        public void Dispose()
        {
        }

        public void FinalDisposition(MsmqMessageProperty messageProperty)
        {
            this.receiver.DropOrRejectReceivedMessage(messageProperty, false);
        }

        public void Open()
        {
        }
    }
}

