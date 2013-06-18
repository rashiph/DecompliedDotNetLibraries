namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web.Util;

    public sealed class StateBag : IStateManager, IDictionary, ICollection, IEnumerable
    {
        private IDictionary bag;
        private bool ignoreCase;
        private bool marked;

        public StateBag() : this(false)
        {
        }

        public StateBag(bool ignoreCase)
        {
            this.marked = false;
            this.ignoreCase = ignoreCase;
            this.bag = this.CreateBag();
        }

        public StateItem Add(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("key");
            }
            StateItem item = this.bag[key] as StateItem;
            if (item == null)
            {
                if ((value != null) || this.marked)
                {
                    item = new StateItem(value);
                    this.bag.Add(key, item);
                }
            }
            else if ((value == null) && !this.marked)
            {
                this.bag.Remove(key);
            }
            else
            {
                item.Value = value;
            }
            if ((item != null) && this.marked)
            {
                item.IsDirty = true;
            }
            return item;
        }

        public void Clear()
        {
            this.bag.Clear();
        }

        private IDictionary CreateBag()
        {
            return new HybridDictionary(this.ignoreCase);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return this.bag.GetEnumerator();
        }

        public bool IsItemDirty(string key)
        {
            StateItem item = this.bag[key] as StateItem;
            return ((item != null) && item.IsDirty);
        }

        internal void LoadViewState(object state)
        {
            if (state != null)
            {
                ArrayList list = (ArrayList) state;
                for (int i = 0; i < list.Count; i += 2)
                {
                    string key = ((IndexedString) list[i]).Value;
                    object obj2 = list[i + 1];
                    this.Add(key, obj2);
                }
            }
        }

        public void Remove(string key)
        {
            this.bag.Remove(key);
        }

        internal object SaveViewState()
        {
            ArrayList list = null;
            if (this.bag.Count != 0)
            {
                IDictionaryEnumerator enumerator = this.bag.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    StateItem item = (StateItem) enumerator.Value;
                    if (item.IsDirty)
                    {
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        list.Add(new IndexedString((string) enumerator.Key));
                        list.Add(item.Value);
                    }
                }
            }
            return list;
        }

        public void SetDirty(bool dirty)
        {
            if (this.bag.Count != 0)
            {
                foreach (StateItem item in this.bag.Values)
                {
                    item.IsDirty = dirty;
                }
            }
        }

        public void SetItemDirty(string key, bool dirty)
        {
            StateItem item = this.bag[key] as StateItem;
            if (item != null)
            {
                item.IsDirty = dirty;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.Values.CopyTo(array, index);
        }

        void IDictionary.Add(object key, object value)
        {
            this.Add((string) key, value);
        }

        bool IDictionary.Contains(object key)
        {
            return this.bag.Contains((string) key);
        }

        void IDictionary.Remove(object key)
        {
            this.Remove((string) key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        internal void TrackViewState()
        {
            this.marked = true;
        }

        public int Count
        {
            get
            {
                return this.bag.Count;
            }
        }

        internal bool IsTrackingViewState
        {
            get
            {
                return this.marked;
            }
        }

        public object this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw ExceptionUtil.ParameterNullOrEmpty("key");
                }
                StateItem item = this.bag[key] as StateItem;
                if (item != null)
                {
                    return item.Value;
                }
                return null;
            }
            set
            {
                this.Add(key, value);
            }
        }

        public ICollection Keys
        {
            get
            {
                return this.bag.Keys;
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
                return this;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this[(string) key];
            }
            set
            {
                this[(string) key] = value;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        public ICollection Values
        {
            get
            {
                return this.bag.Values;
            }
        }
    }
}

