namespace System.Dynamic.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ReadOnlyDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {
        private readonly IDictionary<K, V> _dict;

        internal ReadOnlyDictionary(IDictionary<K, V> dict)
        {
            ReadOnlyDictionary<K, V> dictionary = dict as ReadOnlyDictionary<K, V>;
            this._dict = (dictionary != null) ? dictionary._dict : dict;
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return this._dict.Contains(item);
        }

        public bool ContainsKey(K key)
        {
            return this._dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            this._dict.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this._dict.GetEnumerator();
        }

        void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item)
        {
            throw Error.CollectionReadOnly();
        }

        void ICollection<KeyValuePair<K, V>>.Clear()
        {
            throw Error.CollectionReadOnly();
        }

        bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item)
        {
            throw Error.CollectionReadOnly();
        }

        void IDictionary<K, V>.Add(K key, V value)
        {
            throw Error.CollectionReadOnly();
        }

        bool IDictionary<K, V>.Remove(K key)
        {
            throw Error.CollectionReadOnly();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._dict.GetEnumerator();
        }

        public bool TryGetValue(K key, out V value)
        {
            return this._dict.TryGetValue(key, out value);
        }

        public int Count
        {
            get
            {
                return this._dict.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public V this[K key]
        {
            get
            {
                return this._dict[key];
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                ICollection<K> keys = this._dict.Keys;
                if (!keys.IsReadOnly)
                {
                    return new ReadOnlyWrapper<K, V, K>(keys);
                }
                return keys;
            }
        }

        V IDictionary<K, V>.this[K key]
        {
            get
            {
                return this._dict[key];
            }
            set
            {
                throw Error.CollectionReadOnly();
            }
        }

        public ICollection<V> Values
        {
            get
            {
                ICollection<V> values = this._dict.Values;
                if (!values.IsReadOnly)
                {
                    return new ReadOnlyWrapper<K, V, V>(values);
                }
                return values;
            }
        }

        private sealed class ReadOnlyWrapper<T> : ICollection<T>, IEnumerable<T>, IEnumerable
        {
            private readonly ICollection<T> _collection;

            internal ReadOnlyWrapper(ICollection<T> collection)
            {
                this._collection = collection;
            }

            public void Add(T item)
            {
                throw Error.CollectionReadOnly();
            }

            public void Clear()
            {
                throw Error.CollectionReadOnly();
            }

            public bool Contains(T item)
            {
                return this._collection.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                this._collection.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this._collection.GetEnumerator();
            }

            public bool Remove(T item)
            {
                throw Error.CollectionReadOnly();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._collection.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return this._collection.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }
        }
    }
}

