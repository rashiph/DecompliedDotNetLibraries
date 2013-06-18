namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class ReadOnlyDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {
        private readonly IDictionary<K, V> backing;

        private ReadOnlyDictionary(IDictionary<K, V> backing)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowInternalNull(backing, "backing");
            this.backing = backing;
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
            return this.backing.Contains(item);
        }

        public bool ContainsKey(K key)
        {
            return this.backing.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            this.backing.CopyTo(array, arrayIndex);
        }

        internal static ReadOnlyDictionary<K, V> CreateClone(IDictionary<K, V> original, IEqualityComparer<K> comparer)
        {
            Dictionary<K, V> backing = null;
            if (original != null)
            {
                backing = new Dictionary<K, V>(original, comparer);
            }
            return ReadOnlyDictionary<K, V>.CreateWrapper(backing);
        }

        internal static ReadOnlyDictionary<K, V> CreateFrom(IEnumerable<KeyValuePair<K, V>> source, IEqualityComparer<K> comparer)
        {
            Dictionary<K, V> backing = null;
            if (source != null)
            {
                backing = new Dictionary<K, V>(comparer);
                foreach (KeyValuePair<K, V> pair in source)
                {
                    backing.Add(pair.Key, pair.Value);
                }
            }
            return ReadOnlyDictionary<K, V>.CreateWrapper(backing);
        }

        internal static ReadOnlyDictionary<K, V> CreateWrapper(IDictionary<K, V> backing)
        {
            if (backing == null)
            {
                backing = Microsoft.Build.Collections.ReadOnlyEmptyDictionary<K, V>.Instance;
            }
            return new ReadOnlyDictionary<K, V>(backing);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this.backing.GetEnumerator();
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
            return this.backing.GetEnumerator();
        }

        public bool TryGetValue(K key, out V value)
        {
            return this.backing.TryGetValue(key, out value);
        }

        public int Count
        {
            get
            {
                return this.backing.Count;
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
                return this.backing[key];
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
                return new ReadOnlyCollection<K>(this.backing.Keys);
            }
        }

        public ICollection<V> Values
        {
            get
            {
                return new ReadOnlyCollection<V>(this.backing.Values);
            }
        }
    }
}

