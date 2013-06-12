namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;

    internal sealed class System_DictionaryKeyCollectionDebugView<TKey, TValue>
    {
        private ICollection<TKey> collection;

        public System_DictionaryKeyCollectionDebugView(ICollection<TKey> collection)
        {
            if (collection == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.collection);
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items
        {
            get
            {
                TKey[] array = new TKey[this.collection.Count];
                this.collection.CopyTo(array, 0);
                return array;
            }
        }
    }
}

