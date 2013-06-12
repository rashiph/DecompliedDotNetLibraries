namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security;

    internal abstract class MessageDictionary : IDictionary, ICollection, IEnumerable
    {
        internal IDictionary _dict;
        internal string[] _keys;

        internal MessageDictionary(string[] keys, IDictionary idict)
        {
            this._keys = keys;
            this._dict = idict;
        }

        public virtual void Add(object key, object value)
        {
            if (this.ContainsSpecialKey(key))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidKey"));
            }
            if (this._dict == null)
            {
                this._dict = new Hashtable();
            }
            this._dict.Add(key, value);
        }

        public virtual void Clear()
        {
            if (this._dict != null)
            {
                this._dict.Clear();
            }
        }

        public virtual bool Contains(object key)
        {
            return (this.ContainsSpecialKey(key) || ((this._dict != null) && this._dict.Contains(key)));
        }

        protected virtual bool ContainsSpecialKey(object key)
        {
            if (key is string)
            {
                string str = (string) key;
                for (int i = 0; i < this._keys.Length; i++)
                {
                    if (str.Equals(this._keys[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void CopyTo(Array array, int index)
        {
            for (int i = 0; i < this._keys.Length; i++)
            {
                array.SetValue(this.GetMessageValue(i), (int) (index + i));
            }
            if (this._dict != null)
            {
                this._dict.CopyTo(array, index + this._keys.Length);
            }
        }

        internal abstract object GetMessageValue(int i);
        internal bool HasUserData()
        {
            return ((this._dict != null) && (this._dict.Count > 0));
        }

        public virtual void Remove(object key)
        {
            if (this.ContainsSpecialKey(key) || (this._dict == null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidKey"));
            }
            this._dict.Remove(key);
        }

        [SecurityCritical]
        internal abstract void SetSpecialKey(int keyNum, object value);
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new MessageDictionaryEnumerator(this, this._dict);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public virtual int Count
        {
            get
            {
                if (this._dict != null)
                {
                    return (this._dict.Count + this._keys.Length);
                }
                return this._keys.Length;
            }
        }

        internal IDictionary InternalDictionary
        {
            get
            {
                return this._dict;
            }
        }

        public virtual bool IsFixedSize
        {
            get
            {
                return false;
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
                string str = key as string;
                if (str != null)
                {
                    for (int i = 0; i < this._keys.Length; i++)
                    {
                        if (str.Equals(this._keys[i]))
                        {
                            return this.GetMessageValue(i);
                        }
                    }
                    if (this._dict != null)
                    {
                        return this._dict[key];
                    }
                }
                return null;
            }
            [SecuritySafeCritical]
            set
            {
                if (this.ContainsSpecialKey(key))
                {
                    if (key.Equals(Message.UriKey))
                    {
                        this.SetSpecialKey(0, value);
                    }
                    else
                    {
                        if (!key.Equals(Message.CallContextKey))
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidKey"));
                        }
                        this.SetSpecialKey(1, value);
                    }
                }
                else
                {
                    if (this._dict == null)
                    {
                        this._dict = new Hashtable();
                    }
                    this._dict[key] = value;
                }
            }
        }

        public virtual ICollection Keys
        {
            get
            {
                int length = this._keys.Length;
                ICollection c = (this._dict != null) ? this._dict.Keys : null;
                if (c != null)
                {
                    length += c.Count;
                }
                ArrayList list = new ArrayList(length);
                for (int i = 0; i < this._keys.Length; i++)
                {
                    list.Add(this._keys[i]);
                }
                if (c != null)
                {
                    list.AddRange(c);
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
                int length = this._keys.Length;
                ICollection c = (this._dict != null) ? this._dict.Keys : null;
                if (c != null)
                {
                    length += c.Count;
                }
                ArrayList list = new ArrayList(length);
                for (int i = 0; i < this._keys.Length; i++)
                {
                    list.Add(this.GetMessageValue(i));
                }
                if (c != null)
                {
                    list.AddRange(c);
                }
                return list;
            }
        }
    }
}

