namespace System.Collections
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, DebuggerTypeProxy(typeof(SortedList.SortedListDebugView)), DebuggerDisplay("Count = {Count}"), ComVisible(true)]
    public class SortedList : IDictionary, ICollection, IEnumerable, ICloneable
    {
        private const int _defaultCapacity = 0x10;
        private int _size;
        [NonSerialized]
        private object _syncRoot;
        private IComparer comparer;
        private static object[] emptyArray = new object[0];
        private KeyList keyList;
        private object[] keys;
        private ValueList valueList;
        private object[] values;
        private int version;

        public SortedList()
        {
            this.Init();
        }

        public SortedList(IComparer comparer) : this()
        {
            if (comparer != null)
            {
                this.comparer = comparer;
            }
        }

        public SortedList(IDictionary d) : this(d, null)
        {
        }

        public SortedList(int initialCapacity)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException("initialCapacity", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.keys = new object[initialCapacity];
            this.values = new object[initialCapacity];
            this.comparer = new Comparer(CultureInfo.CurrentCulture);
        }

        public SortedList(IComparer comparer, int capacity) : this(comparer)
        {
            this.Capacity = capacity;
        }

        public SortedList(IDictionary d, IComparer comparer) : this(comparer, (d != null) ? d.Count : 0)
        {
            if (d == null)
            {
                throw new ArgumentNullException("d", Environment.GetResourceString("ArgumentNull_Dictionary"));
            }
            d.Keys.CopyTo(this.keys, 0);
            d.Values.CopyTo(this.values, 0);
            Array.Sort(this.keys, this.values, comparer);
            this._size = d.Count;
        }

        public virtual void Add(object key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
            }
            int index = Array.BinarySearch(this.keys, 0, this._size, key, this.comparer);
            if (index >= 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_AddingDuplicate__", new object[] { this.GetKey(index), key }));
            }
            this.Insert(~index, key, value);
        }

        public virtual void Clear()
        {
            this.version++;
            Array.Clear(this.keys, 0, this._size);
            Array.Clear(this.values, 0, this._size);
            this._size = 0;
        }

        public virtual object Clone()
        {
            SortedList list = new SortedList(this._size);
            Array.Copy(this.keys, 0, list.keys, 0, this._size);
            Array.Copy(this.values, 0, list.values, 0, this._size);
            list._size = this._size;
            list.version = this.version;
            list.comparer = this.comparer;
            return list;
        }

        public virtual bool Contains(object key)
        {
            return (this.IndexOfKey(key) >= 0);
        }

        public virtual bool ContainsKey(object key)
        {
            return (this.IndexOfKey(key) >= 0);
        }

        public virtual bool ContainsValue(object value)
        {
            return (this.IndexOfValue(value) >= 0);
        }

        public virtual void CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException("arrayIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - arrayIndex) < this.Count)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ArrayPlusOffTooSmall"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                DictionaryEntry entry = new DictionaryEntry(this.keys[i], this.values[i]);
                array.SetValue(entry, (int) (i + arrayIndex));
            }
        }

        private void EnsureCapacity(int min)
        {
            int num = (this.keys.Length == 0) ? 0x10 : (this.keys.Length * 2);
            if (num < min)
            {
                num = min;
            }
            this.Capacity = num;
        }

        public virtual object GetByIndex(int index)
        {
            if ((index < 0) || (index >= this.Count))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            return this.values[index];
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return new SortedListEnumerator(this, 0, this._size, 3);
        }

        public virtual object GetKey(int index)
        {
            if ((index < 0) || (index >= this.Count))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            return this.keys[index];
        }

        public virtual IList GetKeyList()
        {
            if (this.keyList == null)
            {
                this.keyList = new KeyList(this);
            }
            return this.keyList;
        }

        public virtual IList GetValueList()
        {
            if (this.valueList == null)
            {
                this.valueList = new ValueList(this);
            }
            return this.valueList;
        }

        public virtual int IndexOfKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
            }
            int num = Array.BinarySearch(this.keys, 0, this._size, key, this.comparer);
            if (num < 0)
            {
                return -1;
            }
            return num;
        }

        public virtual int IndexOfValue(object value)
        {
            return Array.IndexOf<object>(this.values, value, 0, this._size);
        }

        private void Init()
        {
            this.keys = emptyArray;
            this.values = emptyArray;
            this._size = 0;
            this.comparer = new Comparer(CultureInfo.CurrentCulture);
        }

        private void Insert(int index, object key, object value)
        {
            if (this._size == this.keys.Length)
            {
                this.EnsureCapacity(this._size + 1);
            }
            if (index < this._size)
            {
                Array.Copy(this.keys, index, this.keys, index + 1, this._size - index);
                Array.Copy(this.values, index, this.values, index + 1, this._size - index);
            }
            this.keys[index] = key;
            this.values[index] = value;
            this._size++;
            this.version++;
        }

        public virtual void Remove(object key)
        {
            int index = this.IndexOfKey(key);
            if (index >= 0)
            {
                this.RemoveAt(index);
            }
        }

        public virtual void RemoveAt(int index)
        {
            if ((index < 0) || (index >= this.Count))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            this._size--;
            if (index < this._size)
            {
                Array.Copy(this.keys, index + 1, this.keys, index, this._size - index);
                Array.Copy(this.values, index + 1, this.values, index, this._size - index);
            }
            this.keys[this._size] = null;
            this.values[this._size] = null;
            this.version++;
        }

        public virtual void SetByIndex(int index, object value)
        {
            if ((index < 0) || (index >= this.Count))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            this.values[index] = value;
            this.version++;
        }

        [HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
        public static SortedList Synchronized(SortedList list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            return new SyncSortedList(list);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SortedListEnumerator(this, 0, this._size, 3);
        }

        internal virtual KeyValuePairs[] ToKeyValuePairsArray()
        {
            KeyValuePairs[] pairsArray = new KeyValuePairs[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                pairsArray[i] = new KeyValuePairs(this.keys[i], this.values[i]);
            }
            return pairsArray;
        }

        public virtual void TrimToSize()
        {
            this.Capacity = this._size;
        }

        public virtual int Capacity
        {
            get
            {
                return this.keys.Length;
            }
            set
            {
                if (value < this.Count)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
                }
                if (value != this.keys.Length)
                {
                    if (value > 0)
                    {
                        object[] destinationArray = new object[value];
                        object[] objArray2 = new object[value];
                        if (this._size > 0)
                        {
                            Array.Copy(this.keys, 0, destinationArray, 0, this._size);
                            Array.Copy(this.values, 0, objArray2, 0, this._size);
                        }
                        this.keys = destinationArray;
                        this.values = objArray2;
                    }
                    else
                    {
                        this.keys = emptyArray;
                        this.values = emptyArray;
                    }
                }
            }
        }

        public virtual int Count
        {
            get
            {
                return this._size;
            }
        }

        public virtual bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual object this[object key]
        {
            get
            {
                int index = this.IndexOfKey(key);
                if (index >= 0)
                {
                    return this.values[index];
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
                }
                int index = Array.BinarySearch(this.keys, 0, this._size, key, this.comparer);
                if (index >= 0)
                {
                    this.values[index] = value;
                    this.version++;
                }
                else
                {
                    this.Insert(~index, key, value);
                }
            }
        }

        public virtual ICollection Keys
        {
            get
            {
                return this.GetKeyList();
            }
        }

        public virtual object SyncRoot
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

        public virtual ICollection Values
        {
            get
            {
                return this.GetValueList();
            }
        }

        [Serializable]
        private class KeyList : IList, ICollection, IEnumerable
        {
            private SortedList sortedList;

            internal KeyList(SortedList sortedList)
            {
                this.sortedList = sortedList;
            }

            public virtual int Add(object key)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual void Clear()
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual bool Contains(object key)
            {
                return this.sortedList.Contains(key);
            }

            public virtual void CopyTo(Array array, int arrayIndex)
            {
                if ((array != null) && (array.Rank != 1))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
                }
                Array.Copy(this.sortedList.keys, 0, array, arrayIndex, this.sortedList.Count);
            }

            public virtual IEnumerator GetEnumerator()
            {
                return new SortedList.SortedListEnumerator(this.sortedList, 0, this.sortedList.Count, 1);
            }

            public virtual int IndexOf(object key)
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
                }
                int num = Array.BinarySearch(this.sortedList.keys, 0, this.sortedList.Count, key, this.sortedList.comparer);
                if (num >= 0)
                {
                    return num;
                }
                return -1;
            }

            public virtual void Insert(int index, object value)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual void Remove(object key)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual void RemoveAt(int index)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual int Count
            {
                get
                {
                    return this.sortedList._size;
                }
            }

            public virtual bool IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            public virtual bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public virtual bool IsSynchronized
            {
                get
                {
                    return this.sortedList.IsSynchronized;
                }
            }

            public virtual object this[int index]
            {
                get
                {
                    return this.sortedList.GetKey(index);
                }
                set
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_KeyCollectionSet"));
                }
            }

            public virtual object SyncRoot
            {
                get
                {
                    return this.sortedList.SyncRoot;
                }
            }
        }

        internal class SortedListDebugView
        {
            private SortedList sortedList;

            public SortedListDebugView(SortedList sortedList)
            {
                if (sortedList == null)
                {
                    throw new ArgumentNullException("sortedList");
                }
                this.sortedList = sortedList;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePairs[] Items
            {
                get
                {
                    return this.sortedList.ToKeyValuePairsArray();
                }
            }
        }

        [Serializable]
        private class SortedListEnumerator : IDictionaryEnumerator, IEnumerator, ICloneable
        {
            private bool current;
            internal const int DictEntry = 3;
            private int endIndex;
            private int getObjectRetType;
            private int index;
            private object key;
            internal const int Keys = 1;
            private SortedList sortedList;
            private int startIndex;
            private object value;
            internal const int Values = 2;
            private int version;

            internal SortedListEnumerator(SortedList sortedList, int index, int count, int getObjRetType)
            {
                this.sortedList = sortedList;
                this.index = index;
                this.startIndex = index;
                this.endIndex = index + count;
                this.version = sortedList.version;
                this.getObjectRetType = getObjRetType;
                this.current = false;
            }

            public object Clone()
            {
                return base.MemberwiseClone();
            }

            public virtual bool MoveNext()
            {
                if (this.version != this.sortedList.version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                if (this.index < this.endIndex)
                {
                    this.key = this.sortedList.keys[this.index];
                    this.value = this.sortedList.values[this.index];
                    this.index++;
                    this.current = true;
                    return true;
                }
                this.key = null;
                this.value = null;
                this.current = false;
                return false;
            }

            public virtual void Reset()
            {
                if (this.version != this.sortedList.version)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                }
                this.index = this.startIndex;
                this.current = false;
                this.key = null;
                this.value = null;
            }

            public virtual object Current
            {
                get
                {
                    if (!this.current)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    if (this.getObjectRetType == 1)
                    {
                        return this.key;
                    }
                    if (this.getObjectRetType == 2)
                    {
                        return this.value;
                    }
                    return new DictionaryEntry(this.key, this.value);
                }
            }

            public virtual DictionaryEntry Entry
            {
                get
                {
                    if (this.version != this.sortedList.version)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                    }
                    if (!this.current)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return new DictionaryEntry(this.key, this.value);
                }
            }

            public virtual object Key
            {
                get
                {
                    if (this.version != this.sortedList.version)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                    }
                    if (!this.current)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return this.key;
                }
            }

            public virtual object Value
            {
                get
                {
                    if (this.version != this.sortedList.version)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumFailedVersion"));
                    }
                    if (!this.current)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumOpCantHappen"));
                    }
                    return this.value;
                }
            }
        }

        [Serializable]
        private class SyncSortedList : SortedList
        {
            private SortedList _list;
            private object _root;

            internal SyncSortedList(SortedList list)
            {
                this._list = list;
                this._root = list.SyncRoot;
            }

            public override void Add(object key, object value)
            {
                lock (this._root)
                {
                    this._list.Add(key, value);
                }
            }

            public override void Clear()
            {
                lock (this._root)
                {
                    this._list.Clear();
                }
            }

            public override object Clone()
            {
                lock (this._root)
                {
                    return this._list.Clone();
                }
            }

            public override bool Contains(object key)
            {
                lock (this._root)
                {
                    return this._list.Contains(key);
                }
            }

            public override bool ContainsKey(object key)
            {
                lock (this._root)
                {
                    return this._list.ContainsKey(key);
                }
            }

            public override bool ContainsValue(object key)
            {
                lock (this._root)
                {
                    return this._list.ContainsValue(key);
                }
            }

            public override void CopyTo(Array array, int index)
            {
                lock (this._root)
                {
                    this._list.CopyTo(array, index);
                }
            }

            public override object GetByIndex(int index)
            {
                lock (this._root)
                {
                    return this._list.GetByIndex(index);
                }
            }

            public override IDictionaryEnumerator GetEnumerator()
            {
                lock (this._root)
                {
                    return this._list.GetEnumerator();
                }
            }

            public override object GetKey(int index)
            {
                lock (this._root)
                {
                    return this._list.GetKey(index);
                }
            }

            public override IList GetKeyList()
            {
                lock (this._root)
                {
                    return this._list.GetKeyList();
                }
            }

            public override IList GetValueList()
            {
                lock (this._root)
                {
                    return this._list.GetValueList();
                }
            }

            public override int IndexOfKey(object key)
            {
                lock (this._root)
                {
                    return this._list.IndexOfKey(key);
                }
            }

            public override int IndexOfValue(object value)
            {
                lock (this._root)
                {
                    return this._list.IndexOfValue(value);
                }
            }

            public override void Remove(object key)
            {
                lock (this._root)
                {
                    this._list.Remove(key);
                }
            }

            public override void RemoveAt(int index)
            {
                lock (this._root)
                {
                    this._list.RemoveAt(index);
                }
            }

            public override void SetByIndex(int index, object value)
            {
                lock (this._root)
                {
                    this._list.SetByIndex(index, value);
                }
            }

            internal override KeyValuePairs[] ToKeyValuePairsArray()
            {
                return this._list.ToKeyValuePairsArray();
            }

            public override void TrimToSize()
            {
                lock (this._root)
                {
                    this._list.TrimToSize();
                }
            }

            public override int Capacity
            {
                get
                {
                    lock (this._root)
                    {
                        return this._list.Capacity;
                    }
                }
            }

            public override int Count
            {
                get
                {
                    lock (this._root)
                    {
                        return this._list.Count;
                    }
                }
            }

            public override bool IsFixedSize
            {
                get
                {
                    return this._list.IsFixedSize;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this._list.IsReadOnly;
                }
            }

            public override bool IsSynchronized
            {
                get
                {
                    return true;
                }
            }

            public override object this[object key]
            {
                get
                {
                    lock (this._root)
                    {
                        return this._list[key];
                    }
                }
                set
                {
                    lock (this._root)
                    {
                        this._list[key] = value;
                    }
                }
            }

            public override object SyncRoot
            {
                get
                {
                    return this._root;
                }
            }
        }

        [Serializable]
        private class ValueList : IList, ICollection, IEnumerable
        {
            private SortedList sortedList;

            internal ValueList(SortedList sortedList)
            {
                this.sortedList = sortedList;
            }

            public virtual int Add(object key)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual void Clear()
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual bool Contains(object value)
            {
                return this.sortedList.ContainsValue(value);
            }

            public virtual void CopyTo(Array array, int arrayIndex)
            {
                if ((array != null) && (array.Rank != 1))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_RankMultiDimNotSupported"));
                }
                Array.Copy(this.sortedList.values, 0, array, arrayIndex, this.sortedList.Count);
            }

            public virtual IEnumerator GetEnumerator()
            {
                return new SortedList.SortedListEnumerator(this.sortedList, 0, this.sortedList.Count, 2);
            }

            public virtual int IndexOf(object value)
            {
                return Array.IndexOf<object>(this.sortedList.values, value, 0, this.sortedList.Count);
            }

            public virtual void Insert(int index, object value)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual void Remove(object value)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual void RemoveAt(int index)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
            }

            public virtual int Count
            {
                get
                {
                    return this.sortedList._size;
                }
            }

            public virtual bool IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            public virtual bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public virtual bool IsSynchronized
            {
                get
                {
                    return this.sortedList.IsSynchronized;
                }
            }

            public virtual object this[int index]
            {
                get
                {
                    return this.sortedList.GetByIndex(index);
                }
                set
                {
                    throw new NotSupportedException(Environment.GetResourceString("NotSupported_SortedListNestedWrite"));
                }
            }

            public virtual object SyncRoot
            {
                get
                {
                    return this.sortedList.SyncRoot;
                }
            }
        }
    }
}

