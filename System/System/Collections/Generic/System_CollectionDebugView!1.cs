namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;

    internal sealed class System_CollectionDebugView<T>
    {
        private ICollection<T> collection;

        public System_CollectionDebugView(ICollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = new T[this.collection.Count];
                this.collection.CopyTo(array, 0);
                return array;
            }
        }
    }
}

