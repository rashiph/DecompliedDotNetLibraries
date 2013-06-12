namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class AggregateDictionary : IDictionary, ICollection, IEnumerable
    {
        private ICollection _dictionaries;

        public AggregateDictionary(ICollection dictionaries)
        {
            this._dictionaries = dictionaries;
        }

        public virtual void Add(object key, object value)
        {
            throw new NotSupportedException();
        }

        public virtual void Clear()
        {
            throw new NotSupportedException();
        }

        public virtual bool Contains(object key)
        {
            foreach (IDictionary dictionary in this._dictionaries)
            {
                if (dictionary.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return new DictionaryEnumeratorByKeys(this);
        }

        public virtual void Remove(object key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DictionaryEnumeratorByKeys(this);
        }

        public virtual int Count
        {
            get
            {
                int num = 0;
                foreach (IDictionary dictionary in this._dictionaries)
                {
                    num += dictionary.Count;
                }
                return num;
            }
        }

        public virtual bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual object this[object key]
        {
            get
            {
                foreach (IDictionary dictionary in this._dictionaries)
                {
                    if (dictionary.Contains(key))
                    {
                        return dictionary[key];
                    }
                }
                return null;
            }
            set
            {
                foreach (IDictionary dictionary in this._dictionaries)
                {
                    if (dictionary.Contains(key))
                    {
                        dictionary[key] = value;
                    }
                }
            }
        }

        public virtual ICollection Keys
        {
            get
            {
                ArrayList list = new ArrayList();
                foreach (IDictionary dictionary in this._dictionaries)
                {
                    ICollection keys = dictionary.Keys;
                    if (keys != null)
                    {
                        foreach (object obj2 in keys)
                        {
                            list.Add(obj2);
                        }
                    }
                }
                return list;
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public virtual ICollection Values
        {
            get
            {
                ArrayList list = new ArrayList();
                foreach (IDictionary dictionary in this._dictionaries)
                {
                    ICollection values = dictionary.Values;
                    if (values != null)
                    {
                        foreach (object obj2 in values)
                        {
                            list.Add(obj2);
                        }
                    }
                }
                return list;
            }
        }
    }
}

