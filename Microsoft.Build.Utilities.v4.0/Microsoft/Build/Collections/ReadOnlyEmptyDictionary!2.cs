namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;

    internal class ReadOnlyEmptyDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {
        private static ReadOnlyEmptyDictionary<K, V> instance;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private ReadOnlyEmptyDictionary()
        {
        }

        public void Add(KeyValuePair<K, V> item)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public void Add(K key, V value)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
        }

        public void Clear()
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
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
            return EmptyEnumerable<KeyValuePair<K, V>>.Instance.GetEnumerator();
        }

        public bool Remove(K key)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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

        public static ReadOnlyEmptyDictionary<K, V> Instance
        {
            get
            {
                if (ReadOnlyEmptyDictionary<K, V>.instance == null)
                {
                    ReadOnlyEmptyDictionary<K, V>.instance = new ReadOnlyEmptyDictionary<K, V>();
                }
                return ReadOnlyEmptyDictionary<K, V>.instance;
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
                ErrorUtilities.ThrowInvalidOperation("OM_NotSupportedReadOnlyCollection", new object[0]);
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                return ReadOnlyEmptyList<K>.Instance;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                return (ICollection<V>) ReadOnlyEmptyList<V>.Instance;
            }
        }
    }
}

