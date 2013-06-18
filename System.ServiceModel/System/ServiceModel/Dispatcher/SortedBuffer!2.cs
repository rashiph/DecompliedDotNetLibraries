namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SortedBuffer<T, C> where C: IComparer<T>
    {
        private int size;
        private T[] buffer;
        private static DefaultComparer<T, C> Comparer;
        internal SortedBuffer(C comparerInstance)
        {
            this.size = 0;
            this.buffer = null;
            if (SortedBuffer<T, C>.Comparer == null)
            {
                SortedBuffer<T, C>.Comparer = new DefaultComparer<T, C>(comparerInstance);
            }
        }

        internal T this[int index]
        {
            get
            {
                return this.GetAt(index);
            }
        }
        internal int Capacity
        {
            set
            {
                if (this.buffer != null)
                {
                    if (value != this.buffer.Length)
                    {
                        if (value > 0)
                        {
                            Array.Resize<T>(ref this.buffer, value);
                        }
                        else
                        {
                            this.buffer = null;
                        }
                    }
                }
                else
                {
                    this.buffer = new T[value];
                }
            }
        }
        internal int Count
        {
            get
            {
                return this.size;
            }
        }
        internal int Add(T item)
        {
            int index = this.Search(item);
            if (index < 0)
            {
                index = ~index;
                this.InsertAt(index, item);
            }
            return index;
        }

        internal void Clear()
        {
            this.size = 0;
        }

        internal void Exchange(T old, T replace)
        {
            if (SortedBuffer<T, C>.Comparer.Compare(old, replace) == 0)
            {
                int index = this.IndexOf(old);
                if (index >= 0)
                {
                    this.buffer[index] = replace;
                }
                else
                {
                    this.Insert(replace);
                }
            }
            else
            {
                this.Remove(old);
                this.Insert(replace);
            }
        }

        internal T GetAt(int index)
        {
            return this.buffer[index];
        }

        internal int IndexOf(T item)
        {
            return this.Search(item);
        }

        internal int IndexOfKey<K>(K key, IItemComparer<K, T> itemComp)
        {
            return this.Search<K>(key, itemComp);
        }

        internal int Insert(T item)
        {
            int num = this.Search(item);
            if (num >= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new ArgumentException(System.ServiceModel.SR.GetString("QueryItemAlreadyExists")));
            }
            this.InsertAt(~num, item);
            return ~num;
        }

        private void InsertAt(int index, T item)
        {
            if (this.buffer == null)
            {
                this.buffer = new T[1];
            }
            else if (this.buffer.Length == this.size)
            {
                T[] destinationArray = new T[this.size + 1];
                if (index == 0)
                {
                    Array.Copy(this.buffer, 0, destinationArray, 1, this.size);
                }
                else if (index == this.size)
                {
                    Array.Copy(this.buffer, 0, destinationArray, 0, this.size);
                }
                else
                {
                    Array.Copy(this.buffer, 0, destinationArray, 0, index);
                    Array.Copy(this.buffer, index, destinationArray, index + 1, this.size - index);
                }
                this.buffer = destinationArray;
            }
            else
            {
                Array.Copy(this.buffer, index, this.buffer, index + 1, this.size - index);
            }
            this.buffer[index] = item;
            this.size++;
        }

        internal bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        internal void RemoveAt(int index)
        {
            if (index < (this.size - 1))
            {
                Array.Copy(this.buffer, index + 1, this.buffer, index, (this.size - index) - 1);
            }
            this.buffer[--this.size] = default(T);
        }

        private int Search(T item)
        {
            if (this.size == 0)
            {
                return -1;
            }
            return this.Search<T>(item, SortedBuffer<T, C>.Comparer);
        }

        private int Search<K>(K key, IItemComparer<K, T> comparer)
        {
            if (this.size <= 8)
            {
                return this.LinearSearch<K>(key, comparer, 0, this.size);
            }
            return this.BinarySearch<K>(key, comparer);
        }

        private int BinarySearch<K>(K key, IItemComparer<K, T> comparer)
        {
            int start = 0;
            int size = this.size;
            while ((size - start) > 8)
            {
                int index = (size + start) / 2;
                int num4 = comparer.Compare(key, this.buffer[index]);
                if (num4 < 0)
                {
                    size = index;
                }
                else
                {
                    if (num4 > 0)
                    {
                        start = index + 1;
                        continue;
                    }
                    return index;
                }
            }
            return this.LinearSearch<K>(key, comparer, start, size);
        }

        private int LinearSearch<K>(K key, IItemComparer<K, T> comparer, int start, int bound)
        {
            for (int i = start; i < bound; i++)
            {
                int num = comparer.Compare(key, this.buffer[i]);
                if (num == 0)
                {
                    return i;
                }
                if (num < 0)
                {
                    return ~i;
                }
            }
            return ~bound;
        }

        internal void Trim()
        {
            this.Capacity = this.size;
        }
        internal class DefaultComparer : IItemComparer<T, T>
        {
            public static IComparer<T> Comparer;

            public DefaultComparer(C comparer)
            {
                SortedBuffer<T, C>.DefaultComparer.Comparer = comparer;
            }

            public int Compare(T item1, T item2)
            {
                return SortedBuffer<T, C>.DefaultComparer.Comparer.Compare(item1, item2);
            }
        }
    }
}

