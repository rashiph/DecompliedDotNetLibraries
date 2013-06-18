namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;

    internal class ConcurrencyInstanceContextFacet
    {
        private Queue<ConcurrencyBehavior.IWaiter> calloutMessageQueue;
        internal bool Locked;
        private Queue<ConcurrencyBehavior.IWaiter> newMessageQueue;

        private ConcurrencyBehavior.IWaiter DequeueFrom(Queue<ConcurrencyBehavior.IWaiter> queue)
        {
            ConcurrencyBehavior.IWaiter waiter = queue.Dequeue();
            if (queue.Count == 0)
            {
                queue.TrimExcess();
            }
            return waiter;
        }

        internal ConcurrencyBehavior.IWaiter DequeueWaiter()
        {
            if ((this.calloutMessageQueue != null) && (this.calloutMessageQueue.Count > 0))
            {
                return this.DequeueFrom(this.calloutMessageQueue);
            }
            return this.DequeueFrom(this.newMessageQueue);
        }

        internal void EnqueueCalloutMessage(ConcurrencyBehavior.IWaiter waiter)
        {
            if (this.calloutMessageQueue == null)
            {
                this.calloutMessageQueue = new Queue<ConcurrencyBehavior.IWaiter>();
            }
            this.calloutMessageQueue.Enqueue(waiter);
        }

        internal void EnqueueNewMessage(ConcurrencyBehavior.IWaiter waiter)
        {
            if (this.newMessageQueue == null)
            {
                this.newMessageQueue = new Queue<ConcurrencyBehavior.IWaiter>();
            }
            this.newMessageQueue.Enqueue(waiter);
        }

        internal bool HasWaiters
        {
            get
            {
                return (((this.calloutMessageQueue != null) && (this.calloutMessageQueue.Count > 0)) || ((this.newMessageQueue != null) && (this.newMessageQueue.Count > 0)));
            }
        }
    }
}

