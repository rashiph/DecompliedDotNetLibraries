namespace System.Collections.Concurrent
{
    using System;

    internal sealed class SystemThreadingCollection_IProducerConsumerCollectionDebugView<T>
    {
        private IProducerConsumerCollection<T> m_collection;

        public SystemThreadingCollection_IProducerConsumerCollectionDebugView(IProducerConsumerCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.m_collection = collection;
        }

        public T[] Items
        {
            get
            {
                return this.m_collection.ToArray();
            }
        }
    }
}

