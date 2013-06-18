namespace Microsoft.Build.Collections
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    internal class CopyOnWriteDictionary<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IDictionary, ICollection, IEnumerable
    {
        private CopyOnWriteBackingDictionary<K, V, K, V> backing;
        private readonly int capacity;
        private readonly IEqualityComparer<K> keyComparer;

        internal CopyOnWriteDictionary()
        {
        }

        private CopyOnWriteDictionary(Microsoft.Build.Collections.CopyOnWriteDictionary<K, V> that)
        {
            this.keyComparer = that.keyComparer;
            this.backing = that.backing;
            if (this.backing != null)
            {
                lock (this.backing.SyncRoot)
                {
                    this.backing.AddRef();
                }
            }
        }

        internal CopyOnWriteDictionary(IEqualityComparer<K> keyComparer) : this(0, keyComparer)
        {
        }

        internal CopyOnWriteDictionary(int capacity) : this(capacity, null)
        {
        }

        internal CopyOnWriteDictionary(int capacity, IEqualityComparer<K> keyComparer)
        {
            this.capacity = capacity;
            this.keyComparer = keyComparer;
        }

        protected CopyOnWriteDictionary(SerializationInfo info, StreamingContext context)
        {
        }

        public void Add(KeyValuePair<K, V> item)
        {
            this.WriteOperation.Add(item);
        }

        public void Add(K key, V value)
        {
            this.WriteOperation.Add(key, value);
        }

        public void Clear()
        {
            if (this.ReadOperation.Count > 0)
            {
                this.WriteOperation.Clear();
            }
        }

        internal Microsoft.Build.Collections.CopyOnWriteDictionary<K, V> Clone()
        {
            return new Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>((Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>) this);
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return this.ReadOperation.Contains(item);
        }

        public bool ContainsKey(K key)
        {
            return this.ReadOperation.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            this.ReadOperation.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return this.ReadOperation.GetEnumerator();
        }

        internal bool HasSameBacking(Microsoft.Build.Collections.CopyOnWriteDictionary<K, V> other)
        {
            return object.ReferenceEquals(other.backing, this.backing);
        }

        public bool Remove(K key)
        {
            if (!this.ReadOperation.HasNoClones && !this.ReadOperation.ContainsKey(key))
            {
                return false;
            }
            return this.WriteOperation.Remove(key);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            if (!this.ReadOperation.HasNoClones && !this.ReadOperation.ContainsKey(item.Key))
            {
                return false;
            }
            return this.WriteOperation.Remove(item);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            int num = 0;
            foreach (KeyValuePair<K, V> pair in this)
            {
                array.SetValue(new DictionaryEntry(pair.Key, pair.Value), (int) (index + num));
                num++;
            }
        }

        void IDictionary.Add(object key, object value)
        {
            this.Add((K) key, (V) value);
        }

        void IDictionary.Clear()
        {
            this.Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return this.ContainsKey((K) key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return this.ReadOperation.GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            this.Remove((K) key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(K key, out V value)
        {
            return this.ReadOperation.TryGetValue(key, out value);
        }

        internal IEqualityComparer<K> Comparer
        {
            get
            {
                return this.keyComparer;
            }
        }

        public int Count
        {
            get
            {
                return this.ReadOperation.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.ReadOperation.IsReadOnly;
            }
        }

        public V this[K key]
        {
            get
            {
                return this.ReadOperation[key];
            }
            set
            {
                if (this.ReadOperation.HasNoClones)
                {
                    this.WriteOperation[key] = value;
                }
                else
                {
                    V local = default(V);
                    if (!this.ReadOperation.TryGetValue(key, out local) || !EqualityComparer<V>.Default.Equals(local, value))
                    {
                        this.WriteOperation[key] = value;
                    }
                }
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                return this.ReadOperation.Keys;
            }
        }

        private CopyOnWriteBackingDictionary<K, V, K, V> ReadOperation
        {
            get
            {
                if (this.backing == null)
                {
                    return CopyOnWriteBackingDictionary<K, V, K, V>.ReadOnlyEmptyInstance;
                }
                return this.backing;
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return this.IsReadOnly;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (!this.ContainsKey((K) key))
                {
                    return null;
                }
                return this[(K) key];
            }
            set
            {
                this[(K) key] = (V) value;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return (ICollection) this.Keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return (ICollection) this.Values;
            }
        }

        public ICollection<V> Values
        {
            get
            {
                return this.ReadOperation.Values;
            }
        }

        private CopyOnWriteBackingDictionary<K, V, K, V> WriteOperation
        {
            get
            {
                if (this.backing == null)
                {
                    this.backing = new CopyOnWriteBackingDictionary<K, V, K, V>(this.capacity, this.keyComparer);
                }
                else
                {
                    lock (this.backing.SyncRoot)
                    {
                        this.backing = this.backing.CloneForWriteIfNecessary();
                    }
                }
                return this.backing;
            }
        }

        [Serializable]
        private class CopyOnWriteBackingDictionary<K1, V1> : Microsoft.Build.Collections.HybridDictionary<K1, V1>
        {
            private static readonly Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1> readOnlyEmptyDictionary;
            [NonSerialized]
            private int refCount;

            static CopyOnWriteBackingDictionary()
            {
                Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1>.readOnlyEmptyDictionary = new Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1>();
            }

            private CopyOnWriteBackingDictionary()
            {
                this.refCount = 1;
            }

            private CopyOnWriteBackingDictionary(Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1> that) : base(that, that.Comparer)
            {
                this.refCount = 1;
            }

            public CopyOnWriteBackingDictionary(int capacity, IEqualityComparer<K1> comparer) : base(capacity, comparer)
            {
                this.refCount = 1;
            }

            protected CopyOnWriteBackingDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
            {
                this.refCount = 1;
            }

            public int AddRef()
            {
                return ++this.refCount;
            }

            public Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1> CloneForWriteIfNecessary()
            {
                if (!this.HasNoClones)
                {
                    this.refCount--;
                    return new Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1>((Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1>) this);
                }
                return (Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1>) this;
            }

            public bool HasNoClones
            {
                get
                {
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.refCount >= 1, "refCount should not be less than 1.");
                    return (this.refCount == 1);
                }
            }

            public static Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1> ReadOnlyEmptyInstance
            {
                get
                {
                    return Microsoft.Build.Collections.CopyOnWriteDictionary<K, V>.CopyOnWriteBackingDictionary<K1, V1>.readOnlyEmptyDictionary;
                }
            }
        }
    }
}

