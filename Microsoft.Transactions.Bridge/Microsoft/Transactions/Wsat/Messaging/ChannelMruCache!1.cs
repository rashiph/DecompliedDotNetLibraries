namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal class ChannelMruCache<TItem> : MruCache<ChannelMruCacheKey, ReferenceCountedChannel<TItem>> where TItem: class
    {
        public ChannelMruCache() : base(40, 50, new ChannelMruCacheKey(null, null))
        {
        }

        protected override void OnSingleItemRemoved(ReferenceCountedChannel<TItem> referenceCountedObject)
        {
            referenceCountedObject.Release();
        }
    }
}

