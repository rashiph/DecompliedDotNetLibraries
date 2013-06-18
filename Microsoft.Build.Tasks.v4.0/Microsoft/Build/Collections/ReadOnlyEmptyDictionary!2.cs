namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class ReadOnlyEmptyDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {
        private static Microsoft.Build.Collections.ReadOnlyEmptyDictionary<K, V> instance;

        private ReadOnlyEmptyDictionary()
        {
        }

        public void Add(KeyValuePair<K, V> item)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public void Add(K key, V value)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public void Clear()
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return false;
        }

        public bool ContainsKey(K key)
        {
            return false;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return Microsoft.Build.Collections.EmptyEnumerable<KeyValuePair<K, V>>.Instance.GetEnumerator();
        }

        public bool Remove(K key)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(K key, out V value)
        {
            value = default(V);
            return false;
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        public static Microsoft.Build.Collections.ReadOnlyEmptyDictionary<K, V> Instance
        {
            get
            {
                if (Microsoft.Build.Collections.ReadOnlyEmptyDictionary<K, V>.instance == null)
                {
                    Microsoft.Build.Collections.ReadOnlyEmptyDictionary<K, V>.instance = new Microsoft.Build.Collections.ReadOnlyEmptyDictionary<K, V>();
                }
                return Microsoft.Build.Collections.ReadOnlyEmptyDictionary<K, V>.instance;
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
                return new Dictionary<K, V>()[key];
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                return Microsoft.Build.Collections.ReadOnlyEmptyList<K>.Instance;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                return (ICollection<V>) Microsoft.Build.Collections.ReadOnlyEmptyList<V>.Instance;
            }
        }
    }
}

