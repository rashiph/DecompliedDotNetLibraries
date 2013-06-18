namespace System.Data.ProviderBase
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal abstract class DbReferenceCollection
    {
        private CollectionEntry[] _items = new CollectionEntry[5];

        protected DbReferenceCollection()
        {
        }

        public abstract void Add(object value, int tag);
        protected void AddItem(object value, int tag)
        {
            CollectionEntry[] entryArray = this._items;
            for (int i = 0; i < entryArray.Length; i++)
            {
                if (!entryArray[i].HasTarget)
                {
                    entryArray[i].Target = value;
                    entryArray[i].Tag = tag;
                    return;
                }
            }
            int num3 = (5 == entryArray.Length) ? 15 : (entryArray.Length + 15);
            CollectionEntry[] entryArray2 = new CollectionEntry[num3];
            for (int j = 0; j < entryArray.Length; j++)
            {
                entryArray2[j] = entryArray[j];
            }
            entryArray2[entryArray.Length].Target = value;
            entryArray2[entryArray.Length].Tag = tag;
            this._items = entryArray2;
        }

        internal IEnumerable Filter(int tag)
        {
            return new DbFilteredReferenceCollection(this._items, tag);
        }

        public void Notify(int message)
        {
            CollectionEntry[] entryArray = this._items;
            for (int i = 0; i < entryArray.Length; i++)
            {
                if (!entryArray[i].InUse)
                {
                    break;
                }
                object target = entryArray[i].Target;
                if ((target != null) && !this.NotifyItem(message, entryArray[i].Tag, target))
                {
                    entryArray[i].Tag = 0;
                    entryArray[i].Target = null;
                }
            }
        }

        protected abstract bool NotifyItem(int message, int tag, object value);
        public void Purge()
        {
            CollectionEntry[] entryArray = this._items;
            if (100 < entryArray.Length)
            {
                this._items = new CollectionEntry[5];
            }
        }

        public abstract void Remove(object value);
        protected void RemoveItem(object value)
        {
            CollectionEntry[] entryArray = this._items;
            for (int i = 0; i < entryArray.Length; i++)
            {
                if (!entryArray[i].InUse)
                {
                    break;
                }
                if (value == entryArray[i].Target)
                {
                    entryArray[i].Tag = 0;
                    entryArray[i].Target = null;
                    return;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CollectionEntry
        {
            private int _tag;
            private WeakReference _weak;
            public bool HasTarget
            {
                get
                {
                    return (((this._tag != 0) && (this._weak != null)) && this._weak.IsAlive);
                }
            }
            public bool InUse
            {
                get
                {
                    return (null != this._weak);
                }
            }
            public int Tag
            {
                get
                {
                    return this._tag;
                }
                set
                {
                    this._tag = value;
                }
            }
            public object Target
            {
                get
                {
                    if (this._tag != 0)
                    {
                        return this._weak.Target;
                    }
                    return null;
                }
                set
                {
                    if (this._weak == null)
                    {
                        this._weak = new WeakReference(value, false);
                    }
                    else
                    {
                        this._weak.Target = value;
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DbFilteredReferenceCollection : IEnumerable
        {
            private readonly DbReferenceCollection.CollectionEntry[] _items;
            private readonly int _filterTag;
            internal DbFilteredReferenceCollection(DbReferenceCollection.CollectionEntry[] items, int filterTag)
            {
                this._items = items;
                this._filterTag = filterTag;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new DbFilteredReferenceCollectionedEnumerator(this._items, this._filterTag);
            }
            [StructLayout(LayoutKind.Sequential)]
            private struct DbFilteredReferenceCollectionedEnumerator : IEnumerator
            {
                private readonly DbReferenceCollection.CollectionEntry[] _items;
                private readonly int _filterTag;
                private int _current;
                internal DbFilteredReferenceCollectionedEnumerator(DbReferenceCollection.CollectionEntry[] items, int filterTag)
                {
                    this._items = items;
                    this._filterTag = filterTag;
                    this._current = -1;
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return this._items[this._current].Target;
                    }
                }
                bool IEnumerator.MoveNext()
                {
                    while (++this._current < this._items.Length)
                    {
                        if (!this._items[this._current].InUse)
                        {
                            break;
                        }
                        if (this._items[this._current].Tag == this._filterTag)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                void IEnumerator.Reset()
                {
                    this._current = -1;
                }
            }
        }
    }
}

