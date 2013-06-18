namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Web.Util;

    [ConfigurationCollection(typeof(UrlMapping))]
    public sealed class UrlMappingCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public UrlMappingCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(UrlMapping urlMapping)
        {
            this.BaseAdd(urlMapping);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new UrlMapping();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((UrlMapping) element).Url;
        }

        public string GetKey(int index)
        {
            return (string) base.BaseGetKey(index);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void Remove(UrlMapping urlMapping)
        {
            base.BaseRemove(this.GetElementKey(urlMapping));
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public string[] AllKeys
        {
            get
            {
                return System.Web.Util.StringUtil.ObjectArrayToStringArray(base.BaseGetAllKeys());
            }
        }

        public UrlMapping this[string name]
        {
            get
            {
                return (UrlMapping) base.BaseGet(name);
            }
        }

        public UrlMapping this[int index]
        {
            get
            {
                return (UrlMapping) base.BaseGet(index);
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

