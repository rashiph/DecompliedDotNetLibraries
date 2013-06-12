namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(string))]
    public sealed class FullTrustAssemblyCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(FullTrustAssembly fullTrustAssembly)
        {
            this.BaseAdd(fullTrustAssembly);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FullTrustAssembly();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (((FullTrustAssembly) element).AssemblyName + ((FullTrustAssembly) element).Version);
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

        public FullTrustAssembly this[int index]
        {
            get
            {
                return (FullTrustAssembly) base.BaseGet(index);
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

