namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(AssemblyInfo))]
    public sealed class AssemblyCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public void Add(AssemblyInfo assemblyInformation)
        {
            this.BaseAdd(assemblyInformation);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AssemblyInfo();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssemblyInfo) element).Assembly;
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

        public AssemblyInfo this[int index]
        {
            get
            {
                return (AssemblyInfo) base.BaseGet(index);
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

        public AssemblyInfo this[string assemblyName]
        {
            get
            {
                return (AssemblyInfo) base.BaseGet(assemblyName);
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

