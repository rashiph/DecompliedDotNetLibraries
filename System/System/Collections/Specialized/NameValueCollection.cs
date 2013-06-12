namespace System.Collections.Specialized
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable]
    public class NameValueCollection : NameObjectCollectionBase
    {
        private string[] _all;
        private string[] _allKeys;

        public NameValueCollection()
        {
        }

        public NameValueCollection(IEqualityComparer equalityComparer) : base(equalityComparer)
        {
        }

        public NameValueCollection(NameValueCollection col) : base((col != null) ? col.Comparer : null)
        {
            this.Add(col);
        }

        internal NameValueCollection(DBNull dummy) : base(dummy)
        {
        }

        public NameValueCollection(int capacity) : base(capacity)
        {
        }

        [Obsolete("Please use NameValueCollection(IEqualityComparer) instead.")]
        public NameValueCollection(IHashCodeProvider hashProvider, IComparer comparer) : base(hashProvider, comparer)
        {
        }

        public NameValueCollection(int capacity, IEqualityComparer equalityComparer) : base(capacity, equalityComparer)
        {
        }

        public NameValueCollection(int capacity, NameValueCollection col) : base(capacity, (col != null) ? col.Comparer : null)
        {
            if (col == null)
            {
                throw new ArgumentNullException("col");
            }
            base.Comparer = col.Comparer;
            this.Add(col);
        }

        protected NameValueCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [Obsolete("Please use NameValueCollection(Int32, IEqualityComparer) instead.")]
        public NameValueCollection(int capacity, IHashCodeProvider hashProvider, IComparer comparer) : base(capacity, hashProvider, comparer)
        {
        }

        public void Add(NameValueCollection c)
        {
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }
            this.InvalidateCachedArrays();
            int count = c.Count;
            for (int i = 0; i < count; i++)
            {
                string key = c.GetKey(i);
                string[] values = c.GetValues(i);
                if (values != null)
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        this.Add(key, values[j]);
                    }
                }
                else
                {
                    this.Add(key, null);
                }
            }
        }

        public virtual void Add(string name, string value)
        {
            if (base.IsReadOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            this.InvalidateCachedArrays();
            ArrayList list = (ArrayList) base.BaseGet(name);
            if (list == null)
            {
                list = new ArrayList(1);
                if (value != null)
                {
                    list.Add(value);
                }
                base.BaseAdd(name, list);
            }
            else if (value != null)
            {
                list.Add(value);
            }
        }

        public virtual void Clear()
        {
            if (base.IsReadOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            this.InvalidateCachedArrays();
            base.BaseClear();
        }

        public void CopyTo(Array dest, int index)
        {
            if (dest == null)
            {
                throw new ArgumentNullException("dest");
            }
            if (dest.Rank != 1)
            {
                throw new ArgumentException(SR.GetString("Arg_MultiRank"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", SR.GetString("IndexOutOfRange", new object[] { index.ToString(CultureInfo.CurrentCulture) }));
            }
            if ((dest.Length - index) < this.Count)
            {
                throw new ArgumentException(SR.GetString("Arg_InsufficientSpace"));
            }
            int count = this.Count;
            if (this._all == null)
            {
                this._all = new string[count];
                for (int i = 0; i < count; i++)
                {
                    this._all[i] = this.Get(i);
                    dest.SetValue(this._all[i], (int) (i + index));
                }
            }
            else
            {
                for (int j = 0; j < count; j++)
                {
                    dest.SetValue(this._all[j], (int) (j + index));
                }
            }
        }

        public virtual string Get(int index)
        {
            ArrayList list = (ArrayList) base.BaseGet(index);
            return GetAsOneString(list);
        }

        public virtual string Get(string name)
        {
            ArrayList list = (ArrayList) base.BaseGet(name);
            return GetAsOneString(list);
        }

        private static string GetAsOneString(ArrayList list)
        {
            int num = (list != null) ? list.Count : 0;
            if (num == 1)
            {
                return (string) list[0];
            }
            if (num <= 1)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder((string) list[0]);
            for (int i = 1; i < num; i++)
            {
                builder.Append(',');
                builder.Append((string) list[i]);
            }
            return builder.ToString();
        }

        private static string[] GetAsStringArray(ArrayList list)
        {
            int count = (list != null) ? list.Count : 0;
            if (count == 0)
            {
                return null;
            }
            string[] array = new string[count];
            list.CopyTo(0, array, 0, count);
            return array;
        }

        public virtual string GetKey(int index)
        {
            return base.BaseGetKey(index);
        }

        public virtual string[] GetValues(int index)
        {
            ArrayList list = (ArrayList) base.BaseGet(index);
            return GetAsStringArray(list);
        }

        public virtual string[] GetValues(string name)
        {
            ArrayList list = (ArrayList) base.BaseGet(name);
            return GetAsStringArray(list);
        }

        public bool HasKeys()
        {
            return this.InternalHasKeys();
        }

        internal virtual bool InternalHasKeys()
        {
            return base.BaseHasKeys();
        }

        protected void InvalidateCachedArrays()
        {
            this._all = null;
            this._allKeys = null;
        }

        public virtual void Remove(string name)
        {
            this.InvalidateCachedArrays();
            base.BaseRemove(name);
        }

        public virtual void Set(string name, string value)
        {
            if (base.IsReadOnly)
            {
                throw new NotSupportedException(SR.GetString("CollectionReadOnly"));
            }
            this.InvalidateCachedArrays();
            ArrayList list = new ArrayList(1);
            list.Add(value);
            base.BaseSet(name, list);
        }

        public virtual string[] AllKeys
        {
            get
            {
                if (this._allKeys == null)
                {
                    this._allKeys = base.BaseGetAllKeys();
                }
                return this._allKeys;
            }
        }

        public string this[string name]
        {
            get
            {
                return this.Get(name);
            }
            set
            {
                this.Set(name, value);
            }
        }

        public string this[int index]
        {
            get
            {
                return this.Get(index);
            }
        }
    }
}

