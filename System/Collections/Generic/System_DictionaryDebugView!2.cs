namespace System.Collections.Generic
{
    using System;
    using System.Diagnostics;

    internal sealed class System_DictionaryDebugView<K, V>
    {
        private IDictionary<K, V> dict;

        public System_DictionaryDebugView(IDictionary<K, V> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }
            this.dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items
        {
            get
            {
                KeyValuePair<K, V>[] array = new KeyValuePair<K, V>[this.dict.Count];
                this.dict.CopyTo(array, 0);
                return array;
            }
        }
    }
}

