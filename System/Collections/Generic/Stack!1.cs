namespace System.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Serializable, DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(System_StackDebugView<>)), ComVisible(false)]
    public class Stack<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        private T[] _array;
        private const int _defaultCapacity = 4;
        private static T[] _emptyArray;
        private int _size;
        [NonSerialized]
        private object _syncRoot;
        private int _version;

        static Stack()
        {
            Stack<T>._emptyArray = new T[0];
        }

        public Stack()
        {
            this._array = Stack<T>._emptyArray;
            this._size = 0;
            this._version = 0;
        }

        public Stack(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.collection);
            }
            ICollection<T> is2 = collection as ICollection<T>;
            if (is2 != null)
            {
                int count = is2.Count;
                this._array = new T[count];
                is2.CopyTo(this._array, 0);
                this._size = count;
            }
            else
            {
                this._size = 0;
                this._array = new T[4];
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        this.Push(enumerator.Current);
                    }
                }
            }
        }

        public Stack(int capacity)
        {
            if (capacity < 0)
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.capacity, System.ExceptionResource.ArgumentOutOfRange_NeedNonNegNumRequired);
            }
            this._array = new T[capacity];
            this._size = 0;
            this._version = 0;
        }

        public void Clear()
        {
            Array.Clear(this._array, 0, this._size);
            this._size = 0;
            this._version++;
        }

        public bool Contains(T item)
        {
            int index = this._size;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (index-- > 0)
            {
                if (item == null)
                {
                    if (this._array[index] == null)
                    {
                        return true;
                    }
                }
                else if ((this._array[index] != null) && comparer.Equals(this._array[index], item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
            }
            if ((arrayIndex < 0) || (arrayIndex > array.Length))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.arrayIndex, System.ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((array.Length - arrayIndex) < this._size)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidOffLen);
            }
            Array.Copy(this._array, 0, array, arrayIndex, this._size);
            Array.Reverse(array, arrayIndex, this._size);
        }

        public Enumerator<T> GetEnumerator()
        {
            return new Enumerator<T>((Stack<T>) this);
        }

        public T Peek()
        {
            if (this._size == 0)
            {
                System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EmptyStack);
            }
            return this._array[this._size - 1];
        }

        public T Pop()
        {
            if (this._size == 0)
            {
                System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EmptyStack);
            }
            this._version++;
            T local = this._array[--this._size];
            this._array[this._size] = default(T);
            return local;
        }

        public void Push(T item)
        {
            if (this._size == this._array.Length)
            {
                T[] destinationArray = new T[(this._array.Length == 0) ? 4 : (2 * this._array.Length)];
                Array.Copy(this._array, 0, destinationArray, 0, this._size);
                this._array = destinationArray;
            }
            this._array[this._size++] = item;
            this._version++;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator<T>((Stack<T>) this);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                System.ThrowHelper.ThrowArgumentNullException(System.ExceptionArgument.array);
            }
            if (array.Rank != 1)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_RankMultiDimNotSupported);
            }
            if (array.GetLowerBound(0) != 0)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Arg_NonZeroLowerBound);
            }
            if ((arrayIndex < 0) || (arrayIndex > array.Length))
            {
                System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.arrayIndex, System.ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }
            if ((array.Length - arrayIndex) < this._size)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidOffLen);
            }
            try
            {
                Array.Copy(this._array, 0, array, arrayIndex, this._size);
                Array.Reverse(array, arrayIndex, this._size);
            }
            catch (ArrayTypeMismatchException)
            {
                System.ThrowHelper.ThrowArgumentException(System.ExceptionResource.Argument_InvalidArrayType);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<T>((Stack<T>) this);
        }

        public T[] ToArray()
        {
            T[] localArray = new T[this._size];
            for (int i = 0; i < this._size; i++)
            {
                localArray[i] = this._array[(this._size - i) - 1];
            }
            return localArray;
        }

        public void TrimExcess()
        {
            int num = (int) (this._array.Length * 0.9);
            if (this._size < num)
            {
                T[] destinationArray = new T[this._size];
                Array.Copy(this._array, 0, destinationArray, 0, this._size);
                this._array = destinationArray;
                this._version++;
            }
        }

        public int Count
        {
            get
            {
                return this._size;
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

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private Stack<T> _stack;
            private int _index;
            private int _version;
            private T currentElement;
            internal Enumerator(Stack<T> stack)
            {
                this._stack = stack;
                this._version = this._stack._version;
                this._index = -2;
                this.currentElement = default(T);
            }

            public void Dispose()
            {
                this._index = -1;
            }

            public bool MoveNext()
            {
                bool flag;
                if (this._version != this._stack._version)
                {
                    System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                if (this._index == -2)
                {
                    this._index = this._stack._size - 1;
                    flag = this._index >= 0;
                    if (flag)
                    {
                        this.currentElement = this._stack._array[this._index];
                    }
                    return flag;
                }
                if (this._index == -1)
                {
                    return false;
                }
                flag = --this._index >= 0;
                if (flag)
                {
                    this.currentElement = this._stack._array[this._index];
                    return flag;
                }
                this.currentElement = default(T);
                return flag;
            }

            public T Current
            {
                get
                {
                    if (this._index == -2)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumNotStarted);
                    }
                    if (this._index == -1)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumEnded);
                    }
                    return this.currentElement;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    if (this._index == -2)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumNotStarted);
                    }
                    if (this._index == -1)
                    {
                        System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumEnded);
                    }
                    return this.currentElement;
                }
            }
            void IEnumerator.Reset()
            {
                if (this._version != this._stack._version)
                {
                    System.ThrowHelper.ThrowInvalidOperationException(System.ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                this._index = -2;
                this.currentElement = default(T);
            }
        }
    }
}

