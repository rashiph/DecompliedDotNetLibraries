namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;

    [ConfigurationCollection(typeof(BuildProvider))]
    public sealed class BuildProviderCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public BuildProviderCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(BuildProvider buildProvider)
        {
            this.BaseAdd(buildProvider);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BuildProvider();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BuildProvider) element).Extension;
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public BuildProvider this[string name]
        {
            get
            {
                return (BuildProvider) base.BaseGet(name);
            }
        }

        public BuildProvider this[int index]
        {
            get
            {
                return (BuildProvider) base.BaseGet(index);
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

