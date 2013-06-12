namespace System.Collections.Concurrent
{
    using System;
    using System.Diagnostics;

    internal sealed class SystemThreadingCollections_BlockingCollectionDebugView<T>
    {
        private BlockingCollection<T> m_blockingCollection;

        public SystemThreadingCollections_BlockingCollectionDebugView(BlockingCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.m_blockingCollection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return this.m_blockingCollection.ToArray();
            }
        }
    }
}

