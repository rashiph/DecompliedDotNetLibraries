namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;

    public class ConfigurationPropertyCollection : ICollection, IEnumerable
    {
        private ArrayList _items = new ArrayList();

        public void Add(ConfigurationProperty property)
        {
            if (!this.Contains(property.Name))
            {
                this._items.Add(property);
            }
        }

        public void Clear()
        {
            this._items.Clear();
        }

        public bool Contains(string name)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                ConfigurationProperty property = (ConfigurationProperty) this._items[i];
                if (property.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ConfigurationProperty[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        public bool Remove(string name)
        {
            for (int i = 0; i < this._items.Count; i++)
            {
                ConfigurationProperty property = (ConfigurationProperty) this._items[i];
                if (property.Name == name)
                {
                    this._items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this._items.CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        internal ConfigurationProperty DefaultCollectionProperty
        {
            get
            {
                return this[ConfigurationProperty.DefaultCollectionPropertyName];
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public ConfigurationProperty this[string name]
        {
            get
            {
                for (int i = 0; i < this._items.Count; i++)
                {
                    ConfigurationProperty property = (ConfigurationProperty) this._items[i];
                    if (property.Name == name)
                    {
                        return (ConfigurationProperty) this._items[i];
                    }
                }
                return null;
            }
        }

        public object SyncRoot
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._items;
            }
        }
    }
}

