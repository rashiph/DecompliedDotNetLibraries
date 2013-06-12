namespace System.Linq.Expressions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class Set<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly Dictionary<T, object> _data;

        internal Set()
        {
            this._data = new Dictionary<T, object>();
        }

        internal Set(IEnumerable<T> list)
        {
            this._data = new Dictionary<T, object>();
            foreach (T local in list)
            {
                this.Add(local);
            }
        }

        internal Set(IList<T> list)
        {
            this._data = new Dictionary<T, object>(list.Count);
            foreach (T local in list)
            {
                this.Add(local);
            }
        }

        internal Set(IEqualityComparer<T> comparer)
        {
            this._data = new Dictionary<T, object>(comparer);
        }

        internal Set(int capacity)
        {
            this._data = new Dictionary<T, object>(capacity);
        }

        public void Add(T item)
        {
            this._data[item] = null;
        }

        public void Clear()
        {
            this._data.Clear();
        }

        public bool Contains(T item)
        {
            return this._data.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._data.Keys.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._data.Keys.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return this._data.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._data.Keys.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this._data.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}

