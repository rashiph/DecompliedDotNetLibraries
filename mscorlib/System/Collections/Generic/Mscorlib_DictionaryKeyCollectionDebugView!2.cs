namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;

    internal sealed class Mscorlib_DictionaryKeyCollectionDebugView<TKey, TValue>
    {
        private ICollection<TKey> collection;

        public Mscorlib_DictionaryKeyCollectionDebugView(ICollection<TKey> collection)
        {
            if (collection == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
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

