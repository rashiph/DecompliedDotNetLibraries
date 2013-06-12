namespace System.Collections.Concurrent
{
    using System;
    using System.Diagnostics;

    internal sealed class SystemCollectionsConcurrent_ProducerConsumerCollectionDebugView<T>
    {
        private IProducerConsumerCollection<T> m_collection;

        public SystemCollectionsConcurrent_ProducerConsumerCollectionDebugView(IProducerConsumerCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.m_collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return this.m_collection.ToArray();
            }
        }
    }
}

