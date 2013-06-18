namespace System.Runtime.Caching.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(MemoryCacheElement), CollectionType=ConfigurationElementCollectionType.AddRemoveClearMap)]
    public sealed class MemoryCacheSettingsCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(MemoryCacheElement cache)
        {
            this.BaseAdd(cache);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MemoryCacheElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new MemoryCacheElement(elementName);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MemoryCacheElement) element).Name;
        }

        public int IndexOf(MemoryCacheElement cache)
        {
            return base.BaseIndexOf(cache);
        }

        public void Remove(MemoryCacheElement cache)
        {
            base.BaseRemove(cache.Name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMapAlternate;
            }
        }

        public MemoryCacheElement this[int index]
        {
            get
            {
                return (MemoryCacheElement) base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public MemoryCacheElement this[string key]
        {
            get
            {
                return (MemoryCacheElement) base.BaseGet(key);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

