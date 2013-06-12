namespace System.Runtime.CompilerServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;

    [Serializable]
    public sealed class ReadOnlyCollectionBuilder<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private static readonly T[] _emptyArray;
        private T[] _items;
        private int _size;
        [NonSerialized]
        private object _syncRoot;
        private int _version;
        private const int DefaultCapacity = 4;

        static ReadOnlyCollectionBuilder()
        {
            ReadOnlyCollectionBuilder<T>._emptyArray = new T[0];
        }

        public ReadOnlyCollectionBuilder()
        {
            this._items = ReadOnlyCollectionBuilder<T>._emptyArray;
        }

        public ReadOnlyCollectionBuilder(IEnumerable<T> collection)
        {
            ContractUtils.Requires(collection != null, "collection");
            ICollection<T> is2 = collection as ICollection<T>;
            if (is2 != null)
            {
                int count = is2.Count;
                this._items = new T[count];
                is2.CopyTo(this._items, 0);
                this._size = count;
            }
            else
            {
                this._size = 0;
                this._items = new T[4];
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        this.Add(enumerator.Current);
                    }
                }
            }
        }

        public ReadOnlyCollectionBuilder(int capacity)
        {
            ContractUtils.Requires(capacity >= 0, "capacity");
            this._items = new T[capacity];
        }

        public void Add(T item)
        {
            if (this._size == this._items.Length)
            {
                this.EnsureCapacity(this._size + 1);
            }
            this._items[this._size++] = item;
            this._version++;
        }

        public void Clear()
        {
            if (this._size > 0)
            {
                Array.Clear(this._items, 0, this._size);
                this._size = 0;
            }
            this._version++;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int j = 0; j < this._size; j++)
                {
                    if (this._items[j] == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < this._size; i++)
            {
                if (comparer.Equals(this._items[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this._items, 0, array, arrayIndex, this._size);
        }

        private void EnsureCapacity(int min)
        {
            if (this._items.Length < min)
            {
                int num = 4;
                if (this._items.Length > 0)
                {
                    num = this._items.Length * 2;
                }
                if (num < min)
                {
                    num = min;
                }
                this.Capacity = num;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>((ReadOnlyCollectionBuilder<T>) this);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf<T>(this._items, item, 0, this._size);
        }

        public void Insert(int index, T item)
        {
            ContractUtils.Requires(index <= this._size, "index");
            if (this._size == this._items.Length)
            {
                this.EnsureCapacity(this._size + 1);
            }
            if (index < this._size)
            {
                Array.Copy(this._items, index, this._items, index + 1, this._size - index);
            }
            this._items[index] = item;
            this._size++;
            this._version++;
        }

        private static bool IsCompatibleObject(object value)
        {
            return ((value is T) || ((value == null) && (default(T) == null)));
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            ContractUtils.Requires((index >= 0) && (index < this._size), "index");
            this._size--;
            if (index < this._size)
            {
                Array.Copy(this._items, index + 1, this._items, index, this._size - index);
            }
            this._items[this._size] = default(T);
            this._version++;
        }

        public void Reverse()
        {
            this.Reverse(0, this.Count);
        }

        public void Reverse(int index, int count)
        {
            ContractUtils.Requires(index >= 0, "index");
            ContractUtils.Requires(count >= 0, "count");
            Array.Reverse(this._items, index, count);
            this._version++;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.Requires(array.Rank == 1, "array");
            Array.Copy(this._items, 0, array, index, this._size);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object value)
        {
            ReadOnlyCollectionBuilder<T>.ValidateNullValue(value, "value");
            try
            {
                this.Add((T) value);
            }
            catch (InvalidCastException)
            {
                ReadOnlyCollectionBuilder<T>.ThrowInvalidTypeException(value, "value");
            }
            return (this.Count - 1);
        }

        bool IList.Contains(object value)
        {
            return (ReadOnlyCollectionBuilder<T>.IsCompatibleObject(value) && this.Contains((T) value));
        }

        int IList.IndexOf(object value)
        {
            if (ReadOnlyCollectionBuilder<T>.IsCompatibleObject(value))
            {
                return this.IndexOf((T) value);
            }
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            ReadOnlyCollectionBuilder<T>.ValidateNullValue(value, "value");
            try
            {
                this.Insert(index, (T) value);
            }
            catch (InvalidCastException)
            {
                ReadOnlyCollectionBuilder<T>.ThrowInvalidTypeException(value, "value");
            }
        }

        void IList.Remove(object value)
        {
            if (ReadOnlyCollectionBuilder<T>.IsCompatibleObject(value))
            {
                this.Remove((T) value);
            }
        }

        private static void ThrowInvalidTypeException(object value, string argument)
        {
            throw new ArgumentException(Strings.InvalidObjectType((value != null) ? ((object) value.GetType()) : ((object) "null"), typeof(T)), argument);
        }

        public T[] ToArray()
        {
            T[] destinationArray = new T[this._size];
            Array.Copy(this._items, 0, destinationArray, 0, this._size);
            return destinationArray;
        }

        public ReadOnlyCollection<T> ToReadOnlyCollection()
        {
            T[] localArray;
            if (this._size == this._items.Length)
            {
                localArray = this._items;
            }
            else
            {
                localArray = this.ToArray();
            }
            this._items = ReadOnlyCollectionBuilder<T>._emptyArray;
            this._size = 0;
            this._version++;
            return new TrueReadOnlyCollection<T>(localArray);
        }

        private static void ValidateNullValue(object value, string argument)
        {
            if ((value == null) && (default(T) != null))
            {
                throw new ArgumentException(Strings.InvalidNullValue(typeof(T)), argument);
            }
        }

        public int Capacity
        {
            get
            {
                return this._items.Length;
            }
            set
            {
                ContractUtils.Requires(value >= this._size, "value");
                if (value != this._items.Length)
                {
                    if (value > 0)
                    {
                        T[] destinationArray = new T[value];
                        if (this._size > 0)
                        {
                            Array.Copy(this._items, 0, destinationArray, 0, this._size);
                        }
                        this._items = destinationArray;
                    }
                    else
                    {
                        this._items = ReadOnlyCollectionBuilder<T>._emptyArray;
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return this._size;
            }
        }

        public T this[int index]
        {
            get
            {
                ContractUtils.Requires(index < this._size, "index");
                return this._items[index];
            }
            set
            {
                ContractUtils.Requires(index < this._size, "index");
                this._items[index] = value;
                this._version++;
            }
        }

        bool ICollection<T>.IsReadOnly
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
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                ReadOnlyCollectionBuilder<T>.ValidateNullValue(value, "value");
                try
                {
                    this[index] = (T) value;
                }
                catch (InvalidCastException)
                {
                    ReadOnlyCollectionBuilder<T>.ThrowInvalidTypeException(value, "value");
                }
            }
        }

        [Serializable]
        private class Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly ReadOnlyCollectionBuilder<T> _builder;
            private T _current;
            private int _index;
            private readonly int _version;

            internal Enumerator(ReadOnlyCollectionBuilder<T> builder)
            {
                this._builder = builder;
                this._version = builder._version;
                this._index = 0;
                this._current = default(T);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            public bool MoveNext()
            {
                if (this._version != this._builder._version)
                {
                    throw Error.CollectionModifiedWhileEnumerating();
                }
                if (this._index < this._builder._size)
                {
                    this._current = this._builder._items[this._index++];
                    return true;
                }
                this._index = this._builder._size + 1;
                this._current = default(T);
                return false;
            }

            void IEnumerator.Reset()
            {
                if (this._version != this._builder._version)
                {
                    throw Error.CollectionModifiedWhileEnumerating();
                }
                this._index = 0;
                this._current = default(T);
            }

            public T Current
            {
                get
                {
                    return this._current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if ((this._index == 0) || (this._index > this._builder._size))
                    {
                        throw Error.EnumerationIsDone();
                    }
                    return this._current;
                }
            }
        }
    }
}

