namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    public class PersonalizationDictionary : IDictionary, ICollection, IEnumerable
    {
        private HybridDictionary _dictionary;

        public PersonalizationDictionary()
        {
            this._dictionary = new HybridDictionary(true);
        }

        public PersonalizationDictionary(int initialSize)
        {
            this._dictionary = new HybridDictionary(initialSize, true);
        }

        public virtual void Add(string key, PersonalizationEntry value)
        {
            key = StringUtil.CheckAndTrimString(key, "key");
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this._dictionary.Add(key, value);
        }

        public virtual void Clear()
        {
            this._dictionary.Clear();
        }

        public virtual bool Contains(string key)
        {
            key = StringUtil.CheckAndTrimString(key, "key");
            return this._dictionary.Contains(key);
        }

        public virtual void CopyTo(DictionaryEntry[] array, int index)
        {
            this._dictionary.CopyTo(array, index);
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        public virtual void Remove(string key)
        {
            key = StringUtil.CheckAndTrimString(key, "key");
            this._dictionary.Remove(key);
        }

        internal void RemoveSharedProperties()
        {
            DictionaryEntry[] array = new DictionaryEntry[this.Count];
            this.CopyTo(array, 0);
            foreach (DictionaryEntry entry in array)
            {
                if (((PersonalizationEntry) entry.Value).Scope == PersonalizationScope.Shared)
                {
                    this.Remove((string) entry.Key);
                }
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (!(array is DictionaryEntry[]))
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationDictionary_MustBeTypeDictionaryEntryArray"), "array");
            }
            this.CopyTo((DictionaryEntry[]) array, index);
        }

        void IDictionary.Add(object key, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!(key is string))
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationDictionary_MustBeTypeString"), "key");
            }
            if (!(value is PersonalizationEntry))
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationDictionary_MustBeTypePersonalizationEntry"), "value");
            }
            this.Add((string) key, (PersonalizationEntry) value);
        }

        bool IDictionary.Contains(object key)
        {
            if (!(key is string))
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationDictionary_MustBeTypeString"), "key");
            }
            return this.Contains((string) key);
        }

        void IDictionary.Remove(object key)
        {
            if (!(key is string))
            {
                throw new ArgumentException(System.Web.SR.GetString("PersonalizationDictionary_MustBeTypeString"), "key");
            }
            this.Remove((string) key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public virtual int Count
        {
            get
            {
                return this._dictionary.Count;
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

        public virtual PersonalizationEntry this[string key]
        {
            get
            {
                key = StringUtil.CheckAndTrimString(key, "key");
                return (PersonalizationEntry) this._dictionary[key];
            }
            set
            {
                key = StringUtil.CheckAndTrimString(key, "key");
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._dictionary[key] = value;
            }
        }

        public virtual ICollection Keys
        {
            get
            {
                return this._dictionary.Keys;
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                return this;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (!(key is string))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PersonalizationDictionary_MustBeTypeString"), "key");
                }
                return this[(string) key];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (!(key is string))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PersonalizationDictionary_MustBeTypeString"), "key");
                }
                if (!(value is PersonalizationEntry))
                {
                    throw new ArgumentException(System.Web.SR.GetString("PersonalizationDictionary_MustBeTypePersonalizationEntry"), "value");
                }
                this[(string) key] = (PersonalizationEntry) value;
            }
        }

        public virtual ICollection Values
        {
            get
            {
                return this._dictionary.Values;
            }
        }
    }
}

