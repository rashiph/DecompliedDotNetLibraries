namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(string))]
    public sealed class PartialTrustVisibleAssemblyCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(PartialTrustVisibleAssembly partialTrustVisibleAssembly)
        {
            this.BaseAdd(partialTrustVisibleAssembly);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PartialTrustVisibleAssembly();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PartialTrustVisibleAssembly) element).AssemblyName;
        }

        internal bool IsRemoved(string key)
        {
            return base.BaseIsRemoved(key);
        }

        public void Remove(string key)
        {
            base.BaseRemove(key);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public PartialTrustVisibleAssembly this[int index]
        {
            get
            {
                return (PartialTrustVisibleAssembly) base.BaseGet(index);
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

