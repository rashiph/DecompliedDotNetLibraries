namespace System.Collections.Concurrent
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, DebuggerTypeProxy(typeof(Mscorlib_DictionaryDebugView<,>)), ComVisible(false), DebuggerDisplay("Count = {Count}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
        private const int DEFAULT_CAPACITY = 0x1f;
        private const int DEFAULT_CONCURRENCY_MULTIPLIER = 4;
        [NonSerialized]
        private volatile Node<TKey, TValue>[] m_buckets;
        private IEqualityComparer<TKey> m_comparer;
        [NonSerialized]
        private volatile int[] m_countPerLock;
        [NonSerialized]
        private object[] m_locks;
        private KeyValuePair<TKey, TValue>[] m_serializationArray;
        private int m_serializationCapacity;
        private int m_serializationConcurrencyLevel;

        public ConcurrentDictionary() : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, 0x1f)
        {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentDictionary(IEqualityComparer<TKey> comparer) : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, 0x1f, comparer)
        {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(ConcurrentDictionary<TKey, TValue>.DefaultConcurrencyLevel, collection, comparer)
        {
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity) : this(concurrencyLevel, capacity, EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(concurrencyLevel, 0x1f, comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this.InitializeFromCollection(collection);
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            if (concurrencyLevel < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel", this.GetResource("ConcurrentDictionary_ConcurrencyLevelMustBePositive"));
            }
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", this.GetResource("ConcurrentDictionary_CapacityMustNotBeNegative"));
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            if (capacity < concurrencyLevel)
            {
                capacity = concurrencyLevel;
            }
            this.m_locks = new object[concurrencyLevel];
            for (int i = 0; i < this.m_locks.Length; i++)
            {
                this.m_locks[i] = new object();
            }
            this.m_countPerLock = new int[this.m_locks.Length];
            this.m_buckets = new Node<TKey, TValue>[capacity];
            this.m_comparer = comparer;
        }

        private void AcquireAllLocks(ref int locksAcquired)
        {
            if (CDSCollectionETWBCLProvider.Log.IsEnabled())
            {
                CDSCollectionETWBCLProvider.Log.ConcurrentDictionary_AcquiringAllLocks(this.m_buckets.Length);
            }
            this.AcquireLocks(0, this.m_locks.Length, ref locksAcquired);
        }

        private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(this.m_locks[i], ref lockTaken);
                }
                finally
                {
                    if (lockTaken)
                    {
                        locksAcquired++;
                    }
                }
            }
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue local;
            TValue local2;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (addValueFactory == null)
            {
                throw new ArgumentNullException("addValueFactory");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            do
            {
                TValue local3;
                while (this.TryGetValue(key, out local3))
                {
                    local = updateValueFactory(key, local3);
                    if (this.TryUpdate(key, local, local3))
                    {
                        return local;
                    }
                }
                local = addValueFactory(key);
            }
            while (!this.TryAddInternal(key, local, false, true, out local2));
            return local2;
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue local2;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException("updateValueFactory");
            }
            do
            {
                TValue local3;
                while (this.TryGetValue(key, out local3))
                {
                    TValue newValue = updateValueFactory(key, local3);
                    if (this.TryUpdate(key, newValue, local3))
                    {
                        return newValue;
                    }
                }
            }
            while (!this.TryAddInternal(key, addValue, false, true, out local2));
            return local2;
        }

        [Conditional("DEBUG")]
        private void Assert(bool condition)
        {
        }

        public void Clear()
        {
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                this.m_buckets = new Node<TKey, TValue>[0x1f];
                Array.Clear(this.m_countPerLock, 0, this.m_countPerLock.Length);
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        public bool ContainsKey(TKey key)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return this.TryGetValue(key, out local);
        }

        private void CopyToEntries(DictionaryEntry[] array, int index)
        {
            Node<TKey, TValue>[] buckets = this.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (Node<TKey, TValue> node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new DictionaryEntry(node.m_key, node.m_value);
                    index++;
                }
            }
        }

        private void CopyToObjects(object[] array, int index)
        {
            Node<TKey, TValue>[] buckets = this.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (Node<TKey, TValue> node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    index++;
                }
            }
        }

        private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index)
        {
            Node<TKey, TValue>[] buckets = this.m_buckets;
            for (int i = 0; i < buckets.Length; i++)
            {
                for (Node<TKey, TValue> node = buckets[i]; node != null; node = node.m_next)
                {
                    array[index] = new KeyValuePair<TKey, TValue>(node.m_key, node.m_value);
                    index++;
                }
            }
        }

        private void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount)
        {
            bucketNo = (hashcode & 0x7fffffff) % bucketCount;
            lockNo = bucketNo % this.m_locks.Length;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (Node<TKey, TValue> next in this.m_buckets)
            {
                Thread.MemoryBarrier();
                while (next != null)
                {
                    yield return new KeyValuePair<TKey, TValue>(next.m_key, next.m_value);
                    next = next.m_next;
                }
            }
        }

        private ReadOnlyCollection<TKey> GetKeys()
        {
            ReadOnlyCollection<TKey> onlys;
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                List<TKey> list = new List<TKey>();
                for (int i = 0; i < this.m_buckets.Length; i++)
                {
                    for (Node<TKey, TValue> node = this.m_buckets[i]; node != null; node = node.m_next)
                    {
                        list.Add(node.m_key);
                    }
                }
                onlys = new ReadOnlyCollection<TKey>(list);
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
            return onlys;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (valueFactory == null)
            {
                throw new ArgumentNullException("valueFactory");
            }
            if (!this.TryGetValue(key, out local))
            {
                this.TryAddInternal(key, valueFactory(key), false, true, out local);
            }
            return local;
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this.TryAddInternal(key, value, false, true, out local);
            return local;
        }

        private string GetResource(string key)
        {
            return Environment.GetResourceString(key);
        }

        private ReadOnlyCollection<TValue> GetValues()
        {
            ReadOnlyCollection<TValue> onlys;
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                List<TValue> list = new List<TValue>();
                for (int i = 0; i < this.m_buckets.Length; i++)
                {
                    for (Node<TKey, TValue> node = this.m_buckets[i]; node != null; node = node.m_next)
                    {
                        list.Add(node.m_value);
                    }
                }
                onlys = new ReadOnlyCollection<TValue>(list);
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
            return onlys;
        }

        private void GrowTable(Node<TKey, TValue>[] buckets)
        {
            int locksAcquired = 0;
            try
            {
                this.AcquireLocks(0, 1, ref locksAcquired);
                if (buckets == this.m_buckets)
                {
                    int num2;
                    try
                    {
                        num2 = (buckets.Length * 2) + 1;
                        while ((((num2 % 3) == 0) || ((num2 % 5) == 0)) || ((num2 % 7) == 0))
                        {
                            num2 += 2;
                        }
                    }
                    catch (OverflowException)
                    {
                        return;
                    }
                    Node<TKey, TValue>[] nodeArray = new Node<TKey, TValue>[num2];
                    int[] numArray = new int[this.m_locks.Length];
                    this.AcquireLocks(1, this.m_locks.Length, ref locksAcquired);
                    for (int i = 0; i < buckets.Length; i++)
                    {
                        Node<TKey, TValue> next;
                        for (Node<TKey, TValue> node = buckets[i]; node != null; node = next)
                        {
                            int num4;
                            int num5;
                            next = node.m_next;
                            this.GetBucketAndLockNo(node.m_hashcode, out num4, out num5, nodeArray.Length);
                            nodeArray[num4] = new Node<TKey, TValue>(node.m_key, node.m_value, node.m_hashcode, nodeArray[num4]);
                            numArray[num5]++;
                        }
                    }
                    this.m_buckets = nodeArray;
                    this.m_countPerLock = numArray;
                }
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (KeyValuePair<TKey, TValue> pair in collection)
            {
                TValue local;
                if (pair.Key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (!this.TryAddInternal(pair.Key, pair.Value, false, false, out local))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_SourceContainsDuplicateKeys"));
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            KeyValuePair<TKey, TValue>[] serializationArray = this.m_serializationArray;
            this.m_buckets = new Node<TKey, TValue>[this.m_serializationCapacity];
            this.m_countPerLock = new int[this.m_serializationConcurrencyLevel];
            this.m_locks = new object[this.m_serializationConcurrencyLevel];
            for (int i = 0; i < this.m_locks.Length; i++)
            {
                this.m_locks[i] = new object();
            }
            this.InitializeFromCollection(serializationArray);
            this.m_serializationArray = null;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            this.m_serializationArray = this.ToArray();
            this.m_serializationConcurrencyLevel = this.m_locks.Length;
            this.m_serializationCapacity = this.m_buckets.Length;
        }

        private void ReleaseLocks(int fromInclusive, int toExclusive)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                Monitor.Exit(this.m_locks[i]);
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ((IDictionary<TKey, TValue>) this).Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue local;
            if (!this.TryGetValue(keyValuePair.Key, out local))
            {
                return false;
            }
            return EqualityComparer<TValue>.Default.Equals(local, keyValuePair.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", this.GetResource("ConcurrentDictionary_IndexIsNegative"));
            }
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                int num2 = 0;
                for (int i = 0; i < this.m_locks.Length; i++)
                {
                    num2 += this.m_countPerLock[i];
                }
                if (((array.Length - num2) < index) || (num2 < 0))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayNotLargeEnough"));
                }
                this.CopyToPairs(array, index);
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue local;
            if (keyValuePair.Key == null)
            {
                throw new ArgumentNullException(this.GetResource("ConcurrentDictionary_ItemKeyIsNull"));
            }
            return this.TryRemoveInternal(keyValuePair.Key, out local, true, keyValuePair.Value);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            if (!this.TryAdd(key, value))
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_KeyAlreadyExisted"));
            }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            TValue local;
            return this.TryRemove(key, out local);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", this.GetResource("ConcurrentDictionary_IndexIsNegative"));
            }
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                int num2 = 0;
                for (int i = 0; i < this.m_locks.Length; i++)
                {
                    num2 += this.m_countPerLock[i];
                }
                if (((array.Length - num2) < index) || (num2 < 0))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayNotLargeEnough"));
                }
                KeyValuePair<TKey, TValue>[] pairArray = array as KeyValuePair<TKey, TValue>[];
                if (pairArray != null)
                {
                    this.CopyToPairs(pairArray, index);
                }
                else
                {
                    DictionaryEntry[] entryArray = array as DictionaryEntry[];
                    if (entryArray != null)
                    {
                        this.CopyToEntries(entryArray, index);
                    }
                    else
                    {
                        object[] objArray = array as object[];
                        if (objArray == null)
                        {
                            throw new ArgumentException(this.GetResource("ConcurrentDictionary_ArrayIncorrectType"), "array");
                        }
                        this.CopyToObjects(objArray, index);
                    }
                }
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
        }

        void IDictionary.Add(object key, object value)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!(key is TKey))
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfKeyIncorrect"));
            }
            try
            {
                local = (TValue) value;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfValueIncorrect"));
            }
            ((IDictionary<TKey, TValue>) this).Add((TKey) key, local);
        }

        bool IDictionary.Contains(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return ((key is TKey) && this.ContainsKey((TKey) key));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>((ConcurrentDictionary<TKey, TValue>) this);
        }

        void IDictionary.Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key is TKey)
            {
                TValue local;
                this.TryRemove((TKey) key, out local);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            KeyValuePair<TKey, TValue>[] pairArray2;
            int locksAcquired = 0;
            try
            {
                this.AcquireAllLocks(ref locksAcquired);
                int num2 = 0;
                for (int i = 0; i < this.m_locks.Length; i++)
                {
                    num2 += this.m_countPerLock[i];
                }
                KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[num2];
                this.CopyToPairs(array, 0);
                pairArray2 = array;
            }
            finally
            {
                this.ReleaseLocks(0, locksAcquired);
            }
            return pairArray2;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            TValue local;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return this.TryAddInternal(key, value, false, true, out local);
        }

        private bool TryAddInternal(TKey key, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue)
        {
            int num2;
            int num3;
            Node<TKey, TValue>[] nodeArray;
            int hashCode = this.m_comparer.GetHashCode(key);
        Label_000D:
            nodeArray = this.m_buckets;
            this.GetBucketAndLockNo(hashCode, out num2, out num3, nodeArray.Length);
            bool flag = false;
            bool lockTaken = false;
            try
            {
                if (acquireLock)
                {
                    Monitor.Enter(this.m_locks[num3], ref lockTaken);
                }
                if (nodeArray != this.m_buckets)
                {
                    goto Label_000D;
                }
                Node<TKey, TValue> node = null;
                for (Node<TKey, TValue> node2 = nodeArray[num2]; node2 != null; node2 = node2.m_next)
                {
                    if (this.m_comparer.Equals(node2.m_key, key))
                    {
                        if (updateIfExists)
                        {
                            Node<TKey, TValue> node3 = new Node<TKey, TValue>(node2.m_key, value, hashCode, node2.m_next);
                            if (node == null)
                            {
                                nodeArray[num2] = node3;
                            }
                            else
                            {
                                node.m_next = node3;
                            }
                            resultingValue = value;
                        }
                        else
                        {
                            resultingValue = node2.m_value;
                        }
                        return false;
                    }
                    node = node2;
                }
                nodeArray[num2] = new Node<TKey, TValue>(key, value, hashCode, nodeArray[num2]);
                this.m_countPerLock[num3] += 1;
                if (this.m_countPerLock[num3] > (nodeArray.Length / this.m_locks.Length))
                {
                    flag = true;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this.m_locks[num3]);
                }
            }
            if (flag)
            {
                this.GrowTable(nodeArray);
            }
            resultingValue = value;
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int num;
            int num2;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            Node<TKey, TValue>[] buckets = this.m_buckets;
            this.GetBucketAndLockNo(this.m_comparer.GetHashCode(key), out num, out num2, buckets.Length);
            Node<TKey, TValue> next = buckets[num];
            Thread.MemoryBarrier();
            while (next != null)
            {
                if (this.m_comparer.Equals(next.m_key, key))
                {
                    value = next.m_value;
                    return true;
                }
                next = next.m_next;
            }
            value = default(TValue);
            return false;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return this.TryRemoveInternal(key, out value, false, default(TValue));
        }

        private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue)
        {
            Node<TKey, TValue>[] nodeArray;
            int num;
            int num2;
        Label_0000:
            nodeArray = this.m_buckets;
            this.GetBucketAndLockNo(this.m_comparer.GetHashCode(key), out num, out num2, nodeArray.Length);
            lock (this.m_locks[num2])
            {
                if (nodeArray != this.m_buckets)
                {
                    goto Label_0000;
                }
                Node<TKey, TValue> node = null;
                for (Node<TKey, TValue> node2 = this.m_buckets[num]; node2 != null; node2 = node2.m_next)
                {
                    if (this.m_comparer.Equals(node2.m_key, key))
                    {
                        if (matchValue && !EqualityComparer<TValue>.Default.Equals(oldValue, node2.m_value))
                        {
                            value = default(TValue);
                            return false;
                        }
                        if (node == null)
                        {
                            this.m_buckets[num] = node2.m_next;
                        }
                        else
                        {
                            node.m_next = node2.m_next;
                        }
                        value = node2.m_value;
                        this.m_countPerLock[num2] -= 1;
                        return true;
                    }
                    node = node2;
                }
            }
            value = default(TValue);
            return false;
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            int num2;
            int num3;
            Node<TKey, TValue>[] nodeArray;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int hashCode = this.m_comparer.GetHashCode(key);
            IEqualityComparer<TValue> comparer = (IEqualityComparer<TValue>) EqualityComparer<TValue>.Default;
        Label_0026:
            nodeArray = this.m_buckets;
            this.GetBucketAndLockNo(hashCode, out num2, out num3, nodeArray.Length);
            lock (this.m_locks[num3])
            {
                if (nodeArray != this.m_buckets)
                {
                    goto Label_0026;
                }
                Node<TKey, TValue> node = null;
                for (Node<TKey, TValue> node2 = nodeArray[num2]; node2 != null; node2 = node2.m_next)
                {
                    if (this.m_comparer.Equals(node2.m_key, key))
                    {
                        if (!comparer.Equals(node2.m_value, comparisonValue))
                        {
                            return false;
                        }
                        Node<TKey, TValue> node3 = new Node<TKey, TValue>(node2.m_key, newValue, hashCode, node2.m_next);
                        if (node == null)
                        {
                            nodeArray[num2] = node3;
                        }
                        else
                        {
                            node.m_next = node3;
                        }
                        return true;
                    }
                    node = node2;
                }
                return false;
            }
        }

        public int Count
        {
            get
            {
                int num = 0;
                int locksAcquired = 0;
                try
                {
                    this.AcquireAllLocks(ref locksAcquired);
                    for (int i = 0; i < this.m_countPerLock.Length; i++)
                    {
                        num += this.m_countPerLock[i];
                    }
                }
                finally
                {
                    this.ReleaseLocks(0, locksAcquired);
                }
                return num;
            }
        }

        private static int DefaultConcurrencyLevel
        {
            get
            {
                return (4 * Environment.ProcessorCount);
            }
        }

        public bool IsEmpty
        {
            get
            {
                int locksAcquired = 0;
                try
                {
                    this.AcquireAllLocks(ref locksAcquired);
                    for (int i = 0; i < this.m_countPerLock.Length; i++)
                    {
                        if (this.m_countPerLock[i] != null)
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    this.ReleaseLocks(0, locksAcquired);
                }
                return true;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue local;
                if (!this.TryGetValue(key, out local))
                {
                    throw new KeyNotFoundException();
                }
                return local;
            }
            set
            {
                TValue local;
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                this.TryAddInternal(key, value, true, true, out local);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return this.GetKeys();
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
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
                throw new NotSupportedException(Environment.GetResourceString("ConcurrentCollection_SyncRoot_NotSupported"));
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
                return false;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                TValue local;
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if ((key is TKey) && this.TryGetValue((TKey) key, out local))
                {
                    return local;
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (!(key is TKey))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfKeyIncorrect"));
                }
                if (!(value is TValue))
                {
                    throw new ArgumentException(this.GetResource("ConcurrentDictionary_TypeOfValueIncorrect"));
                }
                this[(TKey) key] = (TValue) value;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return this.GetKeys();
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return this.GetValues();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return this.GetValues();
            }
        }

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__2 : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private KeyValuePair<TKey, TValue> <>2__current;
            public ConcurrentDictionary<TKey, TValue> <>4__this;
            public ConcurrentDictionary<TKey, TValue>.Node[] <buckets>5__3;
            public ConcurrentDictionary<TKey, TValue>.Node <current>5__5;
            public int <i>5__4;

            [DebuggerHidden]
            public <GetEnumerator>d__2(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<buckets>5__3 = this.<>4__this.m_buckets;
                        this.<i>5__4 = 0;
                        while (this.<i>5__4 < this.<buckets>5__3.Length)
                        {
                            this.<current>5__5 = this.<buckets>5__3[this.<i>5__4];
                            Thread.MemoryBarrier();
                            while (this.<current>5__5 != null)
                            {
                                this.<>2__current = new KeyValuePair<TKey, TValue>(this.<current>5__5.m_key, this.<current>5__5.m_value);
                                this.<>1__state = 1;
                                return true;
                            Label_0081:
                                this.<>1__state = -1;
                                this.<current>5__5 = this.<current>5__5.m_next;
                            }
                            this.<i>5__4++;
                        }
                        break;

                    case 1:
                        goto Label_0081;
                }
                return false;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }

        private class DictionaryEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private IEnumerator<KeyValuePair<TKey, TValue>> m_enumerator;

            internal DictionaryEnumerator(ConcurrentDictionary<TKey, TValue> dictionary)
            {
                this.m_enumerator = dictionary.GetEnumerator();
            }

            public bool MoveNext()
            {
                return this.m_enumerator.MoveNext();
            }

            public void Reset()
            {
                this.m_enumerator.Reset();
            }

            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    return new DictionaryEntry(this.m_enumerator.Current.Key, this.m_enumerator.Current.Value);
                }
            }

            public object Key
            {
                get
                {
                    return this.m_enumerator.Current.Key;
                }
            }

            public object Value
            {
                get
                {
                    return this.m_enumerator.Current.Value;
                }
            }
        }

        private class Node
        {
            internal int m_hashcode;
            internal TKey m_key;
            internal volatile ConcurrentDictionary<TKey, TValue>.Node m_next;
            internal TValue m_value;

            internal Node(TKey key, TValue value, int hashcode) : this(key, value, hashcode, null)
            {
            }

            internal Node(TKey key, TValue value, int hashcode, ConcurrentDictionary<TKey, TValue>.Node next)
            {
                this.m_key = key;
                this.m_value = value;
                this.m_next = next;
                this.m_hashcode = hashcode;
            }
        }
    }
}

