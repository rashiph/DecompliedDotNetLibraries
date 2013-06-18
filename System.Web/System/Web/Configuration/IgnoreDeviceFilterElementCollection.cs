namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(IgnoreDeviceFilterElement), AddItemName="filter", CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class IgnoreDeviceFilterElementCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public IgnoreDeviceFilterElementCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(IgnoreDeviceFilterElement deviceFilter)
        {
            this.BaseAdd(deviceFilter);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IgnoreDeviceFilterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IgnoreDeviceFilterElement) element).Name;
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void Remove(IgnoreDeviceFilterElement deviceFilter)
        {
            base.BaseRemove(this.GetElementKey(deviceFilter));
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "filter";
            }
        }

        public IgnoreDeviceFilterElement this[string name]
        {
            get
            {
                return (IgnoreDeviceFilterElement) base.BaseGet(name);
            }
        }

        public IgnoreDeviceFilterElement this[int index]
        {
            get
            {
                return (IgnoreDeviceFilterElement) base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
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

