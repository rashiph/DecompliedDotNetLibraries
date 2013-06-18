namespace System.Runtime
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class ReadOnlyKeyedCollection<TKey, TValue> : ReadOnlyCollection<TValue>
    {
        private KeyedCollection<TKey, TValue> innerCollection;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ReadOnlyKeyedCollection(KeyedCollection<TKey, TValue> innerCollection) : base(innerCollection)
        {
            this.innerCollection = innerCollection;
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.innerCollection[key];
            }
        }
    }
}

