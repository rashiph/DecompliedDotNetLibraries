namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    internal class ObjectSet : ICollection, IEnumerable
    {
        private static EmptyEnumerator _emptyEnumerator = new EmptyEnumerator();
        private IDictionary _objects;

        internal ObjectSet()
        {
        }

        public void Add(object o)
        {
            if (this._objects == null)
            {
                this._objects = new HybridDictionary(this.CaseInsensitive);
            }
            this._objects[o] = null;
        }

        public void AddCollection(ICollection c)
        {
            foreach (object obj2 in c)
            {
                this.Add(obj2);
            }
        }

        public bool Contains(object o)
        {
            if (this._objects == null)
            {
                return false;
            }
            return this._objects.Contains(o);
        }

        public void CopyTo(Array array, int index)
        {
            if (this._objects != null)
            {
                this._objects.Keys.CopyTo(array, index);
            }
        }

        public void Remove(object o)
        {
            if (this._objects != null)
            {
                this._objects.Remove(o);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this._objects == null)
            {
                return _emptyEnumerator;
            }
            return this._objects.Keys.GetEnumerator();
        }

        protected virtual bool CaseInsensitive
        {
            get
            {
                return false;
            }
        }

        public int Count
        {
            get
            {
                if (this._objects == null)
                {
                    return 0;
                }
                return this._objects.Keys.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((this._objects == null) || this._objects.Keys.IsSynchronized);
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (this._objects == null)
                {
                    return this;
                }
                return this._objects.Keys.SyncRoot;
            }
        }

        private class EmptyEnumerator : IEnumerator
        {
            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }

            public object Current
            {
                get
                {
                    return null;
                }
            }
        }
    }
}

