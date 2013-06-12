namespace System.Collections.Specialized
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    public class HybridDictionary : IDictionary, ICollection, IEnumerable
    {
        private bool caseInsensitive;
        private const int CutoverPoint = 9;
        private const int FixedSizeCutoverPoint = 6;
        private Hashtable hashtable;
        private const int InitialHashtableSize = 13;
        private ListDictionary list;

        public HybridDictionary()
        {
        }

        public HybridDictionary(bool caseInsensitive)
        {
            this.caseInsensitive = caseInsensitive;
        }

        public HybridDictionary(int initialSize) : this(initialSize, false)
        {
        }

        public HybridDictionary(int initialSize, bool caseInsensitive)
        {
            this.caseInsensitive = caseInsensitive;
            if (initialSize >= 6)
            {
                if (caseInsensitive)
                {
                    this.hashtable = new Hashtable(initialSize, StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    this.hashtable = new Hashtable(initialSize);
                }
            }
        }

        public void Add(object key, object value)
        {
            if (this.hashtable != null)
            {
                this.hashtable.Add(key, value);
            }
            else if (this.list == null)
            {
                this.list = new ListDictionary(this.caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
                this.list.Add(key, value);
            }
            else if ((this.list.Count + 1) >= 9)
            {
                this.ChangeOver();
                this.hashtable.Add(key, value);
            }
            else
            {
                this.list.Add(key, value);
            }
        }

        private void ChangeOver()
        {
            Hashtable hashtable;
            IDictionaryEnumerator enumerator = this.list.GetEnumerator();
            if (this.caseInsensitive)
            {
                hashtable = new Hashtable(13, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                hashtable = new Hashtable(13);
            }
            while (enumerator.MoveNext())
            {
                hashtable.Add(enumerator.Key, enumerator.Value);
            }
            this.hashtable = hashtable;
            this.list = null;
        }

        public void Clear()
        {
            if (this.hashtable != null)
            {
                Hashtable hashtable = this.hashtable;
                this.hashtable = null;
                hashtable.Clear();
            }
            if (this.list != null)
            {
                ListDictionary list = this.list;
                this.list = null;
                list.Clear();
            }
        }

        public bool Contains(object key)
        {
            ListDictionary list = this.list;
            if (this.hashtable != null)
            {
                return this.hashtable.Contains(key);
            }
            if (list != null)
            {
                return list.Contains(key);
            }
            if (key == null)
            {
                throw new ArgumentNullException("key", SR.GetString("ArgumentNull_Key"));
            }
            return false;
        }

        public void CopyTo(Array array, int index)
        {
            if (this.hashtable != null)
            {
                this.hashtable.CopyTo(array, index);
            }
            else
            {
                this.List.CopyTo(array, index);
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            if (this.hashtable != null)
            {
                return this.hashtable.GetEnumerator();
            }
            if (this.list == null)
            {
                this.list = new ListDictionary(this.caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
            }
            return this.list.GetEnumerator();
        }

        public void Remove(object key)
        {
            if (this.hashtable != null)
            {
                this.hashtable.Remove(key);
            }
            else if (this.list != null)
            {
                this.list.Remove(key);
            }
            else if (key == null)
            {
                throw new ArgumentNullException("key", SR.GetString("ArgumentNull_Key"));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.hashtable != null)
            {
                return this.hashtable.GetEnumerator();
            }
            if (this.list == null)
            {
                this.list = new ListDictionary(this.caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
            }
            return this.list.GetEnumerator();
        }

        public int Count
        {
            get
            {
                ListDictionary list = this.list;
                if (this.hashtable != null)
                {
                    return this.hashtable.Count;
                }
                if (list != null)
                {
                    return list.Count;
                }
                return 0;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object this[object key]
        {
            get
            {
                ListDictionary list = this.list;
                if (this.hashtable != null)
                {
                    return this.hashtable[key];
                }
                if (list != null)
                {
                    return list[key];
                }
                if (key == null)
                {
                    throw new ArgumentNullException("key", SR.GetString("ArgumentNull_Key"));
                }
                return null;
            }
            set
            {
                if (this.hashtable != null)
                {
                    this.hashtable[key] = value;
                }
                else if (this.list != null)
                {
                    if (this.list.Count >= 8)
                    {
                        this.ChangeOver();
                        this.hashtable[key] = value;
                    }
                    else
                    {
                        this.list[key] = value;
                    }
                }
                else
                {
                    this.list = new ListDictionary(this.caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
                    this.list[key] = value;
                }
            }
        }

        public ICollection Keys
        {
            get
            {
                if (this.hashtable != null)
                {
                    return this.hashtable.Keys;
                }
                return this.List.Keys;
            }
        }

        private ListDictionary List
        {
            get
            {
                if (this.list == null)
                {
                    this.list = new ListDictionary(this.caseInsensitive ? StringComparer.OrdinalIgnoreCase : null);
                }
                return this.list;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public ICollection Values
        {
            get
            {
                if (this.hashtable != null)
                {
                    return this.hashtable.Values;
                }
                return this.List.Values;
            }
        }
    }
}

