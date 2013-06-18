namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal abstract class DeliveryStrategy<ItemType> : IDisposable where ItemType: class, IDisposable
    {
        private InputQueueChannel<ItemType> channel;
        private Action dequeueCallback;
        private int quota;

        public DeliveryStrategy(InputQueueChannel<ItemType> channel, int quota)
        {
            if (quota <= 0)
            {
                throw Fx.AssertAndThrow("Argument quota must be positive.");
            }
            this.channel = channel;
            this.quota = quota;
        }

        public abstract bool CanEnqueue(long sequenceNumber);
        public virtual void Dispose()
        {
        }

        public abstract bool Enqueue(ItemType item, long sequenceNumber);

        protected InputQueueChannel<ItemType> Channel
        {
            get
            {
                return this.channel;
            }
        }

        public Action DequeueCallback
        {
            get
            {
                return this.dequeueCallback;
            }
            set
            {
                this.dequeueCallback = value;
            }
        }

        public abstract int EnqueuedCount { get; }

        protected int Quota
        {
            get
            {
                return this.quota;
            }
        }
    }
}

