namespace System.Configuration
{
    using System;
    using System.Reflection;
    using System.Runtime;

    [ConfigurationCollection(typeof(ProviderSettings))]
    public sealed class ProviderSettingsCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public ProviderSettingsCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(ProviderSettings provider)
        {
            if (provider != null)
            {
                provider.UpdatePropertyCollection();
                this.BaseAdd(provider);
            }
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderSettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderSettings) element).Name;
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public ProviderSettings this[string key]
        {
            get
            {
                return (ProviderSettings) base.BaseGet(key);
            }
        }

        public ProviderSettings this[int index]
        {
            get
            {
                return (ProviderSettings) base.BaseGet(index);
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

        protected internal override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return _properties;
            }
        }
    }
}

