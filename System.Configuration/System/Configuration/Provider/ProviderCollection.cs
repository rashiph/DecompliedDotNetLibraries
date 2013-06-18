namespace System.Configuration.Provider
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;

    public class ProviderCollection : ICollection, IEnumerable
    {
        private Hashtable _Hashtable = new Hashtable(10, StringComparer.OrdinalIgnoreCase);
        private bool _ReadOnly;

        public virtual void Add(ProviderBase provider)
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException(System.Configuration.SR.GetString("CollectionReadOnly"));
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if ((provider.Name == null) || (provider.Name.Length < 1))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Config_provider_name_null_or_empty"));
            }
            this._Hashtable.Add(provider.Name, provider);
        }

        public void Clear()
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException(System.Configuration.SR.GetString("CollectionReadOnly"));
            }
            this._Hashtable.Clear();
        }

        public void CopyTo(ProviderBase[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this._Hashtable.Values.GetEnumerator();
        }

        public void Remove(string name)
        {
            if (this._ReadOnly)
            {
                throw new NotSupportedException(System.Configuration.SR.GetString("CollectionReadOnly"));
            }
            this._Hashtable.Remove(name);
        }

        public void SetReadOnly()
        {
            if (!this._ReadOnly)
            {
                this._ReadOnly = true;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this._Hashtable.Values.CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                return this._Hashtable.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public ProviderBase this[string name]
        {
            get
            {
                return (this._Hashtable[name] as ProviderBase);
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

