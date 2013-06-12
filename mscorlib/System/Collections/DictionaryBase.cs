namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public abstract class DictionaryBase : IDictionary, ICollection, IEnumerable
    {
        private Hashtable hashtable;

        protected DictionaryBase()
        {
        }

        public void Clear()
        {
            this.OnClear();
            this.InnerHashtable.Clear();
            this.OnClearComplete();
        }

        public void CopyTo(Array array, int index)
        {
            this.InnerHashtable.CopyTo(array, index);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return this.InnerHashtable.GetEnumerator();
        }

        protected virtual void OnClear()
        {
        }

        protected virtual void OnClearComplete()
        {
        }

        protected virtual object OnGet(object key, object currentValue)
        {
            return currentValue;
        }

        protected virtual void OnInsert(object key, object value)
        {
        }

        protected virtual void OnInsertComplete(object key, object value)
        {
        }

        protected virtual void OnRemove(object key, object value)
        {
        }

        protected virtual void OnRemoveComplete(object key, object value)
        {
        }

        protected virtual void OnSet(object key, object oldValue, object newValue)
        {
        }

        protected virtual void OnSetComplete(object key, object oldValue, object newValue)
        {
        }

        protected virtual void OnValidate(object key, object value)
        {
        }

        void IDictionary.Add(object key, object value)
        {
            this.OnValidate(key, value);
            this.OnInsert(key, value);
            this.InnerHashtable.Add(key, value);
            try
            {
                this.OnInsertComplete(key, value);
            }
            catch
            {
                this.InnerHashtable.Remove(key);
                throw;
            }
        }

        bool IDictionary.Contains(object key)
        {
            return this.InnerHashtable.Contains(key);
        }

        void IDictionary.Remove(object key)
        {
            if (this.InnerHashtable.Contains(key))
            {
                object obj2 = this.InnerHashtable[key];
                this.OnValidate(key, obj2);
                this.OnRemove(key, obj2);
                this.InnerHashtable.Remove(key);
                try
                {
                    this.OnRemoveComplete(key, obj2);
                }
                catch
                {
                    this.InnerHashtable.Add(key, obj2);
                    throw;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerHashtable.GetEnumerator();
        }

        public int Count
        {
            get
            {
                if (this.hashtable != null)
                {
                    return this.hashtable.Count;
                }
                return 0;
            }
        }

        protected IDictionary Dictionary
        {
            get
            {
                return this;
            }
        }

        protected Hashtable InnerHashtable
        {
            get
            {
                if (this.hashtable == null)
                {
                    this.hashtable = new Hashtable();
                }
                return this.hashtable;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return this.InnerHashtable.IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this.InnerHashtable.SyncRoot;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return this.InnerHashtable.IsFixedSize;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return this.InnerHashtable.IsReadOnly;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                object currentValue = this.InnerHashtable[key];
                this.OnGet(key, currentValue);
                return currentValue;
            }
            set
            {
                this.OnValidate(key, value);
                bool flag = true;
                object oldValue = this.InnerHashtable[key];
                if (oldValue == null)
                {
                    flag = this.InnerHashtable.Contains(key);
                }
                this.OnSet(key, oldValue, value);
                this.InnerHashtable[key] = value;
                try
                {
                    this.OnSetComplete(key, oldValue, value);
                }
                catch
                {
                    if (flag)
                    {
                        this.InnerHashtable[key] = oldValue;
                    }
                    else
                    {
                        this.InnerHashtable.Remove(key);
                    }
                    throw;
                }
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return this.InnerHashtable.Keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return this.InnerHashtable.Values;
            }
        }
    }
}

