namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, DebuggerDisplay("Count = {Count}"), ComVisible(false), DebuggerTypeProxy(typeof(Mscorlib_DictionaryDebugView<,>))]
    public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        private object _syncRoot;
        private int[] buckets;
        private IEqualityComparer<TKey> comparer;
        private const string ComparerName = "Comparer";
        private int count;
        private Entry<TKey, TValue>[] entries;
        private int freeCount;
        private int freeList;
        private const string HashSizeName = "HashSize";
        private KeyCollection<TKey, TValue> keys;
        private const string KeyValuePairsName = "KeyValuePairs";
        private SerializationInfo m_siInfo;
        private ValueCollection<TKey, TValue> values;
        private int version;
        private const string VersionName = "Version";

        public Dictionary() : this(0, null)
        {
        }

        public Dictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null)
        {
        }

        public Dictionary(IEqualityComparer<TKey> comparer) : this(0, comparer)
        {
        }

        public Dictionary(int capacity) : this(capacity, null)
        {
        }

        protected Dictionary(SerializationInfo info, StreamingContext context)
        {
            this.m_siInfo = info;
        }

        public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : this((dictionary != null) ? dictionary.Count : 0, comparer)
        {
            if (dictionary == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
            }
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                this.Add(pair.Key, pair.Value);
            }
        }

        public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);
            }
            if (capacity > 0)
            {
                this.Initialize(capacity);
            }
            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public void Add(TKey key, TValue value)
        {
            this.Insert(key, value, true);
        }

        public void Clear()
        {
            if (this.count > 0)
            {
                for (int i = 0; i < this.buckets.Length; i++)
                {
                    this.buckets[i] = -1;
                }
                Array.Clear(this.entries, 0, this.count);
                this.freeList = -1;
                this.count = 0;
                this.freeCount = 0;
                this.version++;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return (this.FindEntry(key) >= 0);
        }

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                for (int i = 0; i < this.count; i++)
                {
                    if ((this.entries[i].hashCode >= 0) && (this.entries[i].value == null))
                    {
                        return true;
                    }
                }
            }
            else
            {
                EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
                for (int j = 0; j < this.count; j++)
                {
                    if ((this.entries[j].hashCode >= 0) && comparer.Equals(this.entries[j].value, value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }
            if ((index < 0) || (index > array.Length))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((array.Length - index) < this.Count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }
            int count = this.count;
            Entry<TKey, TValue>[] entries = this.entries;
            for (int i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    array[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
                }
            }
        }

        private int FindEntry(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            if (this.buckets != null)
            {
                int num = this.comparer.GetHashCode(key) & 0x7fffffff;
                for (int i = this.buckets[num % this.buckets.Length]; i >= 0; i = this.entries[i].next)
                {
                    if ((this.entries[i].hashCode == num) && this.comparer.Equals(this.entries[i].key, key))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public Enumerator<TKey, TValue> GetEnumerator()
        {
            return new Enumerator<TKey, TValue>((Dictionary<TKey, TValue>) this, 2);
        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.info);
            }
            info.AddValue("Version", this.version);
            info.AddValue("Comparer", this.comparer, typeof(IEqualityComparer<TKey>));
            info.AddValue("HashSize", (this.buckets == null) ? 0 : this.buckets.Length);
            if (this.buckets != null)
            {
                KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[this.Count];
                this.CopyTo(array, 0);
                info.AddValue("KeyValuePairs", array, typeof(KeyValuePair<TKey, TValue>[]));
            }
        }

        internal TValue GetValueOrDefault(TKey key)
        {
            int index = this.FindEntry(key);
            if (index >= 0)
            {
                return this.entries[index].value;
            }
            return default(TValue);
        }

        private void Initialize(int capacity)
        {
            int prime = HashHelpers.GetPrime(capacity);
            this.buckets = new int[prime];
            for (int i = 0; i < this.buckets.Length; i++)
            {
                this.buckets[i] = -1;
            }
            this.entries = new Entry<TKey, TValue>[prime];
            this.freeList = -1;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            int freeList;
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            if (this.buckets == null)
            {
                this.Initialize(0);
            }
            int num = this.comparer.GetHashCode(key) & 0x7fffffff;
            int index = num % this.buckets.Length;
            for (int i = this.buckets[index]; i >= 0; i = this.entries[i].next)
            {
                if ((this.entries[i].hashCode == num) && this.comparer.Equals(this.entries[i].key, key))
                {
                    if (add)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
                    }
                    this.entries[i].value = value;
                    this.version++;
                    return;
                }
            }
            if (this.freeCount > 0)
            {
                freeList = this.freeList;
                this.freeList = this.entries[freeList].next;
                this.freeCount--;
            }
            else
            {
                if (this.count == this.entries.Length)
                {
                    this.Resize();
                    index = num % this.buckets.Length;
                }
                freeList = this.count;
                this.count++;
            }
            this.entries[freeList].hashCode = num;
            this.entries[freeList].next = this.buckets[index];
            this.entries[freeList].key = key;
            this.entries[freeList].value = value;
            this.buckets[index] = freeList;
            this.version++;
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            return (key is TKey);
        }

        public virtual void OnDeserialization(object sender)
        {
            if (this.m_siInfo != null)
            {
                int num = this.m_siInfo.GetInt32("Version");
                int num2 = this.m_siInfo.GetInt32("HashSize");
                this.comparer = (IEqualityComparer<TKey>) this.m_siInfo.GetValue("Comparer", typeof(IEqualityComparer<TKey>));
                if (num2 != 0)
                {
                    this.buckets = new int[num2];
                    for (int i = 0; i < this.buckets.Length; i++)
                    {
                        this.buckets[i] = -1;
                    }
                    this.entries = new Entry<TKey, TValue>[num2];
                    this.freeList = -1;
                    KeyValuePair<TKey, TValue>[] pairArray = (KeyValuePair<TKey, TValue>[]) this.m_siInfo.GetValue("KeyValuePairs", typeof(KeyValuePair<TKey, TValue>[]));
                    if (pairArray == null)
                    {
                        ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_MissingKeys);
                    }
                    for (int j = 0; j < pairArray.Length; j++)
                    {
                        if (pairArray[j].Key == null)
                        {
                            ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_NullKey);
                        }
                        this.Insert(pairArray[j].Key, pairArray[j].Value, true);
                    }
                }
                else
                {
                    this.buckets = null;
                }
                this.version = num;
                this.m_siInfo = null;
            }
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            if (this.buckets != null)
            {
                int num = this.comparer.GetHashCode(key) & 0x7fffffff;
                int index = num % this.buckets.Length;
                int num3 = -1;
                for (int i = this.buckets[index]; i >= 0; i = this.entries[i].next)
                {
                    if ((this.entries[i].hashCode == num) && this.comparer.Equals(this.entries[i].key, key))
                    {
                        if (num3 < 0)
                        {
                            this.buckets[index] = this.entries[i].next;
                        }
                        else
                        {
                            this.entries[num3].next = this.entries[i].next;
                        }
                        this.entries[i].hashCode = -1;
                        this.entries[i].next = this.freeList;
                        this.entries[i].key = default(TKey);
                        this.entries[i].value = default(TValue);
                        this.freeList = i;
                        this.freeCount++;
                        this.version++;
                        return true;
                    }
                    num3 = i;
                }
            }
            return false;
        }

        private void Resize()
        {
            int prime = HashHelpers.GetPrime(this.count * 2);
            int[] numArray = new int[prime];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = -1;
            }
            Entry<TKey, TValue>[] destinationArray = new Entry<TKey, TValue>[prime];
            Array.Copy(this.entries, 0, destinationArray, 0, this.count);
            for (int j = 0; j < this.count; j++)
            {
                int index = destinationArray[j].hashCode % prime;
                destinationArray[j].next = numArray[index];
                numArray[index] = j;
            }
            this.buckets = numArray;
            this.entries = destinationArray;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            this.Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = this.FindEntry(keyValuePair.Key);
            return ((index >= 0) && EqualityComparer<TValue>.Default.Equals(this.entries[index].value, keyValuePair.Value));
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            this.CopyTo(array, index);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = this.FindEntry(keyValuePair.Key);
            if ((index >= 0) && EqualityComparer<TValue>.Default.Equals(this.entries[index].value, keyValuePair.Value))
            {
                this.Remove(keyValuePair.Key);
                return true;
            }
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator<TKey, TValue>((Dictionary<TKey, TValue>) this, 2);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }
            if (array.Rank != 1)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            }
            if (array.GetLowerBound(0) != 0)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
            }
            if ((index < 0) || (index > array.Length))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((array.Length - index) < this.Count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }
            KeyValuePair<TKey, TValue>[] pairArray = array as KeyValuePair<TKey, TValue>[];
            if (pairArray != null)
            {
                this.CopyTo(pairArray, index);
            }
            else if (array is DictionaryEntry[])
            {
                DictionaryEntry[] entryArray = array as DictionaryEntry[];
                Entry<TKey, TValue>[] entries = this.entries;
                for (int i = 0; i < this.count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        entryArray[index++] = new DictionaryEntry(entries[i].key, entries[i].value);
                    }
                }
            }
            else
            {
                object[] objArray = array as object[];
                if (objArray == null)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                }
                try
                {
                    int count = this.count;
                    Entry<TKey, TValue>[] entryArray3 = this.entries;
                    for (int j = 0; j < count; j++)
                    {
                        if (entryArray3[j].hashCode >= 0)
                        {
                            objArray[index++] = new KeyValuePair<TKey, TValue>(entryArray3[j].key, entryArray3[j].value);
                        }
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                }
            }
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);
            try
            {
                TKey local = (TKey) key;
                try
                {
                    this.Add(local, (TValue) value);
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                }
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
            }
        }

        bool IDictionary.Contains(object key)
        {
            return (Dictionary<TKey, TValue>.IsCompatibleKey(key) && this.ContainsKey((TKey) key));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator<TKey, TValue>((Dictionary<TKey, TValue>) this, 1);
        }

        void IDictionary.Remove(object key)
        {
            if (Dictionary<TKey, TValue>.IsCompatibleKey(key))
            {
                this.Remove((TKey) key);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<TKey, TValue>((Dictionary<TKey, TValue>) this, 2);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = this.FindEntry(key);
            if (index >= 0)
            {
                value = this.entries[index].value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return this.comparer;
            }
        }

        public int Count
        {
            get
            {
                return (this.count - this.freeCount);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = this.FindEntry(key);
                if (index >= 0)
                {
                    return this.entries[index].value;
                }
                ThrowHelper.ThrowKeyNotFoundException();
                return default(TValue);
            }
            set
            {
                this.Insert(key, value, false);
            }
        }

        public KeyCollection<TKey, TValue> Keys
        {
            get
            {
                if (this.keys == null)
                {
                    this.keys = new KeyCollection<TKey, TValue>((Dictionary<TKey, TValue>) this);
                }
                return this.keys;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (this.keys == null)
                {
                    this.keys = new KeyCollection<TKey, TValue>((Dictionary<TKey, TValue>) this);
                }
                return this.keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                if (this.values == null)
                {
                    this.values = new ValueCollection<TKey, TValue>((Dictionary<TKey, TValue>) this);
                }
                return this.values;
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
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
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
                if (Dictionary<TKey, TValue>.IsCompatibleKey(key))
                {
                    int index = this.FindEntry((TKey) key);
                    if (index >= 0)
                    {
                        return this.entries[index].value;
                    }
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                }
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);
                try
                {
                    TKey local = (TKey) key;
                    try
                    {
                        this[local] = (TValue) value;
                    }
                    catch (InvalidCastException)
                    {
                        ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                    }
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
                }
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return this.Keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return this.Values;
            }
        }

        public ValueCollection<TKey, TValue> Values
        {
            get
            {
                if (this.values == null)
                {
                    this.values = new ValueCollection<TKey, TValue>((Dictionary<TKey, TValue>) this);
                }
                return this.values;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            public int hashCode;
            public int next;
            public TKey key;
            public TValue value;
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
        {
            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;
            private Dictionary<TKey, TValue> dictionary;
            private int version;
            private int index;
            private KeyValuePair<TKey, TValue> current;
            private int getEnumeratorRetType;
            internal Enumerator(Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                this.version = dictionary.version;
                this.index = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                this.current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext()
            {
                if (this.version != this.dictionary.version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                while (this.index < this.dictionary.count)
                {
                    if (this.dictionary.entries[this.index].hashCode >= 0)
                    {
                        this.current = new KeyValuePair<TKey, TValue>(this.dictionary.entries[this.index].key, this.dictionary.entries[this.index].value);
                        this.index++;
                        return true;
                    }
                    this.index++;
                }
                this.index = this.dictionary.count + 1;
                this.current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    return this.current;
                }
            }
            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((this.index == 0) || (this.index == (this.dictionary.count + 1)))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    if (this.getEnumeratorRetType == 1)
                    {
                        return new DictionaryEntry(this.current.Key, this.current.Value);
                    }
                    return new KeyValuePair<TKey, TValue>(this.current.Key, this.current.Value);
                }
            }
            void IEnumerator.Reset()
            {
                if (this.version != this.dictionary.version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                this.index = 0;
                this.current = new KeyValuePair<TKey, TValue>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if ((this.index == 0) || (this.index == (this.dictionary.count + 1)))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return new DictionaryEntry(this.current.Key, this.current.Value);
                }
            }
            object IDictionaryEnumerator.Key
            {
                get
                {
                    if ((this.index == 0) || (this.index == (this.dictionary.count + 1)))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return this.current.Key;
                }
            }
            object IDictionaryEnumerator.Value
            {
                get
                {
                    if ((this.index == 0) || (this.index == (this.dictionary.count + 1)))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return this.current.Value;
                }
            }
        }

        [Serializable, DebuggerTypeProxy(typeof(Mscorlib_DictionaryKeyCollectionDebugView<,>)), DebuggerDisplay("Count = {Count}")]
        public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable
        {
            private Dictionary<TKey, TValue> dictionary;

            public KeyCollection(Dictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }
                if ((index < 0) || (index > array.Length))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }
                if ((array.Length - index) < this.dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                int count = this.dictionary.count;
                Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        array[index++] = entries[i].key;
                    }
                }
            }

            public Enumerator<TKey, TValue> GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            void ICollection<TKey>.Add(TKey item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            void ICollection<TKey>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            bool ICollection<TKey>.Contains(TKey item)
            {
                return this.dictionary.ContainsKey(item);
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
                return false;
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }
                if (array.Rank != 1)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                }
                if (array.GetLowerBound(0) != 0)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                }
                if ((index < 0) || (index > array.Length))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }
                if ((array.Length - index) < this.dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                TKey[] localArray = array as TKey[];
                if (localArray != null)
                {
                    this.CopyTo(localArray, index);
                }
                else
                {
                    object[] objArray = array as object[];
                    if (objArray == null)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                    int count = this.dictionary.count;
                    Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries[i].hashCode >= 0)
                            {
                                objArray[index++] = entries[i].key;
                            }
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            public int Count
            {
                get
                {
                    return this.dictionary.Count;
                }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get
                {
                    return true;
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
                    return ((ICollection) this.dictionary).SyncRoot;
                }
            }

            [Serializable, StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
            {
                private Dictionary<TKey, TValue> dictionary;
                private int index;
                private int version;
                private TKey currentKey;
                internal Enumerator(Dictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    this.version = dictionary.version;
                    this.index = 0;
                    this.currentKey = default(TKey);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (this.version != this.dictionary.version)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    }
                    while (this.index < this.dictionary.count)
                    {
                        if (this.dictionary.entries[this.index].hashCode >= 0)
                        {
                            this.currentKey = this.dictionary.entries[this.index].key;
                            this.index++;
                            return true;
                        }
                        this.index++;
                    }
                    this.index = this.dictionary.count + 1;
                    this.currentKey = default(TKey);
                    return false;
                }

                public TKey Current
                {
                    get
                    {
                        return this.currentKey;
                    }
                }
                object IEnumerator.Current
                {
                    get
                    {
                        if ((this.index == 0) || (this.index == (this.dictionary.count + 1)))
                        {
                            ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        }
                        return this.currentKey;
                    }
                }
                void IEnumerator.Reset()
                {
                    if (this.version != this.dictionary.version)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    }
                    this.index = 0;
                    this.currentKey = default(TKey);
                }
            }
        }

        [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(Mscorlib_DictionaryValueCollectionDebugView<,>))]
        public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable
        {
            private Dictionary<TKey, TValue> dictionary;

            public ValueCollection(Dictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }
                if ((index < 0) || (index > array.Length))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }
                if ((array.Length - index) < this.dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                int count = this.dictionary.count;
                Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        array[index++] = entries[i].value;
                    }
                }
            }

            public Enumerator<TKey, TValue> GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            void ICollection<TValue>.Add(TValue item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
            }

            void ICollection<TValue>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                return this.dictionary.ContainsValue(item);
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
                return false;
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }
                if (array.Rank != 1)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                }
                if (array.GetLowerBound(0) != 0)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                }
                if ((index < 0) || (index > array.Length))
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }
                if ((array.Length - index) < this.dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                TValue[] localArray = array as TValue[];
                if (localArray != null)
                {
                    this.CopyTo(localArray, index);
                }
                else
                {
                    object[] objArray = array as object[];
                    if (objArray == null)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                    int count = this.dictionary.count;
                    Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
                    try
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (entries[i].hashCode >= 0)
                            {
                                objArray[index++] = entries[i].value;
                            }
                        }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator<TKey, TValue>(this.dictionary);
            }

            public int Count
            {
                get
                {
                    return this.dictionary.Count;
                }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get
                {
                    return true;
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
                    return ((ICollection) this.dictionary).SyncRoot;
                }
            }

            [Serializable, StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
            {
                private Dictionary<TKey, TValue> dictionary;
                private int index;
                private int version;
                private TValue currentValue;
                internal Enumerator(Dictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    this.version = dictionary.version;
                    this.index = 0;
                    this.currentValue = default(TValue);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (this.version != this.dictionary.version)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    }
                    while (this.index < this.dictionary.count)
                    {
                        if (this.dictionary.entries[this.index].hashCode >= 0)
                        {
                            this.currentValue = this.dictionary.entries[this.index].value;
                            this.index++;
                            return true;
                        }
                        this.index++;
                    }
                    this.index = this.dictionary.count + 1;
                    this.currentValue = default(TValue);
                    return false;
                }

                public TValue Current
                {
                    get
                    {
                        return this.currentValue;
                    }
                }
                object IEnumerator.Current
                {
                    get
                    {
                        if ((this.index == 0) || (this.index == (this.dictionary.count + 1)))
                        {
                            ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        }
                        return this.currentValue;
                    }
                }
                void IEnumerator.Reset()
                {
                    if (this.version != this.dictionary.version)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    }
                    this.index = 0;
                    this.currentValue = default(TValue);
                }
            }
        }
    }
}

