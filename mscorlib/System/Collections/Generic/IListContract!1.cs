namespace System.Collections.Generic
{
    using System;
    using System.Collections;

    internal abstract class IListContract<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        protected IListContract()
        {
        }

        void ICollection<T>.Add(T value)
        {
        }

        void ICollection<T>.Clear()
        {
        }

        bool ICollection<T>.Contains(T value)
        {
            return false;
        }

        void ICollection<T>.CopyTo(T[] array, int startIndex)
        {
        }

        bool ICollection<T>.Remove(T value)
        {
            return false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return null;
        }

        int IList<T>.IndexOf(T value)
        {
            return 0;
        }

        void IList<T>.Insert(int index, T value)
        {
        }

        void IList<T>.RemoveAt(int index)
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }

        int ICollection<T>.Count
        {
            get
            {
                return 0;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return default(T);
            }
            set
            {
            }
        }
    }
}

