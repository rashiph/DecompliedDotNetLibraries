namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [Serializable, DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>)), DebuggerDisplay("Count = {Count}")]
    public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private const int _defaultCapacity = 4;
        private static readonly T[] _emptyArray;
        private T[] _items;
        private int _size;
        [NonSerialized]
        private object _syncRoot;
        private int _version;

        static List()
        {
            List<T>._emptyArray = new T[0];
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public List()
        {
            this._items = List<T>._emptyArray;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public List(int capacity)
        {
            if (capacity < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            this._items = new T[capacity];
        }

        public List(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            }
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

        public void Add(T item)
        {
            if (this._size == this._items.Length)
            {
                this.EnsureCapacity(this._size + 1);
            }
            this._items[this._size++] = item;
            this._version++;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            this.InsertRange(this._size, collection);
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>(this);
        }

        public int BinarySearch(T item)
        {
            return this.BinarySearch(0, this.Count, item, null);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return this.BinarySearch(0, this.Count, item, comparer);
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            if (index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((this._size - index) < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            return Array.BinarySearch<T>(this._items, index, count, item, comparer);
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

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            if (converter == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
            }
            List<TOutput> list = new List<TOutput>(this._size);
            for (int i = 0; i < this._size; i++)
            {
                list._items[i] = converter(this._items[i]);
            }
            list._size = this._size;
            return list;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public void CopyTo(T[] array)
        {
            this.CopyTo(array, 0);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this._items, 0, array, arrayIndex, this._size);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if ((this._size - index) < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            Array.Copy(this._items, index, array, arrayIndex, count);
        }

        private void EnsureCapacity(int min)
        {
            if (this._items.Length < min)
            {
                int num = (this._items.Length == 0) ? 4 : (this._items.Length * 2);
                if (num < min)
                {
                    num = min;
                }
                this.Capacity = num;
            }
        }

        public bool Exists(Predicate<T> match)
        {
            return (this.FindIndex(match) != -1);
        }

        public T Find(Predicate<T> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            for (int i = 0; i < this._size; i++)
            {
                if (match(this._items[i]))
                {
                    return this._items[i];
                }
            }
            return default(T);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            List<T> list = new List<T>();
            for (int i = 0; i < this._size; i++)
            {
                if (match(this._items[i]))
                {
                    list.Add(this._items[i]);
                }
            }
            return list;
        }

        public int FindIndex(Predicate<T> match)
        {
            return this.FindIndex(0, this._size, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return this.FindIndex(startIndex, this._size - startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if (startIndex > this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            }
            if ((count < 0) || (startIndex > (this._size - count)))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            }
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(this._items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public T FindLast(Predicate<T> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            for (int i = this._size - 1; i >= 0; i--)
            {
                if (match(this._items[i]))
                {
                    return this._items[i];
                }
            }
            return default(T);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return this.FindLastIndex(this._size - 1, this._size, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return this.FindLastIndex(startIndex, startIndex + 1, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            if (this._size == 0)
            {
                if (startIndex != -1)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
                }
            }
            else if (startIndex >= this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            }
            if ((count < 0) || (((startIndex - count) + 1) < 0))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            }
            int num = startIndex - count;
            for (int i = startIndex; i > num; i--)
            {
                if (match(this._items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public void ForEach(Action<T> action)
        {
            if (action == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            for (int i = 0; i < this._size; i++)
            {
                action(this._items[i]);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public Enumerator<T> GetEnumerator()
        {
            return new Enumerator<T>((List<T>) this);
        }

        public List<T> GetRange(int index, int count)
        {
            if (index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((this._size - index) < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            List<T> list = new List<T>(count);
            Array.Copy(this._items, index, list._items, 0, count);
            list._size = count;
            return list;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOf(T item)
        {
            return Array.IndexOf<T>(this._items, item, 0, this._size);
        }

        public int IndexOf(T item, int index)
        {
            if (index > this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            }
            return Array.IndexOf<T>(this._items, item, index, this._size - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            if (index > this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            }
            if ((count < 0) || (index > (this._size - count)))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            }
            return Array.IndexOf<T>(this._items, item, index, count);
        }

        public void Insert(int index, T item)
        {
            if (index > this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
            }
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

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            }
            if (index > this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            }
            ICollection<T> is2 = collection as ICollection<T>;
            if (is2 != null)
            {
                int count = is2.Count;
                if (count > 0)
                {
                    this.EnsureCapacity(this._size + count);
                    if (index < this._size)
                    {
                        Array.Copy(this._items, index, this._items, index + count, this._size - index);
                    }
                    if (this == is2)
                    {
                        Array.Copy(this._items, 0, this._items, index, index);
                        Array.Copy(this._items, (int) (index + count), this._items, (int) (index * 2), (int) (this._size - index));
                    }
                    else
                    {
                        T[] array = new T[count];
                        is2.CopyTo(array, 0);
                        array.CopyTo(this._items, index);
                    }
                    this._size += count;
                }
            }
            else
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        this.Insert(index++, enumerator.Current);
                    }
                }
            }
            this._version++;
        }

        private static bool IsCompatibleObject(object value)
        {
            return ((value is T) || ((value == null) && (default(T) == null)));
        }

        public int LastIndexOf(T item)
        {
            if (this._size == 0)
            {
                return -1;
            }
            return this.LastIndexOf(item, this._size - 1, this._size);
        }

        public int LastIndexOf(T item, int index)
        {
            if (index >= this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            }
            return this.LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if ((this.Count != 0) && (index < 0))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((this.Count != 0) && (count < 0))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (this._size == 0)
            {
                return -1;
            }
            if (index >= this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
            }
            if (count > (index + 1))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
            }
            return Array.LastIndexOf<T>(this._items, item, index, count);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
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

        public int RemoveAll(Predicate<T> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            int index = 0;
            while ((index < this._size) && !match(this._items[index]))
            {
                index++;
            }
            if (index >= this._size)
            {
                return 0;
            }
            int num2 = index + 1;
            while (num2 < this._size)
            {
                while ((num2 < this._size) && match(this._items[num2]))
                {
                    num2++;
                }
                if (num2 < this._size)
                {
                    this._items[index++] = this._items[num2++];
                }
            }
            Array.Clear(this._items, index, this._size - index);
            int num3 = this._size - index;
            this._size = index;
            this._version++;
            return num3;
        }

        public void RemoveAt(int index)
        {
            if (index >= this._size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }
            this._size--;
            if (index < this._size)
            {
                Array.Copy(this._items, index + 1, this._items, index, this._size - index);
            }
            this._items[this._size] = default(T);
            this._version++;
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((this._size - index) < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            if (count > 0)
            {
                this._size -= count;
                if (index < this._size)
                {
                    Array.Copy(this._items, index + count, this._items, index, this._size - index);
                }
                Array.Clear(this._items, this._size, count);
                this._version++;
            }
        }

        public void Reverse()
        {
            this.Reverse(0, this.Count);
        }

        public void Reverse(int index, int count)
        {
            if (index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((this._size - index) < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            Array.Reverse(this._items, index, count);
            this._version++;
        }

        public void Sort()
        {
            this.Sort(0, this.Count, null);
        }

        public void Sort(IComparer<T> comparer)
        {
            this.Sort(0, this.Count, comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            if (comparison == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            if (this._size > 0)
            {
                IComparer<T> comparer = new Array.FunctorComparer<T>(comparison);
                Array.Sort<T>(this._items, 0, this._size, comparer);
            }
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (index < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if (count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((this._size - index) < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            Array.Sort<T>(this._items, index, count, comparer);
            this._version++;
        }

        internal static IList<T> Synchronized(List<T> list)
        {
            return new SynchronizedList<T>(list);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator<T>((List<T>) this);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            }
            try
            {
                Array.Copy(this._items, 0, array, arrayIndex, this._size);
            }
            catch (ArrayTypeMismatchException)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>((List<T>) this);
        }

        int IList.Add(object item)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
            try
            {
                this.Add((T) item);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
            }
            return (this.Count - 1);
        }

        [SecuritySafeCritical]
        bool IList.Contains(object item)
        {
            return (List<T>.IsCompatibleObject(item) && this.Contains((T) item));
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        int IList.IndexOf(object item)
        {
            if (List<T>.IsCompatibleObject(item))
            {
                return this.IndexOf((T) item);
            }
            return -1;
        }

        void IList.Insert(int index, object item)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);
            try
            {
                this.Insert(index, (T) item);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
            }
        }

        [SecuritySafeCritical]
        void IList.Remove(object item)
        {
            if (List<T>.IsCompatibleObject(item))
            {
                this.Remove((T) item);
            }
        }

        public T[] ToArray()
        {
            T[] destinationArray = new T[this._size];
            Array.Copy(this._items, 0, destinationArray, 0, this._size);
            return destinationArray;
        }

        public void TrimExcess()
        {
            int num = (int) (this._items.Length * 0.9);
            if (this._size < num)
            {
                this.Capacity = this._size;
            }
        }

        public bool TrueForAll(Predicate<T> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }
            for (int i = 0; i < this._size; i++)
            {
                if (!match(this._items[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public int Capacity
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this._items.Length;
            }
            set
            {
                if (value < this._size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity);
                }
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
                        this._items = List<T>._emptyArray;
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
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (index >= this._size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }
                return this._items[index];
            }
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set
            {
                if (index >= this._size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }
                this._items[index] = value;
                this._version++;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
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
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
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
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
                try
                {
                    this[index] = (T) value;
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
                }
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private List<T> list;
            private int index;
            private int version;
            private T current;
            internal Enumerator(List<T> list)
            {
                this.list = list;
                this.index = 0;
                this.version = list._version;
                this.current = default(T);
            }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                List<T> list = this.list;
                if ((this.version == list._version) && (this.index < list._size))
                {
                    this.current = list._items[this.index];
                    this.index++;
                    return true;
                }
                return this.MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (this.version != this.list._version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                this.index = this.list._size + 1;
                this.current = default(T);
                return false;
            }

            public T Current
            {
                get
                {
                    return this.current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    if ((this.index == 0) || (this.index == (this.list._size + 1)))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return this.Current;
                }
            }
            void IEnumerator.Reset()
            {
                if (this.version != this.list._version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                this.index = 0;
                this.current = default(T);
            }
        }

        [Serializable]
        internal class SynchronizedList : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
        {
            private List<T> _list;
            private object _root;

            internal SynchronizedList(List<T> list)
            {
                this._list = list;
                this._root = ((ICollection) list).SyncRoot;
            }

            public void Add(T item)
            {
                lock (this._root)
                {
                    this._list.Add(item);
                }
            }

            public void Clear()
            {
                lock (this._root)
                {
                    this._list.Clear();
                }
            }

            public bool Contains(T item)
            {
                lock (this._root)
                {
                    return this._list.Contains(item);
                }
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                lock (this._root)
                {
                    this._list.CopyTo(array, arrayIndex);
                }
            }

            public int IndexOf(T item)
            {
                lock (this._root)
                {
                    return this._list.IndexOf(item);
                }
            }

            public void Insert(int index, T item)
            {
                lock (this._root)
                {
                    this._list.Insert(index, item);
                }
            }

            public bool Remove(T item)
            {
                lock (this._root)
                {
                    return this._list.Remove(item);
                }
            }

            public void RemoveAt(int index)
            {
                lock (this._root)
                {
                    this._list.RemoveAt(index);
                }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                lock (this._root)
                {
                    return this._list.GetEnumerator();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                lock (this._root)
                {
                    return this._list.GetEnumerator();
                }
            }

            public int Count
            {
                get
                {
                    lock (this._root)
                    {
                        return this._list.Count;
                    }
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return this._list.IsReadOnly;
                }
            }

            public T this[int index]
            {
                get
                {
                    lock (this._root)
                    {
                        return this._list[index];
                    }
                }
                set
                {
                    lock (this._root)
                    {
                        this._list[index] = value;
                    }
                }
            }
        }
    }
}

