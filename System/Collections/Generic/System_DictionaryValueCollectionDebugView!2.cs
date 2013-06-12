namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;

    internal sealed class System_DictionaryValueCollectionDebugView<TKey, TValue>
    {
        private ICollection<TValue> collection;

        public System_DictionaryValueCollectionDebugView(ICollection<TValue> collection)
        {
            if (collection == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.collection);
            }
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                TValue[] array = new TValue[this.collection.Count];
                this.collection.CopyTo(array, 0);
                return array;
            }
        }
    }
}

