namespace System.Collections.Specialized
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable]
    public class OrderedDictionary : IOrderedDictionary, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
    {
        private IEqualityComparer _comparer;
        private int _initialCapacity;
        private ArrayList _objectsArray;
        private Hashtable _objectsTable;
        private bool _readOnly;
        private SerializationInfo _siInfo;
        private object _syncRoot;
        private const string ArrayListName = "ArrayList";
        private const string InitCapacityName = "InitialCapacity";
        private const string KeyComparerName = "KeyComparer";
        private const string ReadOnlyName = "ReadOnly";

        public OrderedDictionary() : this(0)
        {
        }

        public OrderedDictionary(IEqualityComparer comparer) : this(0, comparer)
        {
        }

        private OrderedDictionary(OrderedDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }
            this._readOnly = true;
            this._objectsArray = dictionary._objectsArray;
            this._objectsTable = dictionary._objectsTable;
            this._comparer = dictionary._comparer;
            this._initialCapacity = dictionary._initialCapacity;
        }

        public OrderedDictionary(int capacity) : this(capacity, null)
        {
        }

        public OrderedDictionary(int capacity, IEqualityComparer comparer)
        {
            this._initialCapacity = capacity;
            this._comparer = comparer;
        }

        protected OrderedDictionary(SerializationInfo info, StreamingContext context)
        {
            this._siInfo = info;
        }

        public void Add(object key, object value)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("OrderedDictionary_ReadOnly"));
            }
            this.objectsTable.Add(key, value);
            this.objectsArray.Add(new DictionaryEntry(key, value));
        }

        public OrderedDictionary AsReadOnly()
        {
            return new OrderedDictionary(this);
        }

        public void Clear()
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("OrderedDictionary_ReadOnly"));
            }
            this.objectsTable.Clear();
            this.objectsArray.Clear();
        }

        public bool Contains(object key)
        {
            return this.objectsTable.Contains(key);
        }

        public void CopyTo(Array array, int index)
        {
            this.objectsTable.CopyTo(array, index);
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return new OrderedDictionaryEnumerator(this.objectsArray, 3);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("KeyComparer", this._comparer, typeof(IEqualityComparer));
            info.AddValue("ReadOnly", this._readOnly);
            info.AddValue("InitialCapacity", this._initialCapacity);
            object[] array = new object[this.Count];
            this._objectsArray.CopyTo(array);
            info.AddValue("ArrayList", array);
        }

        private int IndexOfKey(object key)
        {
            for (int i = 0; i < this.objectsArray.Count; i++)
            {
                DictionaryEntry entry = (DictionaryEntry) this.objectsArray[i];
                object x = entry.Key;
                if (this._comparer != null)
                {
                    if (this._comparer.Equals(x, key))
                    {
                        return i;
                    }
                }
                else if (x.Equals(key))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, object key, object value)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("OrderedDictionary_ReadOnly"));
            }
            if ((index > this.Count) || (index < 0))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            this.objectsTable.Add(key, value);
            this.objectsArray.Insert(index, new DictionaryEntry(key, value));
        }

        protected virtual void OnDeserialization(object sender)
        {
            if (this._siInfo == null)
            {
                throw new SerializationException(SR.GetString("Serialization_InvalidOnDeser"));
            }
            this._comparer = (IEqualityComparer) this._siInfo.GetValue("KeyComparer", typeof(IEqualityComparer));
            this._readOnly = this._siInfo.GetBoolean("ReadOnly");
            this._initialCapacity = this._siInfo.GetInt32("InitialCapacity");
            object[] objArray = (object[]) this._siInfo.GetValue("ArrayList", typeof(object[]));
            if (objArray != null)
            {
                foreach (object obj2 in objArray)
                {
                    DictionaryEntry entry;
                    try
                    {
                        entry = (DictionaryEntry) obj2;
                    }
                    catch
                    {
                        throw new SerializationException(SR.GetString("OrderedDictionary_SerializationMismatch"));
                    }
                    this.objectsArray.Add(entry);
                    this.objectsTable.Add(entry.Key, entry.Value);
                }
            }
        }

        public void Remove(object key)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("OrderedDictionary_ReadOnly"));
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int index = this.IndexOfKey(key);
            if (index >= 0)
            {
                this.objectsTable.Remove(key);
                this.objectsArray.RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            if (this._readOnly)
            {
                throw new NotSupportedException(SR.GetString("OrderedDictionary_ReadOnly"));
            }
            if ((index >= this.Count) || (index < 0))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            DictionaryEntry entry = (DictionaryEntry) this.objectsArray[index];
            object key = entry.Key;
            this.objectsArray.RemoveAt(index);
            this.objectsTable.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new OrderedDictionaryEnumerator(this.objectsArray, 3);
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            this.OnDeserialization(sender);
        }

        public int Count
        {
            get
            {
                return this.objectsArray.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this._readOnly;
            }
        }

        public object this[int index]
        {
            get
            {
                DictionaryEntry entry = (DictionaryEntry) this.objectsArray[index];
                return entry.Value;
            }
            set
            {
                if (this._readOnly)
                {
                    throw new NotSupportedException(SR.GetString("OrderedDictionary_ReadOnly"));
                }
                if ((index < 0) || (index >= this.objectsArray.Count))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                DictionaryEntry entry = (DictionaryEntry) this.objectsArray[index];
                object key = entry.Key;
                this.objectsArray[index] = new DictionaryEntry(key, value);
                this.objectsTable[key] = value;
            }
        }

        public object this[object key]
        {
            get
            {
                return this.objectsTable[key];
            }
            set
            {
                if (this._readOnly)
                {
                    throw new NotSupportedException(SR.GetString("OrderedDictionary_ReadOnly"));
                }
                if (this.objectsTable.Contains(key))
                {
                    this.objectsTable[key] = value;
                    this.objectsArray[this.IndexOfKey(key)] = new DictionaryEntry(key, value);
                }
                else
                {
                    this.Add(key, value);
                }
            }
        }

        public ICollection Keys
        {
            get
            {
                return new OrderedDictionaryKeyValueCollection(this.objectsArray, true);
            }
        }

        private ArrayList objectsArray
        {
            get
            {
                if (this._objectsArray == null)
                {
                    this._objectsArray = new ArrayList(this._initialCapacity);
                }
                return this._objectsArray;
            }
        }

        private Hashtable objectsTable
        {
            get
            {
                if (this._objectsTable == null)
                {
                    this._objectsTable = new Hashtable(this._initialCapacity, this._comparer);
                }
                return this._objectsTable;
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
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return this._readOnly;
            }
        }

        public ICollection Values
        {
            get
            {
                return new OrderedDictionaryKeyValueCollection(this.objectsArray, false);
            }
        }

        private class OrderedDictionaryEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private int _objectReturnType;
            private IEnumerator arrayEnumerator;
            internal const int DictionaryEntry = 3;
            internal const int Keys = 1;
            internal const int Values = 2;

            internal OrderedDictionaryEnumerator(ArrayList array, int objectReturnType)
            {
                this.arrayEnumerator = array.GetEnumerator();
                this._objectReturnType = objectReturnType;
            }

            public bool MoveNext()
            {
                return this.arrayEnumerator.MoveNext();
            }

            public void Reset()
            {
                this.arrayEnumerator.Reset();
            }

            public object Current
            {
                get
                {
                    if (this._objectReturnType == 1)
                    {
                        System.Collections.DictionaryEntry current = (System.Collections.DictionaryEntry) this.arrayEnumerator.Current;
                        return current.Key;
                    }
                    if (this._objectReturnType == 2)
                    {
                        System.Collections.DictionaryEntry entry2 = (System.Collections.DictionaryEntry) this.arrayEnumerator.Current;
                        return entry2.Value;
                    }
                    return this.Entry;
                }
            }

            public System.Collections.DictionaryEntry Entry
            {
                get
                {
                    System.Collections.DictionaryEntry current = (System.Collections.DictionaryEntry) this.arrayEnumerator.Current;
                    System.Collections.DictionaryEntry entry2 = (System.Collections.DictionaryEntry) this.arrayEnumerator.Current;
                    return new System.Collections.DictionaryEntry(current.Key, entry2.Value);
                }
            }

            public object Key
            {
                get
                {
                    System.Collections.DictionaryEntry current = (System.Collections.DictionaryEntry) this.arrayEnumerator.Current;
                    return current.Key;
                }
            }

            public object Value
            {
                get
                {
                    System.Collections.DictionaryEntry current = (System.Collections.DictionaryEntry) this.arrayEnumerator.Current;
                    return current.Value;
                }
            }
        }

        private class OrderedDictionaryKeyValueCollection : ICollection, IEnumerable
        {
            private ArrayList _objects;
            private bool isKeys;

            public OrderedDictionaryKeyValueCollection(ArrayList array, bool isKeys)
            {
                this._objects = array;
                this.isKeys = isKeys;
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                foreach (object obj2 in this._objects)
                {
                    array.SetValue(this.isKeys ? ((DictionaryEntry) obj2).Key : ((DictionaryEntry) obj2).Value, index);
                    index++;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new OrderedDictionary.OrderedDictionaryEnumerator(this._objects, this.isKeys ? 1 : 2);
            }

            int ICollection.Count
            {
                get
                {
                    return this._objects.Count;
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
                    return this._objects.SyncRoot;
                }
            }
        }
    }
}

