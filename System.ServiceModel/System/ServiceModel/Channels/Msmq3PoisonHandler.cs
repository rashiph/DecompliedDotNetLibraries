namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal sealed class Msmq3PoisonHandler : IPoisonHandlingStrategy, IDisposable
    {
        private const int maxTrackedMessages = 0x100;
        private MsmqReceiveHelper receiver;
        private object thisLock = new object();
        private SortedList<long, int> trackedMessages;

        internal Msmq3PoisonHandler(MsmqReceiveHelper receiver)
        {
            this.receiver = receiver;
            this.trackedMessages = new SortedList<long, int>(0x100);
        }

        public bool CheckAndHandlePoisonMessage(MsmqMessageProperty messageProperty)
        {
            int num2;
            long lookupId = messageProperty.LookupId;
            lock (this.thisLock)
            {
                num2 = this.UpdateSeenCount(lookupId);
                if ((num2 > (this.receiver.MsmqReceiveParameters.ReceiveRetryCount + 1)) && (this.receiver.MsmqReceiveParameters.ReceiveRetryCount != 0x7fffffff))
                {
                    this.FinalDisposition(messageProperty);
                    this.trackedMessages.Remove(lookupId);
                    return true;
                }
            }
            messageProperty.AbortCount = num2 - 1;
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
                    if (this.receiver.Channel != null)
                    {
                        this.receiver.Channel.FaultChannel();
                    }
                    return;

                case ReceiveErrorHandling.Drop:
                    this.receiver.DropOrRejectReceivedMessage(messageProperty, false);
                    return;
            }
        }

        public void Open()
        {
        }

        private int UpdateSeenCount(long lookupId)
        {
            int num;
            if (this.trackedMessages.TryGetValue(lookupId, out num))
            {
                num++;
                this.trackedMessages[lookupId] = num;
                return num;
            }
            if (0x100 == this.trackedMessages.Count)
            {
                this.trackedMessages.RemoveAt(0);
            }
            this.trackedMessages.Add(lookupId, 1);
            return 1;
        }
    }
}

