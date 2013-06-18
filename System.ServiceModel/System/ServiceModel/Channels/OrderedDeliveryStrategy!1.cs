namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class OrderedDeliveryStrategy<ItemType> : DeliveryStrategy<ItemType> where ItemType: class, IDisposable
    {
        private bool isEnqueueInOrder;
        private Dictionary<long, ItemType> items;
        private Action<object> onDispatchCallback;
        private long windowStart;

        public OrderedDeliveryStrategy(InputQueueChannel<ItemType> channel, int quota, bool isEnqueueInOrder) : base(channel, quota)
        {
            this.isEnqueueInOrder = isEnqueueInOrder;
            this.items = new Dictionary<long, ItemType>();
            this.windowStart = 1L;
        }

        public override bool CanEnqueue(long sequenceNumber)
        {
            if (this.EnqueuedCount >= base.Quota)
            {
                return false;
            }
            if (this.isEnqueueInOrder && (sequenceNumber > this.windowStart))
            {
                return false;
            }
            return (((base.Channel.InternalPendingItems + sequenceNumber) - this.windowStart) < base.Quota);
        }

        public override void Dispose()
        {
            OrderedDeliveryStrategy<ItemType>.DisposeItems(this.items.GetEnumerator());
            this.items.Clear();
            base.Dispose();
        }

        private static void DisposeItems(Dictionary<long, ItemType>.Enumerator items)
        {
            if (items.MoveNext())
            {
                using (items.Current.Value)
                {
                    OrderedDeliveryStrategy<ItemType>.DisposeItems(items);
                }
            }
        }

        public override bool Enqueue(ItemType item, long sequenceNumber)
        {
            if (sequenceNumber > this.windowStart)
            {
                this.items.Add(sequenceNumber, item);
                return false;
            }
            this.windowStart += 1L;
            while (this.items.ContainsKey(this.windowStart))
            {
                if (base.Channel.EnqueueWithoutDispatch(item, base.DequeueCallback))
                {
                    ActionItem.Schedule(this.OnDispatchCallback, null);
                }
                item = this.items[this.windowStart];
                this.items.Remove(this.windowStart);
                this.windowStart += 1L;
            }
            return base.Channel.EnqueueWithoutDispatch(item, base.DequeueCallback);
        }

        private void OnDispatch(object state)
        {
            base.Channel.Dispatch();
        }

        public override int EnqueuedCount
        {
            get
            {
                return (base.Channel.InternalPendingItems + this.items.Count);
            }
        }

        private Action<object> OnDispatchCallback
        {
            get
            {
                if (this.onDispatchCallback == null)
                {
                    this.onDispatchCallback = new Action<object>(this.OnDispatch);
                }
                return this.onDispatchCallback;
            }
        }
    }
}

