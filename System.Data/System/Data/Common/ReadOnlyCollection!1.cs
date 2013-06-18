namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [Serializable]
    internal sealed class ReadOnlyCollection<T> : ICollection, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private T[] _items;

        internal ReadOnlyCollection(T[] items)
        {
            this._items = items;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this._items, 0, array, arrayIndex, this._items.Length);
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator<T, T>(this._items);
        }

        void ICollection<T>.Add(T value)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains(T value)
        {
            return (Array.IndexOf<T>(this._items, value) >= 0);
        }

        bool ICollection<T>.Remove(T value)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator<T, T>(this._items);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            Array.Copy(this._items, 0, array, arrayIndex, this._items.Length);
        }

        public int Count
        {
            get
            {
                return this._items.Length;
            }
        }

        bool ICollection<T>.IsReadOnly
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
                return this._items;
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        internal struct Enumerator<K> : IEnumerator<K>, IDisposable, IEnumerator
        {
            private K[] _items;
            private int _index;
            internal Enumerator(K[] items)
            {
                this._items = items;
                this._index = -1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return (++this._index < this._items.Length);
            }

            public K Current
            {
                get
                {
                    return this._items[this._index];
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return this._items[this._index];
                }
            }
            void IEnumerator.Reset()
            {
                this._index = -1;
            }
        }
    }
}

